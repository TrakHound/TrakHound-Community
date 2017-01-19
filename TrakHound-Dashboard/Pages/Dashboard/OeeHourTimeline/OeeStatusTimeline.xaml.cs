// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

using TrakHound;
using TrakHound.API;
using TrakHound.Configurations;
using TrakHound.Plugins.Client;
using TrakHound.Tools;
using TrakHound_UI.Extensions;
using TrakHound_Dashboard.Pages.Dashboard.OeeHourTimeline.Controls;

namespace TrakHound_Dashboard.Pages.Dashboard.OeeHourTimeline
{
    public partial class Page : IClientPlugin
    {

        public string Title { get { return "OEE Hour Timeline"; } }

        public string Description { get { return "View OEE Timeline by Hour for the current day."; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Time_Status_01.png"); } }

        public string ParentPlugin { get { return "Dashboard"; } }
        public string ParentPluginCategory { get { return "Pages"; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ZoomEnabled { get { return false; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

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

        public IPage Options { get; set; }

        public event SendData_Handler SendData;


        public bool IsScrollbarVisible
        {
            get { return (bool)GetValue(IsScrollbarVisibleProperty); }
            set { SetValue(IsScrollbarVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsScrollbarVisibleProperty =
            DependencyProperty.Register("IsScrollbarVisible", typeof(bool), typeof(Page), new PropertyMetadata(false));



        public Page()
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

            Dispatcher.BeginInvoke(new Action<EventData>(SortRows), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });

            Update(data);

            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
        }

        private static string GetUniqueIdFromDeviceInfo(Row row)
        {
            if (row != null && row.Device != null)
            {
                return row.Device.UniqueId;
            }
            return null;
        }


        void Update(EventData data)
        {
            if (data != null && data.Id == "STATUS_STATUS" && data.Data01 != null && data.Data02 != null && data.Data02.GetType() == typeof(Data.StatusInfo))
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

            if (data != null && data.Id == "STATUS_HOURS" && data.Data01 != null && data.Data02 != null && data.Data02.GetType() == typeof(List<Data.HourInfo>))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (List<Data.HourInfo>)data.Data02;

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
                if (data.Id == "DEVICES_LOADING")
                {
                    ClearRows();
                }
            }
        }

        private DeviceComparisonTypes comparisonType;

        private void SortRows(EventData data)
        {
            if (data != null && data.Id == "SORT_DEVICES")
            {
                var type = (DeviceComparisonTypes)data.Data01;
                comparisonType = type;

                foreach (var row in Rows) row.ComparisonType = type;
                Rows.Sort();
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
                row.ComparisonType = comparisonType;
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

        private void ScrollViewer_LayoutUpdated(object sender, EventArgs e)
        {
            var scrollviewer = this.GetChildOfType<System.Windows.Controls.ScrollViewer>();
            if (scrollviewer != null)
            {
                IsScrollbarVisible = scrollviewer.ComputedVerticalScrollBarVisibility == Visibility.Visible;
            }
        }

    }
}
