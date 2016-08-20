// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Tools;
using TrakHound.Logging;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;

namespace TrakHound_Dashboard.Pages.Dashboard.StatusData
{
    public partial class StatusData : IClientPlugin
    {

        #region "Descriptive"

        public string Title { get { return "Status Data"; } }

        public string Description { get { return "Retrieve Data from database(s) related to device status"; } }

        public ImageSource Image { get { return null; } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return null; } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return null; } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/statusdata-appinfo.json"; } }

        #endregion

        #region "Plugin Properties/Options"

        public string ParentPlugin { get { return null; } }
        public string ParentPluginCategory { get { return null; } }

        public bool AcceptsPlugins { get { return false; } }

        public bool OpenOnStartUp { get { return false; } }

        public bool ShowInAppMenu { get { return false; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed()
        {
            Abort();
        }

        public bool Closing() { return true; }

        #endregion

        #region "Events"

        public void GetSentData(EventData data)
        {
            UpdateLogin(data);

            UpdateDeviceAdded(data);
            UpdateDeviceUpdated(data);
            UpdateDeviceRemoved(data);
        }

        public UserConfiguration UserConfiguration { get; set; }

        private void UpdateLogin(EventData data)
        {
            if (data != null && data.Id == "USER_LOGIN")
            {
                if (data.Data01.GetType() == typeof(UserConfiguration))
                {
                    UserConfiguration = (UserConfiguration)data.Data01;
                }
            }
        }

        //public void GetSentData(EventData data)
        //{
        //    UpdateDeviceAdded(data);
        //    UpdateDeviceUpdated(data);
        //    UpdateDeviceRemoved(data);
        //}

        public event SendData_Handler SendData;

        #endregion

        #region "Devices"

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

        public void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //StartDelayTimer();
        }

        //private System.Timers.Timer startDelayTimer;

        //private void StartDelayTimer()
        //{
        //    if (startDelayTimer != null) startDelayTimer.Enabled = false;

        //    startDelayTimer = new System.Timers.Timer();
        //    startDelayTimer.Interval = 1000;
        //    startDelayTimer.Elapsed += StartDelayTimer_Elapsed;
        //    startDelayTimer.Enabled = true;
        //}

        //private void StartDelayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    var timer = (System.Timers.Timer)sender;
        //    timer.Enabled = false;

        //    Start();
        //}

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    Devices.Add((DeviceConfiguration)data.Data01);
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

                    int i = Devices.ToList().FindIndex(x => x.UniqueId == config.UniqueId);
                    if (i >= 0)
                    {
                        Devices.RemoveAt(i);
                        Devices.Insert(i, config);
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

                    int i = Devices.ToList().FindIndex(x => x.UniqueId == config.UniqueId);
                    if (i >= 0)
                    {
                        Devices.RemoveAt(i);
                    }
                }
            }
        }

        #endregion

        #region "Options"

        //public IPage Options { get; set; }

        private IPage _options;
        public IPage Options
        {
            get
            {
                if (_options == null) _options = new OptionsPage();
                return _options;
            }
            set
            {
                _options = value;
            }
        }

        #endregion

    }
}
