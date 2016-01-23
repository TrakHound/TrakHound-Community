using System;
using System.Collections.Generic;
using System.Xml;
using System.Data;
using System.Text.RegularExpressions;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement;

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

        public static bool CreateUser(UserConfiguration userConfig, string password, Database_Settings db)
        {

            CreateUserTable(db);

            List<string> columns = new List<string>();
            columns.Add("username");
            columns.Add("hash");
            columns.Add("salt");
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

            Security.Password pwd = new Security.Password(password);

            List<object> values = new List<object>();
            values.Add(userConfig.username.ToLower());
            values.Add(pwd.hash);
            values.Add(pwd.salt);
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

        public static void UpdateLoginTime(UserConfiguration userConfig, Database_Settings db)
        {

            List<string> columns = new List<string>();
            columns.Add("username");
            columns.Add("last_login");

            List<object> values = new List<object>();
            values.Add(userConfig.username.ToLower());
            values.Add(userConfig.last_login);

            Row.Insert(db, tablename, columns.ToArray(), values.ToArray(), primaryKey, true);
        }

        public static void UpdateImageURL(string imageURL, UserConfiguration userConfig, Database_Settings db)
        {
            userConfig.image_url = imageURL;

            List<string> columns = new List<string>();
            columns.Add("username");
            columns.Add("image_url");

            List<object> values = new List<object>();
            values.Add(userConfig.username.ToLower());
            values.Add(imageURL);

            Row.Insert(db, tablename, columns.ToArray(), values.ToArray(), primaryKey, true);
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

        //public static class RememberMe
        //{
        //    public static bool Set(UserConfiguration userConfig, RememberMeType type)
        //    {
        //        bool result = false;

        //        string id = userConfig.username;

        //        string t = type.ToString().ToLower();

        //        NameValueCollection values = new NameValueCollection();

        //        values["id"] = id;
        //        values["type"] = type.ToString();

        //        string url = "https://www.feenux.com/php/users/setrememberme.php";


        //        string responseString = HTTP.SendData(url, values);

        //        if (responseString != null) if (responseString.Trim() != "")
        //            {
        //                string hash = responseString.Trim();

        //                // Set "id" registry key
        //                TH_UserManagement.Management.RememberMe.Registry_Functions.SetRegistryKey(t + "_id", userConfig.username);

        //                // Set Date of Last Login using Remember Me
        //                TH_UserManagement.Management.RememberMe.Registry_Functions.SetRegistryKey(t + "_last_login", DateTime.UtcNow.ToString());

        //                // Set "hash" registry key
        //                TH_UserManagement.Management.RememberMe.Registry_Functions.SetRegistryKey(t + "_hash", hash);

        //                result = true;
        //            }

        //        return result;
        //    }

        //    public static UserConfiguration Get(RememberMeType type)
        //    {
        //        UserConfiguration result = null;

        //        string t = type.ToString().ToLower();

        //        string strLast_Login = TH_UserManagement.Management.RememberMe.Registry_Functions.GetRegistryKey(t + "_last_login");
        //        DateTime last_login = DateTime.MinValue;
        //        if (DateTime.TryParse(strLast_Login, out last_login))
        //        {
        //            TimeSpan sinceLastLogin = DateTime.UtcNow - last_login;
        //            if (sinceLastLogin > TimeSpan.FromDays(14))
        //            {
        //                Clear(type);
        //            }
        //            else
        //            {
        //                string id = TH_UserManagement.Management.RememberMe.Registry_Functions.GetRegistryKey(t + "_id");
        //                string hash = TH_UserManagement.Management.RememberMe.Registry_Functions.GetRegistryKey(t + "_hash");

        //                if (hash != null)
        //                {
        //                    NameValueCollection values = new NameValueCollection();

        //                    values["id"] = id;
        //                    values["hash"] = hash;
        //                    values["type"] = type.ToString();

        //                    string url = "https://www.feenux.com/php/users/getrememberme.php";


        //                    string responseString = HTTP.SendData(url, values);

        //                    DataTable dt = JSON.ToTable(responseString);
        //                    if (dt != null) if (dt.Rows.Count > 0) result = LoginSuccess(dt.Rows[0]);
        //                }
        //            }
        //        }

        //        return result;
        //    }

        //    public static bool Clear(RememberMeType type)
        //    {
        //        bool result = false;

        //        string t = type.ToString().ToLower();

        //        // Set "id" registry key
        //        string key = TH_UserManagement.Management.RememberMe.Registry_Functions.GetRegistryKey(t + "_id");
        //        if (key != null)
        //        {
        //            NameValueCollection values = new NameValueCollection();

        //            values["id"] = key;
        //            values["type"] = type.ToString();


        //            string url = "https://www.feenux.com/php/users/clearrememberme.php";

        //            string responseString = HTTP.SendData(url, values);

        //            if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;
        //        }

        //        TH_UserManagement.Management.RememberMe.Registry_Functions.DeleteRegistryKey(t + "_id");
        //        TH_UserManagement.Management.RememberMe.Registry_Functions.DeleteRegistryKey(t + "_last_login");
        //        TH_UserManagement.Management.RememberMe.Registry_Functions.DeleteRegistryKey(t + "__hash");

        //        return result;
        //    }
        //}
    }

}
