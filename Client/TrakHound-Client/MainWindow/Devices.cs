using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;
using System.Threading;
using System.Data;
using System.Xml;
using System.IO;

using TH_Configuration;
using TH_Database;
using TH_DeviceManager;
using TH_Global;
using TH_UserManagement.Management;

namespace TrakHound_Client
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
            }
        }

        public List<Configuration> Devices { get; set; }

        private void Devicemanager_DeviceListUpdated(List<Configuration> configs)
        {
            this.Dispatcher.BeginInvoke(new Action<List<Configuration>>(UpdateDeviceList), MainWindow.PRIORITY_BACKGROUND, new object[] { configs });
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
            this.Dispatcher.BeginInvoke(new Action(DeviceManager_DevicesLoaded_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { });
        }

        private void DeviceManager_DevicesLoaded_GUI()
        {
            // Send message to plugins that Devices have been loaded
            TH_Plugins_Client.DataEvent_Data de_d = new TH_Plugins_Client.DataEvent_Data();
            de_d.id = "DevicesLoaded";
            Plugin_DataEvent(de_d);
        }

        private void DeviceManager_LoadingDevices()
        {
            this.Dispatcher.BeginInvoke(new Action(DeviceManager_LoadingDevices_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { });
        }

        private void DeviceManager_LoadingDevices_GUI()
        {
            // Send message to plugins that Devices are being loaded
            TH_Plugins_Client.DataEvent_Data de_d = new TH_Plugins_Client.DataEvent_Data();
            de_d.id = "LoadingDevices";
            Plugin_DataEvent(de_d);
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

            Plugins_UpdateDeviceList(enabledConfigs);

            // Send message to plugins that Devices have been loaded
            TH_Plugins_Client.DataEvent_Data de_d = new TH_Plugins_Client.DataEvent_Data();
            de_d.id = "devicesloaded";
            Plugin_DataEvent(de_d);
        }


        /// <summary>
        /// Device Manager Added a device so add this device to Devices
        /// </summary>
        /// <param name="config"></param>
        private void AddDevice(Configuration config)
        {
            Logger.Log("AddDevice() :: " + config.Description.Description);

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
            Logger.Log("UpdateDevice() :: " + config.Description.Description);

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
            Logger.Log("RemoveDevice() :: " + config.Description.Description);

            var match = Devices.Find(x => x.UniqueId == config.UniqueId);
            if (match != null)
            {
                Devices.Remove(match);

                Plugins_RemoveDevice(config);
            }
        }

    }
}
