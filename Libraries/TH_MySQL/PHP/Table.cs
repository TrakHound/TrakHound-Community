using System;
using System.Collections.Generic;
using System.Net;
using System.Collections.Specialized;
using System.Data;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using TH_Global;

namespace TH_MySQL.PHP
{
    public static class Table
    {

        public static bool Create(MySQL_Configuration config, string tablename, object[] columnDefinitions, string primaryKey)
        {

            bool Result = false;

            int attempts = 0;
            bool success = false;
            string message = null;

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

                        string coldef = "";

                        //Create Column Definition string
                        for (int x = 0; x <= columnDefinitions.Length - 1; x++)
                        {
                            coldef += columnDefinitions[x].ToString();
                            if (x < columnDefinitions.Length - 1) coldef += ",";
                        }

                        string Keydef = "";
                        if (primaryKey != null) Keydef = ", PRIMARY KEY (" + primaryKey.ToLower() + ")";

                        values["query"] = "CREATE TABLE IF NOT EXISTS " + tablename + " (" + coldef + Keydef + ")";


                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                        byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Send.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        if (responseString.ToLower().Trim() == "true") Result = true;

                        success = true;
                    }
                }
                catch (Exception ex) { message = ex.Message; }
            }

            if (!success) Logger.Log(attempts.ToString() + " Attempts :: " + message);

            return Result;

        }

        public static bool Drop(MySQL_Configuration config, string tablename)
        {

            bool Result = false;

            int attempts = 0;
            bool success = false;
            string message = null;

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


                        values["query"] = "DROP TABLE IF EXISTS " + tablename;


                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                        byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Send.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        if (responseString.ToLower().Trim() == "true") Result = true;

                        success = true;
                    }
                }
                catch (Exception ex) { message = ex.Message; }
            }

            if (!success) Logger.Log(attempts.ToString() + " Attempts :: " + message);

            return Result;

        }

        public static bool Drop(MySQL_Configuration config, string[] tablenames)
        {

            bool Result = false;

            int attempts = 0;
            bool success = false;
            string message = null;

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

                        string sTablenames = "";
                        for (int x = 0; x <= tablenames.Length - 1; x++)
                        {
                            sTablenames += sTablenames[x];
                            if (x < tablenames.Length - 1) sTablenames += ", ";
                        }

                        values["query"] = "DROP TABLE IF EXISTS " + sTablenames;


                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                        byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Send.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        if (responseString.ToLower().Trim() == "true") Result = true;

                        success = true;
                    }
                }
                catch (Exception ex) { message = ex.Message; }
            }

            if (!success) Logger.Log(attempts.ToString() + " Attempts :: " + message);

            return Result;

        }

        public static bool Truncate(MySQL_Configuration config, string tablename)
        {

            bool Result = false;

            int attempts = 0;
            bool success = false;
            string message = null;

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


                        values["query"] = "TRUNCATE TABLE " + tablename;


                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                        byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Send.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        if (responseString.ToLower().Trim() == "true") Result = true;

                        success = true;
                    }
                }
                catch (Exception ex) { message = ex.Message; }
            }

            if (!success) Logger.Log(attempts.ToString() + " Attempts :: " + message);

            return Result;

        }


        public static string[] List(MySQL_Configuration config)
        {

            List<string> Result = new List<string>();

            int attempts = 0;
            bool success = false;
            string message = null;

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

                        values["query"] = "SHOW TABLES";

                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                        Console.WriteLine("http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php");

                        byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        try
                        {

                            DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)));

                            if (DT != null)
                            {
                                foreach (DataRow Row in DT.Rows) Result.Add(Row[0].ToString());
                            }

                        }
                        catch (Exception ex) { }

                        success = true;
                    }
                }
                catch (Exception ex) { message = ex.Message; }
            }

            if (!success) Logger.Log(attempts.ToString() + " Attempts :: " + message);

            return Result.ToArray();

        }

        public static string[] List(MySQL_Configuration config, string filterExpression)
        {

            List<string> Result = new List<string>();

            int attempts = 0;
            bool success = false;
            string message = null;

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

                        values["query"] = "SHOW TABLES " + filterExpression;

                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                        Console.WriteLine("http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php");

                        byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        try
                        {

                            DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)));

                            if (DT != null)
                            {
                                foreach (DataRow Row in DT.Rows) Result.Add(Row[0].ToString());
                            }

                        }
                        catch (Exception ex) { }

                        success = true;
                    }
                }
                catch (Exception ex) { message = ex.Message; }
            }

            if (!success) Logger.Log(attempts.ToString() + " Attempts :: " + message);

            return Result.ToArray();

        }

        public static Int64 GetRowCount(MySQL_Configuration config, string tablename)
        {

            Int64 Result = -1;

            int attempts = 0;
            bool success = false;
            string message = null;

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

                        values["query"] = "SELECT COUNT(*) FROM " + tablename;

                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                        byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        JsonSerializerSettings JSS = new JsonSerializerSettings();
                        JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                        JSS.DateParseHandling = DateParseHandling.DateTime;
                        JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                        DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                        if (DT.Rows.Count > 0)
                        {
                            Int64 rowCount = -1;
                            Int64.TryParse(DT.Rows[0][0].ToString(), out rowCount);
                            if (rowCount >= 0) Result = rowCount;
                        }

                        success = true;
                    }
                }
                //catch (System.Net.WebException wex) { mesage = wex.Message); }
                catch (Exception ex) { message = ex.Message; }
            }

            if (!success) Logger.Log(attempts.ToString() + " Attempts :: " + message);

            return Result;

        }

        public static Int64 GetSize(MySQL_Configuration config, string tablename)
        {

            Int64 Result = -1;

            int attempts = 0;
            bool success = false;
            string message = null;

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

                        values["query"] = "SELECT data_length + index_length 'Total Size bytes' FROM information_schema.TABLES WHERE table_schema = '" + config.Database + "' AND table_name = '" + tablename + "'";

                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                        byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        JsonSerializerSettings JSS = new JsonSerializerSettings();
                        JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                        JSS.DateParseHandling = DateParseHandling.DateTime;
                        JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                        DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                        if (DT.Rows.Count > 0)
                        {
                            Int64 size = -1;
                            Int64.TryParse(DT.Rows[0][0].ToString(), out size);
                            if (size >= 0) Result = size;
                        }

                        success = true;
                    }
                }
                //catch (System.Net.WebException wex) { message = wex.Message); }
                catch (Exception ex) { message = ex.Message; }
            }

            if (!success) Logger.Log(attempts.ToString() + " Attempts :: " + message);

            return Result;

        }


        public static DataTable Get(MySQL_Configuration config, string tablename)
        {

            DataTable Result = null;

            int attempts = 0;
            bool success = false;
            string message = null;

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

                        values["query"] = "SELECT * FROM " + tablename;

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
                //catch (System.Net.WebException wex) { message = wex.Message); }
                catch (Exception ex) { message = ex.Message; }
            }

            if (!success) Logger.Log(attempts.ToString() + " Attempts :: " + message);

            return Result;

        }

        public static DataTable Get(MySQL_Configuration config, string tablename, string FilterExpression)
        {

            DataTable Result = null;

            int attempts = 0;
            bool success = false;
            string message = null;

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

                        values["query"] = "SELECT * FROM " + tablename + " " + FilterExpression;

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
                catch (Exception ex) { message = ex.Message; }
            }

            if (!success) Logger.Log(attempts.ToString() + " Attempts :: " + message);

            return Result;

        }

        public static DataTable Get(MySQL_Configuration config, string tablename, string FilterExpression, string Columns)
        {

            DataTable Result = null;

            int attempts = 0;
            bool success = false;
            string message = null;

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

                        values["query"] = "SELECT " + Columns + " FROM " + tablename + " " + FilterExpression;

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
                catch (Exception ex) { message = ex.Message; }
            }

            if (!success) Logger.Log(attempts.ToString() + " Attempts :: " + message);

            return Result;

        }

    }
}
