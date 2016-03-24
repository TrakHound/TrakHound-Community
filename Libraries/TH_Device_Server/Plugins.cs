// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;

using TH_Configuration;
using TH_Global;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_Device_Server
{
    public partial class Device_Server
    {
        public event SendData_Handler SendData;


        List<IServerPlugin> plugins { get; set; }

        public void LoadPlugins()
        {
            string plugin_rootpath = FileLocations.Plugins + @"\Server";

            if (!Directory.Exists(plugin_rootpath)) Directory.CreateDirectory(plugin_rootpath);

            plugins = new List<IServerPlugin>();

            string pluginsPath;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            pluginsPath = plugin_rootpath;
            if (Directory.Exists(pluginsPath)) LoadPlugins(pluginsPath);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            pluginsPath = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(pluginsPath)) LoadPlugins(pluginsPath);

            pluginsPath = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";
            if (Directory.Exists(pluginsPath)) LoadPlugins(pluginsPath);

            Logger.Log("Server Plugins --------------------------");
            Logger.Log(plugins.Count.ToString() + " Plugins Found");
            Logger.Log("------------------------------");
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

                Logger.Log(plugin.Name + " : " + version);
            }
            Logger.Log("----------------------------------------");
        }

        void LoadPlugins(string path)
        {
            if (Directory.Exists(path))
            {
                var foundPlugins = TH_Plugins.Reader.FindPlugins<IServerPlugin>(path, new ServerPlugin.PluginContainer(), ServerPlugin.PLUGIN_EXTENSION);
                foreach (var plugin in foundPlugins)
                {
                    if (!plugins.Exists(x => x.Name.ToLower() == plugin.Name.ToLower()))
                    {
                        plugins.Add(plugin);
                    }
                }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(path))
                {
                    LoadPlugins(directory);
                }
            }
            else Logger.Log("Plugins Directory Doesn't Exist (" + path + ")");
        }

        void Plugins_Initialize(Configuration config)
        {
            if (plugins != null && config != null)
            {
                foreach (var plugin in plugins)
                {
                    try
                    {
                        //plugin.TablePrefix = TablePrefix;
                        //plugin.UseDatabases = useDatabases;
                        plugin.SendData -= Plugins_Update_SendData;
                        plugin.SendData += Plugins_Update_SendData;
                        //plugin.StatusChanged += tpi_StatusChanged;
                        plugin.Initialize(config);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Initialize :: Exception :: " + plugin.Name + " :: " + ex.Message);
                    }
                }
            }
        }

        void tpi_StatusChanged(string status)
        {
            //UpdateProcessingStatus(status);
        }

        void Plugins_Update_SendData(EventData data)
        {
            if (plugins != null)
            {
                foreach (var plugin in plugins)
                {
                    try
                    {
                        plugin.GetSentData(data);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Plugin :: Exception : " + plugin.Name + " :: " + ex.Message);
                    }
                }
            }

            if (SendData != null) SendData(data);
        }

        void Plugins_Closing()
        {
            if (plugins != null)
            {
                foreach (var plugin in plugins)
                {
                    try
                    {
                        plugin.Closing();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Plugin :: Exception :: " + plugin.Name + " :: " + ex.Message);
                    }
                }
            }
        }

    }
}
