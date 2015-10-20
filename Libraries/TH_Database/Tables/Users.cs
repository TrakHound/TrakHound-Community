using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Data;

using TH_Configuration;
using TH_Global;

namespace TH_Database.Tables
{
    public static class Users
    {

        public const string tablename = "users";

        public static void AddConfigurationToUser(UserConfiguration userConfig, Database_Settings db, Configuration configuration)
        {
            CreateUserTable(db);

            //CreateUser(userConfig, db, configuration);

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
                    string path = TH_Configuration.Converter.TableToXML(dt, @"C:\Temp\" + TH_Global.Functions.RandomString(20));

                    Configuration config = TH_Configuration.Configuration.ReadConfigFile(path);
                    if (config != null)
                    {
                        result.Add(config);
                    }
                }
            }

            return result;
        }

        #region "User Management"

        public class UserConfiguration
        {
            public string username { get; set; }
            public string hash { get; set; }
            public int salt { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string company { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string address { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string country { get; set; }
            public string zipcode { get; set; }
            public string image_url { get; set; }
            public DateTime last_login { get; set; }

            public static UserConfiguration GetFromDataRow(DataRow row)
            {
                UserConfiguration result = new UserConfiguration();

                foreach (System.Reflection.PropertyInfo info in typeof(UserConfiguration).GetProperties())
                {
                    if (info.Name == "last_login") result.last_login = DateTime.UtcNow;
                    else
                    {
                        if (row.Table.Columns.Contains(info.Name))
                        {
                            object value = row[info.Name];

                            Type t = info.PropertyType;
                            info.SetValue(result, Convert.ChangeType(value, t), null);
                        }
                    }
                }

                return result;
            }
        }

        public static void CreateUserTable(Database_Settings config)
        {
            ColumnDefinition[] Columns = new ColumnDefinition[]
            {
                new ColumnDefinition("username", DataType.LargeText),
                new ColumnDefinition("hash", DataType.LargeText),
                new ColumnDefinition("salt", DataType.Short),
                new ColumnDefinition("first_name", DataType.LargeText),
                new ColumnDefinition("last_name", DataType.LargeText),
                new ColumnDefinition("company", DataType.LargeText),
                new ColumnDefinition("email", DataType.LargeText),
                new ColumnDefinition("phone", DataType.LargeText),
                new ColumnDefinition("address", DataType.LargeText),
                new ColumnDefinition("city", DataType.LargeText),
                new ColumnDefinition("state", DataType.LargeText),
                new ColumnDefinition("country", DataType.LargeText),
                new ColumnDefinition("zipcode", DataType.LargeText),
                new ColumnDefinition("image_url", DataType.LargeText),
                new ColumnDefinition("last_login", DataType.DateTime)
            };

            Table.Create(config, tablename, Columns, "username");
        }

        public static UserConfiguration Login(string username, string password, Database_Settings db)
        {
            UserConfiguration result = null;

            DataRow dbrow = Row.Get(db, tablename, "WHERE username='" + username.ToLower() + "'");
            if (dbrow != null)
            {
                if (dbrow.Table.Columns.Contains("salt"))
                {
                    string strSalt = dbrow["salt"].ToString();
                    int salt = -1;
                    if (int.TryParse(strSalt, out salt))
                    {
                        if (dbrow.Table.Columns.Contains("hash"))
                        {
                            string hash = dbrow["hash"].ToString();

                            Security.Password pwd = new Security.Password(password, salt);

                            if (pwd.hash == hash)
                            {
                                Console.WriteLine(username + " Logged in Successfully!");

                                result = UserConfiguration.GetFromDataRow(dbrow);

                                UpdateLoginTime(result, db);
                            } 
                            else Console.WriteLine("Incorrect Password!");
                        }
                    }
                }
            }
            else Console.WriteLine("Username '" + username + "' Not Found in Database!");

            return result;
        }

        public static void CreateUser(UserConfiguration userConfig, string password, Database_Settings db)
        {

            List<string> columns = new List<string>();
            columns.Add("username");
            columns.Add("hash");
            columns.Add("salt");
            columns.Add("first_name");
            columns.Add("last_name");
            columns.Add("company");
            columns.Add("email");
            columns.Add("phone");
            columns.Add("address");
            columns.Add("city");
            columns.Add("state");
            columns.Add("country");
            columns.Add("zipcode");
            //columns.Add("image_url");
            //columns.Add("last_login");

            Security.Password pwd = new Security.Password(password);

            List<object> values = new List<object>();
            values.Add(userConfig.username.ToLower());
            values.Add(pwd.hash);
            values.Add(pwd.salt);
            values.Add(TH_Global.Formatting.UppercaseFirst(userConfig.first_name));
            values.Add(TH_Global.Formatting.UppercaseFirst(userConfig.last_name));
            values.Add(TH_Global.Formatting.UppercaseFirst(userConfig.company));
            values.Add(userConfig.email);
            values.Add(userConfig.phone);
            values.Add(userConfig.address);
            values.Add(TH_Global.Formatting.UppercaseFirst(userConfig.city));
            values.Add(userConfig.state);
            values.Add(TH_Global.Formatting.UppercaseFirst(userConfig.country));
            values.Add(userConfig.zipcode);
            //values.Add(userConfig.image_url);
            //values.Add(userConfig.last_login);

            Row.Insert(db, tablename, columns.ToArray(), values.ToArray(), true);
        }

        public static void UpdateLoginTime(UserConfiguration userConfig, Database_Settings db)
        {

            List<string> columns = new List<string>();
            columns.Add("username");
            columns.Add("last_login");

            List<object> values = new List<object>();
            values.Add(userConfig.username.ToLower());
            values.Add(userConfig.last_login);

            Row.Insert(db, tablename, columns.ToArray(), values.ToArray(), true);
        }

        //public static void CreateUserTable(Database_Settings config)
        //{
        //    ColumnDefinition[] Columns = new ColumnDefinition[]
        //    {
        //        new ColumnDefinition("tablename", DataType.LargeText),
        //        new ColumnDefinition("username", DataType.LargeText),
        //        new ColumnDefinition("password", DataType.LargeText)
        //    };

        //    Table.Create(config, tablename, Columns, "tablename");
        //}

        //public static void CreateUser(TH_Configuration.Users.User_Settings userConfig, Database_Settings db, Configuration configuration)
        //{
        //    List<string> columns = new List<string>();
        //    columns.Add("tablename");
        //    columns.Add("username");
        //    columns.Add("password");

        //    List<object> values = new List<object>();
        //    values.Add(GetConfigurationTableName(userConfig, configuration));
        //    values.Add(userConfig.username);
        //    values.Add(userConfig.password);

        //    Row.Insert(db, tablename, columns.ToArray(), values.ToArray(), true);
        //}

        #endregion

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

            Table.Create(db, table, Columns, "address");
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

            Row.Insert(db, table, columns.ToArray(), rowValues, true);
        }


        public static string GetConfigurationTableName(UserConfiguration userConfig, Configuration configuration)
        {
            string table = userConfig.username + "_" + configuration.Description.Manufacturer + "_" + configuration.Description.Machine_Type + "_" + configuration.Description.Machine_ID + "_Configuration";
            table = table.Replace(' ', '_');

            return table;
        }

    }
}
