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
    public static class Column
    {

        public static List<string> Get(MySQL_Configuration config, string tablename)
        {

            List<string> Result = new List<string>();

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

                        values["query"] = "SHOW COLUMNS FROM " + tablename;

                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

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
                catch (Exception ex) { Logger.Log(ex.Message); }
            }

            return Result;

        }

        public static bool Add(MySQL_Configuration config, string tablename, string ColumnDefinition)
        {

            bool Result = false;

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

                        values["query"] = "ALTER IGNORE TABLE " + tablename + " ADD COLUMN " + ColumnDefinition;

                        string PHP_Directory = "";
                        if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                        byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Send.php", values);

                        string responseString = Encoding.Default.GetString(response);

                        if (responseString.ToLower().Trim() == "true") Result = true;

                        success = true;
                    }
                }
                catch (Exception ex) { Logger.Log(ex.Message); }
            }

            return Result;

        }

    }
}
