using System;
using System.Data;
using System.Collections.Generic;
using MySql.Data.MySqlClient;


using TH_Configuration;
using TH_Global;

namespace TH_MySQL.Connector
{
    public static class Table
    {

        public static bool Create(MySQL_Configuration config, string tableName, object[] columnDefinitions, string primaryKey)
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

                    string coldef = "";

                    //Create Column Definition string
                    for (int x = 0; x <= columnDefinitions.Length - 1; x++)
                    {
                        coldef += columnDefinitions[x].ToString();
                        if (x < columnDefinitions.Length - 1) coldef += ",";
                    }

                    string Keydef = "";
                    if (primaryKey != null) Keydef = ", PRIMARY KEY (" + primaryKey.ToLower() + ")";

                    Command.CommandText = "CREATE TABLE IF NOT EXISTS " + tableName + " (" + coldef + Keydef + ")";

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
                    Logger.Log(ex.Message);
                }
                catch (Exception ex) { }
            }

            return Result;

        }

        public static bool Truncate(MySQL_Configuration config, string tableName)
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

                    Command.CommandText = "TRUNCATE TABLE " + tableName;

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
                    Logger.Log(ex.Message);
                }
                catch (Exception ex) { }
            }

            return Result;

        }

        public static bool Drop(MySQL_Configuration config, string tableName)
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

                    Command.CommandText = "DROP TABLE IF EXISTS " + tableName;

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
                    Logger.Log(ex.Message);
                }
                catch (Exception ex) { }
            }

            return Result;

        }

        public static bool Drop(MySQL_Configuration config, string[] tableNames)
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

                    string tablenames = "";
                    for (int x = 0; x <= tableNames.Length - 1; x++)
                    {
                        tablenames += tableNames[x];
                        if (x < tableNames.Length - 1) tablenames += ", ";
                    }

                    Command.CommandText = "DROP TABLE IF EXISTS " + tablenames;

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
                    Logger.Log(ex.Message);
                }
                catch (Exception ex) { }
            }

            return Result;

        }


        public static string[] List(MySQL_Configuration config)
        {

            List<string> Tables = new List<string>();

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

                    success = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    Logger.Log(ex.Message);
                }
                catch (Exception ex) { }
            }

            string[] Result;
            Result = Tables.ToArray();

            return Result;

        }

        public static string[] List(MySQL_Configuration config, string filterExpression)
        {

            List<string> Tables = new List<string>();

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
                    Command.CommandText = "SHOW TABLES " + filterExpression;

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

                    success = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    Logger.Log(ex.Message);
                }
                catch (Exception ex) { }
            }

            string[] Result;
            Result = Tables.ToArray();

            return Result;

        }

        public static Int64 RowCount(MySQL_Configuration config, string tableName)
        {

            Int64 Result = -1;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                    conn.Open();

                    string query = "SELECT COUNT(*) FROM " + tableName;
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

        public static Int64 Size(MySQL_Configuration config, string tableName)
        {

            Int64 Result = -1;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                    conn.Open();

                    string query = "SELECT data_length + index_length 'Total Size bytes' FROM information_schema.TABLES WHERE table_schema = '" + config.Database + "' AND table_name = '" + tableName + "'";

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


        public static DataTable Get(MySQL_Configuration config, string tableName)
        {

            DataTable Result = null;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                    conn.Open();

                    string query = "SELECT * FROM " + tableName;
                    MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);

                    DataTable t1 = new DataTable();
                    using (MySql.Data.MySqlClient.MySqlDataAdapter a = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd))
                    {
                        if (a != null) a.Fill(t1);
                    }

                    conn.Close();

                    cmd.Dispose();
                    conn.Dispose();

                    t1.TableName = tableName;

                    Result = t1.Copy();

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

        public static DataTable Get(MySQL_Configuration config, string tableName, string filterExpression)
        {

            DataTable Result = null;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                    conn.Open();

                    string query = "SELECT * FROM " + tableName + " " + filterExpression;

                    MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);

                    DataTable t1 = new DataTable();
                    using (MySql.Data.MySqlClient.MySqlDataAdapter a = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd))
                    {
                        a.Fill(t1);
                    }

                    conn.Close();

                    cmd.Dispose();
                    conn.Dispose();

                    t1.TableName = tableName;

                    Result = t1.Copy();

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

        public static DataTable Get(MySQL_Configuration config, string tableName, string filterExpression, string columns)
        {

            DataTable Result = null;

            int attempts = 0;
            bool success = false;

            while (attempts < Database.connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                    conn.Open();

                    string query = "SELECT " + columns + " FROM " + tableName + " " + filterExpression;

                    MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);

                    DataTable t1 = new DataTable();
                    using (MySql.Data.MySqlClient.MySqlDataAdapter a = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd))
                    {
                        a.Fill(t1);
                    }

                    conn.Close();

                    cmd.Dispose();
                    conn.Dispose();

                    t1.TableName = tableName;

                    Result = t1.Copy();

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
