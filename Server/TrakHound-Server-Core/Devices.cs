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
            List<Configuration> configs = null;

            if (CurrentUser != null)
            {
                configs = Configurations.GetConfigurationsListForUser(CurrentUser);
            }
            else
            {
                configs = Configuration.ReadAll(FileLocations.Devices).ToList();
            }

            if (configs != null)
            {
                if (configs.Count > 0)
                {
                    foreach (Configuration config in configs)
                    {
                        if (config != null)
                        {
                            int index = -1;

                            if (CurrentUser != null) index = Devices.FindIndex(x => x.configuration.UniqueId == config.UniqueId);
                            else index = Devices.FindIndex(x => x.configuration.FilePath == config.FilePath);

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

        #endregion

    }
}
