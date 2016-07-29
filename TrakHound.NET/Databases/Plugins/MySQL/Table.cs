using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.SqlClient;

using TrakHound.Configurations;
using TrakHound.Databases;
using TrakHound;
using TrakHound.Logging;
using TrakHound.Tools;
using TrakHound.Plugins;
using TrakHound.Plugins.Database;

namespace TrakHound.Databases.Plugins.MySQL
{
    public partial class Plugin
    {

        public bool Table_Create(object settings, string tablename, ColumnDefinition[] columnDefinitions, string[] primaryKey)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    string coldef = "";

                    //Create Column Definition string
                    for (int x = 0; x <= columnDefinitions.Length - 1; x++)
                    {
                        coldef += ConvertColumnDefinition(columnDefinitions[x]).ToString();
                        if (x < columnDefinitions.Length - 1) coldef += ",";
                    }

                    string keydef = "";
                    if (primaryKey != null)
                    {
                        keydef = ", PRIMARY KEY (";

                        for (var k = 0; k <= primaryKey.Length - 1; k++)
                        {
                            keydef += primaryKey[k];
                            if (k < primaryKey.Length - 1) keydef += ", ";
                        }

                        keydef += ")";
                    }

                    var query = "IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES" +
                                " WHERE TABLE_NAME = '" + tablename + "'))" +
                                " BEGIN" +
                                " CREATE TABLE " + tablename + " (" + coldef + keydef + ")" +
                                " END" +
                                " ELSE" +
                                " BEGIN";

                    for (int x = 0; x <= columnDefinitions.Length - 1; x++)
                    {
                        query += " IF (NOT EXISTS (SELECT * FROM sys.columns" +
                                 " WHERE Name = N'" + columnDefinitions[x].ColumnName + "' AND Object_ID = Object_ID(N'" + tablename + "')))" +
                                 " ALTER TABLE " + tablename + " ADD " + ConvertColumnDefinition(columnDefinitions[x]);
                    }

                    query += " END";

                    result = (bool)ExecuteQuery<bool>(config, query);
                }
            }

            return result;
        }

        public bool Table_Replace(object settings, string tablename, ColumnDefinition[] columnDefinitions, string[] primaryKey)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    string coldef = "";

                    //Create Column Definition string
                    for (int x = 0; x <= columnDefinitions.Length - 1; x++)
                    {
                        coldef += ConvertColumnDefinition(columnDefinitions[x]).ToString();
                        if (x < columnDefinitions.Length - 1) coldef += ",";
                    }

                    string keydef = "";
                    if (primaryKey != null)
                    {
                        keydef = ", PRIMARY KEY (";

                        for (var k = 0; k <= primaryKey.Length - 1; k++)
                        {
                            keydef += primaryKey[k];
                            if (k < primaryKey.Length - 1) keydef += ", ";
                        }

                        keydef += ")";
                    }

                    bool success = false;

                    // Drop Table (Replace
                    var query = "IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES" +
                                " WHERE TABLE_NAME = '" + tablename + "'))" +
                                " DROP TABLE " + tablename;

                    success = (bool)ExecuteQuery<bool>(config, query);

                    // Create new Table
                    query = "IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES" +
                                " WHERE TABLE_NAME = '" + tablename + "'))" +
                                " BEGIN" +
                                " CREATE TABLE " + tablename + " (" + coldef + keydef + ")" +
                                " END" +
                                " ELSE" +
                                " BEGIN";

                    for (int x = 0; x <= columnDefinitions.Length - 1; x++)
                    {
                        query += " IF (NOT EXISTS (SELECT * FROM sys.columns" +
                                 " WHERE Name = N'" + columnDefinitions[x].ColumnName + "' AND Object_ID = Object_ID(N'" + tablename + "')))" +
                                 " ALTER TABLE " + tablename + " ADD " + ConvertColumnDefinition(columnDefinitions[x]);
                    }

                    query += " END";

                    result = (bool)ExecuteQuery<bool>(config, query);
                }
            }

            return result;
        }

        public bool Table_Drop(object settings, string tablename)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES" +
                                " WHERE TABLE_NAME = '" + tablename + "'))" +
                                " DROP TABLE " + tablename;

                    result = (bool)ExecuteQuery<bool>(config, query);
                }
            }

            return result;
        }

        public bool Table_Drop(object settings, string[] tableNames)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    string query = "";

                    foreach (var tablename in tableNames)
                    {
                        query += "IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES" +
                                " WHERE TABLE_NAME = '" + tablename + "'))" +
                                " BEGIN" +
                                " DROP TABLE " + tablename +
                                " END";
                    }


                    result = (bool)ExecuteQuery<bool>(config, query);
                }
            }

            return result;
        }

        public bool Table_Truncate(object settings, string tablename)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES" +
                                " WHERE TABLE_NAME = '" + tablename + "'))" +
                                " TRUNCATE TABLE " + tablename;

                    result = (bool)ExecuteQuery<bool>(config, query);
                }
            }

            return result;
        }

        public DataTable Table_Get(object settings, string tablename)
        {
            DataTable result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT * FROM " + tablename;

                    result = (DataTable)ExecuteQuery<DataTable>(config, query);

                    if (result != null) result.TableName = tablename;
                }
            }

            return result;
        }

        public DataTable Table_Get(object settings, string tablename, Int64 limit, Int64 offset)
        {
            DataTable result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT *, ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) AS RowNum INTO tempTable FROM " + tablename + "; ";
                    query += "SELECT * INTO tempTable1 FROM tempTable WHERE tempTable.RowNum BETWEEN " + offset.ToString() + " AND " + (limit + offset).ToString() + "; ";
                    query += "ALTER TABLE tempTable1 DROP COLUMN RowNum; ";
                    query += "SELECT * FROM tempTable1; ";
                    query += "DROP TABLE tempTable; ";
                    query += "DROP TABLE tempTable1; ";

                    result = (DataTable)ExecuteQuery<DataTable>(config, query);

                    if (result != null) result.TableName = tablename;
                }
            }

            return result;
        }

        public DataTable Table_Get(object settings, string tablename, string filterExpression)
        {
            DataTable result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT * FROM " + tablename + " " + filterExpression;

                    result = (DataTable)ExecuteQuery<DataTable>(config, query);

                    if (result != null) result.TableName = tablename;
                }
            }

            return result;
        }

        public DataTable Table_Get(object settings, string tablename, string filterExpression, string columns)
        {
            DataTable result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT " + columns + " FROM " + tablename + " " + filterExpression;

                    result = (DataTable)ExecuteQuery<DataTable>(config, query);

                    if (result != null) result.TableName = tablename;
                }
            }

            return result;
        }

        public DataTable[] Table_Get(object settings, string[] tablenames)
        {
            DataTable[] result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var tables = new List<DataTable>();

                    foreach (var tablename in tablenames)
                    {
                        var query = "SELECT * FROM " + tablename;

                        var table = (DataTable)ExecuteQuery<DataTable>(config, query);

                        if (table != null)
                        {
                            table.TableName = tablename;
                            tables.Add(table);
                        }
                    }

                    result = tables.ToArray();
                }
            }

            return result;
        }

        public DataTable[] Table_Get(object settings, string[] tablenames, string[] filterExpressions)
        {
            DataTable[] result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var tables = new List<DataTable>();

                    if (tablenames.Length == filterExpressions.Length)
                    {
                        for (var x = 0; x <= tablenames.Length - 1; x++)
                        {
                            string filterExpression = "";
                            if (filterExpressions[x] != null) filterExpression = " " + filterExpressions[x];

                            var query = "SELECT * FROM " + tablenames[x] + filterExpression;

                            var table = (DataTable)ExecuteQuery<DataTable>(config, query);

                            if (table != null)
                            {
                                table.TableName = tablenames[x];
                                tables.Add(table);
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static string GetColumnValues(string columns)
        {
            string result = "*";

            string[] columnValues = columns.Split(',');
            if (columnValues != null)
            {
                result = "";

                for (var i = 0; i <= columnValues.Length - 1; i++)
                {
                    string columnName = columnValues[i].Trim();
                    result += COLUMN_NAME_START + columnName + COLUMN_NAME_END;
                    if (i < columnValues.Length - 1) result += ", ";
                }
            }

            return result;
        }

        public DataTable[] Table_Get(object settings, string[] tablenames, string[] filterExpressions, string[] columns)
        {
            DataTable[] result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var tables = new List<DataTable>();

                    if (tablenames.Length == filterExpressions.Length && tablenames.Length == columns.Length)
                    {
                        for (var x = 0; x <= tablenames.Length - 1; x++)
                        {
                            string column = "*";
                            if (columns[x] != null) column = GetColumnValues(columns[x]);

                            string filterExpression = "";
                            if (filterExpressions[x] != null) filterExpression = " " + filterExpressions[x];

                            var query = "SELECT " + column + " FROM " + tablenames[x] + filterExpression;

                            var table = (DataTable)ExecuteQuery<DataTable>(config, query);

                            if (table != null)
                            {
                                table.TableName = tablenames[x];
                                tables.Add(table);
                            }
                        }
                    }

                    result = tables.ToArray();
                }
            }

            return result;
        }


        public string[] Table_List(object settings)
        {
            string[] result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT TABLE_NAME FROM " + config.Database + ".INFORMATION_SCHEMA.Tables";

                    result = (string[])ExecuteQuery<string[]>(config, query);
                }
            }

            return result;
        }

        public string[] Table_List(object settings, string filterExpression)
        {
            string[] result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT TABLE_NAME FROM " + config.Database + ".INFORMATION_SCHEMA.Tables WHERE TABLE_NAME " + filterExpression;

                    result = (string[])ExecuteQuery<string[]>(config, query);
                }
            }

            return result;
        }

        public Int64 Table_GetRowCount(object settings, string tablename)
        {
            Int64 result = -1;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT COUNT(*) FROM " + tablename;

                    result = (Int64)ExecuteQuery<Int64>(config, query);
                }
            }

            return result;
        }

        public Int64 Table_GetSize(object settings, string tablename)
        {
            Int64 result = -1;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "exec sp_spaceused " + tablename;

                    var dt = (DataTable)ExecuteQuery<DataTable>(config, query);
                    if (dt != null)
                    {
                        if (dt.Rows.Count > 0 && dt.Columns.Contains("data"))
                        {
                            object o = dt.Rows[0];
                            if (o != null)
                            {
                                Int64.TryParse(o.ToString(), out result);
                            }
                        }
                    }
                }
            }

            return result;
        }

    }
}
