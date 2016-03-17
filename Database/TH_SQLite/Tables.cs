using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using TH_Database;
using TH_Global.Functions;

namespace TH_SQLite
{
    public partial class Plugin
    {

        public bool Table_Create(object settings, string tablename, ColumnDefinition[] columnDefinitions, string[] primaryKey)
        {
            bool result = false;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
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
                            keydef += "`" + primaryKey[k] + "`";
                            if (k < primaryKey.Length - 1) keydef += ", ";
                        }

                        keydef += ")";
                    }

                    var query = "CREATE TABLE IF NOT EXISTS " + tablename + " (" + coldef + keydef + "); ";
                    //query = "PRAGMA table_info(" + tablename + ");";
                    //var tableInfo = (DataTable)ExecuteQuery<DataTable>(config, query);
                    result = (bool)ExecuteQuery<bool>(config, query);

                    if (result)
                    {
                        //Get list of existing columns(if table already existed, they may not be up to date)
                        query = "PRAGMA table_info(" + tablename + ")";
                        var tableInfo = (DataTable)ExecuteQuery<DataTable>(config, query);

                        var existingColumns = new List<string>();

                        if (tableInfo != null && tableInfo.Rows.Count > 0)
                        {
                            foreach (DataRow row in tableInfo.Rows)
                            {
                                string columnName = DataTable_Functions.GetRowValue("name", row);

                                existingColumns.Add(columnName);
                            }
                        }

                        query = "";
                        foreach (var columnDefinition in columnDefinitions)
                        {
                            if (!existingColumns.Exists(x => x == columnDefinition.ColumnName))
                            {
                                query += "ALTER TABLE " + tablename + " ADD COLUMN " + ConvertColumnDefinition(columnDefinition) + "; ";
                            }
                        }

                        if (query != "")
                        {
                            result = (bool)ExecuteQuery<bool>(config, query);
                        }
                    }
                }
            }

            return result;
        }

        public bool Table_Replace(object settings, string tablename, ColumnDefinition[] columnDefinitions, string[] primaryKey)
        {
            bool result = false;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
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
                            keydef += "`" + primaryKey[k] + "`";
                            if (k < primaryKey.Length - 1) keydef += ", ";
                        }

                        keydef += ")";
                    }

                    var query = "DROP TABLE IF EXISTS " + tablename + "; ";
                    query = "CREATE TABLE IF NOT EXISTS " + tablename + " (" + coldef + keydef + ")";

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
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var query = "DROP TABLE IF EXISTS " + tablename;
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
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    string query = "";

                    foreach (var tablename in tableNames)
                    {
                        query += "DROP TABLE IF EXISTS " + tablename + "; ";
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
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var query = "DELETE FROM TABLE " + tablename;

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
                var config = SQLite_Configuration.Get(settings);
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
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT * FROM " + tablename + " LIMIT " + limit.ToString() + " OFFSET " + offset.ToString();

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
                var config = SQLite_Configuration.Get(settings);
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
                var config = SQLite_Configuration.Get(settings);
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
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var tables = new List<DataTable>();

                    foreach (string tablename in tablenames)
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
                var config = SQLite_Configuration.Get(settings);
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

                    result = tables.ToArray();
                }
            }

            return result;
        }

        public DataTable[] Table_Get(object settings, string[] tablenames, string[] filterExpressions, string[] columns)
        {
            DataTable[] result = null;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var tables = new List<DataTable>();

                    if (tablenames.Length == filterExpressions.Length && tablenames.Length == columns.Length)
                    {
                        for (var x = 0; x <= tablenames.Length - 1; x++)
                        {
                            string column = "*";
                            if (columns[x] != null) column = columns[x];

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
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT name FROM sqlite_master WHERE type='table'";

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
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT name FROM sqlite_master WHERE type='table' AND " + filterExpression;

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
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT COUNT(*) FROM " + tablename;

                    result = (Int64)ExecuteQuery<Int64>(config, query);
                }
            }

            return result;
        }

        // Can't find an equivalent in SQLite. Since it is a local file, maybe it's not as important as server based databases anyways
        public Int64 Table_GetSize(object settings, string tablename)
        {
            Int64 result = -1;

            return result;
        }

    }
}
