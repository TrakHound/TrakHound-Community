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
using TH_Plugins_Server;

namespace TH_Device_Server
{
    public partial class Device_Server
    {
        public delegate void DataEvent_Handler(DataEvent_Data de_data);
        public event DataEvent_Handler DataEvent;

        IEnumerable<Lazy<IServerPlugin>> plugins { get; set; }

        public List<Lazy<IServerPlugin>> Plugins { get; set; }

        Plugs PLUGS;

        class Plugs
        {
            [ImportMany(typeof(IServerPlugin))]
            public IEnumerable<Lazy<IServerPlugin>> Plugins { get; set; }
        }

        public void LoadPlugins()
        {
            string plugin_rootpath = FileLocations.Plugins + @"\Server";

            if (!Directory.Exists(plugin_rootpath)) Directory.CreateDirectory(plugin_rootpath);

            Plugins = new List<Lazy<IServerPlugin>>();

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
            Logger.Log(Plugins.Count.ToString() + " Plugins Found");
            Logger.Log("------------------------------");
            foreach (Lazy<IServerPlugin> lplugin in Plugins)
            {
                IServerPlugin plugin = lplugin.Value;

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

        void LoadPlugins(string Path)
        {
            if (Directory.Exists(Path))
            {
                try
                {
                    PLUGS = new Plugs();

                    var PageCatalog = new DirectoryCatalog(Path);
                    var PageContainer = new CompositionContainer(PageCatalog);
                    PageContainer.SatisfyImportsOnce(PLUGS);

                    plugins = PLUGS.Plugins;

                    foreach (Lazy<IServerPlugin> lplugin in plugins)
                    {
                        IServerPlugin plugin = lplugin.Value;

                        if (Plugins.ToList().Find(x => x.Value.Name.ToLower() == plugin.Name.ToLower()) == null)
                        {
                            Plugins.Add(lplugin);
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("LoadPlugins() : Exception : " + ex.Message); }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(Path))
                {
                    LoadPlugins(directory);
                }
            }
            else Logger.Log("Plugins Directory Doesn't Exist (" + Path + ")");
        }


        class InitializeWorkerInfo
        {
            public Configuration config { get; set; }
            public bool useDatabases { get; set; }
            public IServerPlugin tablePlugin { get; set; }
        }

        class ComponentWorkerInfo
        {
            public TH_MTConnect.Components.ReturnData returnData { get; set; }
            public IServerPlugin tablePlugin { get; set; }
        }

        class StreamWorkerInfo
        {
            public TH_MTConnect.Streams.ReturnData returnData { get; set; }
            public IServerPlugin tablePlugin { get; set; }
        }

        class DataEventWorkerInfo
        {
            public DataEvent_Data de_data { get; set; }
            public IServerPlugin tablePlugin { get; set; }
        }

        void Plugins_Initialize(Configuration Config, bool useDatabases = true)
        {
            if (Plugins != null && Config != null)
            {
                foreach (Lazy<IServerPlugin> lplugin in Plugins.ToList())
                {
                    IServerPlugin plugin = lplugin.Value;

                    InitializeWorkerInfo info = new InitializeWorkerInfo();
                    info.config = Config;
                    info.useDatabases = useDatabases;
                    info.tablePlugin = plugin;

                    Plugin_Initialize_Worker(info);
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(Plugin_Initialize_Worker), info);
                }
            }
        }

        void Plugin_Initialize_Worker(object o)
        {
            InitializeWorkerInfo info = (InitializeWorkerInfo)o;

            try
            {
                IServerPlugin tpi = info.tablePlugin;
                tpi.TablePrefix = TablePrefix;
                tpi.UseDatabases = info.useDatabases;
                tpi.DataEvent -= Plugin_Update_DataEvent;
                tpi.DataEvent += Plugin_Update_DataEvent;
                tpi.StatusChanged += tpi_StatusChanged;
                tpi.Initialize(info.config);
            }
            catch (Exception ex)
            {
                Log("Plugin Exception! : " + ex.Message);
            }
        }

        void tpi_StatusChanged(string status)
        {
            UpdateProcessingStatus(status);
        }


        void Plugins_Update_Probe(TH_MTConnect.Components.ReturnData returnData)
        {
            if (Plugins != null)
            {
                foreach (Lazy<IServerPlugin> lplugin in Plugins.ToList())
                {
                    IServerPlugin plugin = lplugin.Value;

                    ComponentWorkerInfo info = new ComponentWorkerInfo();
                    info.returnData = returnData;
                    info.tablePlugin = plugin;

                    Plugins_Update_Probe_Worker(info);
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(Plugins_Update_Probe_Worker), info);
                }
            }
        }

        void Plugins_Update_Probe_Worker(object o)
        {
            ComponentWorkerInfo info = (ComponentWorkerInfo)o;

            if (info != null)
            {
                if (info.tablePlugin != null)
                {
                    try
                    {
                        IServerPlugin tpi = info.tablePlugin;
                        tpi.Update_Probe(info.returnData);
                    }
                    catch (Exception ex)
                    {
                        Log("Plugin Exception : Probe : " + info.tablePlugin.Name + " : " + ex.Message);
                    }
                }
            }
        }


        void Plugins_Update_Current(TH_MTConnect.Streams.ReturnData returnData)
        {
            if (Plugins != null)
            {
                foreach (Lazy<IServerPlugin> lplugin in Plugins.ToList())
                {
                    IServerPlugin plugin = lplugin.Value;

                    StreamWorkerInfo info = new StreamWorkerInfo();
                    info.returnData = returnData;
                    info.tablePlugin = plugin;

                    Plugins_Update_Current_Worker(info);
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(Plugins_Update_Current_Worker), info);
                }
            }
        }

        void Plugins_Update_Current_Worker(object o)
        {
            StreamWorkerInfo info = (StreamWorkerInfo)o;

            if (info != null)
            {
                if (info.tablePlugin != null)
                {
                    try
                    {
                        IServerPlugin tpi = info.tablePlugin;
                        tpi.Update_Current(info.returnData);
                    }
                    catch (Exception ex)
                    {
                        Log("Plugin Exception : Current : " + info.tablePlugin.Name + " : " + ex.Message);
                    }
                }
            }
        }


        void Plugins_Update_Sample(TH_MTConnect.Streams.ReturnData returnData)
        {
            if (Plugins != null)
            {
                foreach (Lazy<IServerPlugin> lplugin in Plugins.ToList())
                {
                    IServerPlugin plugin = lplugin.Value;

                    StreamWorkerInfo info = new StreamWorkerInfo();
                    info.returnData = returnData;
                    info.tablePlugin = plugin;

                    Plugins_Update_Sample_Worker(info);
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(Plugins_Update_Sample_Worker), info);
                }
            }
        }

        void Plugins_Update_Sample_Worker(object o)
        {
            StreamWorkerInfo info = (StreamWorkerInfo)o;

            if (info != null)
            {
                if (info.tablePlugin != null)
                {
                    try
                    {
                        IServerPlugin tpi = info.tablePlugin;
                        tpi.Update_Sample(info.returnData);
                    }
                    catch (Exception ex)
                    {
                        Log("Plugin Exception : Sample : " + info.tablePlugin.Name + " : " + ex.Message);
                    }
                }
            }
        }


        void Plugin_Update_DataEvent(DataEvent_Data de_data)
        {
            if (Plugins != null)
            {
                foreach (Lazy<IServerPlugin> lplugin in Plugins.ToList())
                {
                    IServerPlugin plugin = lplugin.Value;

                    DataEventWorkerInfo info = new DataEventWorkerInfo();
                    info.de_data = de_data;
                    info.tablePlugin = plugin;

                    Plugin_Update_DataEvent_Worker(info);
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(Plugin_Update_DataEvent_Worker), info);
                }
            }

            if (DataEvent != null) DataEvent(de_data);
        }

        void Plugin_Update_DataEvent_Worker(object o)
        {
            DataEventWorkerInfo info = (DataEventWorkerInfo)o;

            if (info != null)
            {
                if (info.tablePlugin != null)
                {
                    try
                    {
                        IServerPlugin tpi = info.tablePlugin;
                        tpi.Update_DataEvent(info.de_data);
                    }
                    catch (Exception ex)
                    {
                        Log("Plugin Exception : DataEvent : " + info.tablePlugin.Name + " : " + ex.Message);
                    }
                }
            }
        }


        void Plugins_Closing()
        {
            if (Plugins != null)
            {
                foreach (Lazy<IServerPlugin> lplugin in Plugins.ToList())
                {
                    IServerPlugin plugin = lplugin.Value;
                    Plugin_Closing_Worker(plugin);
                }
            }
        }

        void Plugin_Closing_Worker(object o)
        {
            IServerPlugin tp = (IServerPlugin)o;

            try
            {
                tp.Closing();
            }
            catch (Exception ex)
            {
                Log("Plugin Exception! : " + ex.Message);
            }
        }

    }
}
