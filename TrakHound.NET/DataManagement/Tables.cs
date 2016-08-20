// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;

using TrakHound.Logging;
using TrakHound.Plugins.Database;
using TrakHound.Tools;

namespace TrakHound.DataManagement
{
    public static class Table
    {
        public static string GetName(string tablename, string databaseId)
        {
            if (!string.IsNullOrEmpty(databaseId)) return databaseId + "_" + tablename;
            return tablename;
        }

        public static bool Create(object settings, string tablename, ColumnDefinition[] columnDefinitions, string[] primaryKey)
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
                        coldef += Column.ConvertColumnDefinition(columnDefinitions[x]).ToString();
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
                    result = (bool)Query.Execute<bool>(config, query);

                    if (result)
                    {
                        //Get list of existing columns(if table already existed, they may not be up to date)
                        query = "PRAGMA table_info(" + tablename + ")";
                        var tableInfo = (DataTable)Query.Execute<DataTable>(config, query);

                        var existingColumns = new List<string>();

                        if (tableInfo != null && tableInfo.Rows.Count > 0)
                        {
                            foreach (DataRow row in tableInfo.Rows)
                            {
                                string columnName = DataTable_Functions.GetRowValue("name", row);

                                existingColumns.Add(columnName);
                            }

                            query = "";
                            foreach (var columnDefinition in columnDefinitions)
                            {
                                if (!existingColumns.Exists(x => x == columnDefinition.ColumnName))
                                {
                                    query += "ALTER TABLE " + tablename + " ADD COLUMN " + Column.ConvertColumnDefinition(columnDefinition) + "; ";
                                }
                            }

                            if (query != "")
                            {
                                result = (bool)Query.Execute<bool>(config, query);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static bool Replace(object settings, string tablename, ColumnDefinition[] columnDefinitions, string[] primaryKey)
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
                        coldef += Column.ConvertColumnDefinition(columnDefinitions[x]).ToString();
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

                    var query = "DROP TABLE IF EXISTS " + tablename;
                    result = (bool)Query.Execute<bool>(config, query);

                    query = "CREATE TABLE IF NOT EXISTS " + tablename + " (" + coldef + keydef + ")";
                    result = (bool)Query.Execute<bool>(config, query);
                }
            }

            return result;
        }

        public static bool Drop(object settings, string tablename)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "DROP TABLE IF EXISTS " + tablename;
                    result = (bool)Query.Execute<bool>(config, query);
                }
            }

            return result;
        }

        public static bool Drop(object settings, string[] tableNames)
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
                        query += "DROP TABLE IF EXISTS " + tablename + "; ";
                    }

                    result = (bool)Query.Execute<bool>(config, query);
                }
            }

            return result;
        }

        public static bool Truncate(object settings, string tablename)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "DELETE FROM " + tablename;

                    result = (bool)Query.Execute<bool>(config, query);
                }
            }

            return result;
        }

        public static DataTable Get(object settings, string tablename)
        {
            DataTable result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tablename + "';";
                    bool tableExists = (int)Query.Execute<int>(config, query) >= 0;
                    if (tableExists)
                    {
                        query = "SELECT * FROM " + tablename;
                        result = (DataTable)Query.Execute<DataTable>(config, query);

                        if (result != null) result.TableName = tablename;
                    }
                    else Logger.Log("Table " + tablename + " Not Found in database", LogLineType.Warning);
                }
            }

            return result;
        }

        public static DataTable Get(object settings, string tablename, Int64 limit, Int64 offset)
        {
            DataTable result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tablename + "';";
                    bool tableExists = (int)Query.Execute<int>(config, query) >= 0;
                    if (tableExists)
                    {
                        query = "SELECT * FROM " + tablename + " LIMIT " + limit.ToString() + " OFFSET " + offset.ToString();
                        result = (DataTable)Query.Execute<DataTable>(config, query);

                        if (result != null) result.TableName = tablename;
                    }
                    else Logger.Log("Table " + tablename + " Not Found in database", LogLineType.Warning);
                }
            }

            return result;
        }

        public static DataTable Get(object settings, string tablename, string filterExpression)
        {
            DataTable result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tablename + "';";
                    bool tableExists = (int)Query.Execute<int>(config, query) >= 0;
                    if (tableExists)
                    {
                        query = "SELECT * FROM " + tablename + " " + filterExpression;
                        result = (DataTable)Query.Execute<DataTable>(config, query);

                        if (result != null) result.TableName = tablename;
                    }
                    else Logger.Log("Table " + tablename + " Not Found in database", LogLineType.Warning);
                }
            }

            return result;
        }

        public static DataTable Get(object settings, string tablename, string filterExpression, string columns)
        {
            DataTable result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tablename + "';";
                    bool tableExists = (int)Query.Execute<int>(config, query) >= 0;
                    if (tableExists)
                    {
                        query = "SELECT " + columns + " FROM " + tablename + " " + filterExpression;
                        result = (DataTable)Query.Execute<DataTable>(config, query);

                        if (result != null) result.TableName = tablename;
                    }
                    else Logger.Log("Table " + tablename + " Not Found in database", LogLineType.Warning);
                }
            }

            return result;
        }

        public static DataTable[] Get(object settings, string[] tablenames)
        {
            DataTable[] result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var tables = new List<DataTable>();

                    foreach (string tablename in tablenames)
                    {
                        var query = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tablename + "';";
                        bool tableExists = (int)Query.Execute<int>(config, query) >= 0;
                        if (tableExists)
                        {
                            query = "SELECT * FROM " + tablename;
                            var table = (DataTable)Query.Execute<DataTable>(config, query);

                            if (table != null)
                            {
                                table.TableName = tablename;
                                tables.Add(table);
                            }
                        }
                        else Logger.Log("Table " + tablename + " Not Found in database", LogLineType.Warning);
                    }

                    result = tables.ToArray();
                }
            }

            return result;
        }

        public static DataTable[] Get(object settings, string[] tablenames, string[] filterExpressions)
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

                            var query = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tablenames[x] + "';";
                            bool tableExists = (int)Query.Execute<int>(config, query) >= 0;
                            if (tableExists)
                            {
                                query = "SELECT * FROM " + tablenames[x] + filterExpression;
                                var table = (DataTable)Query.Execute<DataTable>(config, query);

                                if (table != null)
                                {
                                    table.TableName = tablenames[x];
                                    tables.Add(table);
                                }
                            }
                            else Logger.Log("Table " + tablenames[x] + " Not Found in database", LogLineType.Warning);
                        }
                    }

                    result = tables.ToArray();
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
                    result += Column.COLUMN_NAME_START + columnName + Column.COLUMN_NAME_END;
                    if (i < columnValues.Length - 1) result += ", ";
                }
            }

            return result;
        }

        public static DataTable[] Get(object settings, string[] tablenames, string[] filterExpressions, string[] columns)
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

                            var query = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tablenames[x] + "';";
                            bool tableExists = (int)Query.Execute<int>(config, query) >= 0;
                            if (tableExists)
                            {
                                query = "SELECT " + column + " FROM " + tablenames[x] + filterExpression;
                                var table = (DataTable)Query.Execute<DataTable>(config, query);

                                if (table != null)
                                {
                                    table.TableName = tablenames[x];
                                    tables.Add(table);
                                }
                            }
                            else Logger.Log("Table " + tablenames[x] + " Not Found in database", LogLineType.Warning);
                        }
                    }

                    result = tables.ToArray();
                }
            }

            return result;
        }

        public static string[] List(object settings)
        {
            string[] result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT name FROM sqlite_master WHERE type='table'";

                    result = (string[])Query.Execute<string[]>(config, query);
                }
            }

            return result;
        }

        public static string[] List(object settings, string filterExpression)
        {
            string[] result = null;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT name FROM sqlite_master WHERE type='table' AND " + filterExpression;

                    result = (string[])Query.Execute<string[]>(config, query);
                }
            }

            return result;
        }

        public static long GetRowCount(object settings, string tablename)
        {
            long result = -1;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    var query = "SELECT COUNT(*) FROM " + tablename;

                    result = (long)Query.Execute<long>(config, query);
                }
            }

            return result;
        }

        // Can't find an equivalent in SQLite. Since it is a local file, maybe it's not as important as server based databases anyways
        public static long GetSize(object settings, string tablename)
        {
            long result = -1;

            return result;
        }

    }
}
