// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using TrakHound;
using TrakHound.API;
using TrakHound.Configurations;
using TrakHound.Plugins.Client;
using TrakHound.Tools;
using TrakHound_Dashboard.Pages.Dashboard.DeviceStatusTimes.Controls;

namespace TrakHound_Dashboard.Pages.Dashboard.DeviceStatusTimes
{
    public partial class DeviceStatusTimes : IClientPlugin
    {

        public string Title { get { return "Device Status Times"; } }

        public string Description { get { return "View Device Status Times and Percentages for the current day"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Status_Percentage_01.png"); } }

        public string ParentPlugin { get { return "Dashboard"; } }
        public string ParentPluginCategory { get { return "Pages"; } }

        public bool OpenOnStartUp { get { return true; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        public event SendData_Handler SendData;

        public IPage Options { get; set; }

        private ObservableCollection<Row> _rows;
        public ObservableCollection<Row> Rows
        {
            get
            {
                if (_rows == null) _rows = new ObservableCollection<Row>();
                return _rows;
            }
            set
            {
                _rows = value;
            }
        }


        public void Initialize() { }

        public bool Opening() { return true; }

        public void Opened() { }

        public bool Closing() { return true; }

        public void Closed() { }

        public void GetSentData(EventData data)
        {
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDevicesLoading), System.Windows.Threading.DispatcherPriority.Normal, new object[] { data });

            if (data != null && data.Id == "USER_LOGIN")
            {
                if (data.Data01.GetType() == typeof(TrakHound.API.Users.UserConfiguration))
                {
                    ClearRows();
                }
            }

            if (data != null && data.Id == "USER_LOGOUT")
            {
                ClearRows();
            }

            Update(data);

            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
        }


        public DeviceStatusTimes()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        void Update(EventData data)
        {
            if (data != null && data.Id == "STATUS_STATUS" && data.Data02 != null && data.Data02.GetType() == typeof(Data.StatusInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (Data.StatusInfo)data.Data02;

                    int index = Rows.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var row = Rows[index];
                        row.UpdateData(info);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
            }

            if (data != null && data.Id == "STATUS_TIMERS" && data.Data02 != null && data.Data02.GetType() == typeof(Data.TimersInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (Data.TimersInfo)data.Data02;

                    int index = Rows.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var row = Rows[index];
                        row.UpdateData(info);
                    }
                }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
            }
        }

        void UpdateDevicesLoading(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "LOADING_DEVICES")
                {
                    ClearRows();
                }
            }
        }


        private void ClearRows()
        {
            var rows = Rows.ToList();

            foreach (var row in rows)
            {
                var match = Rows.ToList().Find(o => o.Device.UniqueId == row.Device.UniqueId);
                if (match != null)
                {
                    match.Clicked -= Row_Clicked;
                }
            }

            Rows.Clear();
        }

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;
                    AddRow(device);
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

                    if (device.Enabled)
                    {
                        AddRow(device);
                        UpdateRow(device);
                    }
                    else
                    {
                        RemoveRow(device);
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

                    RemoveRow(device);
                }
            }
        }

        private void AddRow(DeviceDescription device)
        {
            if (device != null && device.Enabled && !Rows.ToList().Exists(o => o.Device.UniqueId == device.UniqueId))
            {
                var row = new Controls.Row(device);
                row.Clicked += Row_Clicked;
                Rows.Add(row);
                Rows.Sort();
            }
        }

        private void UpdateRow(DeviceDescription device)
        {
            int index = Rows.ToList().FindIndex(x => x.Device.UniqueId == device.UniqueId);
            if (index >= 0)
            {
                var column = Rows[index];
                column.Device = device;
                Rows.Sort();
            }
        }

        private void RemoveRow(DeviceDescription device)
        {
            int index = Rows.ToList().FindIndex(x => x.Device.UniqueId == device.UniqueId);
            if (index >= 0)
            {
                // Remove Event Handlers
                var row = Rows[index];
                row.Clicked -= Row_Clicked;

                Rows.RemoveAt(index);
            }
        }

        private void Row_Clicked(Row row)
        {
            var data = new EventData(this);
            data.Id = "OPEN_DEVICE_DETAILS";
            data.Data01 = row.Device;
            SendData?.Invoke(data);
        }
    }
}
