// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Windows.Controls;

using TrakHound;
using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Plugins.Client;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.DeviceDetails
{
    public class Plugin : UserControl, IClientPlugin
    {
        private UserConfiguration userConfiguration;

        public string Title { get { return "Device Details"; } }

        public string Description { get { return null; } }

        public Uri Image { get { return null; } }

        public bool ZoomEnabled { get { return false; } }

        public void SetZoom(double zoomPercentage) { }

        public string ParentPlugin { get { return null; } }
        public string ParentPluginCategory { get { return null; } }

        public bool OpenOnStartUp { get { return false; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        public IPage Options { get; set; }

        public event SendData_Handler SendData;

        public void Initialize() { }

        public bool Opening() { return true; }

        public void Opened() { }

        public bool Closing() { return true; }

        public void Closed() { }

        public void GetSentData(EventData data)
        {
            if (data != null && data.Id == "USER_LOGIN")
            {
                if (data.Data01.GetType() == typeof(UserConfiguration))
                {
                    userConfiguration = (UserConfiguration)data.Data01;
                }
            }

            if (data != null && data.Id == "USER_LOGOUT")
            {
                userConfiguration = null;
            }

            if (data != null)
            {
                if (data.Id == "STATUS_STATUS" && data.Data01 != null && data.Data02 != null) UpdateDeviceData(data.Data01.ToString(), (Data.StatusInfo)data.Data02);
                if (data.Id == "STATUS_CONTROLLER" && data.Data01 != null && data.Data02 != null) UpdateDeviceData(data.Data01.ToString(), (Data.ControllerInfo)data.Data02);
                if (data.Id == "STATUS_OEE" && data.Data01 != null && data.Data02 != null) UpdateDeviceData(data.Data01.ToString(), (Data.OeeInfo)data.Data02);
                if (data.Id == "STATUS_TIMERS" && data.Data01 != null && data.Data02 != null) UpdateDeviceData(data.Data01.ToString(), (Data.TimersInfo)data.Data02);
                if (data.Id == "STATUS_HOURS" && data.Data01 != null && data.Data02 != null) UpdateDeviceData(data.Data01.ToString(), (List<Data.HourInfo>)data.Data02);

                if (data.Id == "OPEN_DEVICE_DETAILS" && data.Data01 != null && data.Data01.GetType() == typeof(DeviceDescription))
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var device = (DeviceDescription)data.Data01;
                        var deviceInfo = cachedDevicesInfos.Find(o => o.UniqueId == device.UniqueId);

                        var page = new Page(device, deviceInfo, userConfiguration);

                        var sendData = new EventData(this);
                        sendData.Id = "SHOW";
                        sendData.Data02 = page;
                        sendData.Data03 = GetPageHeading(device);
                        SendData?.Invoke(sendData);

                    }), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { });
                }
            }
        }

        private static string GetPageHeading(DeviceDescription device)
        {
            if (device != null && device.Description != null)
            {
                string heading = "Device Details";

                if (!string.IsNullOrEmpty(device.Description.Description)) heading += " : " + device.Description.Description;
                if (!string.IsNullOrEmpty(device.Description.DeviceId)) heading += " : " + device.Description.DeviceId;
                if (!string.IsNullOrEmpty(device.Description.Manufacturer)) heading += " : " + device.Description.Manufacturer;
                if (!string.IsNullOrEmpty(device.Description.Model)) heading += " : " + device.Description.Model;
                if (!string.IsNullOrEmpty(device.Description.Serial)) heading += " : " + device.Description.Serial;

                return heading;
            }

            return null;
        }

        private List<Data.DeviceInfo> cachedDevicesInfos = new List<Data.DeviceInfo>();

        private void UpdateDeviceData(string uniqueId, Data.StatusInfo info)
        {
            var deviceInfo = cachedDevicesInfos.Find(o => o.UniqueId == uniqueId);
            if (deviceInfo == null)
            {
                deviceInfo = new Data.DeviceInfo();
                deviceInfo.UniqueId = uniqueId;
                cachedDevicesInfos.Add(deviceInfo);
            }

            deviceInfo.Status = info;
        }

        private void UpdateDeviceData(string uniqueId, Data.ControllerInfo info)
        {
            var deviceInfo = cachedDevicesInfos.Find(o => o.UniqueId == uniqueId);
            if (deviceInfo == null)
            {
                deviceInfo = new Data.DeviceInfo();
                deviceInfo.UniqueId = uniqueId;
                cachedDevicesInfos.Add(deviceInfo);
            }

            deviceInfo.Controller = info;
        }

        private void UpdateDeviceData(string uniqueId, Data.OeeInfo info)
        {
            var deviceInfo = cachedDevicesInfos.Find(o => o.UniqueId == uniqueId);
            if (deviceInfo == null)
            {
                deviceInfo = new Data.DeviceInfo();
                deviceInfo.UniqueId = uniqueId;
                cachedDevicesInfos.Add(deviceInfo);
            }

            deviceInfo.Oee = info;
        }

        private void UpdateDeviceData(string uniqueId, Data.TimersInfo info)
        {
            var deviceInfo = cachedDevicesInfos.Find(o => o.UniqueId == uniqueId);
            if (deviceInfo == null)
            {
                deviceInfo = new Data.DeviceInfo();
                deviceInfo.UniqueId = uniqueId;
                cachedDevicesInfos.Add(deviceInfo);
            }

            deviceInfo.Timers = info;
        }

        private void UpdateDeviceData(string uniqueId, List<Data.HourInfo> infos)
        {
            var deviceInfo = cachedDevicesInfos.Find(o => o.UniqueId == uniqueId);
            if (deviceInfo == null)
            {
                deviceInfo = new Data.DeviceInfo();
                deviceInfo.UniqueId = uniqueId;
                cachedDevicesInfos.Add(deviceInfo);
            }

            deviceInfo.Hours = infos;
        }
    }
}
