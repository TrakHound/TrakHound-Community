using System;
using System.Collections.Generic;

using System.Data;

using TH_Configuration;
using TH_Database;

namespace TH_MySQL
{
    public class Plugin : IDatabasePlugin
    {
        public string Name { get { return "MySQL Database Plugin"; } }

        public string Type { get { return "MySQL"; } }

        public Type Config_Page { get { return typeof(ConfigurationPage.Page); } }

        public object CreateConfigurationButton(DataTable dt)
        {
            ConfigurationPage.Button result = new ConfigurationPage.Button();

            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    result.DatabaseName = GetTableValue("Database", dt);
                    result.Server = GetTableValue("Server", dt);
                }
            }

            return result;
        }

        public void Initialize(Database_Configuration config)
        {
            if (config.Type.ToLower() == Type.ToLower())
            {
                MySQL_Configuration c = MySQL_Configuration.ReadXML(config.Node);
                
                config.Configuration = c;
            }
        }

        public bool Ping(object settings)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHPPing.PingHost(config.PHP_Server);
                }
                else
                {
                    result = MySQLPing.Ping(config);
                }
            }

            return result;          
        }

        public bool CheckPermissions(object settings, Application_Type type)
        {
            bool Result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                string username = config.Username;
                DataTable dt = null;

                if (config.UsePHP) dt = PHP.etc.GetGrants(config, username);
                else dt = Connector.etc.GetGrants(config, username);
                if (dt != null)
                {
                    foreach (DataRow Row in dt.Rows)
                    {
                        if (Row[0] != null)
                        {
                            string Field = Row[0].ToString();

                            string databasename = config.Database;
                            databasename = databasename.Replace("_", "\\_").ToString().ToLower();

                            if (
                                Field.Contains("ALL PRIVILEGES") ||
                                (
                                Field.Contains("ALTER") &&
                                Field.Contains("CREATE") &&
                                Field.Contains("DELETE") &&
                                Field.Contains("DROP") &&
                                Field.Contains("INSERT") &&
                                Field.Contains("SELECT") &&
                                Field.Contains("UPDATE") &&
                                (Field.Contains(databasename) || Field.Contains("%"))
                                )
                                )
                            {
                                Result = true;
                            }
                        }
                    }
                }
            }

            return Result;
        }

        string GetTableValue(string name, DataTable dt)
        {
            string result = null;

            DataRow[] rows = dt.Select("Name = '" + name + "'");
            if (rows.Length > 0)
            {
                result = rows[0]["Value"].ToString();
            }

            return result;
        }

        // Database Functions -----------------------------------------------------------

        static string GetDatabaseName(MySQL_Configuration config)
        {
            return config.Database;
        }


        public bool Database_Create(object settings)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Database.Create(config, config.Database);
                }
                else
                {
                    result =  Connector.Database.Create(config, config.Database);
                }
            }

            return result;
        }

        public bool Database_Drop(object settings)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Database.Drop(config, config.Database);
                }
                else
                {
                    result = Connector.Database.Drop(config, config.Database);
                }
            }

            return result;
        }

        // ------------------------------------------------------------------------------


        // Table ------------------------------------------------------------------------

        public bool Table_Create(object settings, string tablename, ColumnDefinition[] columnDefinitions, string[] primaryKey) 
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                //object[] coldefs = MySQL_Tools.ConvertColumnDefinitions(columnDefinitions);

                if (config.UsePHP)
                {
                    result = PHP.Table.Create(config, tablename, columnDefinitions, primaryKey);
                }
                else
                {
                    result = Connector.Table.Create(config, tablename, columnDefinitions, primaryKey);
                }
            }

            return result; 
        }

        public bool Table_Drop(object settings, string tablename)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Table.Drop(config, tablename);
                }
                else
                {
                    result = Connector.Table.Drop(config, tablename);
                }
            }

            return result;
        }

        public bool Table_Drop(object settings, string[] tablenames)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Table.Drop(config, tablenames);
                }
                else
                {
                    result = Connector.Table.Drop(config, tablenames);
                }
            }

            return result;
        }

        public bool Table_Truncate(object settings, string tablename)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Table.Truncate(config, tablename);
                }
                else
                {
                    result = Connector.Table.Truncate(config, tablename);
                }
            }

            return result;
        }

        public DataTable Table_Get(object settings, string tablename)
        {
            DataTable result = null;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Table.Get(config, tablename);
                }
                else
                {
                    result = Connector.Table.Get(config, tablename);
                }
            }

            return result;
        }

        public DataTable Table_Get(object settings, string tablename, string filterExpression)
        {
            DataTable result = null;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Table.Get(config, tablename, filterExpression);
                }
                else
                {
                    result = Connector.Table.Get(config, tablename, filterExpression);
                }
            }

            return result;
        }

        public DataTable Table_Get(object settings, string tablename, string filterExpression, string columns)
        {
            DataTable result = null;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Table.Get(config, tablename, filterExpression, columns);
                }
                else
                {
                    result = Connector.Table.Get(config, tablename, filterExpression, columns);
                }
            }

            return result;
        }

        public string[] Table_List(object settings)
        {
            string[] result = null;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Table.List(config);
                }
                else
                {
                    result = Connector.Table.List(config);
                }
            }

            return result;
        }

        public string[] Table_List(object settings, string filterExpression)
        {
            string[] result = null;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Table.List(config, filterExpression);
                }
                else
                {
                    result = Connector.Table.List(config, filterExpression);
                }
            }

            return result;
        }

        public Int64 Table_GetRowCount(object settings, string tablename)
        {
            Int64 result = -1;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Table.GetRowCount(config, tablename);
                }
                else
                {
                    result = Connector.Table.RowCount(config, tablename);
                }
            }

            return result;
        }

        public Int64 Table_GetSize(object settings, string tablename)
        {
            Int64 result = -1;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Table.GetSize(config, tablename);
                }
                else
                {
                    result = Connector.Table.Size(config, tablename);
                }
            }

            return result;
        }

        // ------------------------------------------------------------------------------


        // Column -----------------------------------------------------------------------

        public List<string> Column_Get(object settings, string tablename)
        {
            List<string> result = null;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Column.Get(config, tablename);
                }
                else
                {
                    result = Connector.Column.Get(config, tablename);
                }
            }

            return result;
        }

        public bool Column_Add(object settings, string tablename, ColumnDefinition columnDefinition)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                string coldef = MySQL_Tools.ConvertColumnDefinition(columnDefinition);

                if (config.UsePHP)
                {
                    result = PHP.Column.Add(config, tablename, coldef);
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

        public bool Row_Insert(object settings, string tablename, object[] columns, object[] values, string[] primaryKey, bool update)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Row.Insert(config, tablename, columns, values, true);
                }
                else
                {
                    result = Connector.Row.Insert(config, tablename, columns, values, true);
                }
            }

            return result;
        }

        public bool Row_Insert(object settings, string tablename, object[] columns, List<List<object>> values, string[] primaryKey, bool update)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Row.Insert(config, tablename, columns, values, true);
                }
                else
                {
                    result = Connector.Row.Insert(config, tablename, columns, values, true);
                }
            }

            return result;
        }

        public bool Row_Insert(object settings, string tablename, List<object[]> columnsList, List<object[]> valuesList, string[] primaryKey, bool update)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    //result = PHP.Row.Insert(config, tablename, columnsList, valuesList, true);
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
                if (config.UsePHP)
                {
                    result = PHP.Row.Insert(config, query);
                }
                else
                {
                    result = Connector.Row.Insert(config, query);
                }
            }

            return result;
        }


        public DataRow Row_Get(object settings, string tablename, string tableKey, string rowKey)
        {
            DataRow result = null;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Row.Get(config, tablename, tableKey, rowKey);
                }
                else
                {
                    result = Connector.Row.Get(config, tablename, tableKey, rowKey);
                }
            }

            return result;
        }

        public DataRow Row_Get(object settings, string tablename, string query)
        {
            DataRow result = null;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Row.Get(config, tablename, query);
                }
                else
                {
                    result = Connector.Row.Get(config, tablename, query);
                }
            }

            return result;
        }


        public bool Row_Exists(object settings, string tablename, string filterString)
        {
            bool result = false;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.Row.Exists(config, tablename, filterString);
                }
                else
                {
                    result = Connector.Row.Exists(config, tablename, filterString);
                }
            }

            return result;
        }

        // ------------------------------------------------------------------------------


        // Etc --------------------------------------------------------------------------

        public string CustomCommand(object settings, string commandText)
        {
            string result = null;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.etc.CustomCommand(config, commandText);
                }
                else
                {
                    result = Connector.etc.CustomCommand(config, commandText);
                }
            }

            return result;
        }

        public object GetValue(object settings, string tablename, string column, string filterExpression)
        {
            object result = null;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                if (config.UsePHP)
                {
                    result = PHP.etc.GetValue(config, tablename, column, filterExpression);
                }
                else
                {
                    result = Connector.etc.GetValue(config, tablename, column, filterExpression);
                }
            }

            return result;
        }

        public DataTable GetGrants(object settings)
        {
            DataTable result = null;

            MySQL_Configuration config = MySQL_Configuration.Get(settings);
            if (config != null)
            {
                string username = config.Username;

                if (config.UsePHP)
                {
                    result = PHP.etc.GetGrants(config, username);
                }
                else
                {
                    result = Connector.etc.GetGrants(config, username);
                }
            }

            return result;
        }

        // ------------------------------------------------------------------------------

    }
}
