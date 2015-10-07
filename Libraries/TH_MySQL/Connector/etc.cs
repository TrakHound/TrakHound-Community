using System;
using System.Collections.Generic;
using System.Data;

using MySql.Data.MySqlClient;

using TH_Configuration;
using TH_Global;

namespace TH_MySQL.Connector
{
    public static class etc
    {

        public static object[] CustomCommand(MySQL_Configuration config, string commandText)
        {

            object[] Result = null;

            try
            {
                MySql.Data.MySqlClient.MySqlConnection conn;
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                conn.Open();

                MySql.Data.MySqlClient.MySqlCommand Command;
                Command = new MySql.Data.MySqlClient.MySqlCommand();
                Command.Connection = conn;
                Command.CommandText = commandText;

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

        public static object GetValue(MySQL_Configuration config, string tableName, string column, string filterExpression)
        {

            object Result = null;

            try
            {
                MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                conn.Open();

                string query = "SELECT " + column + " FROM " + tableName + " " + filterExpression;


                MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand(query, conn);

                DataTable t1 = new DataTable();
                using (MySql.Data.MySqlClient.MySqlDataAdapter a = new MySql.Data.MySqlClient.MySqlDataAdapter(cmd))
                {
                    if (a != null) a.Fill(t1);
                }

                if (t1.Rows.Count > 0) Result = t1.Rows[0][column];

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

        public static DataTable GetGrants(MySQL_Configuration config, string username)
        {

            DataTable Result = null;

            try
            {
                MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection();

                conn.ConnectionString = "server=" + config.Server + ";user=" + config.Username + ";port=" + config.Port + ";password=" + config.Password + ";database=" + config.Database + ";";
                conn.Open();

                string query = "SHOW GRANTS FOR '" + username + "'@'%'";


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
