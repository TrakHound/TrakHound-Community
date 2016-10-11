// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

using TrakHound;
using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Plugins.Client;

namespace TrakHound_Dashboard.Pages.Dashboard.StatusData
{
    public partial class StatusData : IClientPlugin
    {
        private const int INTERVAL_MIN = 500;
        private const int INTERVAL_MAX = 60000;

        private Thread updateThread;
        private ManualResetEvent updateStop;

        private DateTime beginTime;
        private DateTime endTime;

        public string Title { get { return "Status Data"; } }

        public string Description { get { return "Retrieve Data from database(s) related to device status"; } }

        public Uri Image { get { return null; } }

        public string ParentPlugin { get { return null; } }
        public string ParentPluginCategory { get { return null; } }

        public bool OpenOnStartUp { get { return false; } }

        public bool ZoomEnabled { get { return false; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        private IPage _options;
        public IPage Options
        {
            get
            {
                if (_options == null) _options = new OptionsPage();
                return _options;
            }
            set
            {
                _options = value;
            }
        }

        public UserConfiguration UserConfiguration { get; set; }

        private ObservableCollection<DeviceDescription> _devices;
        public ObservableCollection<DeviceDescription> Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = new ObservableCollection<DeviceDescription>();
                }
                return _devices;
            }
            set
            {
                _devices = value;
            }
        }

        public event SendData_Handler SendData;


        public StatusData() { }

        public void Initialize()
        {
            Start();
        }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed()
        {
            Abort();
        }

        public bool Closing() { return true; }

        public void SetZoom(double zoomPercentage) { }

        public void GetSentData(EventData data)
        {
            UpdateLogin(data);
            UpdateTimespan(data);

            UpdateDeviceAdded(data);
            UpdateDeviceUpdated(data);
            UpdateDeviceRemoved(data);
        }

        private void UpdateLogin(EventData data)
        {
            if (data != null && data.Id == "USER_LOGIN")
            {
                if (data.Data01.GetType() == typeof(UserConfiguration))
                {
                    UserConfiguration = (UserConfiguration)data.Data01;
                }
            }

            if (data != null && data.Id == "USER_LOGOUT")
            {
                UserConfiguration = null;
            }
        }

        private void UpdateTimespan(EventData data)
        {
            if (data != null && data.Id == "DASHBOARD_TIMESPAN")
            {
                if (data.Data01.GetType() == typeof(DateTime) && data.Data02.GetType() == typeof(DateTime))
                {
                    beginTime = (DateTime)data.Data01;
                    endTime = (DateTime)data.Data02;

                    Update();
                }
            }
        }

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    Devices.Add((DeviceDescription)data.Data01);
                }
            }
        }

        void UpdateDeviceUpdated(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_UPDATED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;

                    int i = Devices.ToList().FindIndex(x => x.UniqueId == device.UniqueId);
                    if (i >= 0)
                    {
                        Devices.RemoveAt(i);
                        Devices.Insert(i, device);
                    }
                }
            }
        }

        void UpdateDeviceRemoved(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_REMOVED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;

                    int i = Devices.ToList().FindIndex(x => x.UniqueId == device.UniqueId);
                    if (i >= 0)
                    {
                        Devices.RemoveAt(i);
                    }
                }
            }
        }

        private void Start()
        {
            Stop();

            updateStop = new ManualResetEvent(false);

            updateThread = new Thread(new ThreadStart(Update_Worker));
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

        private void Update_Worker()
        {
            int interval = Math.Max(INTERVAL_MIN, Properties.Settings.Default.DatabaseReadInterval);

            do
            {
                Update();

                // Put thread to sleep for specified interval (Data Read Interval)
                interval = Math.Max(INTERVAL_MIN, Properties.Settings.Default.DatabaseReadInterval);

            } while (!updateStop.WaitOne(interval, true));
        }

        private void Update()
        {
            // Get Timestamp for current entire day
            var now = DateTime.Now;
            DateTime from = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Local);
            DateTime to = from.AddDays(1);

            if (beginTime >= from && endTime <= to)
            {
                from = beginTime;
                to = endTime;
            }

            // Get list of UniqueId's from Devices list
            var devices = Devices.ToList().Select(o => o.UniqueId).ToList();

            // Retrieve device Data from API
            List<Data.DeviceInfo> deviceInfos = null;
            if (UserConfiguration != null) deviceInfos = Data.Get(UserConfiguration, devices, from.ToUniversalTime(), to.ToUniversalTime(), 2000);
            else deviceInfos = Data.Get(null, devices, from.ToUniversalTime(), to.ToUniversalTime(), 2000);
            if (deviceInfos != null)
            {
                foreach (var deviceInfo in deviceInfos)
                {
                    foreach (var c in deviceInfo.Classes)
                    {
                        var data = new EventData(this);
                        data.Id = "STATUS_" + c.Key.ToUpper();
                        data.Data01 = deviceInfo.UniqueId;
                        data.Data02 = c.Value;
                        SendDataEvent(data);
                    }
                }
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
