// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TH_Configuration;
using TH_Plugins;
using TH_Plugins.Client;

namespace TH_StatusTable
{
    public partial class StatusTable : IClientPlugin
    {


        #region "Descriptive"

        public string Title { get { return "Status Table"; } }

        public string Description { get { return "View Production Status data for the current day"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_StatusTable;component/Resources/Stoplight_01.png")); } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2016 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return null; } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return null; } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return null; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return "Dashboard"; } }
        public string DefaultParentCategory { get { return "Pages"; } }

        public bool AcceptsPlugins { get { return false; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ShowInAppMenu { get { return true; } }

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
        }

        public event SendData_Handler SendData;

        #endregion

        private ObservableCollection<Configuration> _devices;
        public ObservableCollection<Configuration> Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = new ObservableCollection<Configuration>();
                    _devices.CollectionChanged += Devices_CollectionChanged;
                }
                return _devices;
            }
            set
            {
                _devices = value;
            }
        }

        public void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    DeviceInfos.Clear();
                }

                if (e.NewItems != null)
                {
                    foreach (Configuration newConfig in e.NewItems)
                    {
                        if (newConfig != null) AddDevice(newConfig);
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (Configuration oldConfig in e.OldItems)
                    {
                        Devices.Remove(oldConfig);

                        int index = DeviceInfos.ToList().FindIndex(x => GetUniqueIdFromDeviceInfo(x) == oldConfig.UniqueId);
                        if (index >= 0) DeviceInfos.RemoveAt(index);
                    }
                }
            }));

           
        }

        private static string GetUniqueIdFromDeviceInfo(DeviceInfo info)
        {
            if (info != null && info.Configuration != null)
            {
                return info.Configuration.UniqueId;
            }
            return null;
        }
        

        public TH_Global.IPage Options { get; set; }


    }
}
