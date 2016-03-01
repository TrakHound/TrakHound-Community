using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using System.Collections.Specialized;
using System.Data;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Global.Web;

namespace TH_UserManagement.Management.Remote
{
    public static class Configurations
    {

        const string PHP_PATH_PREFIX = "https://www.feenux.com/php/configurations/";
        const int connectionAttempts = 3;
        const string TABLE_DELIMITER_START = "^^^";
        const string TABLE_DELIMITER_END = "~~~";

        /// <summary>
        /// Sets the path to use for PHP files. 
        /// References the Major Revision number in this assembly's version for the subdirectory to look in
        /// ex. v2.3.0.1 would look in /v2/
        /// </summary>
        public static string PHPFilePath
        {
            get
            {
                var a = System.Reflection.Assembly.GetExecutingAssembly();
                string path = "v" + a.GetName().Version.Major.ToString();
                path = PHP_PATH_PREFIX + path + "/";

                return path;
            }
        }

        
        public static bool Add(UserConfiguration userConfig, Configuration configuration)
        {
            bool result = false;

            string tableName = CreateTableName(userConfig);

            //configuration.TableName = tableName;

            result = Create(tableName);
            if (result)
            {
                // Set Table Name
                configuration.TableName = tableName;
                XML_Functions.SetInnerText(configuration.ConfigurationXML, "TableName", tableName);

                // Set new Unique Id
                string uniqueId = String_Functions.RandomString(20);
                configuration.UniqueId = uniqueId;
                XML_Functions.SetInnerText(configuration.ConfigurationXML, "UniqueId", uniqueId);

                // Set Enabled to False
                configuration.ClientEnabled = false;
                configuration.ServerEnabled = false;
                XML_Functions.SetInnerText(configuration.ConfigurationXML, "ClientEnabled", "false");
                XML_Functions.SetInnerText(configuration.ConfigurationXML, "ServerEnabled", "false");

                DataTable dt = TH_Configuration.Converter.XMLToTable(configuration.ConfigurationXML);

                result = Update(tableName, dt);
            }

            return result;
        }

        public static string[] GetList(UserConfiguration userConfig)
        {
            NameValueCollection values = new NameValueCollection();

            values["username"] = userConfig.username;

            string url = "https://www.feenux.com/php/configurations/getconfigurations.php";
            string responseString = HTTP.SendData(url, values);

            if (responseString != null)
            {
                DataTable DT = JSON.ToTable(responseString);
                if (DT != null)
                {
                    List<string> result = new List<string>();

                    foreach (DataRow Row in DT.Rows)
                    {
                        string tablename = Row[0].ToString();

                        result.Add(tablename);
                    }

                    return result.ToArray();
                }
                else return new string[0];
            }
            else return null;
        }

        public static List<Configuration> Get(UserConfiguration userConfig)
        {
            List<Configuration> result = null;

            NameValueCollection values = new NameValueCollection();

            if (userConfig.username != null) values["username"] = userConfig.username.ToLower();

            //string url = "https://www.feenux.com/php/configurations/getconfigurationslist.php";
            string url = PHPFilePath + "getconfigurationslist.php";
            string responseString = HTTP.SendData(url, values);

            if (responseString != null)
            {
                result = new List<Configuration>();

                //string[] tables = responseString.Split('%');
                string[] tables = responseString.Split(TABLE_DELIMITER_START.ToCharArray());

                foreach (string table in tables)
                {
                    if (!String.IsNullOrEmpty(table))
                    {
                        //var delimiter = table.IndexOf('~');
                        var delimiter = table.IndexOf(TABLE_DELIMITER_END);
                        if (delimiter > 0)
                        {
                            string tablename = table.Substring(0, delimiter);
                            //string tabledata = table.Substring(delimiter + 1);
                            string tabledata = table.Substring(delimiter + TABLE_DELIMITER_END.Length);

                            DataTable dt = JSON.ToTable(tabledata);
                            if (dt != null)
                            {
                                XmlDocument xml = TH_Configuration.Converter.TableToXML(dt);
                                if (xml != null)
                                {
                                    Configuration config = TH_Configuration.Configuration.Read(xml);
                                    if (config != null)
                                    {
                                        config.Remote = true;
                                        config.TableName = tablename;
                                        result.Add(config);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static List<DataTable> GetDeviceInfos(UserConfiguration userConfig)
        {
            List<DataTable> result = null;

            NameValueCollection values = new NameValueCollection();

            if (userConfig.username != null) values["username"] = userConfig.username.ToLower();

            string url = "https://www.feenux.com/php/configurations/getdeviceinfolist.php";
            string responseString = HTTP.SendData(url, values);

            if (responseString != null)
            {
                result = new List<DataTable>();

                string[] tables = responseString.Split('%');

                foreach (string table in tables)
                {
                    if (!String.IsNullOrEmpty(table))
                    {
                        var delimiter = table.IndexOf('~');
                        if (delimiter > 0)
                        {
                            string tablename = table.Substring(0, delimiter);
                            string tabledata = table.Substring(delimiter + 1);

                            DataTable dt = JSON.ToTable(tabledata);
                            if (dt != null)
                            {
                                dt.TableName = tablename;

                                result.Add(dt);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static DataTable GetTable(string table)
        {
            DataTable Result = null;

            NameValueCollection values = new NameValueCollection();
            values["tablename"] = table;

            string url = "https://www.feenux.com/php/configurations/getconfigurationtable.php";


            string responseString = HTTP.SendData(url, values);

            Result = JSON.ToTable(responseString);

            return Result;
        }

        //public static Management.Configurations.UpdateInfo GetClientUpdateInfo(string table)
        //{
        //    Management.Configurations.UpdateInfo result = null;

        //    NameValueCollection values = new NameValueCollection();
        //    values["tablename"] = table;

        //    string url = "https://www.feenux.com/php/configurations/getclientupdateinfo.php";

        //    string responseString = HTTP.SendData(url, values);

        //    result = JSON.ToType<Management.Configurations.UpdateInfo>(responseString);

        //    return result;
        //}

        //public static Management.Configurations.UpdateInfo GetServerUpdateInfo(string table)
        //{
        //    Management.Configurations.UpdateInfo result = null;

        //    NameValueCollection values = new NameValueCollection();
        //    values["tablename"] = table;

        //    string url = "https://www.feenux.com/php/configurations/getserverupdateinfo.php";

        //    string responseString = HTTP.SendData(url, values);

        //    result = JSON.ToType<Management.Configurations.UpdateInfo>(responseString);

        //    return result;
        //}

        public static bool Update(string tableName, DataTable dt)
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


                NameValueCollection postValues = new NameValueCollection();

                postValues["query"] = query;

                string url = "https://www.feenux.com/php/configurations/updateconfigurationtable.php";


                string responseString = HTTP.SendData(url, postValues);

                if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;

            }

            return result;
        }

        public static bool Update(string address, string value, string tableName)
        {
            bool result = false;

            if (address != null && value != null)
            {

                string columns = " (address, value) ";

                string set = " VALUES ('" + address + "', '" + value + "')";

                string update = " ON DUPLICATE KEY UPDATE address='" + address + "', value='" + value + "'";

                string query = "INSERT IGNORE INTO " + tableName + columns + set + update;

                NameValueCollection values = new NameValueCollection();

                values["query"] = query;

                string url = "https://www.feenux.com/php/configurations/updateconfigurationtable.php";


                string responseString = HTTP.SendData(url, values);

                if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;

            }

            return result;
        }

        public static bool Update(string address, string value, string attributes, string tableName)
        {
            bool result = false;

            if (address != null && value != null && attributes != null)
            {

                string columns = " (address, value, attributes) ";

                string set = " VALUES ('" + address + "', '" + value + "', '" + attributes + "')";

                string update = " ON DUPLICATE KEY UPDATE address='" + address + "', value='" + value + "', attributes='" + attributes + "'";

                string query = "INSERT IGNORE INTO " + tableName + columns + set + update;

                NameValueCollection values = new NameValueCollection();

                values["query"] = query;

                string url = "https://www.feenux.com/php/configurations/updateconfigurationtable.php";

                string responseString = HTTP.SendData(url, values);

                if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;

            }

            return result;
        }

        public static bool Clear(string tableName)
        {
            bool result = false;

            NameValueCollection values = new NameValueCollection();

            values["query"] = "TRUNCATE TABLE " + tableName;

            string url = "https://www.feenux.com/php/configurations/createconfigurationtable.php";


            string responseString = HTTP.SendData(url, values);
            if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;

            return result;
        }

        public static bool Create(string tableName)
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

            string url = "https://www.feenux.com/php/configurations/createconfigurationtable.php";


            string responseString = HTTP.SendData(url, values);

            if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;

            return result;
        }

        //public static bool Create(UserConfiguration userConfig, Configuration configuration)
        //{
        //    bool result = false;

        //    object[] columns = new object[] 
        //    {
        //        "address varchar(90)",
        //        "name varchar(90)",
        //        "value varchar(90)",
        //        "attributes varchar(90)"
        //    };

        //    string primaryKey = "address";

        //    string table = GetConfigurationTableName(userConfig, configuration);

        //    NameValueCollection values = new NameValueCollection();

        //    string coldef = "";

        //    //Create Column Definition string
        //    for (int x = 0; x <= columns.Length - 1; x++)
        //    {
        //        coldef += columns[x].ToString();
        //        if (x < columns.Length - 1) coldef += ",";
        //    }

        //    string Keydef = "";
        //    if (primaryKey != null) Keydef = ", PRIMARY KEY (" + primaryKey.ToLower() + ")";

        //    values["query"] = "CREATE TABLE IF NOT EXISTS " + table + " (" + coldef + Keydef + ")";

        //    string url = "https://www.feenux.com/php/configurations/createconfigurationtable.php";


        //    string responseString = HTTP.SendData(url, values);

        //    if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;

        //    return result;
        //}

        //public static string GetConfigurationTableName(UserConfiguration userConfig, Configuration configuration)
        //{
        //    string table = userConfig.username + "_" + configuration.Description.Manufacturer + "_" + configuration.Description.Device_Type + "_" + configuration.Description.Device_ID + "_Configuration";
        //    table = table.Replace(' ', '_');

        //    return table;
        //}


        public static bool Remove(string tableName)
        {
            bool result = false;

            NameValueCollection values = new NameValueCollection();

            values["tablename"] = tableName;

            string url = "https://www.feenux.com/php/configurations/removeconfigurationtable.php";


            string responseString = HTTP.SendData(url, values);

            if (responseString != null) if (responseString.ToLower().Trim() == "true") result = true;

            return result;
        }


        static string CreateTableName(UserConfiguration userConfig)
        {
            return userConfig.username.ToLower() + "_" + String_Functions.RandomString(20);
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
            if (r.Contains(@"\")) r = r.Replace(@"\", @"\\");
            if (r.Contains("'")) r = r.Replace("'", "\'");
            //if (r.Contains("%")) r = r.Replace("%", @"\%");
            return r;
        }

    }
}
