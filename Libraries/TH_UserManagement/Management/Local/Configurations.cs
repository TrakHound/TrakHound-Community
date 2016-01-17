using System;
using System.Collections.Generic;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement;

namespace TH_UserManagement.Management.Local
{

    public static class Configurations
    {
        static string[] primaryKey = { "address" };

        public static void AddConfigurationToUser(UserConfiguration userConfig, Configuration configuration, Database_Settings db)
        {
            Users.CreateUserTable(db);

            CreateConfigurationTable(userConfig, db, configuration);
        }

        public static List<Configuration> GetConfigurationsForUser(UserConfiguration userConfig, Database_Settings db)
        {

            List<Configuration> result = new List<Configuration>();

            string[] tables = Table.List(db, "LIKE '" + userConfig.username + "%'");

            foreach (string table in tables)
            {
                DataTable dt = Table.Get(db, table);
                if (dt != null)
                {
                    string path = TH_Configuration.Converter.TableToXML(dt, @"C:\Temp\" + String_Functions.RandomString(20));

                    Configuration config = TH_Configuration.Configuration.ReadConfigFile(path);
                    if (config != null)
                    {
                        result.Add(config);
                    }
                }
            }

            return result;
        }


        public static void CreateConfigurationTable(UserConfiguration userConfig, Database_Settings db, Configuration configuration)
        {
            Configuration_CreateTable(userConfig, db, configuration);
            Configuration_UpdateRows(userConfig, db, configuration);
        }

        public static void Configuration_CreateTable(UserConfiguration userConfig, Database_Settings db, Configuration configuration)
        {
            ColumnDefinition[] Columns = new ColumnDefinition[]
            {
                new ColumnDefinition("address", DataType.LargeText),
                new ColumnDefinition("name", DataType.LargeText),
                new ColumnDefinition("value", DataType.LargeText),
                new ColumnDefinition("attributes", DataType.LargeText)
            };

            string table = GetConfigurationTableName(userConfig, configuration);

            Table.Create(db, table, Columns, primaryKey);
        }

        public static void Configuration_UpdateRows(UserConfiguration userConfig, Database_Settings db, Configuration configuration)
        {
            DataTable dt = TH_Configuration.Converter.XMLToTable(configuration.ConfigurationXML);

            // Add Columns
            List<string> columns = new List<string>();
            foreach (DataColumn col in dt.Columns) columns.Add(col.ColumnName);

            List<List<object>> rowValues = new List<List<object>>();

            foreach (DataRow row in dt.Rows)
            {
                List<object> values = new List<object>();
                foreach (object val in row.ItemArray) values.Add(val);
                rowValues.Add(values);
            }

            string table = GetConfigurationTableName(userConfig, configuration);

            Row.Insert(db, table, columns.ToArray(), rowValues, primaryKey, true);
        }


        public static string GetConfigurationTableName(UserConfiguration userConfig, Configuration configuration)
        {
            string table = userConfig.username + "_" + configuration.Description.Manufacturer + "_" + configuration.Description.Device_Type + "_" + configuration.Description.Device_ID + "_Configuration";
            table = table.Replace(' ', '_');

            return table;
        }
    }

}
