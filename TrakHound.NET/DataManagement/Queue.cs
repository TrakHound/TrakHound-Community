// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

using TrakHound.Logging;
using TrakHound.Tools;

namespace TrakHound.DataManagement
{
    public class DatabaseQueue
    {
        private System.Timers.Timer queueTimer;

        private List<QueueItem> queue;

        public DatabaseQueue()
        {
            queue = new List<QueueItem>();

            queueTimer = new System.Timers.Timer();
            queueTimer.Interval = 5000;
            queueTimer.Elapsed += queue_TIMER_Elapsed;
            queueTimer.Enabled = true;
        }

        private void queue_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ProcessQueue();
        }

        private class QueueItem
        {
            public QueueItem(string query)
            {
                Id = Guid.NewGuid().ToString();
                Query = query;
            }

            public string Id { get; set; }
            public string Query { get; set; }
        }


        public void AddToQueue(string query)
        {
            if (queue != null) queue.Add(new QueueItem(query));
        }

        
        private void ProcessQueue()
        {
            var q = queue.ToList();

            if (q != null && q.Count > 0)
            {
                bool success = false;

                using (var connection = new SQLiteConnection(Connection.GetConnectionString(Database.Configuration)))
                {
                    connection.Close();

                    try
                    {
                        connection.BusyTimeout = 10000;
                        connection.Open();

                        string query = "";

                        foreach (QueueItem item in q)
                        {
                            //Console.WriteLine(item.Query);

                            query += item.Query + ";";
                        }

                        Console.WriteLine("Query Length = " + String_Functions.FileSizeSuffix(query.Length));

                        success = (bool)Query.Execute<bool>(connection, query, 2);
                        if (success)
                        {
                            foreach (QueueItem item in q)
                            {
                                int i = queue.FindIndex(x => x.Id == item.Id);
                                if (i >= 0) queue.RemoveAt(i);
                            }
                        }
                        else Console.WriteLine("Error during ProcessQueue");

                        //foreach (QueueItem item in q)
                        //{
                        //    if (connection.State != System.Data.ConnectionState.Open) connection.Open();

                        //    success = (bool)Query.Execute<bool>(connection, item.Query, 1);
                        //    if (success)
                        //    {
                        //        int i = queue.FindIndex(x => x.Id == item.Id);
                        //        if (i >= 0) queue.RemoveAt(i);
                        //    }
                        //}
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
