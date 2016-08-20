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

namespace TrakHound_Dashboard.Pages.Dashboard
{
    public partial class Dashboard : IClientPlugin
    {

        #region "Descriptive"

        public string Title { get { return "Dashboard"; } }

        public string Description { get { return "Contains and organizes pages for displaying Device data in various ways. Acts as the Home page for other Device Monitoring Plugins."; } }

        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Dashboard_01.png"));
                    _image.Freeze();
                }

                return _image;
            }
        }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2016 Feenux LLC. All Rights Reserved"; } }

        private BitmapImage _authorImage;
        public ImageSource AuthorImage
        {
            get
            {
                if (_authorImage == null)
                {
                    _authorImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/TrakHound_Logo_10_200px.png"));
                    _authorImage.Freeze();
                }

                return _authorImage;
            }
        }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return null; } }

        #endregion

        #region "Plugin Properties/Options"

        public string ParentPlugin { get { return null; } }
        public string ParentPluginCategory { get { return null; } }

        public bool AcceptsPlugins { get { return true; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ShowInAppMenu { get { return true; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/dashboard-appinfo.json"; } }

        #endregion

        #region "Methods"

        const System.Windows.Threading.DispatcherPriority Priority = System.Windows.Threading.DispatcherPriority.Background;

        public void Initialize()
        {
            EnabledPlugins = new List<PluginConfiguration>();

            foreach (PluginConfigurationCategory category in SubCategories)
            {
                foreach (PluginConfiguration config in category.PluginConfigurations)
                {
                    config.EnabledChanged += config_EnabledChanged;

                    if (config.Enabled) Plugins_Load(config);
                }
            }
        }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        #endregion

        #region "Events"

        public void GetSentData(EventData data)
        {
            Dispatcher.BeginInvoke(new Action(UpdateCurrentDate), UI_Functions.PRIORITY_BACKGROUND, new object[] { });

            Dispatcher.BeginInvoke(new Action<EventData>(UpdateLoggedInChanged), UI_Functions.PRIORITY_BACKGROUND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateWindowClick), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDevicesLoading), UI_Functions.PRIORITY_BACKGROUND, new object[] { data });

            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), UI_Functions.PRIORITY_BACKGROUND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), UI_Functions.PRIORITY_BACKGROUND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), UI_Functions.PRIORITY_BACKGROUND, new object[] { data });

            if (Plugins != null)
            {
                foreach (IClientPlugin plugin in Plugins)
                {
                    //ThreadPool.QueueUserWorkItem(o => plugin.GetSentData(data));
                    Dispatcher.BeginInvoke(new Action<EventData>(plugin.GetSentData), UI_Functions.PRIORITY_BACKGROUND, new object[] { data });
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
                }
                return _devices;
            }
            set
            {
                _devices = value;
            }
        }

        #endregion

        #region "Options"

        public IPage Options { get; set; }

        #endregion

    }
}
