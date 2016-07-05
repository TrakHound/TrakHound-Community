using System;
using System.Collections.Generic;
using System.Xml;
using System.Data;
using System.Text.RegularExpressions;

//using TH_Global.TrakHound.Configurations;
using TH_Global.TrakHound.Configurations;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement;
using TH_Plugins.Database;

namespace TH_UserManagement.Management.Local
{

    public static class Users
    {
        public const string tablename = "users";
        static string[] primaryKey = { "username" };

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
                new ColumnDefinition("address1", DataType.LargeText),
                new ColumnDefinition("address2", DataType.LargeText),
                new ColumnDefinition("city", DataType.LargeText),
                new ColumnDefinition("state", DataType.LargeText),
                new ColumnDefinition("country", DataType.LargeText),
                new ColumnDefinition("zipcode", DataType.LargeText),
                new ColumnDefinition("image_url", DataType.LargeText),
                new ColumnDefinition("last_login", DataType.DateTime)
            };

            Table.Create(config, tablename, Columns, primaryKey);
        }

        public static UserConfiguration Login(string username, string password, Database_Settings db)
        {
            UserConfiguration result = null;

            DataRow dbrow = Row.Get(db, tablename, "WHERE username='" + username.ToLower() + "' OR email='" + username + "'");
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
                                Logger.Log(username + " Logged in Successfully!");

                                result = UserConfiguration.GetFromDataRow(dbrow);

                                UpdateLoginTime(result, db);
                            }
                            else Logger.Log("Incorrect Password!");
                        }
                    }
                }
            }
            else Logger.Log("Username '" + username + "' Not Found in Database!");

            return result;
        }

        public static UserConfiguration LoginWithHash(string username, string hash, Database_Settings db)
        {
            UserConfiguration result = null;

            DataRow dbrow = Row.Get(db, tablename, "WHERE username='" + username.ToLower() + "' OR email='" + username + "'");
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
                            string dbhash = dbrow["hash"].ToString();

                            if (hash == dbhash)
                            {
                                Logger.Log(username + " Logged in Successfully!");

                                result = UserConfiguration.GetFromDataRow(dbrow);

                                UpdateLoginTime(result, db);
                            }
                            else Logger.Log("Incorrect Password!");
                        }
                    }
                }
            }
            else Logger.Log("Username '" + username + "' Not Found in Database!");

            return result;
        }

        public static bool CreateUser(UserConfiguration userConfig, string password, Database_Settings db)
        {

            CreateUserTable(db);

            Security.Password pwd = null;

            if (password != null)
            {
                pwd = new Security.Password(password);
            }

            List<string> columns = new List<string>();
            columns.Add("username");

            if (password != null)
            {
                columns.Add("hash");
                columns.Add("salt");
            }

            columns.Add("first_name");
            columns.Add("last_name");
            columns.Add("company");
            columns.Add("email");
            columns.Add("phone");
            columns.Add("address1");
            columns.Add("address2");
            columns.Add("city");
            columns.Add("state");
            columns.Add("country");
            columns.Add("zipcode");


            List<object> values = new List<object>();
            values.Add(userConfig.username.ToLower());

            if (password != null)
            {
                values.Add(pwd.hash);
                values.Add(pwd.salt);
            }

            values.Add(String_Functions.ToLower(userConfig.first_name));
            values.Add(String_Functions.ToLower(userConfig.last_name));
            values.Add(String_Functions.ToLower(userConfig.company));
            values.Add(userConfig.email);
            values.Add(userConfig.phone);
            values.Add(userConfig.address1);
            values.Add(userConfig.address2);
            values.Add(String_Functions.ToLower(userConfig.city));
            values.Add(userConfig.state);
            values.Add(String_Functions.ToLower(userConfig.country));
            values.Add(userConfig.zipcode);

            Row.Insert(db, tablename, columns.ToArray(), values.ToArray(), primaryKey, true);

            return true;

        }

        public static bool UpdateLoginTime(UserConfiguration userConfig, Database_Settings db)
        {

            List<string> columns = new List<string>();
            columns.Add("username");
            columns.Add("last_login");

            List<object> values = new List<object>();
            values.Add(userConfig.username.ToLower());
            values.Add(userConfig.last_login);

            Row.Insert(db, tablename, columns.ToArray(), values.ToArray(), primaryKey, true);

            return true;
        }

        public static bool UpdateImageURL(string imageURL, UserConfiguration userConfig, Database_Settings db)
        {
            userConfig.image_url = imageURL;

            List<string> columns = new List<string>();
            columns.Add("username");
            columns.Add("image_url");

            List<object> values = new List<object>();
            values.Add(userConfig.username.ToLower());
            values.Add(imageURL);

            Row.Insert(db, tablename, columns.ToArray(), values.ToArray(), primaryKey, true);

            return true;
        }

        public static bool VerifyUsername(string username, Database_Settings db)
        {
            bool result = false;

            Regex r = new Regex("^(?=.{6,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$");
            if (r.IsMatch(username) && !username.ToLower().Contains("trakhound"))
            {
                DataRow dbrow = Row.Get(db, tablename, "WHERE username='" + username.ToLower() + "'");
                if (dbrow == null) result = true;
            }

            return result;
        }

        public static class RememberMe
        {
            public static bool Set(UserConfiguration userConfig, RememberMeType type)
            {
                string t = type.ToString().ToLower();

                // Set "id" registry key
                TH_UserManagement.Management.RememberMe.Registry_Functions.SetRegistryKey(t + "_id", userConfig.username);

                // Set Date of Last Login using Remember Me
                TH_UserManagement.Management.RememberMe.Registry_Functions.SetRegistryKey(t + "_last_login", DateTime.UtcNow.ToString());

                return true;
            }

            public static UserConfiguration Get(RememberMeType type, Database_Settings db)
            {
                UserConfiguration result = null;

                string t = type.ToString().ToLower();

                string strLast_Login = TH_UserManagement.Management.RememberMe.Registry_Functions.GetRegistryKey(t + "_last_login");
                DateTime last_login = DateTime.MinValue;
                if (DateTime.TryParse(strLast_Login, out last_login))
                {
                    TimeSpan sinceLastLogin = DateTime.UtcNow - last_login;
                    if (sinceLastLogin > TimeSpan.FromDays(14))
                    {
                        Clear(type);
                    }
                    else
                    {
                        string id = TH_UserManagement.Management.RememberMe.Registry_Functions.GetRegistryKey(t + "_id");
                        DataRow dbrow = Row.Get(db, tablename, "WHERE username='" + id.ToLower() + "'");
                        if (dbrow != null)
                        {
                            result = UserConfiguration.GetFromDataRow(dbrow);

                            if (result != null) Logger.Log(result.username + " Logged in Successfully!");
                            
                            UpdateLoginTime(result, db);
                        }
                    }
                }

                return result;
            }

            public static bool Clear(RememberMeType type)
            {
                bool result = false;

                string t = type.ToString().ToLower();

                TH_UserManagement.Management.RememberMe.Registry_Functions.DeleteRegistryKey(t + "_id");
                TH_UserManagement.Management.RememberMe.Registry_Functions.DeleteRegistryKey(t + "_last_login");
                TH_UserManagement.Management.RememberMe.Registry_Functions.DeleteRegistryKey(t + "__hash");

                return result;
            }
        }
    }

}
