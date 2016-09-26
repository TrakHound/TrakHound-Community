// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.API;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.Dashboard.ControllerStatus
{
    /// <summary>
    /// Interaction logic for StatusTimeline.xaml
    /// </summary>
    public partial class ControllerStatus : UserControl, IClientPlugin
    {

        public string Title { get { return "Controller Status"; } }

        public string Description { get { return "View Controller Status Variables for each Device"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Warning_01.png")); } }

        public string ParentPlugin { get { return "Dashboard"; } }
        public string ParentPluginCategory { get { return "Pages"; } }

        public bool OpenOnStartUp { get { return true; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        public IPage Options { get; set; }

        public event SendData_Handler SendData;

        public ControllerStatus()
        {
            InitializeComponent();
            root.DataContext = this;
        }


        public void Initialize() { }

        public bool Opening() { return true; }

        public void Opened() { }

        public bool Closing() { return true; }

        public void Closed() { }

        public void GetSentData(EventData data)
        {
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDevicesLoading), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });

            Update(data);

            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
        }

        void Update(EventData data)
        {
            if (data != null && data.Id == "STATUS_CONTROLLER" && data.Data02 != null && data.Data02.GetType() == typeof(Data.ControllerInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (Data.ControllerInfo)data.Data02;

                    int index = Rows.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var row = Rows[index];
                        row.UpdateData(info);
                    }
                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }

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
                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
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
            foreach (var row in Rows) row.Clicked -= Row_Clicked;
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

                    int index = Rows.ToList().FindIndex(x => x.Device.UniqueId == device.UniqueId);
                    if (index >= 0)
                    {
                        var row = Rows[index];
                        row.Device = device;
                        Rows.Sort();
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

                    int index = Rows.ToList().FindIndex(x => x.Device.UniqueId == device.UniqueId);
                    if (index >= 0)
                    {
                        // Remove Event Handlers
                        var row = Rows[index];
                        row.Clicked -= Row_Clicked;

                        Rows.RemoveAt(index);
                    }
                }
            }
        }


        ObservableCollection<Controls.Row> _rows;
        public ObservableCollection<Controls.Row> Rows
        {
            get
            {
                if (_rows == null) _rows = new ObservableCollection<Controls.Row>();
                return _rows;
            }
            set
            {
                _rows = value;
            }
        }

        private void AddRow(DeviceDescription device)
        {
            if (device != null && !Rows.ToList().Exists(o => o.Device.UniqueId == device.UniqueId))
            {
                var row = new Controls.Row(device);
                row.Clicked += Row_Clicked;
                Rows.Add(row);
                Rows.Sort();
            }
        }

        private void AddRow(DeviceDescription device, int index)
        {
            if (device != null && !Rows.ToList().Exists(o => o.Device.UniqueId == device.UniqueId))
            {
                var row = new Controls.Row(device);
                row.Clicked += Row_Clicked;
                Rows.Insert(index, row);
                Rows.Sort();
            }
        }

        private void Row_Clicked(Controls.Row row)
        {
            var data = new EventData(this);
            data.Id = "OPEN_DEVICE_DETAILS";
            data.Data01 = row.Device;
            SendData?.Invoke(data);
        }

    }
}
