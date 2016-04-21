// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TH_Configuration;
using TH_Database;
using TH_DeviceManager;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins;

namespace SimpleClient
{
    public partial class MainWindow
    {
        public DeviceManager DeviceManager { get; set; }

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

                DeviceManager.CurrentUser = null;
            }
        }

        public List<Configuration> Devices { get; set; }

        private void Devicemanager_DeviceListUpdated(List<Configuration> configs)
        {
            this.Dispatcher.BeginInvoke(new Action<List<Configuration>>(UpdateDeviceList), UI_Functions.PRIORITY_BACKGROUND, new object[] { configs });
        }

        private void Devicemanager_DeviceUpdated(Configuration config, DeviceManager.DeviceUpdateArgs args)
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
            this.Dispatcher.BeginInvoke(new Action(DeviceManager_DevicesLoaded_GUI), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
        }

        private void DeviceManager_DevicesLoaded_GUI()
        {
            // Send message to plugins that Devices have been loaded
            var data = new EventData();
            data.Id = "DevicesLoaded";
            Plugin_SendData(data);
        }

        private void DeviceManager_LoadingDevices()
        {
            this.Dispatcher.BeginInvoke(new Action(DeviceManager_LoadingDevices_GUI), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
        }

        private void DeviceManager_LoadingDevices_GUI()
        {
            // Send message to plugins that Devices are being loaded
            var data = new EventData();
            data.Id = "LoadingDevices";
            Plugin_SendData(data);
        }


        /// <summary>
        /// Method that loads devices from DeviceManager's DeviceLoaded event
        /// </summary>
        /// <param name="configs"></param>
        private void UpdateDeviceList(List<Configuration> configs)
        {
            var enabledConfigs = new List<Configuration>();

            if (configs != null)
            {
                var orderedConfigs = configs.OrderBy(x => x.Description.Manufacturer).ThenBy(x => x.Description.Description).ThenBy(x => x.Description.Device_ID);

                foreach (Configuration config in orderedConfigs)
                {
                    if (config.ClientEnabled)
                    {
                        Global.Initialize(config.Databases_Client);
                        enabledConfigs.Add(config);
                    }
                }

                Devices = orderedConfigs.ToList();

                //if (!addDeviceOpened && enabledConfigs.Count == 0 && _currentuser != null)
                //{
                //    addDeviceOpened = true;
                //    DeviceManager_DeviceList_Open();
                //}
                //else if (enabledConfigs.Count > 0)
                //{
                //    addDeviceOpened = false;
                //}
            }

            Plugins_UpdateDeviceList(enabledConfigs);

            // Send message to plugins that Devices have been loaded
            var data = new EventData();
            data.Id = "devicesloaded";
            Plugin_SendData(data);
        }


        /// <summary>
        /// Device Manager Added a device so add this device to Devices
        /// </summary>
        /// <param name="config"></param>
        private void AddDevice(Configuration config)
        {
            Logger.Log("AddDevice() :: " + config.Description.Description, Logger.LogLineType.Debug);

            if (!Devices.Exists(x => x.UniqueId == config.UniqueId))
            {
                if (config.ClientEnabled)
                {
                    Devices.Add(config);

                    Plugins_AddDevice(config);
                }
            }
        }

        /// <summary>
        /// Device Manager Updated a device so remove old device and new device to Devices
        /// </summary>
        /// <param name="config"></param>
        private void UpdateDevice(Configuration config)
        {
            Logger.Log("UpdateDevice() :: " + config.Description.Description, Logger.LogLineType.Debug);

            int index = Devices.FindIndex(x => x.UniqueId == config.UniqueId);
            if (index >= 0)
            {
                Devices.RemoveAt(index);
                if (config.ClientEnabled) Devices.Insert(index, config);

                Plugins_UpdateDevice(config);
            }
            else
            {
                AddDevice(config);
            }
        }

        /// <summary>
        /// Device Manager Removed a device so remove this device to Devices
        /// </summary>
        /// <param name="config"></param>
        private void RemoveDevice(Configuration config)
        {
            Logger.Log("RemoveDevice() :: " + config.Description.Description, Logger.LogLineType.Debug);

            var match = Devices.Find(x => x.UniqueId == config.UniqueId);
            if (match != null)
            {
                Devices.Remove(match);

                Plugins_RemoveDevice(config);
            }
        }

    }
}
