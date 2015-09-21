// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using System.Data;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

using TH_Configuration;
using TH_Global;

namespace TH_MySQL
{
    public static class MySQL
    {

        #region "Databases"

        public static bool Database_Create(SQL_Settings SQL, string DatabaseName)
        {

            bool Result = false;

            try
            {

                MySqlConnection conn;
                conn = new MySqlConnection();

                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";";
                conn.Open();

                MySqlCommand Command;
                Command = new MySqlCommand();
                Command.Connection = conn;


                Command.CommandText = "CREATE DATABASE IF NOT EXISTS " + DatabaseName;

                Command.Prepare();
                Command.ExecuteNonQuery();

                Command.Dispose();

                conn.Close();

                Command.Dispose();
                conn.Dispose();

                Result = true;

            }
            catch (MySqlException ex)
            {

                Logger.Log(ex.Message);

            }
            catch (Exception ex) { }

            return Result;

        }

        public static bool Database_Drop(SQL_Settings SQL, string DatabaseName)
        {

            bool Result = false;

            try
            {

                MySqlConnection conn;
                conn = new MySqlConnection();

                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";";
                conn.Open();

                MySqlCommand Command;
                Command = new MySqlCommand();
                Command.Connection = conn;

                Command.CommandText = "DROP DATABASE IF EXISTS " + DatabaseName;

                Command.Prepare();
                Command.ExecuteNonQuery();

                Command.Dispose();

                conn.Close();

                Command.Dispose();
                conn.Dispose();

                Result = true;

            }
            catch (MySqlException ex)
            {

                Logger.Log(ex.Message);

            }
            catch (Exception ex) { }

            return Result;

        }

        #endregion

        #region "Tables"

        public static bool Table_Create(SQL_Settings SQL, string TableName, object[] ColumnDefinitions, string PrimaryKey)
        {

            bool Result = false;

            try
            {

                MySqlConnection conn;
                conn = new MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySqlCommand Command;
                Command = new MySqlCommand();
                Command.Connection = conn;

                string coldef = "";

                //Create Column Definition string
                for (int x = 0; x <= ColumnDefinitions.Length - 1; x++)
                {

                    coldef += ColumnDefinitions[x].ToString();
                    if (x < ColumnDefinitions.Length - 1) coldef += ",";

                }

                string Keydef = "";
                if (PrimaryKey != null) Keydef = ", PRIMARY KEY (" + PrimaryKey.ToLower() + ")";

                Command.CommandText = "CREATE TABLE IF NOT EXISTS " + TableName + " (" + coldef + Keydef + ")";

                Command.Prepare();
                Command.ExecuteNonQuery();

                Command.Dispose();

                conn.Close();

                Command.Dispose();
                conn.Dispose();

                Result = true;

            }
            catch (MySqlException ex)
            {
                Logger.Log(ex.Message);
            }
            catch (Exception ex) { }

            return Result;

        }

        public static bool Table_Truncate(SQL_Settings SQL, string TableName)
        {

            bool Result = false;

            try
            {

                MySqlConnection conn;
                conn = new MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySqlCommand Command;
                Command = new MySqlCommand();
                Command.Connection = conn;

                Command.CommandText = "TRUNCATE TABLE " + TableName;

                Command.Prepare();
                Command.ExecuteNonQuery();

                Command.Dispose();

                conn.Close();

                Command.Dispose();
                conn.Dispose();

                Result = true;

            }
            catch (MySqlException ex)
            {

                Logger.Log(ex.Message);

            }
            catch (Exception ex) { }

            return Result;

        }

        public static bool Table_Drop(SQL_Settings SQL, string TableName)
        {

            bool Result = false;

            try
            {

                MySqlConnection conn;
                conn = new MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySqlCommand Command;
                Command = new MySqlCommand();
                Command.Connection = conn;

                Command.CommandText = "DROP TABLE IF EXISTS " + TableName;

                Command.Prepare();
                Command.ExecuteNonQuery();

                Command.Dispose();

                conn.Close();

                Command.Dispose();
                conn.Dispose();

                Result = true;

            }
            catch (MySqlException ex)
            {

                Logger.Log(ex.Message);

            }
            catch (Exception ex) { }

            return Result;

        }

        public static bool Table_Drop(SQL_Settings SQL, string[] TableNames)
        {

            bool Result = false;

            try
            {

                MySqlConnection conn;
                conn = new MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySqlCommand Command;
                Command = new MySqlCommand();
                Command.Connection = conn;

                string tablenames = "";
                for (int x = 0; x <= TableNames.Length - 1; x++)
                {
                    tablenames += TableNames[x];
                    if (x < TableNames.Length - 1) tablenames += ", ";
                }

                Command.CommandText = "DROP TABLE IF EXISTS " + tablenames;

                Command.Prepare();
                Command.ExecuteNonQuery();

                Command.Dispose();

                conn.Close();

                Command.Dispose();
                conn.Dispose();

                Result = true;

            }
            catch (MySqlException ex)
            {

                Logger.Log(ex.Message);

            }
            catch (Exception ex) { }

            return Result;

        }


        public static string[] Table_List(SQL_Settings SQL)
        {

            List<string> Tables = new List<string>();

            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn;
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySql.Data.MySqlClient.MySqlCommand Command;
                Command = new MySql.Data.MySqlClient.MySqlCommand();
                Command.Connection = conn;
                Command.CommandText = "SHOW TABLES";

                MySql.Data.MySqlClient.MySqlDataReader Reader = Command.ExecuteReader();
                if (Reader.HasRows)
                {
                    while (Reader.Read()) Tables.Add(Reader[0].ToString());
                }

                Reader.Close();
                conn.Close();

                Reader.Dispose();
                Command.Dispose();
                conn.Dispose();

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            string[] Result;
            Result = Tables.ToArray();

            return Result;

        }

        public static Int64 Table_RowCount(SQL_Settings SQL, string TableName)
        {

            Int64 Result = -1;

            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                string query = "SELECT COUNT(*) FROM " + TableName;
                MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);

                DataTable t1 = new DataTable();
                using (MySql.Data.MySqlClient.MySqlDataAdapter a = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd))
                {
                    if (a != null) a.Fill(t1);
                }

                conn.Close();

                cmd.Dispose();
                conn.Dispose();

                if (t1.Rows.Count > 0)
                {
                    Int64 rowCount = -1;
                    Int64.TryParse(t1.Rows[0][0].ToString(), out rowCount);
                    if (rowCount >= 0) Result = rowCount;
                }

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }

        public static Int64 Table_Size(SQL_Settings SQL, string TableName)
        {

            Int64 Result = -1;

            try
            {
                MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                string query = "SELECT data_length + index_length 'Total Size bytes' FROM information_schema.TABLES WHERE table_schema = '" + SQL.Database + "' AND table_name = '" + TableName + "'";

                MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);

                DataTable t1 = new DataTable();
                using (MySql.Data.MySqlClient.MySqlDataAdapter a = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd))
                {
                    if (a != null) a.Fill(t1);
                }

                conn.Close();

                cmd.Dispose();
                conn.Dispose();

                if (t1.Rows.Count > 0)
                {
                    Int64 size = -1;
                    Int64.TryParse(t1.Rows[0][0].ToString(), out size);
                    if (size >= 0) Result = size;
                }

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }


        public static DataTable Table_Get(SQL_Settings SQL, string TableName)
        {

            DataTable Result = null;

            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                string query = "SELECT * FROM " + TableName;
                MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);

                DataTable t1 = new DataTable();
                using (MySql.Data.MySqlClient.MySqlDataAdapter a = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd))
                {
                    if (a != null) a.Fill(t1);
                }

                conn.Close();

                cmd.Dispose();
                conn.Dispose();

                t1.TableName = TableName;

                Result = t1.Copy();

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }

        public static DataTable Table_Get(SQL_Settings SQL, string TableName, string FilterExpression)
        {

            DataTable Result = null;

            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                string query = "SELECT * FROM " + TableName + " " + FilterExpression;

                MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);

                DataTable t1 = new DataTable();
                using (MySql.Data.MySqlClient.MySqlDataAdapter a = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd))
                {
                    a.Fill(t1);
                }

                conn.Close();

                cmd.Dispose();
                conn.Dispose();

                t1.TableName = TableName;

                Result = t1.Copy();

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }

        public static DataTable Table_Get(SQL_Settings SQL, string TableName, string FilterExpression, string Columns)
        {

            DataTable Result = null;

            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                string query = "SELECT " + Columns + " FROM " + TableName + " " + FilterExpression;

                MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);

                DataTable t1 = new DataTable();
                using (MySql.Data.MySqlClient.MySqlDataAdapter a = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd))
                {
                    a.Fill(t1);
                }

                conn.Close();

                cmd.Dispose();
                conn.Dispose();

                t1.TableName = TableName;

                Result = t1.Copy();

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }


        public static List<string> Table_GetColumns(SQL_Settings SQL, string TableName)
        {

            List<string> Result = new List<string>();

            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn;
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySql.Data.MySqlClient.MySqlCommand Command;
                Command = new MySql.Data.MySqlClient.MySqlCommand();
                Command.Connection = conn;
                Command.CommandText = "SHOW COLUMNS FROM " + TableName;

                MySql.Data.MySqlClient.MySqlDataReader Reader = Command.ExecuteReader();
                if (Reader.HasRows)
                {
                    while (Reader.Read()) Result.Add(Reader[0].ToString());
                }

                Reader.Close();
                conn.Close();

                Reader.Dispose();
                Command.Dispose();
                conn.Dispose();

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }

        public static bool Table_AddColumn(SQL_Settings SQL, string TableName, string ColumnDefinition)
        {

            bool Result = false;

            try
            {

                MySqlConnection conn;
                conn = new MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySqlCommand Command;
                Command = new MySqlCommand();
                Command.Connection = conn;

                Command.CommandText = "ALTER IGNORE TABLE " + TableName + " ADD COLUMN " + ColumnDefinition;

                Command.Prepare();
                Command.ExecuteNonQuery();

                Command.Dispose();

                conn.Close();

                Command.Dispose();
                conn.Dispose();

                Result = true;

            }
            catch (MySqlException ex) { }

            catch (Exception ex) { }

            return Result;

        }

        #endregion

        #region "Rows"

        public static void Row_Insert(SQL_Settings SQL, string TableName, object[] Columns, object[] Values, bool Update)
        {

            try
            {

                MySqlConnection conn;
                conn = new MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySqlCommand Command;
                Command = new MySqlCommand();
                Command.Connection = conn;

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


                Command.CommandText = "INSERT IGNORE INTO " + TableName + " (" + cols + ") VALUES (" + vals + ")" + update;

                //Command.CommandText = TH_MySQL.Global.Row_Insert_CreateQuery(TableName, Columns, Values, Update);

                Command.Prepare();
                Command.ExecuteNonQuery();

                Command.Dispose();

                conn.Close();

                Command.Dispose();
                conn.Dispose();

            }
            catch (MySqlException ex)
            {
                Logger.Log("Insert_Row : " + ex.Message);
            }

            catch (Exception ex) { }

        }

        public static void Row_Insert(SQL_Settings SQL, string TableName, object[] Columns, List<List<object>> Values, bool Update)
        {

            if (Values.Count > 0)
            {

                try
                {

                    MySqlConnection conn;
                    conn = new MySqlConnection();
                    conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                    conn.Open();

                    MySqlCommand Command;
                    Command = new MySqlCommand();
                    Command.Connection = conn;

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


                    Command.CommandText = "INSERT IGNORE INTO " + TableName + " (" + cols + ") " + vals + update;

                    Command.Prepare();
                    Command.ExecuteNonQuery();

                    Command.Dispose();

                    conn.Close();

                    Command.Dispose();
                    conn.Dispose();

                }
                catch (MySqlException ex)
                {
                    Logger.Log("Insert_Row : " + ex.Message);
                }

                catch (Exception ex) { }

            }

        }

        public static void Row_Insert(SQL_Settings SQL, string TableName, List<object[]> ColumnsList, List<object[]> ValuesList, bool Update)
        {

            try
            {

                MySqlConnection conn;
                conn = new MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySqlCommand Command;
                Command = new MySqlCommand();
                Command.Connection = conn;

                for (int i = 0; i <= ColumnsList.Count - 1; i++)
                {

                    object[] Columns = ColumnsList[i];
                    object[] Values = ValuesList[i];

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

                    Command.CommandText = "INSERT IGNORE INTO " + TableName + " (" + cols + ") VALUES (" + vals + ")" + update;

                    Command.Prepare();
                    Command.ExecuteNonQuery();

                }

                Command.Dispose();

                conn.Close();

                Command.Dispose();
                conn.Dispose();

            }
            catch (MySqlException ex)
            {
                Logger.Log("Insert_Row : " + ex.Message);
            }

            catch (Exception ex) { }

        }

        public static bool Row_Insert(SQL_Settings SQL, string Query)
        {

            bool Result = false;

            try
            {

                MySqlConnection conn;
                conn = new MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySqlCommand Command;
                Command = new MySqlCommand();
                Command.Connection = conn;

                Command.CommandText = Query;

                Command.Prepare();
                Command.ExecuteNonQuery();

                Command.Dispose();

                conn.Close();

                Command.Dispose();
                conn.Dispose();

                Result = true;

            }
            catch (MySqlException ex)
            {
                Logger.Log("Insert_Row : " + ex.Message);
            }

            catch (Exception ex) { }

            return Result;

        }


        public static DataRow Row_Get(SQL_Settings SQL, string tablename, string TableKey, string RowKey)
        {

            DataRow Result = null;

            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn;
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySql.Data.MySqlClient.MySqlCommand Command;
                Command = new MySql.Data.MySqlClient.MySqlCommand();
                Command.Connection = conn;
                Command.CommandText = "SELECT * FROM " + tablename + " WHERE " + TableKey + " = '" + RowKey + "'";

                MySql.Data.MySqlClient.MySqlDataReader Reader = Command.ExecuteReader();

                if (Reader.HasRows)
                {

                    int ColumnCount = Reader.FieldCount;

                    DataTable Dummy_TABLE = new DataTable();

                    for (int x = 0; x <= ColumnCount - 1; x++)
                    {
                        Dummy_TABLE.Columns.Add(Reader.GetName(x));
                    }

                    DataRow Row = Dummy_TABLE.NewRow();

                    object[] Values = new object[ColumnCount];

                    while (Reader.Read())
                    {
                        for (int j = 0; j <= Values.Length - 1; j++)
                        {
                            Values[j] = Reader[j];
                        }
                    }

                    Row.ItemArray = Values;

                    Result = Row;

                }

                Reader.Close();
                conn.Close();

                Reader.Dispose();
                Command.Dispose();
                conn.Dispose();


            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }

        public static DataRow Row_Get(SQL_Settings SQL, string tablename, string query)
        {

            DataRow Result = null;

            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn;
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySql.Data.MySqlClient.MySqlCommand Command;
                Command = new MySql.Data.MySqlClient.MySqlCommand();
                Command.Connection = conn;
                Command.CommandText = "SELECT * FROM " + tablename + " " + query;

                MySql.Data.MySqlClient.MySqlDataReader Reader = Command.ExecuteReader();
                if (Reader.HasRows)
                {

                    int ColumnCount = Reader.FieldCount;

                    DataTable Dummy_TABLE = new DataTable();

                    for (int x = 0; x <= ColumnCount - 1; x++)
                    {
                        Dummy_TABLE.Columns.Add(Reader.GetName(x));
                    }

                    DataRow Row = Dummy_TABLE.NewRow();

                    object[] Values = new object[ColumnCount];

                    while (Reader.Read())
                    {
                        for (int j = 0; j <= Values.Length - 1; j++)
                        {
                            Values[j] = Reader[j];
                        }
                    }

                    Row.ItemArray = Values;

                    Result = Row;

                }

                Reader.Close();
                conn.Close();

                Reader.Dispose();
                Command.Dispose();
                conn.Dispose();


            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }


        public static bool Row_Exists(SQL_Settings SQL, string TableName, string FilterString)
        {

            bool Result = false;

            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn;
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySql.Data.MySqlClient.MySqlCommand Command;
                Command = new MySql.Data.MySqlClient.MySqlCommand();
                Command.Connection = conn;
                Command.CommandText = "SELECT IF( EXISTS(SELECT * FROM " + TableName + " " + FilterString + "), 1, 0)";

                MySql.Data.MySqlClient.MySqlDataReader Reader = Command.ExecuteReader();
                if (Reader.HasRows)
                {

                    int i = 0;

                    while (Reader.Read())
                    {
                        Result = Reader.GetBoolean(i);
                        i += 1;
                    }
                }

                Reader.Close();
                conn.Close();

                Reader.Dispose();
                Command.Dispose();
                conn.Dispose();

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }

        #endregion


        public static object[] CustomCommand(SQL_Settings SQL, string CommandText)
        {

            object[] Result = null;

            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn;
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                MySql.Data.MySqlClient.MySqlCommand Command;
                Command = new MySql.Data.MySqlClient.MySqlCommand();
                Command.Connection = conn;
                Command.CommandText = CommandText;

                MySql.Data.MySqlClient.MySqlDataReader Reader = Command.ExecuteReader();
                if (Reader.HasRows)
                {

                    List<object> Columns = new List<object>();

                    while (Reader.Read()) Columns.Add(Reader[0].ToString());

                    Result = Columns.ToArray();

                }

                Reader.Close();
                conn.Close();

                Reader.Dispose();
                Command.Dispose();
                conn.Dispose();

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;


        }

        public static object Value_Get(SQL_Settings SQL, string TableName, string Column, string FilterExpression)
        {

            object Result = null;

            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                string query = "SELECT " + Column + " FROM " + TableName + " " + FilterExpression;


                MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);

                DataTable t1 = new DataTable();
                using (MySql.Data.MySqlClient.MySqlDataAdapter a = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd))
                {
                    if (a != null) a.Fill(t1);
                }

                if (t1.Rows.Count > 0) Result = t1.Rows[0][Column];

                conn.Close();

                cmd.Dispose();
                conn.Dispose();

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }


        public static DataTable GetGrants(SQL_Settings SQL, string UserName)
        {

            DataTable Result = null;

            try
            {

                MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                conn.ConnectionString = "server=" + SQL.Server + ";user=" + SQL.Username + ";port=" + SQL.Port + ";password=" + SQL.Password + ";database=" + SQL.Database + ";";
                conn.Open();

                string query = "SHOW GRANTS FOR '" + UserName + "'@'%'";


                MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);

                DataTable t1 = new DataTable();
                using (MySql.Data.MySqlClient.MySqlDataAdapter a = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd))
                {
                    if (a != null) a.Fill(t1);
                }

                conn.Close();

                cmd.Dispose();
                conn.Dispose();

                Result = t1.Copy();

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }

    }

}
