using System;

using MySql.Data.MySqlClient;

using TH_Global.TrakHound.Configurations;
using TH_Global;

namespace TH_MySQL.Connector
{
    public static class Database
    {

        public static int connectionAttempts = 3;

        public static bool Create(MySQL_Configuration config, string databaseName)
        {

            bool Result = false;

            int attempts = 0;
            bool success = false;

            while (attempts < connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    MySqlConnection conn;
                    conn = new MySqlConnection();

                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";";
                    conn.Open();

                    MySqlCommand Command;
                    Command = new MySqlCommand();
                    Command.Connection = conn;


                    Command.CommandText = "CREATE DATABASE IF NOT EXISTS " + databaseName;

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
                    Logger.Log(ex.Message, Logger.LogLineType.Error);
                }
                catch (Exception ex) { }
            }

            return Result;

        }

        public static bool Drop(MySQL_Configuration config, string databaseName)
        {

            bool Result = false;

            int attempts = 0;
            bool success = false;

            while (attempts < connectionAttempts && !success)
            {
                attempts += 1;

                try
                {
                    MySqlConnection conn;
                    conn = new MySqlConnection();

                    conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";";
                    conn.Open();

                    MySqlCommand Command;
                    Command = new MySqlCommand();
                    Command.Connection = conn;

                    Command.CommandText = "DROP DATABASE IF EXISTS " + databaseName;

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
                    Logger.Log(ex.Message, Logger.LogLineType.Error);
                }
                catch (Exception ex) { }
            }

            return Result;

        }

    }
}
