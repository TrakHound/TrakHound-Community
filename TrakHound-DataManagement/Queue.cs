using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SQLite;

using TrakHound.Tools;
using TrakHound.Databases;
using TrakHound.Plugins;
using TrakHound.Plugins.Database;
using TrakHound.Logging;

namespace TrakHound_DataManagement
{
    internal class DatabaseQueue
    {
        private System.Timers.Timer queueTimer;

        private Configuration configuration;
        private List<string> queue;

        public DatabaseQueue(Configuration config)
        {
            configuration = config;

            queue = new List<string>();

            queueTimer = new System.Timers.Timer();
            queueTimer.Interval = 5000;
            queueTimer.Elapsed += queue_TIMER_Elapsed;
            queueTimer.Enabled = true;
        }

        private void queue_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ProcessQueue();
        }


        public void AddToQueue(string query)
        {
            if (queue != null) queue.Add(query);
        }

        
        private void ProcessQueue()
        {
            if (queue != null && queue.Count > 0)
            {
                bool success = false;

                using (var connection = new SQLiteConnection(Connection.GetConnectionString(configuration)))
                {
                    connection.Close();

                    try
                    {
                        connection.BusyTimeout = 10000;
                        connection.Open();

                        foreach (string query in queue)
                        {
                            success = (bool)Query.Execute<bool>(connection, query, 1);
                            if (success) queue.Remove(query);
                        }
                    }
                    catch (ObjectDisposedException ex)
                    {
                        Logger.Log("ObjectDisposedException :: " + ex.Message, LogLineType.Error);
                        success = false;
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.Log("InvalidOperationException :: " + ex.Message, LogLineType.Error);
                        success = false;
                    }
                    catch (SQLiteException ex)
                    {
                        Logger.Log("SQLiteException :: " + ex.Message, LogLineType.Error);
                        success = false;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception :: " + ex.Message, LogLineType.Error);
                        success = false;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

    }
}
