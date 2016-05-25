using System;
using System.Collections.Generic;
using System.Net;
using System.Collections.Specialized;
using System.Data;
using System.Text;

using TH_Global;
using TH_Global.Web;

namespace TH_MySQL.PHP
{
    public static class Database
    {

        public static int connectionAttempts = 3;

        public static bool Create(MySQL_Configuration config, string databaseName)
        {

            bool Result = false;

            NameValueCollection values = new NameValueCollection();
            if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
            else values["server"] = config.Server;

            values["user"] = config.Username;
            values["password"] = config.Password;
            values["query"] = "CREATE DATABASE IF NOT EXISTS " + databaseName;

            string PHP_Directory = "";
            if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;


            string url = "http://" + config.PHP_Server + PHP_Directory + "/Create_Database.php";

            if (HTTP.POST(url, values) == "true") Result = true;

            return Result;

        }

        public static bool Drop(MySQL_Configuration config, string databaseName)
        {

            bool Result = false;

            NameValueCollection values = new NameValueCollection();
            if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
            else values["server"] = config.Server;

            values["user"] = config.Username;
            values["password"] = config.Password;
            values["query"] = "DROP DATABASE IF EXISTS " + databaseName;

            string PHP_Directory = "";
            if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

            string url = "http://" + config.PHP_Server + PHP_Directory + "/Create_Database.php";

            if (HTTP.POST(url, values) == "true") Result = true;

            return Result;

        }

    }
}
