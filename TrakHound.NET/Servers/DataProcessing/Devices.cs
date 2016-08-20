// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using TrakHound.Configurations;
using TrakHound.Logging;
using TrakHound.Plugins.Server;
using TrakHound.Tools;

namespace TrakHound.Servers.DataProcessing
{
    public partial class ProcessingServer
    {
        List<DeviceServer> Devices = new List<DeviceServer>();

        void LoadDevices()
        {
            Devices.Clear();

            DevicesMonitor_Initialize();
        }

        void AddDevice(DeviceConfiguration config)
        {
            config.Index = Devices.Count;

            var deviceThread = new Thread(new ParameterizedThreadStart(StartDeviceServer));
            deviceThread.Start(config);
        }

        private void StartDeviceServer(object obj)
        {
            var config = (DeviceConfiguration)obj;

            var plugins = new List<IServerPlugin>();

            foreach (var plugin in serverPlugins)
            {
                var newInstance = CreatePluginInstance(plugin);
                if (newInstance != null) plugins.Add(newInstance);
            }

            var server = new DeviceServer(config, plugins);
            server.Started += Server_Started;
            server.Stopped += Server_Stopped;
            Devices.Add(server);
            server.Start();

            UpdateLoginInformation(server);
        }

        private void Server_Started(DeviceServer server)
        {
            Logger.Log(server.Configuration.UniqueId + " Started..");
        }

        private void Server_Stopped(DeviceServer server)
        {
            Logger.Log(server.Configuration.UniqueId + " Stopped..");
        }


        private IServerPlugin CreatePluginInstance(IServerPlugin plugin)
        {
            ConstructorInfo ctor = plugin.GetType().GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, CallingConventions.HasThis, new Type[] { }, null);

            Object_Functions.ObjectActivator<IServerPlugin> createdActivator = Object_Functions.GetActivator<IServerPlugin>(ctor);

            return createdActivator();
        }

        #region "Devices Monitor"

        /// <summary>
        /// Devices Monitor is used to monitor when devices are Changed, Added, or Removed.
        /// 'Changed' includes whether device was Enabled or Disabled.
        /// Monitor runs at a fixed interval of 5 seconds and compares Devices with list of tables for current user
        /// </summary>

        private Thread devicesmonitor_THREAD;
        private ManualResetEvent monitorstop = null;

        void DevicesMonitor_Initialize()
        {
            if (devicesmonitor_THREAD != null) devicesmonitor_THREAD.Abort();

            devicesmonitor_THREAD = new Thread(new ThreadStart(DevicesMonitor_Start));
            devicesmonitor_THREAD.Start();
        }

        void DevicesMonitor_Start()
        {
            monitorstop = new ManualResetEvent(false);

            while (!monitorstop.WaitOne(0, true))
            {
                DevicesMonitor_Worker();

                Thread.Sleep(10000);
            }
        }

        void DevicesMonitor_Stop()
        {
            if (monitorstop != null) monitorstop.Set();
        }

        private int attempts = 0;

        void DevicesMonitor_Worker()
        {
            // Retrieves a list of devices either by an API request (if user is logged in) or by reading the local 'Devices' folder (if not logged in)
            // If user is logged in, request is sent to API to Get Devices and if after three consecutive request failures all devices are removed

            List<DeviceConfiguration> configs = null;

            // Get Devices from either API or Device folder
            if (CurrentUser != null) configs = API.Devices.Get(CurrentUser);
            else configs = DeviceConfiguration.ReadAll(FileLocations.Devices).ToList();

            if (configs != null)
            {
                attempts = 0;

                if (configs.Count > 0)
                {
                    foreach (DeviceConfiguration config in configs)
                    {
                        if (config != null)
                        {
                            int index = -1;

                            if (CurrentUser != null) index = Devices.FindIndex(x => x.Configuration.UniqueId == config.UniqueId);
                            else index = Devices.FindIndex(x => x.Configuration.FilePath == config.FilePath);

                            if (index >= 0) // Server is already part of list
                            {
                                var server = Devices[index];

                                // Check if Configuration has changed
                                if (server.Configuration.UpdateId != config.UpdateId || !server.IsRunning)
                                {
                                    // If changed at all then stop the current server
                                    server.Stop();

                                    // If changed and still enabled then restart server
                                    if (config.Enabled)
                                    {
                                        server.Configuration = config;
                                        server.Start();

                                        UpdateLoginInformation(server);
                                    }
                                    else // Remove from List
                                    {
                                        Devices.RemoveAt(index);
                                        Logger.Log("Device Removed :: " + server.Configuration.Description.Description + " [" + server.Configuration.Description.DeviceId + "]");
                                    }
                                }
                            }
                            else // Create & Add Device Server
                            {
                                if (config.Enabled) AddDevice(config);
                            }
                        }
                    }

                    // Find devices that were removed
                    foreach (var device in Devices.ToList())
                    {
                        if (!configs.Exists(x => x.UniqueId == device.Configuration.UniqueId))
                        {
                            var d = Devices.Find(x => x.Configuration.UniqueId == device.Configuration.UniqueId);
                            if (d != null)
                            {
                                d.Stop();

                                Devices.Remove(d);
                            }
                        }
                    }
                }
                else RemoveAllDevices();
            }
            else if (CurrentUser == null || attempts > 2)
            {
                RemoveAllDevices();
            }
            else
            {
                attempts++;
            }         
        }

        private void RemoveAllDevices()
        {
            attempts = 0;

            if (Devices != null)
            {
                foreach (var device in Devices) device.Stop();

                Devices.Clear();
            }
        }
        
        #endregion

        private void SendPluginData(string id, string message)
        {
            foreach (var device in Devices)
            {
                device.SendPluginsData(id, message);
            }
        }

    }
}
