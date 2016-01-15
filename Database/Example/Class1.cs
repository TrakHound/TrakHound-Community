using System;
using System.Collections.Generic;

using System.Data;

using TH_Configuration;
using TH_Database;

namespace TH_Example
{
    public class Plugin : IDatabasePlugin
    {
        public string Name { get { return "Example Database Plugin"; } }

        public string Type { get { return "Example"; } }

        //public object Configuration { get; set; }

        public object Initialize(Database_Configuration config)
        {
            object result = null;

            return result;
        }


        // Database Functions -----------------------------------------------------------

        public bool Database_Create(object settings)
        {
            return true;




        }

        public bool Database_Drop(object settings) { return true; }

        // ------------------------------------------------------------------------------


        // Table ------------------------------------------------------------------------

        public bool Table_Create(object settings, string tablename, object[] columnDefinitions, string primaryKey) { return true; }

        public bool Table_Drop(object settings, string tablename) { return true; }

        public bool Table_Drop(object settings, string[] tablenames) { return true; }

        public bool Table_Truncate(object settings, string tablename) { return true; }

        public DataTable Table_Get(object settings, string tablename) { return null; }

        public DataTable Table_Get(object settings, string tablename, string filterExpression) { return null; }

        public DataTable Table_Get(object settings, string tablename, string filterExpression, string columns) { return null; }

        // ------------------------------------------------------------------------------


        // Column -----------------------------------------------------------------------

        public List<string> Column_Get(object settings, string tablename) { return null; }

        public bool Column_Add(object settings, string tablename, string columnDefinition) { return true; }

        // ------------------------------------------------------------------------------


        // Row --------------------------------------------------------------------------

        public bool Row_Insert(object settings, string tablename, object[] columns, object[] values, bool update) { return true; }

        public bool Row_Insert(object settings, string tablename, object[] columns, List<List<object>> values, bool update) { return true; }

        public bool Row_Insert(object settings, string tablename, List<object[]> columnsList, List<object[]> valuesList, bool update) { return true; }

        public bool Row_Insert(object settings, string query) { return true; }


        public DataRow Row_Get(object settings, string tablename, string tableKey, string rowKey) { return null; }

        public DataRow Row_Get(object settings, string tablename, string query) { return null; }


        public bool Row_Exists(object settings, string tablename, string filterString) { return true; }

        // ------------------------------------------------------------------------------


        // Etc --------------------------------------------------------------------------

        public string CustomCommand(object settings, string commandText) { return null; }

        public object GetValue(object settings, string tablename, string column, string filterExpression) { return null; }

        public DataTable GetGrants(object settings, string username) { return null; }

        // ------------------------------------------------------------------------------

    }
}
