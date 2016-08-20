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

using TrakHound;
using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Logging;
using TrakHound.Tools;

namespace TrakHound_Device_Manager
{
    /// <summary>
    /// Class used to manage Devices (TrakHound.Configurations.Configuration) objects
    /// </summary>
    public class DeviceManager
    {

        private bool firstLoad = true;

        UserConfiguration _currentuser;
        /// <summary>
        /// Sets the current UserConfiguration object representing the user that is logged in
        /// </summary>
        public UserConfiguration CurrentUser
        {
            get { return _currentuser; }
            set
            {
                var prevUser = _currentuser;
                _currentuser = value;

                if (firstLoad || 
                    (prevUser == null && _currentuser != null) ||
                    (prevUser != null && _currentuser == null) ||
                    ((prevUser != null && _currentuser != null) && (prevUser.Username != _currentuser.Username))
                    )
                {
                        LoadDevices();
                    }

                firstLoad = false;
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
        /// Delegate with a List<TrakHound.Configurations.Configuration> argument to update this DeviceManager's device lists
        /// </summary>
        /// <param name="configs"></param>
        public delegate void Update_Handler(List<DeviceConfiguration> configs);

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
        /// Delegate with the TrakHound.Configurations.Configuration object that has been udpated and a DeviceUpdateArgs object that describes the update
        /// </summary>
        /// <param name="config">TrakHound.Configurations.Configuration object that has been updated</param>
        /// <param name="args">DeviceUpdateArgs object used to describe the type of udpates</param>
        public delegate void DeviceUpdated_Handler(DeviceConfiguration config, DeviceUpdateArgs args);

        /// <summary>
        /// Event to tell when a Device has been updated (Added/Removed/Changed)
        /// </summary>
        public event DeviceUpdated_Handler DeviceUpdated;

        #endregion

        private List<DeviceConfiguration> _devices;
        /// <summary>
        /// List of TrakHound.Configurations.DeviceConfiguration objects that represent the active devices
        /// </summary>
        public List<DeviceConfiguration> Devices
        {
            get
            {
                if (_devices == null) _devices = new List<DeviceConfiguration>();
                return _devices;
            }
            set { _devices = value; }
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

            if (loaddevices_THREAD != null) loaddevices_THREAD.Abort();

            loaddevices_THREAD = new Thread(new ThreadStart(LoadDevices_Worker));
            loaddevices_THREAD.Start();

            // Raise DevicesLoading Event
            LoadingDevices?.Invoke();
        }

        private void LoadDevices_Worker()
        {
            List<DeviceConfiguration> devices = null;

            if (_currentuser != null)
            {
                // Get Configurations
                var result = TrakHound.API.Devices.Get(_currentuser);
                if (result != null)
                {
                    result = result.OrderBy(x => x.Index).ToList();

                    devices = result;

                    // Reset order to be in intervals of 1000 in order to leave room in between for changes in index
                    // This index model allows for devices to change index without having to update every device each time.
                    for (var x = 0; x <= result.Count - 1; x++)
                    {
                        result[x].Index = 1000 + (1000 * x);
                    }

                    var deviceInfos = new List<Devices.DeviceInfo>();

                    foreach (var config in result)
                    {
                        deviceInfos.Add(new Devices.DeviceInfo(config.UniqueId, new Devices.DeviceInfo.Row("/Index", config.Index.ToString(), null)));
                    }

                    TrakHound.API.Devices.Update(_currentuser, deviceInfos, false);
                }
            }
            // If not logged in Read from File in 'C:\TrakHound\Devices\'
            else
            {
                devices = DeviceConfiguration.ReadAll(FileLocations.Devices).ToList();

                devices = devices.OrderBy(x => x.Index).ToList();

                // Reset order to be in intervals of 1000 in order to leave room in between for changes in index
                // This index model allows for devices to change index without having to update every device each time.
                for (var x = 0; x <= devices.Count - 1; x++)
                {
                    devices[x].Index = 1000 + (1000 * x);

                    Logger.Log("Device Configuration Loaded :: " + devices[x].Description.Description);
                }

                foreach (var device in devices) DeviceConfiguration.Save(device);
            }

            Devices = devices;

            UpdateDeviceList();

            DevicesLoaded?.Invoke();
        }

        #endregion


        /// <summary>
        /// Adds a device to the Device List
        /// </summary>
        /// <param name="config">The Device to add</param>
        public void AddDevice(DeviceConfiguration config)
        {
            Devices.Add(config);

            // Raise DeviceUpdated Event
            var args = new DeviceUpdateArgs();
            args.Event = DeviceUpdateEvent.Added;
            UpdateDevice(config, args);
        }

        /// <summary>
        /// Raise the DeviceListUpdated event for the Devices List
        /// </summary>
        public void UpdateDeviceList()
        {
            // Raise DevicesLoaded event to update devices for rest of TrakHound 
            // (Client is the only one that uses this for now)
            DeviceListUpdated?.Invoke(Devices);
        }


        /// <summary>
        /// Raise the DeviceUpdated event to signal that a device has been Added/Removed/Changed
        /// </summary>
        /// <param name="config">The Device that was updated</param>
        /// <param name="args">Tells what type of update took place</param>
        public void UpdateDevice(DeviceConfiguration config, DeviceUpdateArgs args)
        {
            DeviceUpdated?.Invoke(config, args);
        }

        /// <summary>
        /// Remove a device from the Devices List
        /// </summary>
        /// <param name="config">The Device to remove</param>
        /// <returns></returns>
        public bool RemoveDevice(DeviceConfiguration config)
        {
            bool result = false;

            // If logged in then remove table
            if (_currentuser != null)
            {
                var uniqueIds = new string[1];
                uniqueIds[0] = config.UniqueId;

                result = TrakHound.API.Devices.Remove(_currentuser, uniqueIds);
            }
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
        /// Remove devices from the Devices List
        /// </summary>
        /// <param name="configs">The Devices to remove</param>
        /// <returns></returns>
        public bool RemoveDevices(List<DeviceConfiguration> configs)
        {
            bool result = false;

            // If logged in then remove table
            if (_currentuser != null)
            {
                var uniqueIds = configs.Select(x => x.UniqueId).ToArray();

                result = TrakHound.API.Devices.Remove(_currentuser, uniqueIds);

                if (result)
                {
                    foreach (var config in configs) RemoveDeviceFromList(config);
                }
            }
            // If not logged in then delete local configuration file
            else
            {
                foreach (var config in configs)
                {
                    result = false;

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

                    if (result) RemoveDeviceFromList(config);
                }                
            }

            return result;
        }

        /// <summary>
        /// Enable a Device
        /// </summary>
        /// <param name="config">The Device to change</param>
        /// <param name="type">Sets whether the Client or Server is enabled</param>
        /// <returns></returns>
        public bool EnableDevice(DeviceConfiguration config)
        {
            bool result = false;

            if (_currentuser != null)
            {
                result = TrakHound.API.Devices.Update(_currentuser, config.UniqueId, new Devices.DeviceInfo.Row("/Enabled", "True", null));
                if (result) result = UpdateEnabledXML(config.Xml, true);
                if (result) result = ResetUpdateId(config);
            }
            else
            {
                result = UpdateEnabledXML(config.Xml, true);
                if (result) result = ResetUpdateId(config);
                if (result) result = DeviceConfiguration.Save(config);
            }

            return result;
        }

        /// <summary>
        /// Disable a Device
        /// </summary>
        /// <param name="config">The Device to change</param>
        /// <param name="type">Sets whether the Client or Server is disabled</param>
        /// <returns></returns>
        public bool DisableDevice(DeviceConfiguration config)
        {
            bool result = false;

            if (_currentuser != null)
            {
                result = TrakHound.API.Devices.Update(_currentuser, config.UniqueId, new Devices.DeviceInfo.Row("/Enabled", "False", null));

                if (result) result = UpdateEnabledXML(config.Xml, false);
                if (result) result = ResetUpdateId(config);
            }
            else
            {
                result = UpdateEnabledXML(config.Xml, false);
                if (result) result = ResetUpdateId(config);
                if (result) result = DeviceConfiguration.Save(config);
            }

            return result;
        }
        
        /// <summary>
        /// Changes a Device's Index property
        /// </summary>
        /// <param name="config">The Device to change</param>
        /// <param name="newIndex">The new Index value</param>
        /// <returns></returns>
        public bool ChangeDeviceIndex(DeviceConfiguration config, int newIndex)
        {
            bool result = false;

            if (_currentuser != null)
            {
                result = TrakHound.API.Devices.Update(_currentuser, config.UniqueId, new Devices.DeviceInfo.Row("/Index", newIndex.ToString(), null));
                if (result) result = XML_Functions.SetInnerText(config.Xml, "Index", newIndex.ToString());
            }
            else
            {
                result = XML_Functions.SetInnerText(config.Xml, "Index", newIndex.ToString());
                if (result) result = DeviceConfiguration.Save(config);
            }

            return result;
        }


        private void RemoveDeviceFromList(DeviceConfiguration config)
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

        private bool ResetUpdateId(DeviceConfiguration config)
        {
            bool result = false;

            var updateId = Guid.NewGuid().ToString();

            if (_currentuser != null) result = TrakHound.API.Devices.Update(_currentuser, config.UniqueId, new Devices.DeviceInfo.Row("/UpdateId", updateId, null));
            else result = true;

            if (result)
            {
                config.UpdateId = updateId;
                XML_Functions.SetInnerText(config.Xml, "UpdateId", updateId);
            }

            return result;
        }

        private static bool UpdateEnabledXML(XmlDocument xml, bool enabled)
        {
            bool result = false;

            result = XML_Functions.SetInnerText(xml, "Enabled", enabled.ToString());

            return result;
        }
        
    }
}
