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

        public static void Insert(SQL_Settings sql, string tableName, object[] columns, object[] values, bool update)
        {

            try
            {
                MySqlConnection conn;
                conn = new MySqlConnection();
                conn.ConnectionString = "server=" + sql.Server + ";user=" + sql.Username + ";port=" + sql.Port + ";password=" + sql.Password + ";database=" + sql.Database + ";";
                conn.Open();

                MySqlCommand Command;
                Command = new MySqlCommand();
                Command.Connection = conn;

                Command.CommandText = Global.Row_Insert_CreateQuery(tableName, columns, values, update);

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

        public static void Insert(SQL_Settings sql, string tableName, object[] columns, List<List<object>> values, bool update)
        {

            if (values.Count > 0)
            {
                try
                {
                    MySqlConnection conn;
                    conn = new MySqlConnection();
                    conn.ConnectionString = "server=" + sql.Server + ";user=" + sql.Username + ";port=" + sql.Port + ";password=" + sql.Password + ";database=" + sql.Database + ";";
                    conn.Open();

                    MySqlCommand Command;
                    Command = new MySqlCommand();
                    Command.Connection = conn;

                    Command.CommandText = Global.Row_Insert_CreateQuery(tableName, columns, values, update);

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

        public static void Insert(SQL_Settings sql, string tableName, List<object[]> columnsList, List<object[]> valuesList, bool update)
        {

            try
            {
                MySqlConnection conn;
                conn = new MySqlConnection();
                conn.ConnectionString = "server=" + sql.Server + ";user=" + sql.Username + ";port=" + sql.Port + ";password=" + sql.Password + ";database=" + sql.Database + ";";
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
                            if (Values[x].ToString().ToLower() != "null") vals += "'" + Values[x].ToString() + "'";
                            else vals += Values[x].ToString();
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
                                sUpdate += "'" + Values[x].ToString() + "'";

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
            }
            catch (MySqlException ex)
            {
                Logger.Log("Insert_Row : " + ex.Message);
            }

            catch (Exception ex) { }

        }

        public static bool Insert(SQL_Settings sql, string query)
        {

            bool Result = false;

            try
            {
                MySqlConnection conn;
                conn = new MySqlConnection();
                conn.ConnectionString = "server=" + sql.Server + ";user=" + sql.Username + ";port=" + sql.Port + ";password=" + sql.Password + ";database=" + sql.Database + ";";
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
            }
            catch (MySqlException ex)
            {
                Logger.Log("Insert_Row : " + ex.Message);
            }

            catch (Exception ex) { }

            return Result;

        }


        public static DataRow Get(SQL_Settings sql, string tablename, string tableKey, string rowKey)
        {

            DataRow Result = null;

            try
            {
                MySql.Data.MySqlClient.MySqlConnection conn;
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = "server=" + sql.Server + ";user=" + sql.Username + ";port=" + sql.Port + ";password=" + sql.Password + ";database=" + sql.Database + ";";
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
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }

        public static DataRow Get(SQL_Settings sql, string tablename, string query)
        {

            DataRow Result = null;

            try
            {
                MySql.Data.MySqlClient.MySqlConnection conn;
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = "server=" + sql.Server + ";user=" + sql.Username + ";port=" + sql.Port + ";password=" + sql.Password + ";database=" + sql.Database + ";";
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
            catch (MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }


        public static bool Exists(SQL_Settings sql, string tableName, string filterString)
        {

            bool Result = false;

            try
            {
                MySql.Data.MySqlClient.MySqlConnection conn;
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = "server=" + sql.Server + ";user=" + sql.Username + ";port=" + sql.Port + ";password=" + sql.Password + ";database=" + sql.Database + ";";
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
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                Logger.Log(ex.Message);
            }

            return Result;

        }

    }
}
