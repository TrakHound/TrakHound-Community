using System;
using System.Collections.Generic;
using System.Net;
using System.Collections.Specialized;
using System.Data;
using System.Text;

using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using TH_Global;

namespace TH_MySQL.PHP
{
    public static class etc
    {

        public static object[] CustomCommand(MySQL_Configuration config, string commandText)
        {
            object[] Result = null;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    using (WebClient client = new WebClient())
                    {

                        NameValueCollection values = new NameValueCollection();
                        if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
                        else values["server"] = config.Server;

                        values["user"] = config.Username;
                        values["password"] = config.Password;
                        values["db"] = config.Database;

                        values["query"] = commandText;

                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                        byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        success = true;
                    }
                }
                catch (Exception ex) { Logger.Log(ex.Message); }
            }

            return Result;

        }

        public static object GetValue(MySQL_Configuration config, string tablename, string column, string filterExpression)
        {

            object Result = null;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    using (WebClient client = new WebClient())
                    {

                        NameValueCollection values = new NameValueCollection();
                        if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
                        else values["server"] = config.Server;

                        values["user"] = config.Username;
                        values["password"] = config.Password;
                        values["db"] = config.Database;

                        values["query"] = "SELECT " + column + " FROM " + tablename + " " + filterExpression;

                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                        byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        JsonSerializerSettings JSS = new JsonSerializerSettings();
                        JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                        JSS.DateParseHandling = DateParseHandling.DateTime;
                        JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                        DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                        if (DT.Rows.Count > 0) Result = DT.Rows[0][column];

                        success = true;
                    }
                }
                catch (Exception ex) { Logger.Log(ex.Message); }
            }

            return Result;

        }

        public static DataTable GetGrants(MySQL_Configuration config, string username)
        {

            DataTable Result = null;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    using (WebClient client = new WebClient())
                    {

                        NameValueCollection values = new NameValueCollection();
                        if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
                        else values["server"] = config.Server;

                        values["server"] = config.Server;
                        values["user"] = config.Username;
                        values["password"] = config.Password;
                        values["db"] = config.Database;

                        values["query"] = "SHOW GRANTS FOR '" + username + "'@'%'";

                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                        byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        JsonSerializerSettings JSS = new JsonSerializerSettings();
                        JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                        JSS.DateParseHandling = DateParseHandling.DateTime;
                        JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                        DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                        Result = DT;

                        success = true;
                    }
                }
                catch (JsonReaderException JRE) { Logger.Log(JRE.Message); }
                catch (Exception ex) { Logger.Log(ex.Message); }
            }

            return Result;

        }

    }
}
