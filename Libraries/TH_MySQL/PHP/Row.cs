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
    public static class Row
    {

        public static bool Insert(MySQL_Configuration config, string tablename, object[] Columns, object[] Values, bool Update)
        {

            bool Result = false;

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

                    values["query"] = MySQL_Tools.Row_Insert_CreateQuery(tablename, Columns, Values, Update);

                    string PHP_Directory = "";
                    if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Send.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        // Bulk Row Insert (uses seperate thread to process data for query and runs query Async)
        public static bool Insert(MySQL_Configuration config, string tablename, object[] Columns, List<List<object>> Values, bool Update)
        {

            bool Result = true;

            try
            {
                InsertParameters iparams = new InsertParameters();
                iparams.sql = config;
                iparams.tablename = tablename;
                iparams.columns = Columns;
                iparams.values = Values;
                iparams.update = Update;


                Thread worker = new Thread(new ParameterizedThreadStart(Insert_Worker));
                worker.Start(iparams);
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        class InsertParameters
        {
            public MySQL_Configuration sql { get; set; }
            public string tablename { get; set; }
            public object[] columns { get; set; }
            public List<List<object>> values { get; set; }
            public bool update { get; set; }
        }

        static void Insert_Worker(object insertParameters)
        {
            // Cast object parameter to InsertParameters object
            InsertParameters iparams = (InsertParameters)insertParameters;

            // Set variables to keep same format as other SQL methods
            MySQL_Configuration config = iparams.sql;
            string tablename = iparams.tablename;
            object[] Columns = iparams.columns;
            List<List<object>> Values = iparams.values;
            bool Update = iparams.update;


            using (WebClient client = new WebClient())
            {

                System.Diagnostics.Stopwatch stpw = new System.Diagnostics.Stopwatch();
                stpw.Start();

                NameValueCollection values = new NameValueCollection();
                if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
                else values["server"] = config.Server;

                values["user"] = config.Username;
                values["password"] = config.Password;
                values["db"] = config.Database;

                values["query"] = MySQL_Tools.Row_Insert_CreateQuery(tablename, Columns, Values, Update);

                string PHP_Directory = "";
                if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                stpw = new System.Diagnostics.Stopwatch();
                stpw.Start();

                client.UploadValuesAsync(new Uri("http://" + config.PHP_Server + PHP_Directory + "/Send.php"), values);

            }
        }

        public static bool Insert(MySQL_Configuration config, string Query)
        {

            bool Result = false;

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

                    values["query"] = Query;

                    string PHP_Directory = "";
                    if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Send.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }


        public static DataRow Get(MySQL_Configuration config, string tablename, string TableKey, string RowKey)
        {

            DataRow Result = null;

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

                    values["query"] = "SELECT * FROM " + tablename + " WHERE " + TableKey + " = '" + RowKey + "'";

                    string PHP_Directory = "";
                    if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php", values);

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

        public static DataRow Get(MySQL_Configuration config, string tablename, string Query)
        {

            DataRow Result = null;

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

                    values["query"] = "SELECT * FROM " + tablename + " " + Query;

                    string PHP_Directory = "";
                    if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    JsonSerializerSettings JSS = new JsonSerializerSettings();
                    JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    JSS.DateParseHandling = DateParseHandling.DateTime;
                    JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                    DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                    if (DT.Rows.Count > 0) Result = DT.Rows[0];

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }


        public static bool Exists(MySQL_Configuration config, string tablename, string FilterString)
        {

            bool Result = false;

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

                    values["query"] = "SELECT IF( EXISTS(SELECT * FROM " + tablename + " " + FilterString + "), 1, 0)";

                    string PHP_Directory = "";
                    if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)));

                    if (DT.Rows.Count > 0)
                    {
                        if (DT.Rows[0][0].ToString() == "1") Result = true;
                    }

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

    }
}
