using System;
using System.Collections.Generic;

using System.Data;

using TH_Configuration;
using TH_Database;

namespace TH_MySQL
{
    public class Plugin : Database_Plugin
    {
        public string Name { get { return "MySQL Database Plugin"; } }

        public string Type { get { return "MYSQL"; } }

        //public object Configuration { get; set; }

        public void Initialize(Database_Configuration config)
        {
            if (config.Type.ToLower() == Type.ToLower())
            {
                MySQL_Configuration c = MySQL_Configuration.ReadXML(config.Node);
                
                config.Configuration = c;
            }
        }


        // Database Functions -----------------------------------------------------------

        public bool Database_Create(object settings)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.PHP_Server != null)
                {

                }
                else
                {
                    result =  Connector.Database.Create(config, config.Database);
                }
            }

            return result;
        }

        public bool Database_Drop(object settings) { return true; }

        // ------------------------------------------------------------------------------


        // Table ------------------------------------------------------------------------

        public bool Table_Create(object settings, string tablename, ColumnDefinition[] columnDefinitions, string primaryKey) 
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                object[] coldefs = MySQL_Tools.ConvertColumnDefinitions(columnDefinitions);

                if (config.PHP_Server != null)
                {

                }
                else
                {
                    result = Connector.Table.Create(config, tablename, coldefs, primaryKey);
                }
            }

            return result; 
        }

        public bool Table_Drop(object settings, string tablename) { return true; }

        public bool Table_Drop(object settings, string[] tablenames) { return true; }

        public bool Table_Truncate(object settings, string tablename) { return true; }

        public DataTable Table_Get(object settings, string tablename) { return null; }

        public DataTable Table_Get(object settings, string tablename, string filterExpression) { return null; }

        public DataTable Table_Get(object settings, string tablename, string filterExpression, string columns) { return null; }

        // ------------------------------------------------------------------------------


        // Column -----------------------------------------------------------------------

        public List<string> Column_Get(object settings, string tablename) { return null; }

        public bool Column_Add(object settings, string tablename, ColumnDefinition columnDefinition)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                string coldef = MySQL_Tools.ConvertColumnDefinition(columnDefinition);

                if (config.PHP_Server != null)
                {

                }
                else
                {
                    result = Connector.Column.Add(config, tablename, coldef);
                }
            }

            return result; 
        }

        // ------------------------------------------------------------------------------


        // Row --------------------------------------------------------------------------

        public bool Row_Insert(object settings, string tablename, object[] columns, object[] values, bool update)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.PHP_Server != null)
                {

                }
                else
                {
                    result = Connector.Row.Insert(config, tablename, columns, values, true);
                }
            }

            return result;
        }

        public bool Row_Insert(object settings, string tablename, object[] columns, List<List<object>> values, bool update)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.PHP_Server != null)
                {

                }
                else
                {
                    result = Connector.Row.Insert(config, tablename, columns, values, true);
                }
            }

            return result;
        }

        public bool Row_Insert(object settings, string tablename, List<object[]> columnsList, List<object[]> valuesList, bool update)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.PHP_Server != null)
                {

                }
                else
                {
                    result = Connector.Row.Insert(config, tablename, columnsList, valuesList, true);
                }
            }

            return result;
        }

        public bool Row_Insert(object settings, string query)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.PHP_Server != null)
                {

                }
                else
                {
                    result = Connector.Row.Insert(config, query);
                }
            }

            return result;
        }


        public DataRow Row_Get(object settings, string tablename, string tableKey, string rowKey) { return null; }

        public DataRow Row_Get(object settings, string tablename, string query) { return null; }


        public bool Row_Exists(object settings, string tablename, string filterString) { return true; }

        // ------------------------------------------------------------------------------


        // Etc --------------------------------------------------------------------------

        public object[] CustomCommand(object settings, string commandText) { return null; }

        public object GetValue(object settings, string tablename, string column, string filterExpression) { return null; }

        public DataTable GetGrants(object settings, string username) { return null; }

        // ------------------------------------------------------------------------------

    }
}
