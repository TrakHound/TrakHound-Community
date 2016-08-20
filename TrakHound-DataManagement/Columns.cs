using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TrakHound.Tools;
using TrakHound.Databases;
using TrakHound.Plugins;
using TrakHound.Plugins.Database;

namespace TrakHound_DataManagement
{
    public static class Column
    {
        public static string COLUMN_NAME_START = "`";
        public static string COLUMN_NAME_END = "`";

        public static object[] ConvertColumnDefinitions(ColumnDefinition[] columns)
        {
            List<object> result = new List<object>();

            foreach (ColumnDefinition coldef in columns.ToList())
            {
                string def = ConvertColumnDefinition(coldef);
                if (coldef.NotNull) def += " NOT NULL";

                result.Add(def);
            }

            return result.ToArray();
        }

        public static string ConvertColumnDefinition(ColumnDefinition column)
        {
            return COLUMN_NAME_START + column.ColumnName + COLUMN_NAME_END + " " + ConvertColumnDataType(column.DataType);
        }

        public const string VarChar = "varchar(1000)";
        public const string BigInt = "bigint";
        public const string Double = "double";
        public const string Datetime = "varchar(90)";
        public const string Bool = "boolean";

        public static string ConvertColumnDataType(DataType dataType)
        {
            if (dataType == DataType.Boolean) return Bool;

            if (dataType == DataType.Short) return BigInt;
            if (dataType == DataType.Long) return BigInt;
            if (dataType == DataType.Double) return Double;

            if (dataType == DataType.SmallText) return VarChar;
            if (dataType == DataType.MediumText) return VarChar;
            if (dataType == DataType.LargeText) return VarChar;

            if (dataType == DataType.DateTime) return Datetime;

            return null;
        }

        public static List<string> Get(object settings, string tablename)
        {
            var result = new List<string>();

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    // Get list of existing columns (if table already existed, they may not be up to date)
                    var query = "PRAGMA table_info(" + tablename + ")";
                    var tableInfo = (DataTable)Query.Execute<DataTable>(config, query);

                    var existingColumns = new List<string>();

                    if (tableInfo != null && tableInfo.Rows.Count > 0)
                    {
                        foreach (DataRow row in tableInfo.Rows)
                        {
                            var name = DataTable_Functions.GetRowValue("name", row);
                            if (name != null) result.Add(name);
                        }
                    }
                }
            }

            return result;
        }

        public static bool Add(object settings, string tablename, ColumnDefinition columnDefinition)
        {
            bool result = false;

            if (settings != null)
            {
                var config = Configuration.Get(settings);
                if (config != null)
                {
                    // Get list of existing columns (if table already existed, they may not be up to date)
                    var query = "PRAGMA table_info(" + tablename + ")";
                    var tableInfo = (DataTable)Query.Execute<DataTable>(config, query);

                    var existingColumns = new List<string>();

                    if (tableInfo != null && tableInfo.Rows.Count > 0)
                    {
                        foreach (DataRow row in tableInfo.Rows)
                        {
                            var name = DataTable_Functions.GetRowValue("name", row);
                            if (name != null) existingColumns.Add(name);
                        }
                    }

                    if (!existingColumns.Exists(x => x == columnDefinition.ColumnName))
                    {
                        query = "ALTER TABLE " + tablename + " ADD COLUMN " + ConvertColumnDefinition(columnDefinition);
                        result = (bool)Query.Execute<bool>(config, query);
                    }
                }
            }

            return result;
        }

    }
}
