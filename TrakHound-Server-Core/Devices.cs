// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Xml;
using System;

//using TH_Global.TrakHound.Configurations;
using TH_Database;
using TH_Device_Server;
using TH_Global;
using TH_Global.TrakHound.Configurations;
using TH_Global.TrakHound.Users;
using TH_UserManagement.Management;

namespace TrakHound_Server_Core
{
    public partial class Server
    {
        List<Device_Server> Devices = new List<Device_Server>();

        void LoadDevices()
        {
            Devices.Clear();

            DevicesMonitor_Initialize();
        }

        void AddDevice(DeviceConfiguration config)
        {
            config.Index = Devices.Count;

            ThreadPool.QueueUserWorkItem(new WaitCallback(StartDeviceServer), config); 
        }

        private void StartDeviceServer(object obj)
        {
            var config = (DeviceConfiguration)obj;

            var server = new Device_Server(config);
            Devices.Add(server);
            server.Start();

            UpdateLoginInformation(server);
        }

        private void UpdateLoginInformation(Device_Server server)
        {
            // Send User Login info
            //if (CurrentUser != null) server.SendPluginsData("UserLogin", CurrentUser.Id);
            //else server.SendPluginsData("UserLogin", GetLoginRegistyKey());

            if (CurrentUser != null) server.SendPluginsData("UserLoginId", CurrentUser.Id);
            else server.SendPluginsData("UserLoginId", GetLoginRegistyKey());

            if (CurrentUser != null) server.SendPluginsData("UserLogin", CurrentUser);
            else server.SendPluginsData("UserLogin", null);
        }

        #region "Devices Monitor"

        /// <summary>
        /// Devices Monitor is used to monitor when devices are Changed, Added, or Removed.
        /// 'Changed' includes whether device was Enabled or Disabled.
        /// Monitor runs at a fixed interval of 5 seconds and compares Devices with list of tables for current user
        /// </summary>

        Thread devicesmonitor_THREAD;
        ManualResetEvent monitorstop = null;

        void DevicesMonitor_Initialize()
        {
            monitorstop = new ManualResetEvent(false);

            if (devicesmonitor_THREAD != null) devicesmonitor_THREAD.Abort();

            devicesmonitor_THREAD = new Thread(new ThreadStart(DevicesMonitor_Start));
            devicesmonitor_THREAD.Start();
        }

        void DevicesMonitor_Start()
        {
            while (!monitorstop.WaitOne(0, true))
            {
                DevicesMonitor_Worker();

                Thread.Sleep(5000);
            }
        }

        void DevicesMonitor_Stop()
        {
            if (monitorstop != null) monitorstop.Set();
        }

        void DevicesMonitor_Worker()
        {
            List<DeviceConfiguration> configs = null;

            if (CurrentUser != null)
            {
                configs = TH_Global.TrakHound.Devices.Get(CurrentUser);

                //var userConfig = TH_UserManagement.Management.UserConfiguration.FromNewUserConfiguration(CurrentUser);
                //configs = Configurations.GetConfigurationsListForUser(userConfig);
            }
            else
            {
                configs = DeviceConfiguration.ReadAll(FileLocations.Devices).ToList();
            }

            if (configs != null)
            {
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
                                Device_Server server = Devices[index];

                                // Check if Configuration has changed
                                if (server.Configuration.ServerUpdateId != config.ServerUpdateId)
                                {
                                    // If changed at all then stop the current server
                                    server.Stop();

                                    // If changed and still enabled then restart server
                                    if (config.ServerEnabled)
                                    {
                                        server.Configuration = config;
                                        server.Start();

                                        UpdateLoginInformation(server);
                                    }
                                    else // Remove from List
                                    {
                                        Devices.RemoveAt(index);
                                        //Logger.Log("Device[" + index.ToString() + "] Removed");
                                        Logger.Log("Device Removed :: " + server.Configuration.Description.Description + " [" + server.Configuration.Description.Device_ID + "]");
                                    }
                                }
                            }
                            else // Create & Add Device Server
                            {
                                if (config.ServerEnabled) AddDevice(config);
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
                }
            }
        }

        private DeviceConfiguration GetSettingsFromNode(XmlNode Node)
        {

            DeviceConfiguration Result = null;

            string configPath = null;

            foreach (XmlNode ChildNode in Node.ChildNodes)
            {
                switch (ChildNode.Name.ToLower())
                {
                    case "configuration_path": configPath = ChildNode.InnerText; break;
                }
            }

            if (configPath != null)
            {
                configPath = GetConfigurationPath(configPath);

                Result = DeviceConfiguration.Read(configPath);
            }

            return Result;

        }

        static Device_Server ProcessDevice(int index, XmlNode node)
        {
            Device_Server Result = null;

            string configPath = null;

            foreach (XmlNode childNode in node.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element)
                {
                    if (childNode.Name.ToLower() == "configuration_path")
                    {
                        configPath = childNode.InnerText;
                    }
                }
            }

            if (configPath != null)
            {
                configPath = GetConfigurationPath(configPath);

                Logger.Log("Reading Device Configuration File @ '" + configPath + "'");

                if (File.Exists(configPath))
                {
                    var config = new DeviceConfiguration();
                    config = DeviceConfiguration.Read(configPath);

                    if (config != null)
                    {
                        config.Index = index;

                        Device_Server server = new Device_Server(config);
                        server.ConfigurationPath = configPath;
                        server.UpdateConfigurationFile = false;

                        // Initialize Database Configurations
                        Global.Initialize(server.Configuration.Databases_Server);

                        Result = server;
                    }
                    else Logger.Log("Error Occurred While Reading : " + configPath);
                }
                else Logger.Log("Can't find Device Configuration file @ " + configPath);
            }
            else Logger.Log("No Device Congifuration found");

            return Result;

        }

        static string GetConfigurationPath(string path)
        {
            // If not full path, try System Dir ('C:\TrakHound\') and then local App Dir
            if (!System.IO.Path.IsPathRooted(path))
            {
                // Remove initial Backslash if contained in "configuration_path"
                if (path[0] == '\\' && path.Length > 1) path.Substring(1);

                string original = path;

                // Check System Path
                path = TH_Global.FileLocations.TrakHound + "\\Configuration Files\\" + original;
                if (File.Exists(path)) return path;
                else Logger.Log(path + " Not Found");


                // Check local app Path
                path = AppDomain.CurrentDomain.BaseDirectory + "Configuration Files\\" + original;
                if (File.Exists(path)) return path;
                else Logger.Log(path + " Not Found");

                // if no files exist return null
                return null;
            }
            else return path;
        }

        #endregion

    }
}
