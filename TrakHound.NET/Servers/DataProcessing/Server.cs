// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Logging;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;

namespace TrakHound.Servers.DataProcessing
{
    public partial class ProcessingServer
    {
        public ProcessingServer()
        {
            //PrintHeader();

            // Set Logger Application identifier name
            Logger.AppicationName = "TrakHound-Server";

            // Insure all standard TrakHound directories are created
            FileLocations.CreateAllDirectories();

            // Read the API Configuration file
            ApiConfiguration.Set(ApiConfiguration.Read());

            // Read Server Plugins
            LoadServerPlugins();

            // Start User login file monitor
            var loginMonitor = new ServerCredentials.Monitor();
            loginMonitor.UserChanged += LoginMonitor_UserChanged;

            // Start API Configuration file monitor
            var apiMonitor = new ApiConfiguration.Monitor();
            apiMonitor.ApiConfigurationChanged += ApiMonitor_ApiConfigurationChanged;
        }

        public delegate void StatusChanged_Handler();
        public event StatusChanged_Handler Started;
        public event StatusChanged_Handler Stopped;

        public bool IsRunnning;

        public void Start()
        {
            LoadDevices();

            IsRunnning = true;

            Started?.Invoke();
        }

        public void Stop()
        {
            foreach (var device in Devices)
            {
                if (device != null) device.Stop();
            }

            DevicesMonitor_Stop();
            if (devicesmonitor_THREAD != null) devicesmonitor_THREAD.Abort();

            IsRunnning = false;

            Stopped?.Invoke();
        }

        private void LoginMonitor_UserChanged(ServerCredentials.LoginData loginData)
        {
            Login(loginData);
        }

        private void ApiMonitor_ApiConfigurationChanged(ApiConfiguration config)
        {
            ApiConfiguration.Set(config);
            Login();
        }

        #region "Server Plugins"

        private List<IServerPlugin> serverPlugins;

        private void LoadServerPlugins()
        {
            serverPlugins = new List<IServerPlugin>();

            string pluginsPath;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            pluginsPath = FileLocations.Plugins;
            if (Directory.Exists(pluginsPath)) LoadServerPlugins(pluginsPath);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            pluginsPath = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(pluginsPath)) LoadServerPlugins(pluginsPath);

            // Load from running assembly (ex. TrakHound-Server)
            LoadServerPlugins(Assembly.GetEntryAssembly());

            PrintServerPluginInfo(serverPlugins);
        }

        private void LoadServerPlugins(string path)
        {
            if (Directory.Exists(path))
            {
                var foundPlugins = Reader.FindPlugins<IServerPlugin>(path, new ServerPlugin.PluginContainer());
                foreach (var plugin in foundPlugins)
                {
                    if (!serverPlugins.Exists(x => x.Name.ToLower() == plugin.Name.ToLower()))
                    {
                        serverPlugins.Add(plugin);
                    }
                }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(path))
                {
                    LoadServerPlugins(directory);
                }
            }
            else Logger.Log("Plugins Directory Doesn't Exist (" + path + ")", LogLineType.Warning);
        }

        private void LoadServerPlugins(Assembly assembly)
        {
            var foundPlugins = Reader.FindPlugins<IServerPlugin>(assembly, new ServerPlugin.PluginContainer());
            foreach (var plugin in foundPlugins)
            {
                if (!serverPlugins.Exists(x => x.Name.ToLower() == plugin.Name.ToLower()))
                {
                    serverPlugins.Add(plugin);
                }
            }
        }

        private static void PrintServerPluginInfo(List<IServerPlugin> plugins)
        {
            Logger.Log("Server Plugins --------------------------", LogLineType.Console);
            Logger.Log(plugins.Count.ToString() + " Plugins Found", LogLineType.Console);
            Logger.Log("------------------------------", LogLineType.Console);
            foreach (var plugin in plugins)
            {
                string name = plugin.Name;
                string version = null;

                // Version Info
                Assembly assembly = Assembly.GetAssembly(plugin.GetType());
                if (assembly != null)
                {
                    Version v = assembly.GetName().Version;
                    version = "v" + v.Major.ToString() + "." + v.Minor.ToString() + "." + v.Build.ToString() + "." + v.Revision.ToString();
                }

                Logger.Log(plugin.Name + " : " + version, LogLineType.Notification);
            }
            Logger.Log("----------------------------------------", LogLineType.Console);
        }

        #endregion


    }
}
