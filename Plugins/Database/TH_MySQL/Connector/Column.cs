using System;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

using TH_Global.TrakHound.Configurations;
using TH_Global;

namespace TH_MySQL.Connector
{
    public static class Column
    {

        public static List<string> Get(MySQL_Configuration config, string tableName)
        {

            List<string> Result = new List<string>();

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
                    Command.CommandText = "SHOW COLUMNS FROM " + tableName;

                    MySql.Data.MySqlClient.MySqlDataReader Reader = Command.ExecuteReader();
                    if (Reader.HasRows)
                    {
                        while (Reader.Read())
                        {
                            string line = Reader[0].ToString();
                            Result.Add(line);
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
                    Logger.Log("MySqlException :: " + ex.Message, Logger.LogLineType.Error);
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception :: " + ex.Message, Logger.LogLineType.Error);
                }
            }

            return Result;

        }

        public static bool Add(MySQL_Configuration config, string tableName, string columnDefinition)
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

                    Command.CommandText = "ALTER IGNORE TABLE " + tableName + " ADD COLUMN " + columnDefinition;

                    Command.Prepare();
                    Command.ExecuteNonQuery();

                    Command.Dispose();

                    conn.Close();

                    Command.Dispose();
                    conn.Dispose();

                    Result = true;

                    success = true;
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    Logger.Log("MySqlException :: " + ex.Message, Logger.LogLineType.Error);
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception :: " + ex.Message, Logger.LogLineType.Error);
                }
            }

            return Result;

        }

    }
}
