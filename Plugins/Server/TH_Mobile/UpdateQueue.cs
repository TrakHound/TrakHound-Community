// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;

namespace TH_Mobile
{
    public class UpdateQueue
    {
        private static List<UpdateData> updateDatas = new List<UpdateData>();

        private System.Timers.Timer queueTimer;


        public UpdateQueue()
        {
            Start();
        }

        public void Add(UpdateData updateData)
        {
            int index = updateDatas.FindIndex(x => x.UniqueId == updateData.UniqueId);
            if (index >= 0)
            {
                var data = updateDatas[index];

                // Set Description Info
                if (data.Description.Equals(updateData.Description))
                {
                    data.Description = updateData.Description;
                    data.Description.Changed = true;
                }

                // Set Status Info
                if (data.Status.Equals(updateData.Status))
                {
                    data.Status = updateData.Status;
                    data.Status.Changed = true;
                }

                // Set Controller Info
                if (data.Controller.Equals(updateData.Controller))
                {
                    data.Controller = updateData.Controller;
                    data.Controller.Changed = true;
                }

                // Set Oee Info
                if (data.Oee.Equals(updateData.Oee))
                {
                    data.Oee = updateData.Oee;
                    data.Oee.Changed = true;
                }

                // Set Timers Info
                if (data.Timers.Equals(updateData.Timers))
                {
                    data.Timers = updateData.Timers;
                    data.Timers.Changed = true;
                }

                updateDatas[index] = data;
            }
            else updateDatas.Add(updateData);
        }

        private void Start()
        {
            if (queueTimer != null) queueTimer.Enabled = false;

            queueTimer = new System.Timers.Timer();
            queueTimer.Interval = 2000;
            queueTimer.Elapsed += QueueTimer_Elapsed;
            queueTimer.Enabled = true;
        }

        private void Stop()
        {
            if (queueTimer != null) queueTimer.Enabled = false;
            queueTimer = null;
        }


        private void QueueTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            if (updateDatas.Count > 0)
            {
                Database.Update(updateDatas.ToList());

                foreach (var updateData in updateDatas)
                {
                    updateData.Reset();
                }
            }
        }

    }
}
