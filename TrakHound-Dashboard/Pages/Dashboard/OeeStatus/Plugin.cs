// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.Dashboard.OeeStatus
{
    public partial class Plugin : IClientPlugin
    {

        #region "Descriptive"

        public string Title { get { return "OEE Status"; } }

        public string Description { get { return "View OEE status for the current day"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Oee_Percent_03.png")); } }

        #endregion

        #region "Plugin Properties/Options"

        public string ParentPlugin { get { return "Dashboard"; } }
        public string ParentPluginCategory { get { return "Pages"; } }

        public bool OpenOnStartUp { get { return true; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public bool Opening() { return true; }

        public void Opened() { }

        public bool Closing() { return true; }

        public void Closed() { }

        #endregion

        #region "Events"

        public void GetSentData(EventData data)
        {
            Update(data);

            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
        }

        public event SendData_Handler SendData;

        #endregion

        //private ObservableCollection<DeviceConfiguration> _devices;
        //public ObservableCollection<DeviceConfiguration> Devices
        //{
        //    get
        //    {
        //        if (_devices == null)
        //        {
        //            _devices = new ObservableCollection<DeviceConfiguration>();
        //            _devices.CollectionChanged += Devices_CollectionChanged;
        //        }
        //        return _devices;
        //    }
        //    set
        //    {
        //        _devices = value;
        //    }
        //}

        //public void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    //Dispatcher.BeginInvoke(new Action(() =>
        //    //{
        //    //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
        //    //    {
        //    //        Rows.Clear();
        //    //    }

        //    //    if (e.NewItems != null)
        //    //    {
        //    //        foreach (DeviceConfiguration newConfig in e.NewItems)
        //    //        {
        //    //            if (newConfig != null) AddRow(newConfig);
        //    //        }
        //    //    }

        //    //    if (e.OldItems != null)
        //    //    {
        //    //        foreach (DeviceConfiguration oldConfig in e.OldItems)
        //    //        {
        //    //            Devices.Remove(oldConfig);

        //    //            int index = Rows.ToList().FindIndex(x => GetUniqueIdFromDeviceInfo(x) == oldConfig.UniqueId);
        //    //            if (index >= 0) Rows.RemoveAt(index);
        //    //        }
        //    //    }
        //    //}));
        //}

        private static string GetUniqueIdFromDeviceInfo(Controls.Row row)
        {
            if (row != null && row.Device != null)
            {
                return row.Device.UniqueId;
            }
            return null;
        }

        public IPage Options { get; set; }

    }
}
