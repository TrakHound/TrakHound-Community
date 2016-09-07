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
            Update(data);

            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
        }

        void Update(EventData data)
        {
            if (data != null && data.Id == "STATUS_CONTROLLER")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (Data.ControllerInfo)data.Data02;

                    int index = Rows.ToList().FindIndex(x => x.Configuration.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var row = Rows[index];
                        row.UpdateData(info);
                    }
                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }

            if (data != null && data.Id == "STATUS_STATUS")
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (Data.StatusInfo)data.Data02;

                    int index = Rows.ToList().FindIndex(x => x.Configuration.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var row = Rows[index];
                        row.UpdateData(info);
                    }
                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }
        }

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    var config = (DeviceConfiguration)data.Data01;
                    AddRow(config);
                }
            }
        }

        void UpdateDeviceUpdated(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_UPDATED" && data.Data01 != null)
                {
                    var config = (DeviceConfiguration)data.Data01;

                    int index = Rows.ToList().FindIndex(x => x.Configuration.UniqueId == config.UniqueId);
                    if (index >= 0)
                    {
                        Rows.RemoveAt(index);
                        AddRow(config, index);
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
                    var config = (DeviceConfiguration)data.Data01;

                    int index = Rows.ToList().FindIndex(x => x.Configuration.UniqueId == config.UniqueId);
                    if (index >= 0) Rows.RemoveAt(index);
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

        private void AddRow(DeviceConfiguration config)
        {
            if (config != null && !Rows.ToList().Exists(o => o.Configuration.UniqueId == config.UniqueId))
            {
                var row = new Controls.Row(config);
                Rows.Add(row);
            }
        }

        private void AddRow(DeviceConfiguration config, int index)
        {
            if (config != null && !Rows.ToList().Exists(o => o.Configuration.UniqueId == config.UniqueId))
            {
                var row = new Controls.Row(config);
                Rows.Insert(index, row);
            }
        }

    }
}
