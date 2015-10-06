using System;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

using TH_Configuration;
using TH_Global;

namespace TH_MySQL.Connector
{
    public static class Column
    {

        public static List<string> Get(SQL_Settings sql, string tableName)
        {

            List<string> Result = new List<string>();

            try
            {
                MySql.Data.MySqlClient.MySqlConnection conn;
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = "server=" + sql.Server + ";user=" + sql.Username + ";port=" + sql.Port + ";password=" + sql.Password + ";database=" + sql.Database + ";";
                conn.Open();

                MySql.Data.MySqlClient.MySqlCommand Command;
                Command = new MySql.Data.MySqlClient.MySqlCommand();
                Command.Connection = conn;
                Command.CommandText = "SHOW COLUMNS FROM " + tableName;

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

        public static bool Add(SQL_Settings sql, string tableName, string columnDefinition)
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

                Command.CommandText = "ALTER IGNORE TABLE " + tableName + " ADD COLUMN " + columnDefinition;

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

    }
}
