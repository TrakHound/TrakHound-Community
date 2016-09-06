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

        public StatusData()
        {
            Start();
        }

        private const int INTERVAL_MIN = 500;
        private const int INTERVAL_MAX = 60000;

        Thread updateThread;
        ManualResetEvent updateStop;

        void Start()
        {
            Stop();

            updateStop = new ManualResetEvent(false);

            updateThread = new Thread(new ThreadStart(Update));
            updateThread.Start();
        }

        void Stop()
        {
            if (updateStop != null) updateStop.Set();
        }

        void Abort()
        {
            if (updateThread != null) updateThread.Abort();
        }

        void Update()
        {
            int interval = Math.Max(INTERVAL_MIN, Properties.Settings.Default.DatabaseReadInterval);

            while (!updateStop.WaitOne(0, true))
            {
                List<Data.DeviceInfo> deviceInfos = null;

                var devices = Devices.ToList().Select(o => o.UniqueId).ToList();

                //string command = "011111";

                var now = DateTime.Now;
                DateTime from = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                DateTime to = from.AddDays(1);

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



                        //SendControllerInfo(deviceInfo);
                        //SendOeeInfo(deviceInfo);
                        //SendStatusInfo(deviceInfo);
                        //SendTimersInfo(deviceInfo);
                        //SendDayInfo(deviceInfo);
                    }
                }

                Thread.Sleep(interval);
            }
        }

        private void SendControllerInfo(Data.DeviceInfo info)
        {
            var obj = info.GetClass("controller");
            if (obj != null)
            {
                var data = new EventData();
                data.Id = "STATUS_CONTROLLER";
                data.Data01 = info.UniqueId;
                data.Data02 = obj;
                SendDataEvent(data);
            }
        }

        private void SendOeeInfo(Data.DeviceInfo info)
        {
            var obj = info.GetClass("oee");
            if (obj != null)
            {
                var data = new EventData();
                data.Id = "STATUS_OEE";
                data.Data01 = info.UniqueId;
                data.Data02 = obj;
                SendDataEvent(data);
            }
        }

        private void SendStatusInfo(Data.DeviceInfo info)
        {
            var obj = info.GetClass("status");
            if (obj != null)
            {
                var data = new EventData();
                data.Id = "STATUS_STATUS";
                data.Data01 = info.UniqueId;
                data.Data02 = obj;
                SendDataEvent(data);
            }
        }

        private void SendTimersInfo(Data.DeviceInfo info)
        {
            var obj = info.GetClass("timers");
            if (obj != null)
            {
                var data = new EventData();
                data.Id = "STATUS_TIMERS";
                data.Data01 = info.UniqueId;
                data.Data02 = obj;
                SendDataEvent(data);
            }
        }

        private void SendDayInfo(Data.DeviceInfo info)
        {
            var obj = info.GetClass("hours");
            if (obj != null)
            {
                var data = new EventData();
                data.Id = "STATUS_HOURS";
                data.Data01 = info.UniqueId;
                data.Data02 = obj;
                SendDataEvent(data);
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
