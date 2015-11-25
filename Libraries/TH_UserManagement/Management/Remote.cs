using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using System.IO;
using System.Data;
using System.Net;
using System.Collections.Specialized;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;

using Newtonsoft.Json;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;

namespace TH_UserManagement.Management
{
    public static class Remote
    {

        public static class Users
        {
            public const string tablename = "users";

            public static UserConfiguration Login(string username, string password)
            {
                UserConfiguration result = null;

                DataRow dbrow = GetLoginData(username, password);
                if (dbrow != null)
                {
                    Console.WriteLine(username + " Logged in Successfully!");

                    result = LoginSuccess(dbrow);
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

            public static class RememberMe
            {
                public static bool Set(UserConfiguration userConfig, RememberMeType type)
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
                                TH_UserManagement.Management.RememberMe.Registry_Functions.SetRegistryKey(t + "_id", userConfig.username);

                                // Set Date of Last Login using Remember Me
                                TH_UserManagement.Management.RememberMe.Registry_Functions.SetRegistryKey(t + "_last_login", DateTime.UtcNow.ToString());

                                // Set "hash" registry key
                                TH_UserManagement.Management.RememberMe.Registry_Functions.SetRegistryKey(t + "_hash", hash);

                                result = true;
                            }
                        }
                    }
                    catch (Exception ex) { Logger.Log(ex.Message); }

                    return result;
                }

                public static UserConfiguration Get(RememberMeType type)
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
                            string hash = TH_UserManagement.Management.RememberMe.Registry_Functions.GetRegistryKey(t + "_hash");

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

                public static bool Clear(RememberMeType type)
                {
                    bool result = false;

                    string t = type.ToString().ToLower();

                    // Set "id" registry key
                    string key = TH_UserManagement.Management.RememberMe.Registry_Functions.GetRegistryKey(t + "_id");
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

                    TH_UserManagement.Management.RememberMe.Registry_Functions.DeleteRegistryKey(t + "_id");
                    TH_UserManagement.Management.RememberMe.Registry_Functions.DeleteRegistryKey(t + "_last_login");
                    TH_UserManagement.Management.RememberMe.Registry_Functions.DeleteRegistryKey(t + "__hash");

                    return result;
                }
            }
        }


        public static class Configurations
        {
            public static bool AddConfigurationToUser(UserConfiguration userConfig, Configuration configuration)
            {
                bool result = false;

                string tableName = CreateConfigurationTableName(userConfig);

                result = CreateConfigurationTable(tableName);
                if (result)
                {
                    DataTable dt = TH_Configuration.Converter.XMLToTable(configuration.ConfigurationXML);

                    result = UpdateConfigurationTable(tableName, dt);
                }

                return result;
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
                                            XmlDocument xml = TH_Configuration.Converter.TableToXML(dt);
                                            if (xml != null)
                                            {
                                                Configuration config = TH_Configuration.Configuration.ReadConfigFile(xml);
                                                if (config != null)
                                                {
                                                    if (config.FileLocations.Manufacturer_Logo_Path != null)
                                                    {
                                                        System.Drawing.Image manufacturer_logo = Images.GetImage(config.FileLocations.Manufacturer_Logo_Path);
                                                        if (manufacturer_logo != null) config.Manufacturer_Logo = manufacturer_logo;
                                                    }

                                                    if (config.FileLocations.Image_Path != null)
                                                    {
                                                        System.Drawing.Image device_image = Images.GetImage(config.FileLocations.Image_Path);
                                                        if (device_image != null) config.Device_Image = device_image;
                                                    }

                                                    config.Remote = true;
                                                    config.TableName = tablename;
                                                    result.Add(config);
                                                }
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

            public static DataTable GetConfigurationTable(string table)
            {
                DataTable Result = null;

                try
                {
                    using (WebClient client = new WebClient())
                    {

                        NameValueCollection values = new NameValueCollection();
                        values["tablename"] = table;

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

            public static bool UpdateConfigurationTable(string tableName, DataTable dt)
            {
                bool result = false;

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

                    //string query = "INSERT IGNORE INTO " + tableName + " (" + cols + ") " + vals + update;

                    string query = "INSERT IGNORE INTO " + tableName + " (" + cols + ") " + vals;

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

            public static bool UpdateConfigurationTable(string address, string value, string tableName)
            {
                bool result = false;

                if (address != null && value != null)
                {

                    string columns = " (address, value) ";

                    string set = " VALUES ('" + address + "', '" + value + "')";

                    string update = " ON DUPLICATE KEY UPDATE address='" + address + "', value='" + value + "'";

                    string query = "INSERT IGNORE INTO " + tableName + columns + set + update;

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

            public static bool UpdateConfigurationTable(string address, string value, string attributes, string tableName)
            {
                bool result = false;

                if (address != null && value != null && attributes != null)
                {

                    string columns = " (address, value, attributes) ";

                    string set = " VALUES ('" + address + "', '" + value + "', '" + attributes + "')";

                    string update = " ON DUPLICATE KEY UPDATE address='" + address + "', value='" + value + "', attributes='" + attributes + "'";

                    string query = "INSERT IGNORE INTO " + tableName + columns + set + update;

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

            public static bool ClearConfigurationTable(string tableName)
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

            public static bool CreateConfigurationTable(string tableName)
            {
                bool result = false;

                object[] columns = new object[] 
            {
                "address varchar(90)",
                "name varchar(90)",
                "value varchar(90)",
                "attributes mediumtext"
            };

                string primaryKey = "address";

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
                return userConfig.username + "_" + String_Functions.RandomString(20);
            }

            public static bool RemoveConfigurationTable(string tableName)
            {
                bool result = false;

                try
                {
                    using (WebClient client = new WebClient())
                    {

                        NameValueCollection values = new NameValueCollection();

                        values["tablename"] = tableName;

                        byte[] response = client.UploadValues("http://www.feenux.com/php/configurations/removeconfigurationtable.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        if (responseString.ToLower().Trim() == "true") result = true;

                    }
                }
                catch (Exception ex) { Logger.Log(ex.Message); }

                return result;
            }
        }



        public static class Images
        {
            public static string OpenImageBrowse(string title)
            {
                string result = null;

                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                dlg.InitialDirectory = FileLocations.TrakHound;
                dlg.Multiselect = false;
                dlg.Title = title;
                dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

                Nullable<bool> dialogResult = dlg.ShowDialog();

                if (dialogResult == true)
                {
                    if (dlg.FileName != null) result = dlg.FileName;
                }

                return result;
            }

            /// <summary>
            /// Uploads an Image to the TrakHound Server
            /// </summary>
            /// <param name="localpath"></param>
            /// <returns></returns>
            public static bool UploadImage(string localpath)
            {
                bool result = false;

                if (File.Exists(localpath))
                {
                    Image img = Image.FromFile(localpath);
                    if (img != null)
                    {
                        string contentFormat = null;

                        if (ImageFormat.Jpeg.Equals(img.RawFormat)) contentFormat = "image/jpeg";
                        else if (ImageFormat.Png.Equals(img.RawFormat)) contentFormat = "image/png";
                        else if (ImageFormat.Gif.Equals(img.RawFormat)) contentFormat = "image/gif";
                        else if (ImageFormat.Bmp.Equals(img.RawFormat)) contentFormat = "image/bmp";
                        else if (ImageFormat.Tiff.Equals(img.RawFormat)) contentFormat = "image/tiff";

                        NameValueCollection nvc = new NameValueCollection();
                        if (Web.HttpUploadFile("http://www.feenux.com/php/configurations/uploadimage.php", localpath, "file", contentFormat, nvc))
                        {
                            result = true;
                        }
                    }
                }

                return result;
            }

            public static System.Drawing.Image GetImage(string filename)
            {
                System.Drawing.Image result = null;

                if (filename != String.Empty)
                {
                    using (WebClient webClient = new WebClient())
                    {
                        try
                        {
                            byte[] data = webClient.DownloadData("http://www.feenux.com/trakhound/users/files/" + filename);

                            using (MemoryStream mem = new MemoryStream(data))
                            {
                                result = System.Drawing.Image.FromStream(mem);
                            }
                        }
                        catch (Exception ex) { Logger.Log("GetImage() : Exception : " + ex.Message); }
                    }
                }

                return result;
            }
        }

        public static class ProfileImages
        {
            public static string OpenImageBrowse()
            {
                string result = null;

                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                dlg.InitialDirectory = FileLocations.TrakHound;
                dlg.Multiselect = false;
                dlg.Title = "Browse for Profile Image";
                dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

                Nullable<bool> dialogResult = dlg.ShowDialog();

                if (dialogResult == true)
                {
                    if (dlg.FileName != null) result = dlg.FileName;
                }

                return result;
            }

            public static System.Drawing.Image ProcessImage(string path)
            {
                System.Drawing.Image result = null;

                if (File.Exists(path))
                {
                    System.Drawing.Image img = Image_Functions.CropImageToCenter(System.Drawing.Image.FromFile(path));

                    result = Image_Functions.SetImageSize(img, 200, 200);
                }

                return result;
            }

            /// <summary>
            /// Uploads a Profile Image to the TrakHound Server
            /// </summary>
            /// <param name="filename"></param>
            /// <param name="localpath"></param>
            /// <returns></returns>
            public static bool UploadProfileImage(string filename, string localpath)
            {
                bool result = false;

                NameValueCollection nvc = new NameValueCollection();
                if (Web.HttpUploadFile("http://www.feenux.com/php/users/uploadprofileimage.php", localpath, "file", "image/jpeg", nvc))
                {
                    result = true;
                }

                return result;
            }

            public static System.Drawing.Image GetProfileImage(UserConfiguration userConfig)
            {
                System.Drawing.Image result = null;

                if (userConfig.image_url != String.Empty)
                {
                    using (WebClient webClient = new WebClient())
                    {
                        try
                        {
                            byte[] data = webClient.DownloadData("http://www.feenux.com/trakhound/users/files/" + userConfig.image_url);

                            using (MemoryStream mem = new MemoryStream(data))
                            {
                                result = System.Drawing.Image.FromStream(mem);
                            }
                        }
                        catch (Exception ex) { Logger.Log("GetProfileImage() : Exception : " + ex.Message); }
                    }
                }

                return result;
            }
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

    }
}


