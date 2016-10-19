// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Logging;
using TrakHound.Plugins.Server;
using TrakHound.Tools;

namespace TrakHound.Servers.DataProcessing
{
    public partial class ProcessingServer
    {
        private List<DeviceServer> devices = new List<DeviceServer>();

        private void LoadDevices()
        {
            devices.Clear();

            DevicesMonitor_Initialize();
        }

        private void AddDevice(DeviceConfiguration config)
        {
            if (config != null)
            {
                var deviceThread = new Thread(new ParameterizedThreadStart(StartDeviceServer));
                deviceThread.Start(config);
            }
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
            devices.Add(server);
            server.Start();

            UpdateLoginInformation(server);
        }

        private void Server_Started(DeviceServer server)
        {
            Logger.Log(server.Configuration.UniqueId + " :: Device Started");
        }

        private void Server_Stopped(DeviceServer server)
        {
            Logger.Log(server.Configuration.UniqueId + " :: Device Stopped");
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

        private Thread devicesMonitorThread;
        private ManualResetEvent monitorstop = null;

        private void DevicesMonitor_Initialize()
        {
            if (devicesMonitorThread != null) devicesMonitorThread.Abort();

            devicesMonitorThread = new Thread(new ThreadStart(DevicesMonitor_Start));
            devicesMonitorThread.Start();
        }

        private void DevicesMonitor_Start()
        {
            monitorstop = new ManualResetEvent(false);

            do
            {
                DevicesMonitor_Worker();

            } while (!monitorstop.WaitOne(10000, true));

            Logger.Log("Device Monitor Stopped", LogLineType.Console);
        }

        private void DevicesMonitor_Stop()
        {
            if (monitorstop != null) monitorstop.Set();
        }

        private void DevicesMonitor_Worker()
        {
            // Clean Devices from any possible null DeviceConfigurations
            devices = devices.FindAll(o => o != null);

            if (CurrentUser != null) CheckUserDevices(CurrentUser);
            else CheckLocalDevices();
        }

        private void CheckUserDevices(UserConfiguration userConfig)
        {
            // Get CheckInfo objects from TrakHound API (each object contains the device's UniqueId and UpdateId)
            var checkInfos = API.Devices.Check(userConfig);
            if (checkInfos != null)
            {
                // List of Devices to Get
                var getIds = new List<string>();

                foreach (var checkInfo in checkInfos)
                {
                    int index = -1;

                    index = devices.FindIndex(x => x != null && x.Configuration.UniqueId == checkInfo.UniqueId);
                    if (index >= 0) // Server is already part of list
                    {
                        var server = devices[index];

                        // Check if Configuration has changed
                        if (server.Configuration.UpdateId != checkInfo.UpdateId || !server.IsRunning)
                        {
                            // If changed at all then stop the current server
                            server.Stop();

                            // If changed and still enabled then restart server
                            if (checkInfo.Enabled) getIds.Add(checkInfo.UniqueId);
                            else 
                            {
                                // Remove from List
                                devices.RemoveAt(index);
                            }
                        }
                    }
                    else if (checkInfo.Enabled) getIds.Add(checkInfo.UniqueId);
                }

                // Find devices that were removed
                foreach (var device in devices.ToList())
                {
                    if (!checkInfos.Exists(x => x != null && x.UniqueId == device.Configuration.UniqueId))
                    {
                        var d = devices.Find(x => x != null && x.Configuration.UniqueId == device.Configuration.UniqueId);
                        if (d != null)
                        {
                            d.Stop();

                            devices.Remove(d);
                        }
                    }
                }

                // Get full DeviceConfigurations for each device that have been changed or added
                if (getIds.Count > 0)
                {
                    var configs = API.Devices.Get(userConfig, getIds.ToArray());
                    foreach (var config in configs)
                    {
                        int index = devices.FindIndex(x => x != null && x.Configuration.UniqueId == config.UniqueId);
                        if (index >= 0)
                        {
                            var server = devices[index];
                            server.Stop();

                            server.Configuration = config;
                            server.Start();

                            UpdateLoginInformation(server);
                        }
                        else
                        {
                            AddDevice(config);
                        }
                    }
                }
            }
            else
            {
                RemoveAllDevices();
            }
        }

        private void CheckLocalDevices()
        {
            // Retrieves a list of devices by reading the local 'Devices' folder (if not logged in)

            var configs = DeviceConfiguration.ReadAll(FileLocations.Devices).ToList();
            if (configs != null)
            {
                if (configs.Count > 0)
                {
                    foreach (DeviceConfiguration config in configs)
                    {
                        if (config != null)
                        {
                            int index = -1;

                            index = devices.FindIndex(x => x != null && x.Configuration.UniqueId == config.UniqueId);
                            if (index >= 0) // Server is already part of list
                            {
                                var server = devices[index];

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
                                        devices.RemoveAt(index);
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
                    foreach (var device in devices.ToList())
                    {
                        if (!configs.Exists(x => x != null && x.UniqueId == device.Configuration.UniqueId))
                        {
                            var d = devices.Find(x => x != null && x.Configuration.UniqueId == device.Configuration.UniqueId);
                            if (d != null)
                            {
                                d.Stop();

                                devices.Remove(d);
                            }
                        }
                    }
                }
                else RemoveAllDevices();
            }
            else
            {
                RemoveAllDevices();
            }
        }

        private void RemoveAllDevices()
        {
            if (devices != null)
            {
                foreach (var device in devices) device.Stop();

                devices.Clear();
            }
        }
        
        #endregion

        private void SendPluginData(string id, string message)
        {
            foreach (var device in devices)
            {
                device.SendPluginData(id, message);
            }
        }

    }
}
