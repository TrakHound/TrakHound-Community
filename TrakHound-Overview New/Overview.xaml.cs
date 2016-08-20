// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;
using TrakHound.Tools;

namespace TrakHound_Overview
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    public partial class Overview : UserControl, IClientPlugin
    {
        public Overview()
        {
            InitializeComponent();
            DataContext = this;

            SubCategories = new List<PluginConfigurationCategory>();
            PluginConfigurationCategory components = new PluginConfigurationCategory();
            components.Name = "Components";
            SubCategories.Add(components);
        }

        public static UserConfiguration CurrentUser { get; set; }

        #region "Descriptive"

        public string Title { get { return "Overview (NEW)"; } }

        public string Description { get { return "Compare Device Status and Data in a 'side-by-side' view"; } }

        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Overview;component/Resources/Analyse_01.png"));
                    _image.Freeze();
                }

                return _image;
            }
        }


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

        public bool AcceptsPlugins { get { return true; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ShowInAppMenu { get { return true; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        #endregion

        #region "Events"

        public void GetSentData(EventData data)
        {
            UpdateCurrentUser(data);

            UpdateData(data);

            this.Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            this.Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            this.Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
        }

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id.ToLower() == "deviceadded" && data.Data01 != null)
                {
                    var config = (DeviceConfiguration)data.Data01;

                    AddDevice(config);

                    SortDataItems();
                    LoadHeaderView();
                    SortDeviceDisplays();
                }
            }
        }

        void UpdateDeviceUpdated(EventData data)
        {
            if (data != null)
            {
                if (data.Id.ToLower() == "deviceupdated" && data.Data01 != null)
                {
                    var config = (DeviceConfiguration)data.Data01;

                    int index = DeviceDisplays.FindIndex(x => x.UniqueId == config.UniqueId);
                    if (index >= 0)
                    {
                        var dd = DeviceDisplays[index];
                        Headers.Remove(dd.Group.Header);
                        Columns.Remove(dd.Group.Column);
                        dd.CellAdded -= Display_CellAdded;
                        dd.CellSizeChanged -= display_CellSizeChanged;

                        DeviceDisplays.RemoveAt(index);
                        DeviceDisplays.Insert(index, dd);
                    }
                }
            }
        }

        void UpdateDeviceRemoved(EventData data)
        {
            if (data != null)
            {
                if (data.Id.ToLower() == "deviceremoved" && data.Data01 != null)
                {
                    var config = (DeviceConfiguration)data.Data01;

                    int index = DeviceDisplays.FindIndex(x => x.UniqueId == config.UniqueId);
                    if (index >= 0)
                    {
                        var dd = DeviceDisplays[index];
                        Headers.Remove(dd.Group.Header);
                        Columns.Remove(dd.Group.Column);
                        dd.CellAdded -= Display_CellAdded;
                        dd.CellSizeChanged -= display_CellSizeChanged;

                        DeviceDisplays.Remove(dd);
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        #endregion

        #region "Device Properties"

        private ObservableCollection<DeviceConfiguration> _devices;
        public ObservableCollection<DeviceConfiguration> Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = new ObservableCollection<DeviceConfiguration>();
                    _devices.CollectionChanged += Devices_CollectionChanged;
                }
                return _devices;
            }
            set
            {
                _devices = value;
            }
        }

        #endregion

        public IPage Options { get; set; }

    }
}
