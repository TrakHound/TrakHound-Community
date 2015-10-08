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
    public static class Database
    {

        public static bool Create(MySQL_Configuration config, string databaseName)
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
                    values["query"] = "CREATE DATABASE IF NOT EXISTS " + databaseName;

                    string PHP_Directory = "";
                    if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Create_Database.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        public static bool Drop(MySQL_Configuration config, string databaseName)
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
                    values["query"] = "DROP DATABASE IF EXISTS " + databaseName;

                    string PHP_Directory = "";
                    if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + config.PHP_Server + PHP_Directory + "/Create_Database.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

    }
}
