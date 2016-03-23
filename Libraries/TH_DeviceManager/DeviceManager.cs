// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement.Management;

namespace TH_DeviceManager
{
    /// <summary>
    /// Class used to manage Devices (TH_Configuration.Configuration) objects
    /// </summary>
    public class DeviceManager
    {

        UserConfiguration currentuser;
        /// <summary>
        /// Sets the current UserConfiguration object representing the user that is logged in
        /// </summary>
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                LoadDevices();
            }
        }

        #region "Events"

        /// <summary>
        /// Delegate with no arguments used to update this DeviceManager's status
        /// </summary>
        public delegate void Status_Handler();

        /// <summary>
        /// Event to tell that Devices are in the process of being loaded
        /// </summary>
        public event Status_Handler LoadingDevices;

        /// <summary>
        /// Event to tell that all Devices have been loaded
        /// </summary>
        public event Status_Handler DevicesLoaded;

        /// <summary>
        /// Delegate with a List<TH_Configuration.Configuration> argument to update this DeviceManager's device lists
        /// </summary>
        /// <param name="configs"></param>
        public delegate void Update_Handler(List<Configuration> configs);

        /// <summary>
        /// Event to tell that the Device list has been updated and passes the list as an argument
        /// </summary>
        public event Update_Handler DeviceListUpdated;

        /// <summary>
        /// Event to tell that the Shared Device list has been updated and passes the list as an argument
        /// </summary>
        public event Update_Handler SharedDeviceListUpdated;

        /// <summary>
        /// Enumerated list to describe the type of update to the Device list
        /// </summary>
        public enum DeviceUpdateEvent
        {
            Added,
            Changed,
            Removed
        }

        /// <summary>
        /// Class to pass as an argument when a device has been updated
        /// </summary>
        public class DeviceUpdateArgs
        {
            public object Sender { get; set; }
            public DeviceUpdateEvent Event { get; set; }
        }

        /// <summary>
        /// Delegate with the TH_Configuration.Configuration object that has been udpated and a DeviceUpdateArgs object that describes the update
        /// </summary>
        /// <param name="config">TH_Configuration.Configuration object that has been updated</param>
        /// <param name="args">DeviceUpdateArgs object used to describe the type of udpates</param>
        public delegate void DeviceUpdated_Handler(Configuration config, DeviceUpdateArgs args);

        /// <summary>
        /// Event to tell when a Device has been updated (Added/Removed/Changed)
        /// </summary>
        public event DeviceUpdated_Handler DeviceUpdated;

        #endregion

        private List<Configuration> _devices;
        /// <summary>
        /// List of TH_Configuration.Configuration objects that represent the active devices
        /// </summary>
        public List<Configuration> Devices
        {
            get
            {
                if (_devices == null) _devices = new List<Configuration>();
                return _devices;
            }
            set { _devices = value; }
        }

        private List<Configuration> _sharedDevices;
        /// <summary>
        /// List of TH_Configuration.Configuration objects that represent the shared devices that are
        /// owned by the current user
        /// </summary>
        public List<Configuration> SharedDevices
        {
            get
            {
                if (_sharedDevices == null) _sharedDevices = new List<Configuration>();
                return _sharedDevices;
            }
            set { _sharedDevices = value; }
        }


        #region "Load Devices"

        Thread loaddevices_THREAD;

        /// <summary>
        /// Loads the Devices for the current user
        /// </summary>
        public void LoadDevices()
        {
            // Remove all devices
            if (Devices != null)
            {
                foreach (var device in Devices.ToList())
                {
                    RemoveDeviceFromList(device);
                }
            }

            Devices.Clear();
            SharedDevices.Clear();

            if (loaddevices_THREAD != null) loaddevices_THREAD.Abort();

            loaddevices_THREAD = new Thread(new ThreadStart(LoadDevices_Worker));
            loaddevices_THREAD.Start();

            // Raise DevicesLoading Event
            if (LoadingDevices != null) LoadingDevices();
        }

        private void LoadDevices_Worker()
        {
            List<Configuration> added = null;
            List<Configuration> shared = null;

            if (currentuser != null)
            {
                // Get Added Configurations
                added = LoadDevices_Added();

                shared = LoadDevices_Shared();
            }
            // If not logged in Read from File in 'C:\TrakHound\Devices\'
            else
            {
                added = Configuration.ReadAll(FileLocations.Devices).ToList();

                added = added.OrderBy(x => x.Index).ToList();

                // Reset order to be in intervals of 1000 in order to leave room in between for changes in index
                // This index model allows for devices to change index without having to update every device each time.
                for (var x = 0; x <= added.Count - 1; x++)
                {
                    added[x].Index = 1000 + (1000 * x);

                    Logger.Log("Device Configuration Loaded :: " + added[x].Description.Description);
                }

                foreach (var device in added) Configuration.Save(device);
            }

            Devices = added;
            SharedDevices = shared;

            UpdateDeviceList();
            UpdateSharedDeviceList();

            if (DevicesLoaded != null) DevicesLoaded();
        }

        private List<Configuration> LoadDevices_Added()
        {
            var result = Configurations.GetConfigurationsListForUser(currentuser);
            if (result != null)
            {
                result = result.OrderBy(x => x.Index).ToList();

                // Reset order to be in intervals of 1000 in order to leave room in between for changes in index
                // This index model allows for devices to change index without having to update every device each time.
                for (var x = 0; x <= result.Count - 1; x++)
                {
                    result[x].Index = 1000 + (1000 * x);
                }

                var indexItems = new List<Tuple<string, int>>();

                foreach (var config in result) indexItems.Add(new Tuple<string, int>(config.TableName, config.Index));

                Configurations.UpdateIndexes(indexItems);
            }

            return result;
        }

        private List<Configuration> LoadDevices_Shared()
        {
            var result = Shared.GetOwnedSharedConfigurations(currentuser);
            if (result != null)
            {
                result = result.OrderBy(x => x.Description.Manufacturer).
                    ThenBy(x => x.Description.Controller).
                    ThenBy(x => x.Description.Model).ToList();
            }

            return result;
        }

        #endregion


        /// <summary>
        /// Adds a device to the Device List
        /// </summary>
        /// <param name="config">The Device to add</param>
        public void AddDevice(Configuration config)
        {
            Devices.Add(config);

            // Raise DeviceUpdated Event
            var args = new DeviceUpdateArgs();
            args.Event = DeviceUpdateEvent.Added;
            UpdateDevice(config, args);
        }

        /// <summary>
        /// Adds a device to the Shared Device List
        /// </summary>
        /// <param name="config">The Device to add</param>
        public void AddSharedDevice(Configuration config)
        {
            SharedDevices.Add(config);
        }

        /// <summary>
        /// Raise the DeviceListUpdated event for the Devices List
        /// </summary>
        public void UpdateDeviceList()
        {
            // Raise DevicesLoaded event to update devices for rest of TrakHound 
            // (Client is the only one that uses this for now)
            if (DeviceListUpdated != null) DeviceListUpdated(Devices);
        }

        /// <summary>
        /// Raise the SharedDeviceListUpdated event for the Shared Device List
        /// </summary>
        public void UpdateSharedDeviceList()
        {
            if (SharedDeviceListUpdated != null) SharedDeviceListUpdated(SharedDevices);
        }

        /// <summary>
        /// Raise the DeviceUpdated event to signal that a device has been Added/Removed/Changed
        /// </summary>
        /// <param name="config">The Device that was updated</param>
        /// <param name="args">Tells what type of update took place</param>
        public void UpdateDevice(Configuration config, DeviceUpdateArgs args)
        {
            if (DeviceUpdated != null) DeviceUpdated(config, args);
        }

        /// <summary>
        /// Remove a device from the Devices List
        /// </summary>
        /// <param name="config">The Device to remove</param>
        /// <returns></returns>
        public bool RemoveDevice(Configuration config)
        {
            bool result = false;

            // If logged in then remove table
            if (currentuser != null) result = Configurations.RemoveConfigurationTable(config.TableName);
            // If not logged in then delete local configuration file
            else
            {
                string path = FileLocations.Devices + "\\" + config.FilePath + ".xml";

                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);

                        result = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Remove Local Device :: Exception :: " + path + " :: " + ex.Message);
                    }
                }
            }

            if (result) RemoveDeviceFromList(config);

            return result;
        }

        /// <summary>
        /// Enable a Device
        /// </summary>
        /// <param name="config">The Device to change</param>
        /// <param name="type">Sets whether the Client or Server is enabled</param>
        /// <returns></returns>
        public bool EnableDevice(Configuration config, ManagementType type)
        {
            bool result = false;

            if (currentuser != null)
            {
                string tablename = config.TableName;
                if (tablename != null)
                {
                    if (type == ManagementType.Client) result = Configurations.UpdateConfigurationTable("/ClientEnabled", "True", tablename);
                    else if (type == ManagementType.Server) result = Configurations.UpdateConfigurationTable("/ServerEnabled", "True", tablename);
                }

                if (result) result = UpdateEnabledXML(config.ConfigurationXML, true, type);
                if (result) result = ResetUpdateId(config, type);
            }
            else
            {
                result = UpdateEnabledXML(config.ConfigurationXML, true, type);
                if (result) result = ResetUpdateId(config, type);
                if (result) result = Configuration.Save(config);
            }

            return result;
        }

        /// <summary>
        /// Disable a Device
        /// </summary>
        /// <param name="config">The Device to change</param>
        /// <param name="type">Sets whether the Client or Server is disabled</param>
        /// <returns></returns>
        public bool DisableDevice(Configuration config, ManagementType type)
        {
            bool result = false;

            if (currentuser != null)
            {
                string tablename = config.TableName;
                if (tablename != null)
                {
                    if (type == ManagementType.Client) result = Configurations.UpdateConfigurationTable("/ClientEnabled", "False", tablename);
                    else if (type == ManagementType.Server) result = Configurations.UpdateConfigurationTable("/ServerEnabled", "False", tablename);
                }

                if (result) result = UpdateEnabledXML(config.ConfigurationXML, false, type);
                if (result) result = ResetUpdateId(config, type);
            }
            else
            {
                result = UpdateEnabledXML(config.ConfigurationXML, false, type);
                if (result) result = ResetUpdateId(config, type);
                if (result) result = Configuration.Save(config);
            }

            return result;
        }

        /// <summary>
        /// Changes a Device's Index property
        /// </summary>
        /// <param name="config">The Device to change</param>
        /// <param name="newIndex">The new Index value</param>
        /// <returns></returns>
        public bool ChangeDeviceIndex(Configuration config, int newIndex)
        {
            bool result = false;

            if (currentuser != null)
            {
                string tablename = config.TableName;
                if (tablename != null) result = Configurations.UpdateConfigurationTable("/Index", newIndex.ToString(), tablename);
                if (result) result = XML_Functions.SetInnerText(config.ConfigurationXML, "Index", newIndex.ToString());
            }
            else
            {
                result = XML_Functions.SetInnerText(config.ConfigurationXML, "Index", newIndex.ToString());
                if (result) result = Configuration.Save(config);
            }

            return result;
        }


        private void RemoveDeviceFromList(Configuration config)
        {
            int index = Devices.ToList().FindIndex(x => x.UniqueId == config.UniqueId);
            if (index >= 0)
            {
                Devices.RemoveAt(index);

                // Raise DeviceUpdated Event
                var args = new DeviceUpdateArgs();
                args.Event = DeviceUpdateEvent.Removed;
                UpdateDevice(config, args);
            }
        }

        private bool ResetUpdateId(Configuration config, ManagementType type)
        {
            bool result = false;

            if (type == ManagementType.Client)
            {
                var updateId = String_Functions.RandomString(20);

                if (currentuser != null) result = Configurations.UpdateConfigurationTable("/ClientUpdateId", updateId, config.TableName);
                else result = true;

                if (result)
                {
                    config.ClientUpdateId = updateId;
                    XML_Functions.SetInnerText(config.ConfigurationXML, "ClientUpdateId", updateId);
                }
            }
            else if (type == ManagementType.Server)
            {
                var updateId = String_Functions.RandomString(20);

                if (currentuser != null) result = Configurations.UpdateConfigurationTable("/ServerUpdateId", updateId, config.TableName);
                else result = true;

                if (result)
                {
                    config.ClientUpdateId = updateId;
                    XML_Functions.SetInnerText(config.ConfigurationXML, "ServerUpdateId", updateId);
                }
            }

            return result;
        }

        private static bool UpdateEnabledXML(XmlDocument xml, bool enabled, ManagementType type)
        {
            bool result = false;

            if (type == ManagementType.Client)
            {
                result = XML_Functions.SetInnerText(xml, "ClientEnabled", enabled.ToString());
            }
            else if (type == ManagementType.Server)
            {
                result = XML_Functions.SetInnerText(xml, "ServerEnabled", enabled.ToString());
            }

            return result;
        }

    }
}
