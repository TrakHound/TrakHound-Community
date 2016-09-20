// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.Dashboard.StatusGrid
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    public partial class StatusGrid : UserControl, IClientPlugin
    {
        public StatusGrid()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public string Title { get { return "Status Grid"; } }

        public string Description { get { return "View Basic Device Status in a Grid"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Grid_01.png")); } }


        public string ParentPlugin { get { return "Dashboard"; } }

        public string ParentPluginCategory { get { return "Pages"; } }

        public bool OpenOnStartUp { get { return true; } }
        
        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        public IPage Options { get; set; }

        public event SendData_Handler SendData;

        private UserConfiguration userConfiguration;


        public void Initialize() { }

        public bool Opening() { return true; }

        public void Opened() { }

        public bool Closing() { return true; }

        public void Closed() { }


        public void GetSentData(EventData data)
        {
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });

            if (data != null && data.Id == "USER_LOGIN")
            {
                if (data.Data01.GetType() == typeof(UserConfiguration))
                {
                    userConfiguration = (UserConfiguration)data.Data01;
                }
            }

            if (data != null && data.Id == "USER_LOGOUT")
            {
                userConfiguration = null;
            }

            if (data != null && data.Id == "STATUS_STATUS" && data.Data02 != null && data.Data02.GetType() == typeof(TrakHound.API.Data.StatusInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (TrakHound.API.Data.StatusInfo)data.Data02;

                    int index = Items.ToList().FindIndex(x => x.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var column = Items[index];
                        column.UpdateData(info);
                    }
                }), UI_Functions.PRIORITY_DATA_BIND, new object[] { });
            }

            if (data != null && data.Id == "STATUS_CONTROLLER" && data.Data02 != null && data.Data02.GetType() == typeof(TrakHound.API.Data.ControllerInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (TrakHound.API.Data.ControllerInfo)data.Data02;

                    int index = Items.ToList().FindIndex(x => x.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var column = Items[index];
                        column.UpdateData(info);
                    }
                }), UI_Functions.PRIORITY_DATA_BIND, new object[] { });
            }

            if (data != null && data.Id == "STATUS_OEE" && data.Data02 != null && data.Data02.GetType() == typeof(TrakHound.API.Data.OeeInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (TrakHound.API.Data.OeeInfo)data.Data02;

                    int index = Items.ToList().FindIndex(x => x.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var column = Items[index];
                        column.UpdateData(info);
                    }
                }), UI_Functions.PRIORITY_DATA_BIND, new object[] { });
            }
        }


        ObservableCollection<Controls.Item> _items;
        public ObservableCollection<Controls.Item> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<Controls.Item>();
                }
                return _items;
            }
            set
            {
                _items = value;
            }
        }


        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;

                    if (device != null && !Items.ToList().Exists(o => o.UniqueId == device.UniqueId))
                    {
                        var column = new Controls.Item(device, userConfiguration);
                        Items.Add(column);
                    }
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

                    int index = Items.ToList().FindIndex(x => x.UniqueId == device.UniqueId);
                    if (index >= 0)
                    {
                        Items.RemoveAt(index);

                        if (device != null && !Items.ToList().Exists(o => o.UniqueId == device.UniqueId))
                        {
                            var column = new Controls.Item(device, userConfiguration);
                            Items.Insert(index, column);
                        }
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

                    int index = Items.ToList().FindIndex(x => x.UniqueId == device.UniqueId);
                    if (index >= 0) Items.RemoveAt(index);
                }
            }
        }

    }
}
