// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Plugins.Client;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.Dashboard.Overview
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    public partial class Overview : UserControl, IClientPlugin
    {
        public Overview()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public string Title { get { return "Overview"; } }

        public string Description { get { return "View basic overview data for each device"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Analyse_01.png"); } }

        public string ParentPlugin { get { return "Dashboard"; } }

        public string ParentPluginCategory { get { return "Pages"; } }

        public bool OpenOnStartUp { get { return true; } }
        
        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        public IPage Options { get; set; }

        private UserConfiguration userConfiguration;

        #region "Dependency Properties"

        public int WidthStatus
        {
            get { return (int)GetValue(WidthStatusProperty); }
            set
            {
                SetValue(WidthStatusProperty, value);

                foreach (var column in Columns) column.WidthStatus = value;
            }
        }

        public static readonly DependencyProperty WidthStatusProperty =
            DependencyProperty.Register("WidthStatus", typeof(int), typeof(Overview), new PropertyMetadata(0));

        private ObservableCollection<Controls.Column> _columns;
        public ObservableCollection<Controls.Column> Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new ObservableCollection<Controls.Column>();
                    _columns.CollectionChanged += _columns_CollectionChanged;
                }
                return _columns;
            }
            set
            {
                _columns = value;
            }
        }

        #endregion

        public event SendData_Handler SendData;


        public void Initialize() { }

        public bool Opening() { return true; }

        public void Opened() { }

        public bool Closing() { return true; }

        public void Closed() { }


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
                    ClearColumns();
                }
            }

            if (data != null && data.Id == "USER_LOGOUT")
            {
                userConfiguration = null;
                ClearColumns();
            }

            if (data != null && data.Id == "STATUS_CONTROLLER" && data.Data02 != null && data.Data02.GetType() == typeof(TrakHound.API.Data.ControllerInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (TrakHound.API.Data.ControllerInfo)data.Data02;

                    int index = Columns.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var column = Columns[index];
                        column.UpdateData(info);
                    }
                }), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { });
            }

            if (data != null && data.Id == "STATUS_STATUS" && data.Data02 != null && data.Data02.GetType() == typeof(TrakHound.API.Data.StatusInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (TrakHound.API.Data.StatusInfo)data.Data02;

                    int index = Columns.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var column = Columns[index];
                        column.UpdateData(info);
                    }
                }), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { });
            }

            if (data != null && data.Id == "STATUS_OEE" && data.Data02 != null && data.Data02.GetType() == typeof(TrakHound.API.Data.OeeInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (TrakHound.API.Data.OeeInfo)data.Data02;

                    int index = Columns.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var column = Columns[index];
                        column.UpdateData(info);
                    }
                }), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { });
            }

            if (data != null && data.Id == "STATUS_TIMERS" && data.Data02 != null && data.Data02.GetType() == typeof(TrakHound.API.Data.TimersInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (TrakHound.API.Data.TimersInfo)data.Data02;

                    int index = Columns.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var column = Columns[index];
                        column.UpdateData(info);
                    }
                }), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { });
            }
        }

        private void _columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateColumnWidthStatus();
        }

        void UpdateDevicesLoading(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "LOADING_DEVICES")
                {
                    ClearColumns();
                }
            }
        }

        private void ClearColumns()
        {
            var columns = Columns.ToList();

            foreach (var column in columns)
            {
                var match = Columns.ToList().Find(o => o.Device.UniqueId == column.Device.UniqueId);
                if (match != null)
                {
                    match.Clicked -= ColumnClicked;
                }
            }

            Columns.Clear();
        }

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;
                    AddColumn(device);                
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
                        AddColumn(device);
                        UpdateColumn(device);
                    }
                    else
                    {
                        RemoveColumn(device);
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
                    RemoveColumn(device);
                }
            }
        }

        private void AddColumn(DeviceDescription device)
        {
            if (device != null && device.Enabled && !Columns.ToList().Exists(o => o.Device.UniqueId == device.UniqueId))
            {
                var column = new Controls.Column(device, userConfiguration);
                column.Clicked += ColumnClicked;
                Columns.Add(column);
                Columns.Sort();
            }
        }

        private void UpdateColumn(DeviceDescription device)
        {
            int index = Columns.ToList().FindIndex(x => x.Device.UniqueId == device.UniqueId);
            if (index >= 0)
            {
                var column = Columns[index];
                column.Device = device;
                Columns.Sort();
            }
        }

        private void RemoveColumn(DeviceDescription device)
        {
            int index = Columns.ToList().FindIndex(x => x.Device.UniqueId == device.UniqueId);
            if (index >= 0)
            {
                // Remove Event Handlers
                var column = Columns[index];
                column.Clicked -= ColumnClicked;

                Columns.RemoveAt(index);
            }
        }

        private void UpdateColumnWidthStatus()
        {
            int widthStatus = 0;

            if (ActualWidth > ((Columns.Count * 600) + 121)) widthStatus = 3;
            else if (ActualWidth > ((Columns.Count * 400) + 121)) widthStatus = 2;
            else if (ActualWidth > ((Columns.Count * 150) + 121)) widthStatus = 1;

            WidthStatus = widthStatus;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateColumnWidthStatus();
        }

        private void ColumnClicked(Controls.Column column)
        {
            var data = new EventData(this);
            data.Id = "OPEN_DEVICE_DETAILS";
            data.Data01 = column.Device;
            SendData?.Invoke(data);
        }
    }
}
