using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Data;
using System.Data.SQLite;

using TrakHound.Tools;
using TrakHound.Databases;
using TrakHound.Plugins;
using TrakHound.Plugins.Database;
using TrakHound.Logging;
using TrakHound.Configurations;

namespace TrakHound_DataManagement
{
    public static class Query
    {
        public static object Execute<T>(SQLiteConnection connection, string query, int attempts = 3, int failureDelay = 500)
        {
            object result = null;

            int attempt = 0;
            bool success = false;

            while (!success && attempt < attempts)
            {
                //using (var connection = new SQLiteConnection(GetConnectionString(config)))
                //{
                //    connection.Close();

                    try
                    {
                        //connection.BusyTimeout = 10000;
                        //connection.Open();

                        using (var command = new SQLiteCommand(query, connection))
                        {
                            result = ProcessResult<T>(command);
                        }

                        success = true;
                    }
                    catch (ObjectDisposedException ex)
                    {
                        Logger.Log("ObjectDisposedException :: " + ex.Message, LogLineType.Error);
                        if (typeof(T) == typeof(bool)) result = false;
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.Log("InvalidOperationException :: " + ex.Message, LogLineType.Error);
                        if (typeof(T) == typeof(bool)) result = false;
                    }
                    catch (SQLiteException ex)
                    {
                        Logger.Log("SQLiteException :: " + ex.Message, LogLineType.Error);
                        if (typeof(T) == typeof(bool)) result = false;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception :: " + ex.Message, LogLineType.Error);
                        if (typeof(T) == typeof(bool)) result = false;
                    }
                    finally
                    {
                        connection.Close();
                    }
                //}

                if (!success) Thread.Sleep(failureDelay);

                attempt += 1;
            }

            return result;
        }

        public static object Execute<T>(Configuration config, string query, int attempts = 3, int failureDelay = 500)
        {
            object result = null;

            int attempt = 0;
            bool success = false;

            while (!success && attempt < attempts)
            {
                using (var connection = new SQLiteConnection(Connection.GetConnectionString(config)))
                {
                    connection.Close();

                    try
                    {
                        connection.BusyTimeout = 10000;
                        connection.Open();

                        using (var command = new SQLiteCommand(query, connection))
                        {
                            result = ProcessResult<T>(command);
                        }

                        success = true;
                    }
                    catch (ObjectDisposedException ex)
                    {
                        Logger.Log("ObjectDisposedException :: " + ex.Message, LogLineType.Error);
                        if (typeof(T) == typeof(bool)) result = false;
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.Log("InvalidOperationException :: " + ex.Message, LogLineType.Error);
                        if (typeof(T) == typeof(bool)) result = false;
                    }
                    catch (SQLiteException ex)
                    {
                        Logger.Log("SQLiteException :: " + ex.Message, LogLineType.Error);
                        if (typeof(T) == typeof(bool)) result = false;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception :: " + ex.Message, LogLineType.Error);
                        if (typeof(T) == typeof(bool)) result = false;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }

                if (!success) Thread.Sleep(failureDelay);

                attempt += 1;
            }

            return result;
        }

        private static object ProcessResult<T>(SQLiteCommand command)
        {
            object result = null;

            // Object
            if (typeof(T) == typeof(object))
            {
                using (command)
                {
                    result = command.ExecuteScalar();
                }
            }

            // Boolean
            if (typeof(T) == typeof(bool))
            {
                using (command)
                {
                    object o = command.ExecuteNonQuery();
                    return true;
                    //var val = (int)(-1);
                    //if (o != null) int.TryParse(o.ToString(), out val);

                    //if (val >= 0) result = true;
                    //else result = false;
                }
            }

            // int
            if (typeof(T) == typeof(int))
            {
                using (command)
                {
                    object o = command.ExecuteScalar();
                    var val = (int)(-1);
                    if (o != null) int.TryParse(o.ToString(), out val);
                    result = val;
                }
            }

            // Int64
            if (typeof(T) == typeof(Int64))
            {
                using (command)
                {
                    object o = command.ExecuteScalar();
                    Int64 val = -1;
                    if (o != null) Int64.TryParse(o.ToString(), out val);
                    result = val;
                }
            }

            // string
            if (typeof(T) == typeof(string))
            {
                using (command)
                {
                    object o = command.ExecuteScalar();
                    if (o != null) result = String_Functions.FromSpecial(o.ToString());
                }
            }

            // DataTable
            if (typeof(T) == typeof(DataTable))
            {
                using (command)
                {
                    var dt = new DataTable();
                    using (var a = new SQLiteDataAdapter(command))
                    {
                        a.Fill(dt);
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        ConvertRowFromSafe(row);
                    }
                    result = dt;
                }
            }

            // DataRow
            if (typeof(T) == typeof(DataRow))
            {
                using (command)
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            int columnCount = reader.FieldCount;

                            DataTable temp_dt = new DataTable();

                            for (int x = 0; x <= columnCount - 1; x++)
                            {
                                temp_dt.Columns.Add(reader.GetName(x));
                            }

                            DataRow row = temp_dt.NewRow();

                            object[] values = new object[columnCount];

                            while (reader.Read())
                            {
                                for (int j = 0; j <= values.Length - 1; j++)
                                {
                                    values[j] = reader[j];
                                }
                            }

                            row.ItemArray = values;

                            ConvertRowFromSafe(row);

                            result = row;
                        }
                    }
                }
            }

            // string[]
            if (typeof(T) == typeof(string[]))
            {
                using (command)
                {
                    var tables = new List<string>();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read()) tables.Add(String_Functions.FromSpecial(reader[0].ToString()));
                        }
                    }
                    result = tables.ToArray();
                }
            }

            // List<string>
            if (typeof(T) == typeof(List<string>))
            {
                using (command)
                {
                    var tables = new List<string>();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read()) tables.Add(String_Functions.FromSpecial(reader[0].ToString()));
                        }
                    }
                    result = tables;
                }
            }

            return result;
        }

        private static void ConvertRowFromSafe(DataRow row)
        {
            for (var x = 0; x <= row.ItemArray.Length - 1; x++)
            {
                if (row.Table.Columns[x].DataType == typeof(string))
                {
                    row[x] = String_Functions.FromSpecial(row.ItemArray[x].ToString());
                }
            }
        }

        private const string dateString = "yyyy-MM-dd HH:mm:ss";

        public static string ConvertToDateTime(string s)
        {
            string result = "null";

            DateTime TS;
            if (DateTime.TryParse(s, out TS)) result = TS.ToString(dateString);

            return result;
        }

    }
}
