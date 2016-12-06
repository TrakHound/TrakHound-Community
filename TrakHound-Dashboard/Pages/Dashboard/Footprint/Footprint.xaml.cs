// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins.Client;
using TrakHound.Tools.Web;

namespace TrakHound_Dashboard.Pages.Dashboard.Footprint
{
    /// <summary>
    /// Interaction logic for ShopStatus.xaml
    /// </summary>
    public partial class Footprint : UserControl, IClientPlugin
    {

        private object dragDropItem;


        public Footprint()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public string Title { get { return "Footprint"; } }

        public string Description { get { return "View devices in a layout matching your shop's footprint"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Footprint_01.png"); } }

        public string ParentPlugin { get { return "Dashboard"; } }

        public string ParentPluginCategory { get { return "Pages"; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ZoomEnabled { get { return false; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        public IPage Options { get; set; }

        private List<DeviceDescription> _devices;
        public List<DeviceDescription> Devices
        {
            get
            {
                if (_devices == null) _devices = new List<DeviceDescription>();
                return _devices;
            }
            set
            {
                _devices = value;
            }
        }

        public event SendData_Handler SendData;


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

            if (data != null && data.Id == "STATUS_STATUS" && data.Data01 != null && data.Data02 != null && data.Data02.GetType() == typeof(TrakHound.API.Data.StatusInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (TrakHound.API.Data.StatusInfo)data.Data02;

                    int index = DeviceItems.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var column = DeviceItems[index];
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

                    int index = DeviceItems.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var column = DeviceItems[index];
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
                    ListItems.Clear();
                    ClearDeviceItems();
                }
            }
        }

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;
                    if (device.Enabled)
                    {
                        Devices.Add(device);

                        LoadDeviceItem(device);
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

                    int i = Devices.ToList().FindIndex(x => x.UniqueId == device.UniqueId);
                    if (i >= 0)
                    {
                        if (device.Enabled) Devices[i] = device;
                        else Devices.RemoveAt(i);
                    }
                    else if (device.Enabled) Devices.Add(device);

                    UpdateDeviceItem(device);
                    UpdateListItem(device);
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

                    RemoveListItem(device.UniqueId);
                    RemoveDeviceItem(device.UniqueId);
                }
            }
        }

        
        public bool EditEnabled
        {
            get { return (bool)GetValue(EditEnabledProperty); }
            set { SetValue(EditEnabledProperty, value); }
        }

        public static readonly DependencyProperty EditEnabledProperty =
            DependencyProperty.Register("EditEnabled", typeof(bool), typeof(Footprint), new PropertyMetadata(false));


        #region "List Items"

        private ObservableCollection<Controls.ListItem> _listItems;
        public ObservableCollection<Controls.ListItem> ListItems
        {
            get
            {
                if (_listItems == null) _listItems = new ObservableCollection<Controls.ListItem>();
                return _listItems;
            }
            set
            {
                _listItems = value;
            }
        }

        private Controls.ListItem AddListItem(DeviceDescription device)
        {
            var listItem = new Controls.ListItem(device);
            ListItems.Add(listItem);

            return listItem;
        }

        private void UpdateListItem(DeviceDescription device)
        {
            int i = ListItems.ToList().FindIndex(x => x.Device.UniqueId == device.UniqueId);
            if (i >= 0)
            {
                if (device.Enabled) ListItems[i].Device = device;
                else RemoveListItem(ListItems[i]);
            }
        }

        private void RemoveListItem(Controls.ListItem listItem)
        {
            ListItems.Remove(listItem);
        }

        private void RemoveListItem(string uniqueId)
        {
            int i = ListItems.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
            if (i >= 0)
            {
                RemoveListItem(ListItems[i]);
            } 
        }

        private void ShopCanvas_Drop(object sender, DragEventArgs e)
        {
            if (dragDropItem != null)
            {
                if (dragDropItem.GetType() == typeof(Controls.ListItem))
                {
                    var listItem = (Controls.ListItem)dragDropItem;
                    var device = listItem.Device;

                    // Remove ListItem from ListItems collection
                    RemoveListItem(listItem);

                    var deviceItem = AddDeviceItem(device);

                    // Calculate drop position so that middle of control is placed where cursor is dropped
                    double dropX = e.GetPosition(shopCanvas).X - (deviceItem.Width / 2);
                    double dropY = e.GetPosition(shopCanvas).Y - (deviceItem.Height / 2);

                    // Set Canvas position
                    Canvas.SetTop(deviceItem, dropY);
                    Canvas.SetLeft(deviceItem, dropX);

                    SaveDeviceItemLocation(deviceItem);
                }

                dragDropItem = null;
            }           
        }

        private void ListItems_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var parent = (ListBox)sender;
            object data = GetDataFromListBox(parent, e.GetPosition(parent));

            if (data != null && data.GetType() == typeof(Controls.ListItem))
            {
                StartDrag(parent, (Controls.ListItem)data);
            }
        }

        private void ListBox_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            var parent = (ListBox)sender;
            object data = GetDataFromListBox(parent, e.GetTouchPoint(parent).Position);

            if (data != null && data.GetType() == typeof(Controls.ListItem))
            {
                StartDrag(parent, (Controls.ListItem)data);
            }
        }

        private void StartDrag(ListBox parent, Controls.ListItem listItem)
        {
            if (listItem != null)
            {
                dragDropItem = listItem;

                DragDrop.DoDragDrop(parent, listItem, DragDropEffects.Move);
            }
            else dragDropItem = null;
        }

        private static object GetDataFromListBox(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    if (element == source)
                    {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }

        #endregion

        #region "Device Items"

        private ObservableCollection<Controls.DeviceItem> _deviceItems;
        public ObservableCollection<Controls.DeviceItem> DeviceItems
        {
            get
            {
                if (_deviceItems == null) _deviceItems = new ObservableCollection<Controls.DeviceItem>();
                return _deviceItems;
            }
            set
            {
                _deviceItems = value;
            }
        }

        private Controls.DeviceItem AddDeviceItem(DeviceDescription device)
        {
            var deviceItem = new Controls.DeviceItem(this, device);
            deviceItem.Moved += DeviceItem_Updated;
            deviceItem.Resized += DeviceItem_Updated;
            deviceItem.ViewDetails += DeviceItem_ViewDetails;
            deviceItem.EditDevice += DeviceItem_EditDevice;
            deviceItem.CloseClicked += DeviceItem_CloseClicked;

            // Add to Canvas
            shopCanvas.Children.Add(deviceItem);

            // Add to DeviceItems collection (to update data)
            DeviceItems.Add(deviceItem);

            return deviceItem;
        }

        private void UpdateDeviceItem(DeviceDescription device)
        {
            int i = DeviceItems.ToList().FindIndex(x => x.Device.UniqueId == device.UniqueId);
            if (i >= 0)
            {
                if (device.Enabled) DeviceItems[i].Device = device;
                else RemoveDeviceItem(DeviceItems[i]);
            }
        }

        private void RemoveDeviceItem(Controls.DeviceItem deviceItem)
        {
            shopCanvas.Children.Remove(deviceItem);
            DeviceItems.Remove(deviceItem);
            RemoveDeviceItemLocation(deviceItem);
        }

        private void RemoveDeviceItem(string uniqueId)
        {
            int i = DeviceItems.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
            if (i >= 0)
            {
                RemoveDeviceItem(DeviceItems[i]);
            } 
        }

        private void ClearDeviceItems()
        {
            foreach (var deviceItem in DeviceItems)
            {
                shopCanvas.Children.Remove(deviceItem);
                RemoveDeviceItemLocation(deviceItem);
            }

            DeviceItems.Clear();
        }

        private void LoadDeviceItem(DeviceDescription device)
        {
            var location = LoadDeviceItemLocation(device.UniqueId);
            if (location != null)
            {
                var deviceItem = AddDeviceItem(device);

                Canvas.SetLeft(deviceItem, location.X);
                Canvas.SetTop(deviceItem, location.Y);

                deviceItem.Height = location.Height;
                deviceItem.Width = location.Width;
            }
            else
            {
                AddListItem(device);
            }
        }

        private void DeviceItem_ViewDetails(Controls.DeviceItem item)
        {
            var data = new EventData(this);
            data.Id = "OPEN_DEVICE_DETAILS";
            data.Data01 = item.Device;
            SendData?.Invoke(data);
        }


        private void DeviceItem_EditDevice(Controls.DeviceItem item)
        {
            var data = new EventData(this);
            data.Id = "SHOW_EDIT_DEVICE";
            data.Data01 = item.Device;
            SendData?.Invoke(data);
        }

        private void DeviceItem_CloseClicked(Controls.DeviceItem item)
        {
            AddListItem(item.Device);
            RemoveDeviceItem(item);
        }

        private void DeviceItem_Updated(Controls.DeviceItem item)
        {
            SaveDeviceItemLocation(item);
        }

        private class DeviceItemLocation
        {
            public string DeviceId { get; set; }

            public double X { get; set; }
            public double Y { get; set; }

            public double Height { get; set; }
            public double Width { get; set; }
        }


        private DeviceItemLocation LoadDeviceItemLocation(string deviceId)
        {
            var json = Properties.Settings.Default.ShopStatusItemLocations;
            if (!string.IsNullOrEmpty(json))
            {
                var locations = JSON.ToType<List<DeviceItemLocation>>(json);
                if (locations != null)
                {
                    var location = locations.Find(o => o.DeviceId == deviceId);
                    if (location != null) return location;
                }
            }

            return null;
        }

        private void SaveDeviceItemLocation(Controls.DeviceItem deviceItem)
        {
            List<DeviceItemLocation> locations = null;

            var json = Properties.Settings.Default.ShopStatusItemLocations;
            if (!string.IsNullOrEmpty(json))
            {
                locations = JSON.ToType<List<DeviceItemLocation>>(json);
            }

            if (locations == null)
            {
                locations = new List<DeviceItemLocation>();
            }

            double x = Canvas.GetLeft(deviceItem);
            double y = Canvas.GetTop(deviceItem);

            double width = deviceItem.ActualWidth;
            double height = deviceItem.ActualHeight;

            var location = locations.Find(o => o.DeviceId == deviceItem.Device.UniqueId);
            if (location == null)
            {
                location = new DeviceItemLocation();
                locations.Add(location);
            }

            location.DeviceId = deviceItem.Device.UniqueId;
            location.X = x;
            location.Y = y;
            location.Height = height;
            location.Width = width;

            json = JSON.FromList<DeviceItemLocation>(locations);
            if (!string.IsNullOrEmpty(json))
            {
                Properties.Settings.Default.ShopStatusItemLocations = json;
                Properties.Settings.Default.Save();
            }
        }

        private void RemoveDeviceItemLocation(Controls.DeviceItem deviceItem)
        {
            var json = Properties.Settings.Default.ShopStatusItemLocations;
            if (!string.IsNullOrEmpty(json))
            {
                var locations = JSON.ToType<List<DeviceItemLocation>>(json);

                int i = locations.FindIndex(o => o.DeviceId == deviceItem.Device.UniqueId);
                if (i >= 0)
                {
                    locations.RemoveAt(i);
                }

                json = JSON.FromList<DeviceItemLocation>(locations);
                if (!string.IsNullOrEmpty(json))
                {
                    Console.WriteLine(json);

                    Properties.Settings.Default.ShopStatusItemLocations = json;
                    Properties.Settings.Default.Save();
                }
            } 
        }

        #endregion

        private void EnterEditMode_Clicked(TrakHound_UI.Button bt)
        {
            EditEnabled = true;
        }

        private void ExitEditMode_Clicked(TrakHound_UI.Button bt)
        {
            EditEnabled = false;
        }

        private void ClearAll_Clicked(TrakHound_UI.Button bt)
        {
            var result = TrakHound_UI.MessageBox.Show("Are you sure you want to clear the enire layout?", "Clear Layout", TrakHound_UI.MessageBoxButtons.YesNo);
            if (result == TrakHound_UI.MessageBoxDialogResult.Yes)
            {
                foreach (var deviceItem in DeviceItems)
                {
                    AddListItem(deviceItem.Device);
                }

                ClearDeviceItems();
            }
        }
    }
}
