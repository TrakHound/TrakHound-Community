using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.IO;

using System.Threading;

using TH_Global;
using TH_Global.Web;

namespace TH_MySQL.PHP
{
    public static class Row
    {

        const bool Verbose = false;

        public static bool Insert(MySQL_Configuration config, string tablename, object[] Columns, object[] Values, bool Update)
        {

            bool Result = false;

            NameValueCollection values = new NameValueCollection();
            if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
            else values["server"] = config.Server;

            values["user"] = config.Username;
            values["password"] = config.Password;
            values["db"] = config.Database;

            values["query"] = MySQL_Tools.Row_Insert_CreateQuery(tablename, Columns, Values, Update);

            string PHP_Directory = "";
            if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

            string url = "http://" + config.PHP_Server + PHP_Directory + "/Send.php";

            if (HTTP.SendData(url, values) == "true") Result = true;

            return Result;

        }

        public static bool Insert(MySQL_Configuration config, string tablename, object[] Columns, List<List<object>> Values, bool Update)
        {

            bool Result = false;

            NameValueCollection values = new NameValueCollection();
            if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
            else values["server"] = config.Server;

            values["user"] = config.Username;
            values["password"] = config.Password;
            values["db"] = config.Database;

            string query = MySQL_Tools.Row_Insert_CreateQuery(tablename, Columns, Values, Update);

            values["query"] = query;

            string PHP_Directory = "";
            if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

            string url = "http://" + config.PHP_Server + PHP_Directory + "/Send.php";

            if (HTTP.SendData(url, values) == "true") Result = true;

            return Result;

        }




        #region "Bulk Insert"

        // Bulk Row Insert (uses seperate thread to process data for query and runs query Async)
        //public static bool Insert(MySQL_Configuration config, string tablename, object[] Columns, List<List<object>> Values, bool Update)
        //{

        //    bool Result = true;

        //        try
        //        {
        //            InsertParameters iparams = new InsertParameters();
        //            iparams.sql = config;
        //            iparams.tablename = tablename;
        //            iparams.columns = Columns;
        //            iparams.values = Values;
        //            iparams.update = Update;

        //            Thread worker = new Thread(new ParameterizedThreadStart(Insert_Worker));
        //            worker.Start(iparams);
        //        }
        //        catch (Exception ex) { if (Verbose) Logger.Log(ex.Message); }
            
        //    return Result;

        //}

        //class InsertParameters
        //{
        //    public MySQL_Configuration sql { get; set; }
        //    public string tablename { get; set; }
        //    public object[] columns { get; set; }
        //    public List<List<object>> values { get; set; }
        //    public bool update { get; set; }
        //}

        //static void Insert_Worker(object insertParameters)
        //{
        //    // Cast object parameter to InsertParameters object
        //    InsertParameters iparams = (InsertParameters)insertParameters;

        //    // Set variables to keep same format as other SQL methods
        //    MySQL_Configuration config = iparams.sql;
        //    string tablename = iparams.tablename;
        //    object[] Columns = iparams.columns;
        //    List<List<object>> Values = iparams.values;
        //    bool Update = iparams.update;


        //    NameValueCollection values = new NameValueCollection();
        //    if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
        //    else values["server"] = config.Server;

        //    values["user"] = config.Username;
        //    values["password"] = config.Password;
        //    values["db"] = config.Database;

        //    string query = MySQL_Tools.Row_Insert_CreateQuery(tablename, Columns, Values, Update);

        //    values["query"] = query;

        //    string PHP_Directory = "";
        //    if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

        //    string url = "http://" + config.PHP_Server + PHP_Directory + "/Send.php";

        //    HTTP.SendData(url, values);
        //}

        #endregion

        public static bool Insert(MySQL_Configuration config, string Query)
        {

            bool Result = false;

            NameValueCollection values = new NameValueCollection();
            if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
            else values["server"] = config.Server;

            values["user"] = config.Username;
            values["password"] = config.Password;
            values["db"] = config.Database;

            values["query"] = Query;

            string PHP_Directory = "";
            if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

            string url = "http://" + config.PHP_Server + PHP_Directory + "/Send.php";

            if (HTTP.SendData(url, values) == "true") Result = true;

            return Result;

        }


        public static DataRow Get(MySQL_Configuration config, string tablename, string TableKey, string RowKey)
        {

            DataRow Result = null;

            NameValueCollection values = new NameValueCollection();
            if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
            else values["server"] = config.Server;

            values["user"] = config.Username;
            values["password"] = config.Password;
            values["db"] = config.Database;

            values["query"] = "SELECT * FROM " + tablename + " WHERE " + TableKey + " = '" + RowKey + "'";

            string PHP_Directory = "";
            if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

            string url = "http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php";


            string responseString = HTTP.SendData(url, values);

            DataTable dt = JSON.ToTable(responseString);
            if (dt != null) if (dt.Rows.Count > 0) Result = dt.Rows[0];

            return Result;

        }

        public static DataRow Get(MySQL_Configuration config, string tablename, string Query)
        {

            DataRow Result = null;

            NameValueCollection values = new NameValueCollection();
            if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
            else values["server"] = config.Server;

            values["user"] = config.Username;
            values["password"] = config.Password;
            values["db"] = config.Database;

            values["query"] = "SELECT * FROM " + tablename + " " + Query;

            string PHP_Directory = "";
            if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

            string url = "http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php";


            string responseString = HTTP.SendData(url, values);

            DataTable dt = JSON.ToTable(responseString);
            if (dt != null) if (dt.Rows.Count > 0) Result = dt.Rows[0];

            return Result;

        }


        public static bool Exists(MySQL_Configuration config, string tablename, string FilterString)
        {

            bool Result = false;

            NameValueCollection values = new NameValueCollection();
            if (config.Port > 0) values["server"] = config.Server + ":" + config.Port;
            else values["server"] = config.Server;

            values["user"] = config.Username;
            values["password"] = config.Password;
            values["db"] = config.Database;

            values["query"] = "SELECT IF( EXISTS(SELECT * FROM " + tablename + " " + FilterString + "), 1, 0)";

            string PHP_Directory = "";
            if (config.PHP_Directory != "") PHP_Directory = "/" + config.PHP_Directory;

            string url = "http://" + config.PHP_Server + PHP_Directory + "/Retrieve.php";


            string responseString = HTTP.SendData(url, values);

            DataTable dt = JSON.ToTable(responseString);
            if (dt != null) if (dt.Rows.Count > 0) if (dt.Rows[0][0].ToString() == "1") Result = true;


            return Result;

        }

    }
}
