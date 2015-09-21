using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TH_MySQL
{
    /// <summary>
    /// Queue used for data that must be verified such as the Instance Table. Creates a queue
    /// to hold the queries and verifys each were sent properly, if not it keeps it in the queue to try again.
    /// </summary>
    public class Queue
    {
        public TH_Configuration.SQL_Settings SQL;

        public Queue()
        {
            queue = new List<string>();

            queue_TIMER = new System.Timers.Timer();
            queue_TIMER.Interval = 1000;
            queue_TIMER.Elapsed += queue_TIMER_Elapsed;
            queue_TIMER.Enabled = true;

            this.QueryRemoved += Queue_QueryRemoved;
        }

        void Queue_QueryRemoved(int count)
        {
            int count_adj = Math.Min(count, queue.Count);
            queue.RemoveRange(0, count_adj);
        }

        void queue_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (thread != null)
            {
                if (thread.ThreadState != ThreadState.Running) Start();
            }
            else Start();
        }

        List<string> queue;

        public void Add(string query)
        {
            queue.Add(query);
        }

        Thread thread;

        void Start()
        {
            // Create copy of main Queue List to use in new thread
            List<string> queue_copy = new List<string>(queue);

            thread = new Thread(new ParameterizedThreadStart(Work));
            thread.Start(queue_copy);
        }

        System.Timers.Timer queue_TIMER;

        void Work(object queue_copy)
        {
            // Cast back to List<string>
            List<string> queue_work = (List<string>)queue_copy;

            // count of succesful queries
            int successfulCount = 0;

            // Loop through queue and continue until finished or a query fails
            for (int x = 0; x <= queue_work.Count - 1; x++)
            {
                bool success = Global.Row_Insert(SQL, queue_work[x]);
                
                // Update count of successful queries
                if (success) successfulCount += 1;
                // exit loop if query is unsuccessful
                else break;
            }

            // Remove all of the Queries that were successful from the main Queue List
            if (QueryRemoved != null && successfulCount > 0) QueryRemoved(successfulCount);
        }

        delegate void QueryRemoved_Handler(int index);
        event QueryRemoved_Handler QueryRemoved;

    }
}
