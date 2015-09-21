// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Collections.Specialized;
using System.Data;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TH_Configuration;
using TH_Global;

namespace TH_MySQL
{
    public static class PHP
    {

        #region "Databases"

        public static bool Database_Create(SQL_Settings SQL_S, string DatabaseName)
        {

            bool Result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["query"] = "CREATE DATABASE IF NOT EXISTS " + DatabaseName;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Create_Database.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        public static bool Database_Drop(SQL_Settings SQL_S, string DatabaseName)
        {

            bool Result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["query"] = "DROP DATABASE IF EXISTS " + DatabaseName;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Create_Database.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        #endregion

        #region "Tables"

        public static bool Table_Create(SQL_Settings SQL_S, string TableName, object[] ColumnDefinitions, string PrimaryKey)
        {

            bool Result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    string coldef = "";

                    //Create Column Definition string
                    for (int x = 0; x <= ColumnDefinitions.Length - 1; x++)
                    {
                        coldef += ColumnDefinitions[x].ToString();
                        if (x < ColumnDefinitions.Length - 1) coldef += ",";
                    }

                    string Keydef = "";
                    if (PrimaryKey != null) Keydef = ", PRIMARY KEY (" + PrimaryKey.ToLower() + ")";

                    values["query"] = "CREATE TABLE IF NOT EXISTS " + TableName + " (" + coldef + Keydef + ")";


                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Send.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        public static bool Table_Drop(SQL_Settings SQL_S, string TableName)
        {

            bool Result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;


                    values["query"] = "DROP TABLE IF EXISTS " + TableName;


                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Send.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        public static bool Table_Drop(SQL_Settings SQL_S, string[] TableNames)
        {

            bool Result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    string tablenames = "";
                    for (int x = 0; x <= TableNames.Length - 1; x++)
                    {
                        tablenames += TableNames[x];
                        if (x < TableNames.Length - 1) tablenames += ", ";
                    }

                    values["query"] = "DROP TABLE IF EXISTS " + tablenames;


                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Send.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        public static bool Table_Truncate(SQL_Settings SQL_S, string TableName)
        {

            bool Result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;


                    values["query"] = "TRUNCATE TABLE " + TableName;


                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Send.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }


        public static string[] Table_List(SQL_Settings SQL_S)
        {

            List<string> Result = new List<string>();

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "SHOW TABLES";

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    Console.WriteLine("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php");

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

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

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result.ToArray();

        }

        public static Int64 Table_RowCount(SQL_Settings SQL_S, string TableName)
        {

            Int64 Result = -1;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "SELECT COUNT(*) FROM " + TableName;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

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

                }
            }
            catch (System.Net.WebException wex) { Logger.Log(wex.Message); }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        public static Int64 Table_Size(SQL_Settings SQL_S, string TableName)
        {

            Int64 Result = -1;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "SELECT data_length + index_length 'Total Size bytes' FROM information_schema.TABLES WHERE table_schema = '" + SQL_S.Database + "' AND table_name = '" + TableName + "'";

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

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

                }
            }
            catch (System.Net.WebException wex) { Logger.Log(wex.Message); }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }


        public static DataTable Table_Get(SQL_Settings SQL_S, string TableName)
        {

            DataTable Result = null;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "SELECT * FROM " + TableName;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    JsonSerializerSettings JSS = new JsonSerializerSettings();
                    JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    JSS.DateParseHandling = DateParseHandling.DateTime;
                    JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                    DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                    Result = DT;

                }
            }
            catch (System.Net.WebException wex) { Logger.Log(wex.Message); }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        public static DataTable Table_Get(SQL_Settings SQL_S, string TableName, string FilterExpression)
        {

            DataTable Result = null;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "SELECT * FROM " + TableName + " " + FilterExpression;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    JsonSerializerSettings JSS = new JsonSerializerSettings();
                    JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    JSS.DateParseHandling = DateParseHandling.DateTime;
                    JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                    DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                    Result = DT;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        public static DataTable Table_Get(SQL_Settings SQL_S, string TableName, string FilterExpression, string Columns)
        {

            DataTable Result = null;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "SELECT " + Columns + " FROM " + TableName + " " + FilterExpression;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    JsonSerializerSettings JSS = new JsonSerializerSettings();
                    JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    JSS.DateParseHandling = DateParseHandling.DateTime;
                    JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                    DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                    Result = DT;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }


        public static List<string> Table_GetColumns(SQL_Settings SQL_S, string TableName)
        {

            List<string> Result = new List<string>();

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "SHOW COLUMNS FROM " + TableName;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

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

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        public static bool Table_AddColumn(SQL_Settings SQL_S, string TableName, string ColumnDefinition)
        {

            bool Result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "ALTER IGNORE TABLE " + TableName + " ADD COLUMN " + ColumnDefinition;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Send.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        #endregion

        #region "Rows"

        public static bool Row_Insert(SQL_Settings SQL_S, string TableName, object[] Columns, object[] Values, bool Update)
        {

            bool Result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;


                    //Create Columns string
                    string cols = "";
                    for (int x = 0; x <= Columns.Length - 1; x++)
                    {
                        cols += Columns[x].ToString().ToUpper();
                        if (x < Columns.Length - 1) cols += ", ";
                    }

                    //Create Values string
                    string vals = "";
                    for (int x = 0; x <= Values.Length - 1; x++)
                    {
                        // Dont put the ' characters if the value is null
                        if (Values[x] == null) vals += "null";
                        else
                        {
                            if (Values[x].ToString().ToLower() != "null") vals += "'" + Values[x].ToString() + "'";
                            else vals += Values[x].ToString();
                        }


                        if (x < Values.Length - 1) vals += ", ";
                    }

                    //Create Update string
                    string update = "";
                    if (Update)
                    {
                        update = " ON DUPLICATE KEY UPDATE ";
                        for (int x = 0; x <= Columns.Length - 1; x++)
                        {
                            if (Values[x] != null)
                            {

                                update += Columns[x].ToString().ToUpper();
                                update += "=";
                                update += "'" + Values[x].ToString() + "'";

                                if (x < Columns.Length - 1) update += ", ";

                            }
                        }
                    }

                    values["query"] = "INSERT IGNORE INTO " + TableName + " (" + cols + ") VALUES (" + vals + ")" + update;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Send.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        // Bulk Row Insert (uses seperate thread to process data for query and runs query Async)
        public static bool Row_Insert(SQL_Settings SQL_S, string TableName, object[] Columns, List<List<object>> Values, bool Update)
        {

            bool Result = true;

            try
            {
                InsertParameters iparams = new InsertParameters();
                iparams.sql = SQL_S;
                iparams.tableName = TableName;
                iparams.columns = Columns;
                iparams.values = Values;
                iparams.update = Update;


                Thread worker = new Thread(new ParameterizedThreadStart(Row_Insert_Worker));
                worker.Start(iparams);
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        class InsertParameters
        {
            public SQL_Settings sql { get; set; }
            public string tableName { get; set; }
            public object[] columns { get; set; }
            public List<List<object>> values { get; set; }
            public bool update { get; set; }
        }

        static void Row_Insert_Worker(object insertParameters)
        {
            // Cast object parameter to InsertParameters object
            InsertParameters iparams = (InsertParameters)insertParameters;

            // Set variables to keep same format as other SQL methods
            SQL_Settings SQL_S = iparams.sql;
            string TableName = iparams.tableName;
            object[] Columns = iparams.columns;
            List<List<object>> Values = iparams.values;
            bool Update = iparams.update;


            using (WebClient client = new WebClient())
            {

                System.Diagnostics.Stopwatch stpw = new System.Diagnostics.Stopwatch();
                stpw.Start();

                NameValueCollection values = new NameValueCollection();
                if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                else values["server"] = SQL_S.Server;

                values["user"] = SQL_S.Username;
                values["password"] = SQL_S.Password;
                values["db"] = SQL_S.Database;


                //Create Columns string
                string cols = "";
                for (int x = 0; x <= Columns.Length - 1; x++)
                {
                    cols += Columns[x].ToString().ToUpper();
                    if (x < Columns.Length - 1) cols += ", ";
                }

                //Create Values string
                string vals = "VALUES ";
                for (int i = 0; i <= Values.Count - 1; i++)
                {
                    vals += "(";

                    for (int x = 0; x <= Values[i].Count - 1; x++)
                    {

                        List<object> ValueSet = Values[i];

                        // Dont put the ' characters if the value is null
                        if (ValueSet[x] == null) vals += "null";
                        else
                        {
                            if (ValueSet[x].ToString().ToLower() != "null") vals += "'" + ValueSet[x].ToString() + "'";
                            else vals += ValueSet[x].ToString();
                        }


                        if (x < ValueSet.Count - 1) vals += ", ";
                    }

                    vals += ")";

                    if (i < Values.Count - 1) vals += ",";

                }

                //Create Update string
                string update = "";
                if (Update)
                {
                    update = " ON DUPLICATE KEY UPDATE ";
                    for (int x = 0; x <= Columns.Length - 1; x++)
                    {
                        update += Columns[x].ToString().ToUpper();
                        update += "=";
                        update += "VALUES(" + Columns[x].ToString().ToUpper() + ")";

                        if (x < Columns.Length - 1) update += ", ";
                    }
                }

                values["query"] = "INSERT IGNORE INTO " + TableName + " (" + cols + ") " + vals + update;

                //values["query"] = TH_MySQL.Global.Row_Insert_CreateQuery(TableName, Columns, Values, Update);

                string PHP_Directory = "";
                if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                stpw = new System.Diagnostics.Stopwatch();
                stpw.Start();

                client.UploadValuesAsync(new Uri("http://" + SQL_S.PHP_Server + PHP_Directory + "/Send.php"), values);

            }
        }

        public static bool Row_Insert(SQL_Settings SQL_S, string Query)
        {

            bool Result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = Query;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Send.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    if (responseString.ToLower().Trim() == "true") Result = true;

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        public static DataRow Row_Get(SQL_Settings SQL_S, string TableName, string TableKey, string RowKey)
        {

            DataRow Result = null;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "SELECT * FROM " + TableName + " WHERE " + TableKey + " = '" + RowKey + "'";

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

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

        public static DataRow Row_Get(SQL_Settings SQL_S, string TableName, string Query)
        {

            DataRow Result = null;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "SELECT * FROM " + TableName + " " + Query;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

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


        public static bool Row_Exists(SQL_Settings SQL_S, string TableName, string FilterString)
        {

            bool Result = false;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "SELECT IF( EXISTS(SELECT * FROM " + TableName + " " + FilterString + "), 1, 0)";

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

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

        #endregion


        public static object[] CustomCommand(SQL_Settings SQL_S, string CommandText)
        {

            object[] Result = null;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = CommandText;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                    string responseString = Encoding.Default.GetString(response);

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

        public static object Value_Get(SQL_Settings SQL_S, string TableName, string Column, string FilterExpression)
        {

            object Result = null;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "SELECT " + Column + " FROM " + TableName + " " + FilterExpression;

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    JsonSerializerSettings JSS = new JsonSerializerSettings();
                    JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    JSS.DateParseHandling = DateParseHandling.DateTime;
                    JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                    DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                    if (DT.Rows.Count > 0) Result = DT.Rows[0][Column];

                }
            }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }


        public static DataTable GetGrants(SQL_Settings SQL_S, string UserName)
        {

            DataTable Result = null;

            try
            {
                using (WebClient client = new WebClient())
                {

                    NameValueCollection values = new NameValueCollection();
                    if (SQL_S.Port > 0) values["server"] = SQL_S.Server + ":" + SQL_S.Port;
                    else values["server"] = SQL_S.Server;

                    values["server"] = SQL_S.Server;
                    values["user"] = SQL_S.Username;
                    values["password"] = SQL_S.Password;
                    values["db"] = SQL_S.Database;

                    values["query"] = "SHOW GRANTS FOR '" + UserName + "'@'%'";

                    string PHP_Directory = "";
                    if (SQL_S.PHP_Directory != "") PHP_Directory = "/" + SQL_S.PHP_Directory;

                    byte[] response = client.UploadValues("http://" + SQL_S.PHP_Server + PHP_Directory + "/Retrieve.php", values);

                    string responseString = Encoding.Default.GetString(response);

                    JsonSerializerSettings JSS = new JsonSerializerSettings();
                    JSS.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    JSS.DateParseHandling = DateParseHandling.DateTime;
                    JSS.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

                    DataTable DT = (DataTable)JsonConvert.DeserializeObject(responseString, (typeof(DataTable)), JSS);

                    Result = DT;

                }
            }
            catch (JsonReaderException JRE) { Logger.Log(JRE.Message); }
            catch (Exception ex) { Logger.Log(ex.Message); }

            return Result;

        }

    }

}
