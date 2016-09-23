// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Plugins.Client;

namespace TrakHound_Dashboard.Pages.Dashboard.StatusData
{
    public partial class StatusData : IClientPlugin
    {

        public string Title { get { return "Status Data"; } }

        public string Description { get { return "Retrieve Data from database(s) related to device status"; } }

        public ImageSource Image { get { return null; } }


        public string ParentPlugin { get { return null; } }
        public string ParentPluginCategory { get { return null; } }

        public bool OpenOnStartUp { get { return false; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

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

        public UserConfiguration UserConfiguration { get; set; }

        private ObservableCollection<DeviceDescription> _devices;
        public ObservableCollection<DeviceDescription> Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = new ObservableCollection<DeviceDescription>();
                }
                return _devices;
            }
            set
            {
                _devices = value;
            }
        }

        public event SendData_Handler SendData;



        public void Initialize() { }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed()
        {
            Abort();
        }

        public bool Closing() { return true; }


        #region "Events"

        public void GetSentData(EventData data)
        {
            UpdateLogin(data);

            UpdateDeviceAdded(data);
            UpdateDeviceUpdated(data);
            UpdateDeviceRemoved(data);
        }

        private void UpdateLogin(EventData data)
        {
            if (data != null && data.Id == "USER_LOGIN")
            {
                if (data.Data01.GetType() == typeof(UserConfiguration))
                {
                    UserConfiguration = (UserConfiguration)data.Data01;
                }
            }

            if (data != null && data.Id == "USER_LOGOUT")
            {
                UserConfiguration = null;
            }
        }

        #endregion

        #region "Devices"

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    Devices.Add((DeviceDescription)data.Data01);
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
                        Devices.RemoveAt(i);
                        Devices.Insert(i, device);
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

                    int i = Devices.ToList().FindIndex(x => x.UniqueId == device.UniqueId);
                    if (i >= 0)
                    {
                        Devices.RemoveAt(i);
                    }
                }
            }
        }

        #endregion

    }
}
