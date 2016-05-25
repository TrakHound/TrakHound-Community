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
    public static class Column
    {

        public static List<string> Get(MySQL_Configuration config, string tablename)
        {

            List<string> Result = new List<string>();

            NameValueCollection values = new NameValueCollection();
            if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
            else values["server"] = config.Server;

            values["user"] = config.Username;
            values["password"] = config.Password;
            values["db"] = config.Database;

            values["query"] = "SHOW COLUMNS FROM " + tablename;

            string PHP_Directory = "";
            if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

            string url = "http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php";


            string responseString = HTTP.POST(url, values);

            DataTable dt = JSON.ToTable(responseString);
            if (dt != null) foreach (DataRow Row in dt.Rows) Result.Add(Row[0].ToString()); 

            return Result;

        }

        public static bool Add(MySQL_Configuration config, string tablename, string ColumnDefinition)
        {

            bool Result = false;

            NameValueCollection values = new NameValueCollection();
            if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
            else values["server"] = config.Server;

            values["user"] = config.Username;
            values["password"] = config.Password;
            values["db"] = config.Database;

            values["query"] = "ALTER IGNORE TABLE " + tablename + " ADD COLUMN " + ColumnDefinition;

            string PHP_Directory = "";
            if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;


            string url = "http://" + config.PHP_Server + PHP_Directory + "/Send.php";

            if (HTTP.POST(url, values) == "true") Result = true;

            return Result;

        }

    }
}
