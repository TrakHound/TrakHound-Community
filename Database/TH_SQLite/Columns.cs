using System;
using System.Collections.Generic;
using System.Data;

using TH_Database;
using TH_Global.Functions;

namespace TH_SQLite
{
    public partial class Plugin
    {

        public List<string> Column_Get(object settings, string tablename)
        {
            List<string> result = null;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    // Get list of existing columns (if table already existed, they may not be up to date)
                    var query = "PRAGMA table_info(" + tablename + ")";
                    var tableInfo = (DataTable)ExecuteQuery<DataTable>(config, query);

                    var existingColumns = new List<string>();

                    if (tableInfo != null && tableInfo.Rows.Count > 0)
                    {
                        result = new List<string>();

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

        public bool Column_Add(object settings, string tablename, ColumnDefinition columnDefinition)
        {
            bool result = false;

            if (settings != null)
            {
                var config = SQLite_Configuration.Get(settings);
                if (config != null)
                {
                    // Get list of existing columns (if table already existed, they may not be up to date)
                    var query = "PRAGMA table_info(" + tablename + ")";
                    var tableInfo = (DataTable)ExecuteQuery<DataTable>(config, query);

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
                        query = "ALTER TABLE test_table ADD COLUMN " + ConvertColumnDefinition(columnDefinition);
                        result = (bool)ExecuteQuery<bool>(config, query);
                    }
                }
            }

            return result;
        }

    }
}
