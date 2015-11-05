using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.IO;
using System.Data;
using System.Net;
using System.Collections.Specialized;

using Microsoft.Win32;

using Newtonsoft.Json;

using TH_Global;
using TH_Global.Functions;

namespace TH_Configuration.User
{
    public static class Management
    {

        #region "User Management"

        public const string tablename = "users";

        
        public static UserConfiguration Login(string username, string password)
        {
            UserConfiguration result = null;

            DataRow dbrow = GetLoginData(username, password);
            if (dbrow != null)
            {
                Console.WriteLine(username + " Logged in Successfully!");

                result = LoginSuccess(dbrow);

                //result = UserConfiguration.GetFromDataRow(dbrow);

                //UpdateLoginTime(result);


                //if (dbrow.Table.Columns.Contains("salt"))
                //{
                //    string strSalt = dbrow["salt"].ToString();
                //    int salt = -1;
                //    if (int.TryParse(strSalt, out salt))
                //    {
                //        if (dbrow.Table.Columns.Contains("hash"))
                //        {
                //            string hash = dbrow["hash"].ToString();

                //            Security.Password pwd = new Security.Password(password, salt);

                //            if (pwd.hash == hash)
                //            {
                //                Console.WriteLine(username + " Logged in Successfully!");

                //                result = UserConfiguration.GetFromDataRow(dbrow);

                //                UpdateLoginTime(result);
                //            }
                //            else Console.WriteLine("Incorrect Password!");
                //        }
                //    }
                //}
            }
            else Console.WriteLine("Username '" + username + "' Not Found in Database!");

            return result;
        }

        static DataRow GetLoginData(string id, string password)
        {
            DataRow Result = null;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    values["id"] = id;
                    values["password"] = password;

                    byte[] response = client.UploadValues("http://www.feenux.com/php/users/login.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.Trim() != "")
                    {
                        JsonSerializerSettings JSS = new JsonSerializerSettings();
                        JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                        JSS.DateParseHandling = DateParseHandling.DateTime;
                        JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                        DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                        if (DT.Rows.Count > 0) Result = DT.Rows[0];
                    }

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;
        }

        static UserConfiguration LoginSuccess(DataRow dbrow)
        {
            UserConfiguration result = null;

                result = UserConfiguration.GetFromDataRow(dbrow);

                UpdateLoginTime(result);

                return result;
        }


        public static bool CreateUser(UserConfiguration userConfig, string password)
        {
            bool result = false;

            string[] columns = new string[]
            {
                "username",
                "first_name",
                "last_name",
                "company",
                "email",
                "phone",
                "address1",
                "address2",
                "city",
                "state",
                "country",
                "zipcode"
            };

            object[] rowValues = new object[] 
            {
                userConfig.username.ToLower(),
                TH_Global.Formatting.UppercaseFirst(userConfig.first_name),
                TH_Global.Formatting.UppercaseFirst(userConfig.last_name),
                TH_Global.Formatting.UppercaseFirst(userConfig.company),
                userConfig.email,
                TH_Global.Formatting.ToPhoneNumber(userConfig.phone),
                userConfig.address1,
                userConfig.address2,
                TH_Global.Formatting.UppercaseFirst(userConfig.city),
                userConfig.state,
                TH_Global.Formatting.UppercaseFirst(userConfig.country),
                userConfig.zipcode
            };

            //Create Columns string
            string cols = "";
            for (int x = 0; x <= columns.Length - 1; x++)
            {
                cols += columns[x].ToString().ToUpper();
                if (x < columns.Length - 1) cols += ", ";
            }

            //Create Values string
            string vals = "";
            for (int x = 0; x <= rowValues.Length - 1; x++)
            {
                // Dont put the ' characters if the value is null
                if (rowValues[x] == null) vals += "null";
                else
                {
                    object val = rowValues[x];
                    if (val.GetType() == typeof(DateTime)) val = ConvertDateStringtoMySQL(val.ToString());

                    if (rowValues[x].ToString().ToLower() != "null") vals += "'" + ConvertToSafe(val.ToString()) + "'";
                    else vals += rowValues[x].ToString();
                }

                if (x < rowValues.Length - 1) vals += ", ";
            }

            //Create Update string
            string update = "";

            update = " ON DUPLICATE KEY UPDATE ";
            for (int x = 0; x <= columns.Length - 1; x++)
            {
                if (rowValues[x] != null)
                {
                    update += columns[x].ToString().ToUpper();
                    update += "=";

                    object val = rowValues[x];
                    if (val.GetType() == typeof(DateTime)) val = ConvertDateStringtoMySQL(val.ToString());

                    update += "'" + ConvertToSafe(val.ToString()) + "'";

                    if (x < columns.Length - 1) update += ", ";
                }
            }

            string query = "INSERT IGNORE INTO " + tablename + " (" + cols + ") VALUES (" + vals + ")" + update;


            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();

                    values["username"] = userConfig.username.ToLower();
                    values["password"] = password;
                    values["query"] = query;

                    byte[] response = client.UploadValues("http://www.feenux.com/php/users/createuser.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;

        }


        public class VerifyUsernameReturn
        {
            public bool available { get; set; }
            public string message { get; set; }
        }

        public static VerifyUsernameReturn VerifyUsername(string username)
        {
            VerifyUsernameReturn result = null;

            try
            {
                using (WebClient client = new WebClient())
                {
                    NameValueCollection values = new NameValueCollection();
                    values["username"] = username;

                    byte[] response = client.UploadValues("http://www.feenux.com/php/users/verifyusername.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    string output = responseString.Trim();

                    if (output != "")
                    {
                        result = new VerifyUsernameReturn();

                        if (output.Contains(';'))
                        {
                            int index = output.IndexOf(';');

                            bool available = false;
                            string avail = output.Substring(0, index);
                            bool.TryParse(avail, out available);

                            string message = null;
                            if (output.Length > index + 1) message = output.Substring(index + 1);

                            result.available = available;
                            result.message = message;
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;
        }

        public static bool UpdateLoginTime(UserConfiguration userConfig)
        {
            bool result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();

                    values["username"] = userConfig.username;
                    values["lastlogin"] = ConvertDateStringtoMySQL(userConfig.last_login.ToString());

                    byte[] response = client.UploadValues("http://www.feenux.com/php/users/updatelastlogin.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;
        }

        public static bool UpdateImageURL(string imageURL, UserConfiguration userConfig)
        {
            userConfig.image_url = imageURL;

            bool result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();

                    values["username"] = userConfig.username;
                    values["imageurl"] = userConfig.image_url;

                    byte[] response = client.UploadValues("http://www.feenux.com/php/users/updateimageurl.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;

        }


        static string ConvertDateStringtoMySQL(string DateString)
        {
            string Result = "null";

            DateTime TS;
            if (DateTime.TryParse(DateString, out TS)) Result = TS.ToString("yyyy-MM-dd H:mm:ss");

            return Result;
        }

        static string ConvertToSafe(string s)
        {
            string r = s;
            if (r.Contains("'")) r = r.Replace("'", "\'");
            return r;
        }


        #region "Remember Me"

        public enum RememberMeType
        {
            Client = 0,
            Server = 1
        }

        public static bool SetRememberMe(UserConfiguration userConfig, RememberMeType type)
        {
            bool result = false;

            string id = userConfig.username;

            string t = type.ToString().ToLower();

            try
            {
                using (WebClient client = new WebClient())
                {
                    NameValueCollection values = new NameValueCollection();

                    values["id"] = id;
                    values["type"] = type.ToString();

                    byte[] response = client.UploadValues("http://www.feenux.com/php/users/setrememberme.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.Trim() != "")
                    {
                        string hash = responseString.Trim();

                        // Set "id" registry key
                        SetRegistryKey(t + "_id", userConfig.username);

                        // Set Date of Last Login using Remember Me
                        SetRegistryKey(t + "_last_login", DateTime.UtcNow.ToString());

                        // Set "hash" registry key
                        SetRegistryKey(t + "_hash", hash);

                        result = true;
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;
        }

        public static UserConfiguration GetRememberMe(RememberMeType type)
        {
            UserConfiguration result = null;

            string t = type.ToString().ToLower();

            string strLast_Login = GetRegistryKey(t + "_last_login");
            DateTime last_login = DateTime.MinValue;
            if (DateTime.TryParse(strLast_Login, out last_login))
            {
                TimeSpan sinceLastLogin = DateTime.UtcNow - last_login;
                if (sinceLastLogin > TimeSpan.FromDays(14))
                {
                    ClearRememberMe(type);
                }
                else
                {
                    string id = GetRegistryKey(t + "_id");
                    string hash = GetRegistryKey(t + "_hash");

                    if (hash != null)
                    {
                        try
                        {
                            using (WebClient client = new WebClient())
                            {
                                NameValueCollection values = new NameValueCollection();

                                values["id"] = id;
                                values["hash"] = hash;
                                values["type"] = type.ToString();
                                
                                byte[] response = client.UploadValues("http://www.feenux.com/php/users/getrememberme.php", values);

                                string responseString = Encoding.Default.GetString(response);

                                if (responseString.Trim() != "")
                                {
                                    JsonSerializerSettings JSS = new JsonSerializerSettings();
                                    JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                                    JSS.DateParseHandling = DateParseHandling.DateTime;
                                    JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                                    DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                                    if (DT.Rows.Count > 0)
                                    {
                                        result = LoginSuccess(DT.Rows[0]);

                                        Console.WriteLine(result.username + " Auto Logged in Successfully!");
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { Logger.Log(ex.Message); }
                    }
                }
            }

            return result;
        }

        public static bool ClearRememberMe(RememberMeType type)
        {
            bool result = false;

            string t = type.ToString().ToLower();

            // Set "id" registry key
            string key = GetRegistryKey(t + "_id");
            if (key != null)
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        NameValueCollection values = new NameValueCollection();

                        values["id"] = key;
                        values["type"] = type.ToString();

                        byte[] response = client.UploadValues("http://www.feenux.com/php/users/clearrememberme.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        if (responseString.ToLower().Trim() == "true") result = true;
                    }
                }
                catch (Exception ex) { Logger.Log(ex.Message); }
            }

            DeleteRegistryKey(t + "_id");
            DeleteRegistryKey(t + "_last_login");
            DeleteRegistryKey(t + "__hash");

            return result;
        }

        #region "Registry Functions"

        public static void SetRegistryKey(string keyName, object keyValue)
        {
            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Create/Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.CreateSubKey("TrakHound");

                // Create/Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.CreateSubKey("RememberMe");

                // Create/Open CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                RegistryKey updateKey = updatesKey.CreateSubKey(keyName);

                // Update value for [keyName] to [keyValue]
                updateKey.SetValue(keyName, keyValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UserManagement_RememberMe_SetRegistryKey() : " + ex.Message);
            }
        }

        public static string GetRegistryKey(string keyName)
        {
            string Result = null;

            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound");

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey("RememberMe");

                // Open CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                RegistryKey updateKey = updatesKey.OpenSubKey(keyName);

                // Read value for [keyName] to [keyValue]
                Result = updateKey.GetValue(keyName).ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("UserManagement_RememberMe_GetRegistryKey() : " + ex.Message);
            }

            return Result;
        }

        public static string[] GetRegistryKeyNames()
        {
            string[] Result = null;

            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound");

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey("RememberMe");

                Result = updatesKey.GetSubKeyNames();
            }
            catch (Exception ex)
            {
                Console.WriteLine("UserManagement_RememberMe_GetRegistryKeys() : " + ex.Message);
            }

            return Result;
        }

        public static void DeleteRegistryKey(string keyName)
        {
            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound", true);

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey("RememberMe", true);

                // Delete CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                updatesKey.DeleteSubKey(keyName, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UserManagement_RememberMe_DeleteRegistryKey() : " + ex.Message);
            }
        }

        #endregion

        #endregion


        #endregion

        #region "Configuration Management"

        public static void AddConfigurationToUser(UserConfiguration userConfig, Configuration configuration)
        {
            //string tableName = GetConfigurationTableName(userConfig, configuration);
            string tableName = CreateConfigurationTableName(userConfig);

            CreateConfigurationTable(userConfig, tableName);

            DataTable dt = TH_Configuration.Converter.XMLToTable(configuration.ConfigurationXML);

            UpdateConfigurationTable(userConfig, tableName, dt);
        }

        public static List<Configuration> GetConfigurationsForUser(UserConfiguration userConfig)
        {
            List<Configuration> result = null;

            try
            {
                using (WebClient client = new WebClient())
                {
                    NameValueCollection values = new NameValueCollection();

                    values["username"] = userConfig.username;

                    byte[] response = client.UploadValues("http://www.feenux.com/php/configurations/getconfigurations.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.Trim() != "")
                    {
                        try
                        {
                            DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)));

                            if (DT != null)
                            {
                                result = new List<Configuration>();

                                foreach (DataRow Row in DT.Rows)
                                {
                                    string tablename = Row[0].ToString();

                                    DataTable dt = GetConfigurationTable(tablename);
                                    if (dt != null)
                                    {
                                        string filename = String_Functions.RandomString(20);

                                        string tempdir = FileLocations.TrakHound + @"\temp";
                                        if (!Directory.Exists(tempdir)) Directory.CreateDirectory(tempdir);

                                        string tempPath = tempdir + @"\" + filename;

                                        string path = TH_Configuration.Converter.TableToXML(dt, tempPath);

                                        Configuration config = TH_Configuration.Configuration.ReadConfigFile(path);
                                        if (config != null)
                                        {
                                            config.TableName = tablename;
                                            result.Add(config);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex) { Logger.Log("GetConfigurationsForUser() :: Exception :: " + ex.Message); }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;
        }

        static DataTable GetConfigurationTable(string tablename)
        {
            DataTable Result = null;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    values["tablename"] = tablename;

                    byte[] response = client.UploadValues("http://www.feenux.com/php/configurations/getconfigurationtable.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.Trim() != "")
                    {
                        JsonSerializerSettings JSS = new JsonSerializerSettings();
                        JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                        JSS.DateParseHandling = DateParseHandling.DateTime;
                        JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                        DataTable dt = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                        Result = dt;
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;
        }


        //public static bool UpdateConfigurationTable(UserConfiguration userConfig, Configuration configuration)
        public static bool UpdateConfigurationTable(UserConfiguration userConfig, string tableName, DataTable dt)
        {
            bool result = false;

            //DataTable dt = TH_Configuration.Converter.XMLToTable(configuration.ConfigurationXML);
            if (dt != null)
            {
                // Add Columns
                List<string> columnsList = new List<string>();
                foreach (DataColumn col in dt.Columns) columnsList.Add(col.ColumnName);
                object[] columns = columnsList.ToArray();

                List<List<object>> rowValues = new List<List<object>>();

                foreach (DataRow row in dt.Rows)
                {
                    List<object> values = new List<object>();
                    foreach (object val in row.ItemArray) values.Add(val);
                    rowValues.Add(values);
                }


                //Create Columns string
                string cols = "";
                for (int x = 0; x <= columns.Length - 1; x++)
                {
                    cols += columns[x].ToString().ToUpper();
                    if (x < columns.Length - 1) cols += ", ";
                }

                //Create Values string
                string vals = "VALUES ";
                for (int i = 0; i <= rowValues.Count - 1; i++)
                {
                    vals += "(";

                    for (int x = 0; x <= rowValues[i].Count - 1; x++)
                    {

                        List<object> ValueSet = rowValues[i];

                        // Dont put the ' characters if the value is null
                        if (ValueSet[x] == null) vals += "null";
                        else
                        {
                            object val = ValueSet[x];
                            if (val.GetType() == typeof(DateTime)) val = ConvertDateStringtoMySQL(val.ToString());

                            if (val.ToString().ToLower() != "null") vals += "'" + ConvertToSafe(val.ToString()) + "'";
                            else vals += val.ToString();
                        }


                        if (x < ValueSet.Count - 1) vals += ", ";
                    }

                    vals += ")";

                    if (i < rowValues.Count - 1) vals += ",";

                }

                //Create Update string
                string update = "";
                update = " ON DUPLICATE KEY UPDATE ";
                for (int x = 0; x <= columns.Length - 1; x++)
                {
                    update += columns[x].ToString().ToUpper();
                    update += "=";

                    update += "VALUES(" + columns[x].ToString().ToUpper() + ")";
                    if (x < columns.Length - 1) update += ", ";
                }

                //string table = GetConfigurationTableName(userConfig, configuration);


                string query = "INSERT IGNORE INTO " + tableName + " (" + cols + ") " + vals + update;

                try
                {
                    using (WebClient client = new WebClient())
                    {

                        NameValueCollection values = new NameValueCollection();

                        values["query"] = query;

                        byte[] response = client.UploadValues("http://www.feenux.com/php/configurations/updateconfigurationtable.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        if (responseString.ToLower().Trim() == "true") result = true;

                    }
                }
                catch (Exception ex) { Logger.Log(ex.Message); }

            }

            return result;
        }

        public static bool ClearConfigurationTable(UserConfiguration userConfig, string tableName)
        {
            bool result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();

                    values["query"] = "TRUNCATE TABLE " + tableName;

                    byte[] response = client.UploadValues("http://www.feenux.com/php/configurations/createconfigurationtable.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;
        }

        public static bool CreateConfigurationTable(UserConfiguration userConfig, string tableName)
        {
            bool result = false;

            object[] columns = new object[] 
            {
                "address varchar(90)",
                "name varchar(90)",
                "value varchar(90)",
                "attributes varchar(90)"
            };

            string primaryKey = "address";

            //string table = GetConfigurationTableName(userConfig, configuration);

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();

                    string coldef = "";

                    //Create Column Definition string
                    for (int x = 0; x <= columns.Length - 1; x++)
                    {
                        coldef += columns[x].ToString();
                        if (x < columns.Length - 1) coldef += ",";
                    }

                    string Keydef = "";
                    if (primaryKey != null) Keydef = ", PRIMARY KEY (" + primaryKey.ToLower() + ")";

                    values["query"] = "CREATE TABLE IF NOT EXISTS " + tableName + " (" + coldef + Keydef + ")";

                    byte[] response = client.UploadValues("http://www.feenux.com/php/configurations/createconfigurationtable.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;
        }

        public static bool CreateConfigurationTable(UserConfiguration userConfig, Configuration configuration)
        {
            bool result = false;

            object[] columns = new object[] 
            {
                "address varchar(90)",
                "name varchar(90)",
                "value varchar(90)",
                "attributes varchar(90)"
            };

            string primaryKey = "address";

            string table = GetConfigurationTableName(userConfig, configuration);

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();

                    string coldef = "";

                    //Create Column Definition string
                    for (int x = 0; x <= columns.Length - 1; x++)
                    {
                        coldef += columns[x].ToString();
                        if (x < columns.Length - 1) coldef += ",";
                    }

                    string Keydef = "";
                    if (primaryKey != null) Keydef = ", PRIMARY KEY (" + primaryKey.ToLower() + ")";

                    values["query"] = "CREATE TABLE IF NOT EXISTS " + table + " (" + coldef + Keydef + ")";

                    byte[] response = client.UploadValues("http://www.feenux.com/php/configurations/createconfigurationtable.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;
        }

        public static string GetConfigurationTableName(UserConfiguration userConfig, Configuration configuration)
        {
            string table = userConfig.username + "_" + configuration.Description.Manufacturer + "_" + configuration.Description.Machine_Type + "_" + configuration.Description.Machine_ID + "_Configuration";
            table = table.Replace(' ', '_');

            return table;
        }

        public static string CreateConfigurationTableName(UserConfiguration userConfig)
        {
            //string table = userConfig.username + "_" + configuration.Description.Manufacturer + "_" + configuration.Description.Machine_Type + "_" + configuration.Description.Machine_ID + "_Configuration";
            //table = table.Replace(' ', '_');

            return userConfig.username + "_" + String_Functions.RandomString(20);
        }

        #endregion


        public static bool VerifyPasswordMinimum(System.Security.SecureString pwd)
        {
            if (pwd.Length > 7) return true;
            else return false;
        }

        public static bool VerifyPasswordMaximum(System.Security.SecureString pwd)
        {
            if (pwd.Length < 101) return true;
            else return false;
        }

    }

    public static class Table_Functions
    {
        public static string GetTableValue(string key, DataTable dt)
        {
            string result = null;

            DataRow row = dt.Rows.Find(key);
            if (row != null)
            {
                result = row["value"].ToString();
            }

            return result;
        }

        public static string RemoveTableRow(string key, DataTable dt)
        {
            string result = null;

            DataRow row = dt.Rows.Find(key);
            if (row != null)
            {
                dt.Rows.Remove(row);
            }

            return result;
        }

        public static void UpdateTableValue(string value, string key, DataTable dt)
        {
            DataRow row = dt.Rows.Find(key);
            if (row != null)
            {
                row["value"] = value;
            }
            else
            {
                row = dt.NewRow();
                row["address"] = key;
                row["value"] = value;
                dt.Rows.Add(row);
            }
        }

        public static void UpdateTableValue(string value, string attributes, string key, DataTable dt)
        {
            DataRow row = dt.Rows.Find(key);
            if (row != null)
            {
                row["value"] = value;
            }
            else
            {
                row = dt.NewRow();
                row["address"] = key;
                row["value"] = value;
                row["attributes"] = attributes;
                dt.Rows.Add(row);
            }
        }
    }
}
