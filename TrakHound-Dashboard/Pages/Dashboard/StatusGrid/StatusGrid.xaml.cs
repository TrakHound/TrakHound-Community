// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Plugins.Client;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.Dashboard.StatusGrid
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    public partial class StatusGrid : UserControl, IClientPlugin
    {

        public string Title { get { return "Status Grid"; } }

        public string Description { get { return "View Basic Device Status in a Grid"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Grid_01.png"); } }

        public string ParentPlugin { get { return "Dashboard"; } }

        public string ParentPluginCategory { get { return "Pages"; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ZoomEnabled { get { return false; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        public IPage Options { get; set; }

        private UserConfiguration userConfiguration;

        private ObservableCollection<Controls.Item> _items;
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

        public event SendData_Handler SendData;


        public StatusGrid()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public void Initialize() { }

        public bool Opening() { return true; }

        public void Opened() { }

        public bool Closing() { return true; }

        public void Closed() { }

        public void SetZoom(double zoomPercentage) { }

        public void GetSentData(EventData data)
        {
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDevicesLoading), System.Windows.Threading.DispatcherPriority.Normal, new object[] { data });

            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });

            if (data != null && data.Id == "USER_LOGIN")
            {
                if (data.Data01.GetType() == typeof(UserConfiguration))
                {
                    userConfiguration = (UserConfiguration)data.Data01;
                    ClearItems();
                }
            }

            if (data != null && data.Id == "USER_LOGOUT")
            {
                userConfiguration = null;
                ClearItems();
            }

            if (data != null && data.Id == "STATUS_STATUS" && data.Data01 != null && data.Data02 != null && data.Data02.GetType() == typeof(TrakHound.API.Data.StatusInfo))
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
                }), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { });
            }

            if (data != null && data.Id == "STATUS_CONTROLLER" && data.Data01 != null && data.Data02 != null && data.Data02.GetType() == typeof(TrakHound.API.Data.ControllerInfo))
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
                }), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { });
            }

            if (data != null && data.Id == "STATUS_OEE" && data.Data01 != null && data.Data02 != null && data.Data02.GetType() == typeof(TrakHound.API.Data.OeeInfo))
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
                }), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { });
            }
        }

        void UpdateDevicesLoading(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICES_LOADING")
                {
                    ClearItems();
                }
            }
        }

        private void ClearItems()
        {
            var items = Items.ToList();

            foreach (var item in items)
            {
                var match = Items.ToList().Find(o => o.Device.UniqueId == item.Device.UniqueId);
                if (match != null)
                {
                    match.Clicked -= Item_Clicked;
                }
            }

            Items.Clear();
        }

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;

                    AddItem(device);
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
                        AddItem(device);
                        UpdateItem(device);
                    }
                    else
                    {
                        RemoveItem(device);
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

                    RemoveItem(device);
                }
            }
        }

        private void AddItem(DeviceDescription device)
        {
            if (device != null && device.Enabled && !Items.ToList().Exists(o => o.Device.UniqueId == device.UniqueId))
            {
                var column = new Controls.Item(device, userConfiguration);
                column.Clicked += Item_Clicked;
                Items.Add(column);
                Items.Sort();
            }
        }

        private void UpdateItem(DeviceDescription device)
        {
            int index = Items.ToList().FindIndex(x => x.Device.UniqueId == device.UniqueId);
            if (index >= 0)
            {
                var column = Items[index];
                column.Device = device;
                Items.Sort();
            }
        }

        private void RemoveItem(DeviceDescription device)
        {
            int index = Items.ToList().FindIndex(x => x.Device.UniqueId == device.UniqueId);
            if (index >= 0)
            {
                // Remove Event Handlers
                var column = Items[index];
                column.Clicked -= Item_Clicked;

                Items.RemoveAt(index);
            }
        }


        private void Item_Clicked(Controls.Item item)
        {
            var data = new EventData(this);
            data.Id = "OPEN_DEVICE_DETAILS";
            data.Data01 = item.Device;
            SendData?.Invoke(data);
        }

    }
}
