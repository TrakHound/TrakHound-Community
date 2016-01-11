// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Xml;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_Device_Server;
using TH_Global;
using TH_UserManagement;
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

        void AddDevice(Configuration config)
        {
            config.Index = Devices.Count;

            Device_Server server = new Device_Server(config);

            server.Start(false);

            Devices.Add(server);
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
                DevicesMonitor_Worker(Devices.ToList());

                Thread.Sleep(5000);
            }
        }

        void DevicesMonitor_Stop()
        {
            if (monitorstop != null) monitorstop.Set();
        }

        void DevicesMonitor_Worker(List<Device_Server> devs)
        {
            if (CurrentUser != null)
            {
                List<Configuration> configs = Configurations.GetConfigurationsListForUser(CurrentUser, userDatabaseSettings);
                if (configs != null)
                {
                    if (configs.Count > 0)
                    {
                        foreach (Configuration config in configs)
                        {
                            if (config != null)
                            {
                                int index = Devices.FindIndex(x => x.configuration.UniqueId == config.UniqueId);
                                if (index >= 0) // Server is already part of list
                                {
                                    Device_Server server = Devices[index];

                                    // Check if Configuration has changed
                                    if (server.configuration.ServerUpdateId != config.ServerUpdateId)
                                    {
                                        // If changed at all then stop the current server
                                        server.Stop();

                                        // If changed and still enabled then restart server
                                        if (config.ServerEnabled)
                                        {
                                            server.configuration = config;

                                            // Load/Reload Plugins
                                            server.LoadPlugins();

                                            // Initialize Database Configurations
                                            Global.Initialize(server.configuration.Databases_Server);

                                            server.Start(false);
                                        }
                                        else // Remove from List
                                        {
                                            Devices.RemoveAt(index);
                                            Logger.Log("Device[" + index.ToString() + "] Removed");
                                        }
                                    }
                                }
                                else // Create & Add Device Server
                                {
                                    if (config.ServerEnabled) AddDevice(config);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region "Xml File"

        List<Configuration> ReadConfigurationFile()
        {
            List<Configuration> result = new List<Configuration>();

            string configPath;

            string localPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + "Configuration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "Configuration.Xml";

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            if (System.IO.File.Exists(configPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(configPath);

                foreach (XmlNode Node in doc.DocumentElement.ChildNodes)
                {
                    if (Node.NodeType == XmlNodeType.Element)
                    {
                        switch (Node.Name.ToLower())
                        {
                            case "devices":
                                foreach (XmlNode ChildNode in Node.ChildNodes)
                                {
                                    if (ChildNode.NodeType == XmlNodeType.Element)
                                    {
                                        switch (ChildNode.Name.ToLower())
                                        {
                                            case "device":

                                                Configuration config = GetSettingsFromNode(ChildNode);
                                                if (config != null) result.Add(config);

                                                break;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            return result;
        }

        private Configuration GetSettingsFromNode(XmlNode Node)
        {

            Configuration Result = null;

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

                Result = Configuration.ReadConfigFile(configPath);
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
                    Configuration config = new Configuration();
                    config = Configuration.ReadConfigFile(configPath);

                    if (config != null)
                    {
                        config.Index = index;

                        Device_Server server = new Device_Server(config);
                        server.configurationPath = configPath;
                        server.updateConfigurationFile = false;

                        // Initialize Database Configurations
                        Global.Initialize(server.configuration.Databases_Server);

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
