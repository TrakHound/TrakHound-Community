using System;
using System.Collections.Generic;
using System.Data;

using MySql.Data.MySqlClient;

using TH_Configuration;
using TH_Global;

namespace TH_MySQL.Connector
{
    public static class Row
    {

        public static bool Insert(MySQL_Configuration config, string tableName, object[] columns, object[] values, bool update)
        {
            bool result = false;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    MySqlConnection conn;
                    conn = new MySqlConnection();
                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                    conn.Open();

                    MySqlCommand Command;
                    Command = new MySqlCommand();
                    Command.Connection = conn;

                    Command.CommandText = MySQL_Tools.Row_Insert_CreateQuery(tableName, columns, values, update);

                    Command.Prepare();
                    Command.ExecuteNonQuery();

                    Command.Dispose();

                    conn.Close();

                    Command.Dispose();
                    conn.Dispose();

                    result = true;

                    success = true;
                }
                catch (MySqlException ex)
                {
                    Logger.Log("Insert_Row : " + ex.Message);
                }

                catch (Exception ex) { }
            }

            return result;
        }

        public static bool Insert(MySQL_Configuration config, string tableName, object[] columns, List<List<object>> values, bool update)
        {
            bool result = false;

            int attempts = 0;
            bool success = false;

            if (values.Count > 0)
            {
                while (attempts < Database.connectionAttempts && !success)
                {
                    attempts += 1;

                    try
                    {
                        MySqlConnection conn;
                        conn = new MySqlConnection();
                        conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                        conn.Open();

                        MySqlCommand Command;
                        Command = new MySqlCommand();
                        Command.Connection = conn;

                        Command.CommandText = MySQL_Tools.Row_Insert_CreateQuery(tableName, columns, values, update);

                        Command.Prepare();
                        Command.ExecuteNonQuery();

                        Command.Dispose();

                        conn.Close();

                        Command.Dispose();
                        conn.Dispose();

                        result = true;

                        success = true;
                    }
                    catch (MySqlException ex)
                    {
                        Logger.Log("Insert_Row : " + ex.Message);
                    }

                    catch (Exception ex) { }
                }
            }

            return result;
        }

        public static bool Insert(MySQL_Configuration config, string tableName, List<object[]> columnsList, List<object[]> valuesList, bool update)
        {
            bool result = false;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;
                try
                {
                    MySqlConnection conn;
                    conn = new MySqlConnection();
                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                    conn.Open();

                    MySqlCommand Command;
                    Command = new MySqlCommand();
                    Command.Connection = conn;

                    for (int i = 0; i <= columnsList.Count - 1; i++)
                    {
                        object[] Columns = columnsList[i];
                        object[] Values = valuesList[i];

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
                                object val = Values[x];
                                if (val.GetType() == typeof(DateTime)) val = MySQL_Tools.ConvertDateStringtoMySQL(val.ToString());

                                if (val.ToString().ToLower() != "null") vals += "'" + val.ToString() + "'";
                                else vals += val.ToString();
                            }


                            if (x < Values.Length - 1) vals += ", ";
                        }

                        //Create Update string
                        string sUpdate = "";
                        if (update)
                        {
                            sUpdate = " ON DUPLICATE KEY UPDATE ";
                            for (int x = 0; x <= Columns.Length - 1; x++)
                            {
                                if (Values[x] != null)
                                {
                                    sUpdate += Columns[x].ToString().ToUpper();
                                    sUpdate += "=";

                                    object val = Values[x];
                                    if (val.GetType() == typeof(DateTime)) val = MySQL_Tools.ConvertDateStringtoMySQL(val.ToString());

                                    sUpdate += "'" + val.ToString() + "'";

                                    if (x < Columns.Length - 1) sUpdate += ", ";
                                }
                            }
                        }

                        Command.CommandText = "INSERT IGNORE INTO " + tableName + " (" + cols + ") VALUES (" + vals + ")" + sUpdate;

                        Command.Prepare();
                        Command.ExecuteNonQuery();
                    }

                    Command.Dispose();

                    conn.Close();

                    Command.Dispose();
                    conn.Dispose();

                    result = true;

                    success = true;
                }
                catch (MySqlException ex)
                {
                    Logger.Log("Insert_Row : " + ex.Message);
                }

                catch (Exception ex) { }
            }

            return result;
        }

        public static bool Insert(MySQL_Configuration config, string query)
        {

            bool Result = false;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    MySqlConnection conn;
                    conn = new MySqlConnection();
                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                    conn.Open();

                    MySqlCommand Command;
                    Command = new MySqlCommand();
                    Command.Connection = conn;

                    Command.CommandText = query;

                    Command.Prepare();
                    Command.ExecuteNonQuery();

                    Command.Dispose();

                    conn.Close();

                    Command.Dispose();
                    conn.Dispose();

                    Result = true;

                    success = true;
                }
                catch (MySqlException ex)
                {
                    Logger.Log("Insert_Row : " + ex.Message);
                }

                catch (Exception ex) { }
            }

            return Result;

        }


        public static DataRow Get(MySQL_Configuration config, string tablename, string tableKey, string rowKey)
        {

            DataRow Result = null;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    MySql.Data.MySqlClient.MySqlConnection conn;
                    conn = new MySql.Data.MySqlClient.MySqlConnection();
                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                    conn.Open();

                    MySql.Data.MySqlClient.MySqlCommand Command;
                    Command = new MySql.Data.MySqlClient.MySqlCommand();
                    Command.Connection = conn;
                    Command.CommandText = "SELECT * FROM " + tablename + " WHERE " + tableKey + " = '" + rowKey + "'";

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

                    success = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    Logger.Log(ex.Message);
                }
                catch (Exception ex) { }
            }

            return Result;

        }

        public static DataRow Get(MySQL_Configuration config, string tablename, string query)
        {

            DataRow Result = null;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    MySql.Data.MySqlClient.MySqlConnection conn;
                    conn = new MySql.Data.MySqlClient.MySqlConnection();
                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
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

                    success = true;
                }
                catch (MySqlException ex)
                {
                    Logger.Log(ex.Message);
                }
                catch (Exception ex) { }
            }

            return Result;

        }


        public static bool Exists(MySQL_Configuration config, string tableName, string filterString)
        {

            bool Result = false;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    MySql.Data.MySqlClient.MySqlConnection conn;
                    conn = new MySql.Data.MySqlClient.MySqlConnection();
                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                    conn.Open();

                    MySql.Data.MySqlClient.MySqlCommand Command;
                    Command = new MySql.Data.MySqlClient.MySqlCommand();
                    Command.Connection = conn;
                    Command.CommandText = "SELECT IF( EXISTS(SELECT * FROM " + tableName + " " + filterString + "), 1, 0)";

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

                    success = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    Logger.Log(ex.Message);
                }
                catch (Exception ex) { }
            }

            return Result;

        }

    }
}
