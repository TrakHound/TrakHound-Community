// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;
using System.Threading;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Configurations;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {
        private List<DeviceDescription> _devices;
        public List<DeviceDescription> Devices
        {
            get
            {
                if (_devices == null) _devices = new List<DeviceDescription>();
                return _devices;
            }
            set { _devices = value; }
        }


        private Thread loadDevicesThread;

        private void LoadDevices()
        {
            // Send message to other pages that Devices are currently being loaded
            SendDevicesLoadingMessage();

            // Clear current devices
            ClearDevices();

            // Make sure loadDevicesThread is not active
            if (loadDevicesThread != null) loadDevicesThread.Abort();

            // Start LoadDevices in separate thread
            if (CurrentUser != null)
            {
                loadDevicesThread = new Thread(new ParameterizedThreadStart(LoadUserDevices));
                loadDevicesThread.Start(CurrentUser);
            }
            else
            {
                loadDevicesThread = new Thread(new ThreadStart(LoadLocalDevices));
                loadDevicesThread.Start();
            }
        }

        private void LoadUserDevices(object o)
        {
            if (o != null)
            {
                var currentUser = (UserConfiguration)o;

                // Get DeviceDescriptions
                var devices = TrakHound.API.Devices.List(currentUser, TrakHound.API.ApiConfiguration.AuthenticationApiHost);
                if (devices != null && devices.Count > 0)
                {
                    foreach (var device in devices)
                    {
                        AddDevice(device);
                    }
                }
            }

            SendDevicesLoadedMessage();
        }

        private void LoadLocalDevices()
        {
            var deviceConfigs = DeviceConfiguration.ReadAll(FileLocations.Devices);
            if (deviceConfigs != null)
            {
                foreach (var deviceConfig in deviceConfigs)
                {
                    if (deviceConfig.Description != null) AddDevice(new DeviceDescription(deviceConfig));
                }
            }

            SendDevicesLoadedMessage();
        }

        /// <summary>
        /// Adds a device to the Device List
        /// </summary>
        /// <param name="config">The Device to add</param>
        public void AddDevice(DeviceDescription device)
        {
            Devices.Add(device);
            SendDeviceAddedMessage(device);
        }

        /// <summary>
        /// Clears the Device List
        /// </summary>
        private void ClearDevices()
        {
            // Remove all devices
            if (Devices != null)
            {
                foreach (var device in Devices.ToList())
                {
                    SendDeviceRemovedMessage(device);
                }

                Devices.Clear();
            }
        }

        private void SendDevicesLoadingMessage()
        {
            // Send message to plugins that Devices are being loaded
            var data = new EventData(this);
            data.Id = "DEVICES_LOADING";
            SendEventData(data);
        }

        private void SendDevicesLoadedMessage()
        {
            // Send message to plugins that Devices have been loaded
            var data = new EventData(this);
            data.Id = "DEVICES_LOADED";
            SendEventData(data);
        }

        private void SendDeviceAddedMessage(DeviceDescription device)
        {
            // Send message to plugins that Device has been added
            var data = new EventData(this);
            data.Id = "DEVICE_ADDED";
            data.Data01 = device;
            SendEventData(data);
        }

        private void SendDeviceUpdatedMessage(DeviceDescription device)
        {
            // Send message to plugins that Device has been updated
            var data = new EventData(this);
            data.Id = "DEVICE_UPDATED";
            data.Data01 = device;
            SendEventData(data);
        }

        private void SendDeviceRemovedMessage(DeviceDescription device)
        {
            // Send message to plugins that Device has been removed
            var data = new EventData(this);
            data.Id = "DEVICE_REMOVED";
            data.Data01 = device;
            SendEventData(data);
        }

        private void SendCurrentDevices()
        {
            if (Devices != null)
            {
                foreach (var device in Devices.ToList())
                {
                    // Send message to plugins that Device has been added
                    var data = new EventData(this);
                    data.Id = "DEVICE_ADDED";
                    data.Data01 = device;
                    SendEventData(data);
                }
            }
        }

        private void SendCurrentDevices(IPage page)
        {
            if (Devices != null)
            {
                foreach (var device in Devices.ToList())
                {
                    // Send message to plugins that Device has been added
                    var data = new EventData(this);
                    data.Id = "DEVICE_ADDED";
                    data.Data01 = device;
                    page.GetSentData(data);
                }
            }
        }
        
    }
}
