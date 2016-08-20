// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound_Device_Manager;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {
        public DeviceManager DeviceManager { get; set; }

        public List<DeviceConfiguration> Devices { get; set; }

        bool addDeviceOpened = false;

        private void DeviceManager_Initialize()
        {
            if (DeviceManager == null)
            {
                DeviceManager = new DeviceManager();
                DeviceManager.DeviceListUpdated += Devicemanager_DeviceListUpdated;
                DeviceManager.DeviceUpdated += Devicemanager_DeviceUpdated;
                DeviceManager.LoadingDevices += DeviceManager_LoadingDevices;
                DeviceManager.DevicesLoaded += DeviceManager_DevicesLoaded;
            }
        }

        private void Devicemanager_DeviceListUpdated(List<DeviceConfiguration> configs)
        {
            this.Dispatcher.BeginInvoke(new Action<List<DeviceConfiguration>>(UpdateDeviceList), MainWindow.PRIORITY_BACKGROUND, new object[] { configs });
        }

        private void Devicemanager_DeviceUpdated(DeviceConfiguration config, DeviceManager.DeviceUpdateArgs args)
        {
            switch (args.Event)
            {
                case DeviceManager.DeviceUpdateEvent.Added:
                    AddDevice(config);
                    break;

                case DeviceManager.DeviceUpdateEvent.Changed:
                    UpdateDevice(config);
                    break;

                case DeviceManager.DeviceUpdateEvent.Removed:
                    RemoveDevice(config);
                    break;
            }
        }

        private void DeviceManager_DevicesLoaded()
        {
            this.Dispatcher.BeginInvoke(new Action(DeviceManager_DevicesLoaded_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { });
        }

        private void DeviceManager_DevicesLoaded_GUI()
        {
            // Send message to plugins that Devices have been loaded
            var data = new EventData();
            data.Id = "DEVICES_LOADED";
            Plugin_SendData(data);
        }

        private void DeviceManager_LoadingDevices()
        {
            this.Dispatcher.BeginInvoke(new Action(DeviceManager_LoadingDevices_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { });
        }

        private void DeviceManager_LoadingDevices_GUI()
        {
            // Send message to plugins that Devices are being loaded
            var data = new EventData();
            data.Id = "LOADING_DEVICES";
            Plugin_SendData(data);
        }


        /// <summary>
        /// Method that loads devices from DeviceManager's DeviceLoaded event
        /// </summary>
        /// <param name="configs"></param>
        private void UpdateDeviceList(List<DeviceConfiguration> configs)
        {
            var enabledConfigs = new List<DeviceConfiguration>();

            if (configs != null)
            {
                var orderedConfigs = configs.OrderBy(x => x.Description.Manufacturer).ThenBy(x => x.Description.Model).ThenBy(x => x.Description.Description).ThenBy(x => x.Description.DeviceId);

                foreach (DeviceConfiguration config in orderedConfigs)
                {
                    if (config.Enabled)
                    {
                        enabledConfigs.Add(config);
                    }
                }

                Devices = orderedConfigs.ToList();

                if (!addDeviceOpened && enabledConfigs.Count == 0 && _currentuser != null)
                {
                    addDeviceOpened = true;
                    DeviceManager_DeviceList_Open();
                }
                else if (enabledConfigs.Count > 0)
                {
                    addDeviceOpened = false;
                }
            }

            foreach (var config in enabledConfigs)
            {
                AddDevice(config);
            }

            // Send message to plugins that Devices have been loaded
            var data = new EventData();
            data.Id = "DEVICES_LOADED";
            Plugin_SendData(data);
        }

        /// <summary>
        /// Device Manager Added a device so add this device to Devices
        /// </summary>
        /// <param name="config"></param>
        private void AddDevice(DeviceConfiguration config)
        {
            // Send message to plugins that Device has been added
            var data = new EventData();
            data.Id = "DEVICE_ADDED";
            data.Data01 = config;
            Plugin_SendData(data);
        }

        /// <summary>
        /// Device Manager Updated a device so remove old device and new device to Devices
        /// </summary>
        /// <param name="config"></param>
        private void UpdateDevice(DeviceConfiguration config)
        {
            // Send message to plugins that Device has been updated
            var data = new EventData();
            data.Id = "DEVICE_UPDATED";
            data.Data01 = config;
            Plugin_SendData(data);
        }

        /// <summary>
        /// Device Manager Removed a device so remove this device to Devices
        /// </summary>
        /// <param name="config"></param>
        private void RemoveDevice(DeviceConfiguration config)
        {
            // Send message to plugins that Device has been removed
            var data = new EventData();
            data.Id = "DEVICE_REMOVED";
            data.Data01 = config;
            Plugin_SendData(data);
        }

    }
}
