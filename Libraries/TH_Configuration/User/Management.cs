using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.Data;
using System.Net;
using System.Collections.Specialized;

using Newtonsoft.Json;

using TH_Global;

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

                                UpdateLoginTime(result);
                            }
                            else Console.WriteLine("Incorrect Password!");
                        }
                    }
                }
            }
            else Console.WriteLine("Username '" + username + "' Not Found in Database!");

            return result;
        }

        static DataRow GetLoginData(string username, string password)
        {
            DataRow Result = null;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    values["username"] = username;
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


        public static bool CreateUser(UserConfiguration userConfig, string password)
        {
            bool result = false;

            string[] columns = new string[]
            {
                "username",
                "hash",
                "salt",
                "first_name",
                "last_name",
                "company",
                "email",
                "phone",
                "address",
                "city",
                "state",
                "country",
                "zipcode"
            };

            Security.Password pwd = new Security.Password(password);

            object[] rowValues = new object[] 
            {
                userConfig.username.ToLower(),
                pwd.hash,
                pwd.salt,
                TH_Global.Formatting.UppercaseFirst(userConfig.first_name),
                TH_Global.Formatting.UppercaseFirst(userConfig.last_name),
                TH_Global.Formatting.UppercaseFirst(userConfig.company),
                userConfig.email,
                userConfig.phone,
                userConfig.address,
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

                    values["query"] = query;

                    byte[] response = client.UploadValues("http://www.feenux.com/php/users/createuser.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return result;

        }


        public static bool VerifyUsername(string username)
        {
            bool result = false;

            Regex r = new Regex("^(?=.{6,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$");
            if (r.IsMatch(username) && !username.ToLower().Contains("trakhound"))
            {

                try
                {
                    using (WebClient client = new WebClient())
                    {

                        NameValueCollection values = new NameValueCollection();
                        values["username"] = username;

                        byte[] response = client.UploadValues("http://www.feenux.com/php/users/verifyusername.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        if (responseString.Trim() != "")
                        {
                            JsonSerializerSettings JSS = new JsonSerializerSettings();
                            JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                            JSS.DateParseHandling = DateParseHandling.DateTime;
                            JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                            DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                            if (DT.Rows.Count == 0) result = true;
                        }
                        else result = true;


                    }
                }
                catch (Exception ex) { Logger.Log(ex.Message); }

            }

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

        #endregion

        #region "Configuration Management"

        public static void AddConfigurationToUser(UserConfiguration userConfig, Configuration configuration)
        {
            CreateConfigurationTable(userConfig, configuration);
            UpdateConfigurationTable(userConfig, configuration);
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

                    byte[] response = client.UploadValues("http://www.feenux.com/php/users/getconfigurations.php", values);

                    string responseString = Encoding.Default.GetString(response);

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
                                    string path = TH_Configuration.Converter.TableToXML(dt, @"C:\Temp\" + TH_Global.Functions.RandomString(20));

                                    Configuration config = TH_Configuration.Configuration.ReadConfigFile(path);
                                    if (config != null)
                                    {
                                        result.Add(config);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex) { }
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

                    byte[] response = client.UploadValues("http://www.feenux.com/php/users/getconfigurationtable.php", values);

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


        public static bool UpdateConfigurationTable(UserConfiguration userConfig, Configuration configuration)
        {
            bool result = false;

            DataTable dt = TH_Configuration.Converter.XMLToTable(configuration.ConfigurationXML);
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

                string query = "INSERT IGNORE INTO " + tablename + " (" + cols + ") " + vals + update;

                string table = GetConfigurationTableName(userConfig, configuration);

                try
                {
                    using (WebClient client = new WebClient())
                    {

                        NameValueCollection values = new NameValueCollection();

                        values["query"] = query;

                        byte[] response = client.UploadValues("http://www.feenux.com/php/configuration/updateconfigurationtable.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        if (responseString.ToLower().Trim() == "true") result = true;

                    }
                }
                catch (Exception ex) { Logger.Log(ex.Message); }

            }

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

                    values["query"] = "CREATE TABLE IF NOT EXISTS " + tablename + " (" + coldef + Keydef + ")";

                    byte[] response = client.UploadValues("http://www.feenux.com/php/configuration/createconfigurationtable.php", values);

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
}
