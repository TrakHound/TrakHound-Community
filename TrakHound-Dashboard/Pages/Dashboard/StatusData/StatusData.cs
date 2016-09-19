// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using TrakHound.API;
using TrakHound.Plugins;

namespace TrakHound_Dashboard.Pages.Dashboard.StatusData
{
    public partial class StatusData
    {
        private const int INTERVAL_MIN = 500;
        private const int INTERVAL_MAX = 60000;

        private Thread updateThread;
        private ManualResetEvent updateStop;


        public StatusData()
        {
            Start();
        }

        private void Start()
        {
            Stop();

            updateStop = new ManualResetEvent(false);

            updateThread = new Thread(new ThreadStart(Update));
            updateThread.Start();
        }

        private void Stop()
        {
            if (updateStop != null) updateStop.Set();
        }

        private void Abort()
        {
            if (updateThread != null) updateThread.Abort();
        }

        private void Update()
        {
            int interval = Math.Max(INTERVAL_MIN, Properties.Settings.Default.DatabaseReadInterval);

            while (!updateStop.WaitOne(0, true))
            {                
                // Get Timestamp for current entire day
                var now = DateTime.Now;
                DateTime from = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Local);
                DateTime to = from.AddDays(1);

                // Get list of UniqueId's from Devices list
                var devices = Devices.ToList().Select(o => o.UniqueId).ToList();

                // Retrieve device Data from API
                List<Data.DeviceInfo> deviceInfos = null;
                if (UserConfiguration != null) deviceInfos = Data.Get(UserConfiguration, devices, from.ToUniversalTime(), to.ToUniversalTime(), 2000);
                else deviceInfos = Data.Get(null, devices, from, to, 2000);
                if (deviceInfos != null)
                {
                    foreach (var deviceInfo in deviceInfos)
                    {
                        foreach (var c in deviceInfo.Classes)
                        {
                            var data = new EventData();
                            data.Id = "STATUS_" + c.Key.ToUpper();
                            data.Data01 = deviceInfo.UniqueId;
                            data.Data02 = c.Value;
                            SendDataEvent(data);
                        }
                    }
                }

                // Put thread to sleep for specified interval (Data Read Interval
                Thread.Sleep(interval);
            }
        }

        private void SendDataEvent(EventData data)
        {
            if (data != null)
            {
                if (data.Id != null)
                {
                    SendData?.Invoke(data);
                }
            }
        }

    }
}
