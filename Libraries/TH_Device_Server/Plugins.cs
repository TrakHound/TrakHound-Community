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
using TH_PlugIns_Server;

namespace TH_Device_Server
{
    public partial class Device_Server
    {
        public delegate void DataEvent_Handler(DataEvent_Data de_data);
        public event DataEvent_Handler DataEvent;

        public IEnumerable<Lazy<Table_PlugIn>> TablePlugIns { get; set; }

        public List<Lazy<Table_PlugIn>> Table_Plugins { get; set; }

        TablePlugs TPLUGS;

        class TablePlugs
        {
            [ImportMany(typeof(Table_PlugIn))]
            public IEnumerable<Lazy<Table_PlugIn>> PlugIns { get; set; }
        }

        public void LoadPlugins()
        {
            //UpdateProcessingStatus("Loading Plugins...");

            string plugin_rootpath = FileLocations.Plugins + @"\Server";

            if (!Directory.Exists(plugin_rootpath)) Directory.CreateDirectory(plugin_rootpath);

            Table_Plugins = new List<Lazy<Table_PlugIn>>();

            string pluginsPath;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            pluginsPath = TH_Global.FileLocations.Plugins + @"\Server\";
            if (Directory.Exists(pluginsPath)) LoadTablePlugins(pluginsPath);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            pluginsPath = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";
            if (Directory.Exists(pluginsPath)) LoadTablePlugins(pluginsPath);

            TablePlugIns = Table_Plugins;

            Console.WriteLine("Table Plugins --------------------------");
            Console.WriteLine(Table_Plugins.Count.ToString() + " Table Plugins Found");
            Console.WriteLine("------------------------------");
            foreach (Lazy<Table_PlugIn> ltp in Table_Plugins)
            {
                Table_PlugIn tp = ltp.Value;

                string name = tp.Name;
                string version = null;

                 // Version Info
                    Assembly assembly = Assembly.GetAssembly(tp.GetType());
                    if (assembly != null)
                    {
                        Version v = assembly.GetName().Version;
                        version = "v" + v.Major.ToString() + "." + v.Minor.ToString() + "." + v.Build.ToString() + "." + v.Revision.ToString();
                    }

                    Console.WriteLine(tp.Name + " : " + version);
            }
            Console.WriteLine("----------------------------------------");



            //ClearProcessingStatus();
        }

        void LoadTablePlugins(string Path)
        {
            //Logger.Log("Searching for Table Plugins in '" + Path + "'");
            if (Directory.Exists(Path))
            {
                try
                {
                    TPLUGS = new TablePlugs();

                    var PageCatalog = new DirectoryCatalog(Path);
                    var PageContainer = new CompositionContainer(PageCatalog);
                    PageContainer.SatisfyImportsOnce(TPLUGS);

                    TablePlugIns = TPLUGS.PlugIns;

                    foreach (Lazy<Table_PlugIn> ltp in TablePlugIns)
                    {
                        Table_PlugIn tp = ltp.Value;

                        if (Table_Plugins.ToList().Find(x => x.Value.Name.ToLower() == tp.Name.ToLower()) == null)
                        {
                            //Logger.Log(tp.Name + " : PlugIn Found");
                            Table_Plugins.Add(ltp);
                        }
                        else
                        {
                            //Logger.Log(tp.Name + " : PlugIn Already Found");
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("LoadTablePlugins() : Exception : " + ex.Message); }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(Path))
                {
                    LoadTablePlugins(directory);
                }
            }
            else Logger.Log("Table PlugIns Directory Doesn't Exist (" + Path + ")");
        }


        class InitializeWorkerInfo
        {
            public Configuration config { get; set; }
            public bool useDatabases { get; set; }
            public Table_PlugIn tablePlugin { get; set; }
        }

        class ComponentWorkerInfo
        {
            public TH_MTC_Data.Components.ReturnData returnData { get; set; }
            public Table_PlugIn tablePlugin { get; set; }
        }

        class StreamWorkerInfo
        {
            public TH_MTC_Data.Streams.ReturnData returnData { get; set; }
            public Table_PlugIn tablePlugin { get; set; }
        }

        class DataEventWorkerInfo
        {
            public DataEvent_Data de_data { get; set; }
            public Table_PlugIn tablePlugin { get; set; }
        }

        void TablePlugIns_Initialize(Configuration Config, bool useDatabases = true)
        {
            if (TablePlugIns != null && Config != null)
            {
                foreach (Lazy<Table_PlugIn> ltp in TablePlugIns.ToList())
                {
                    Table_PlugIn tp = ltp.Value;

                    InitializeWorkerInfo info = new InitializeWorkerInfo();
                    info.config = Config;
                    info.useDatabases = useDatabases;
                    info.tablePlugin = tp;

                    TablePlugIn_Initialize_Worker(info);
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(TablePlugIn_Initialize_Worker), info);
                }
            }
        }

        void TablePlugIn_Initialize_Worker(object o)
        {
            InitializeWorkerInfo info = (InitializeWorkerInfo)o;

            try
            {
                Table_PlugIn tpi = info.tablePlugin;
                tpi.TablePrefix = TablePrefix;
                tpi.UseDatabases = info.useDatabases;
                tpi.DataEvent -= TablePlugIn_Update_DataEvent;
                tpi.DataEvent += TablePlugIn_Update_DataEvent;
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


        void TablePlugIns_Update_Probe(TH_MTC_Data.Components.ReturnData returnData)
        {
            if (TablePlugIns != null)
            {
                foreach (Lazy<Table_PlugIn> ltp in TablePlugIns.ToList())
                {
                    Table_PlugIn tp = ltp.Value;

                    ComponentWorkerInfo info = new ComponentWorkerInfo();
                    info.returnData = returnData;
                    info.tablePlugin = tp;

                    TablePlugIns_Update_Probe_Worker(info);
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(TablePlugIns_Update_Probe_Worker), info);
                }
            }
        }

        void TablePlugIns_Update_Probe_Worker(object o)
        {
            ComponentWorkerInfo info = (ComponentWorkerInfo)o;

            if (info != null)
            {
                if (info.tablePlugin != null)
                {
                    try
                    {
                        Table_PlugIn tpi = info.tablePlugin;
                        tpi.Update_Probe(info.returnData);
                    }
                    catch (Exception ex)
                    {
                        Log("Plugin Exception : Probe : " + info.tablePlugin.Name + " : " + ex.Message);
                    }
                }
            }
        }


        void TablePlugIns_Update_Current(TH_MTC_Data.Streams.ReturnData returnData)
        {
            if (TablePlugIns != null)
            {
                foreach (Lazy<Table_PlugIn> ltp in TablePlugIns.ToList())
                {
                    Table_PlugIn tp = ltp.Value;

                    StreamWorkerInfo info = new StreamWorkerInfo();
                    info.returnData = returnData;
                    info.tablePlugin = tp;

                    TablePlugIns_Update_Current_Worker(info);
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(TablePlugIns_Update_Current_Worker), info);
                }
            }
        }

        void TablePlugIns_Update_Current_Worker(object o)
        {
            StreamWorkerInfo info = (StreamWorkerInfo)o;

            if (info != null)
            {
                if (info.tablePlugin != null)
                {
                    try
                    {
                        Table_PlugIn tpi = info.tablePlugin;
                        tpi.Update_Current(info.returnData);
                    }
                    catch (Exception ex)
                    {
                        Log("Plugin Exception : Current : " + info.tablePlugin.Name + " : " + ex.Message);
                    }
                }
            }
        }


        void TablePlugIns_Update_Sample(TH_MTC_Data.Streams.ReturnData returnData)
        {
            if (TablePlugIns != null)
            {
                foreach (Lazy<Table_PlugIn> ltp in TablePlugIns.ToList())
                {
                    Table_PlugIn tp = ltp.Value;

                    StreamWorkerInfo info = new StreamWorkerInfo();
                    info.returnData = returnData;
                    info.tablePlugin = tp;

                    TablePlugIns_Update_Sample_Worker(info);
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(TablePlugIns_Update_Sample_Worker), info);
                }
            }
        }

        void TablePlugIns_Update_Sample_Worker(object o)
        {
            StreamWorkerInfo info = (StreamWorkerInfo)o;

            if (info != null)
            {
                if (info.tablePlugin != null)
                {
                    try
                    {
                        Table_PlugIn tpi = info.tablePlugin;
                        tpi.Update_Sample(info.returnData);
                    }
                    catch (Exception ex)
                    {
                        Log("Plugin Exception : Sample : " + info.tablePlugin.Name + " : " + ex.Message);
                    }
                }
            }
        }


        void TablePlugIn_Update_DataEvent(DataEvent_Data de_data)
        {
            if (TablePlugIns != null)
            {
                foreach (Lazy<Table_PlugIn> ltp in TablePlugIns.ToList())
                {
                    Table_PlugIn tp = ltp.Value;

                    DataEventWorkerInfo info = new DataEventWorkerInfo();
                    info.de_data = de_data;
                    info.tablePlugin = tp;

                    TablePlugIn_Update_DataEvent_Worker(info);
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(TablePlugIn_Update_DataEvent_Worker), info);
                }
            }

            if (DataEvent != null) DataEvent(de_data);
        }

        void TablePlugIn_Update_DataEvent_Worker(object o)
        {
            DataEventWorkerInfo info = (DataEventWorkerInfo)o;

            if (info != null)
            {
                if (info.tablePlugin != null)
                {
                    try
                    {
                        Table_PlugIn tpi = info.tablePlugin;
                        tpi.Update_DataEvent(info.de_data);
                    }
                    catch (Exception ex)
                    {
                        Log("Plugin Exception : DataEvent : " + info.tablePlugin.Name + " : " + ex.Message);
                    }
                }
            }
        }


        void TablePlugIns_Closing()
        {
            if (TablePlugIns != null)
            {
                foreach (Lazy<Table_PlugIn> ltp in TablePlugIns.ToList())
                {
                    Table_PlugIn tp = ltp.Value;
                    TablePlugIn_Closing_Worker(tp);

                    //if (tp.IsValueCreated)
                    //{
                    //    TablePlugIn_Closing_Worker
                    //    ThreadPool.QueueUserWorkItem(new WaitCallback(TablePlugIn_Closing_Worker), tp.Value);
                    //}
                }
            }
        }

        void TablePlugIn_Closing_Worker(object o)
        {
            Table_PlugIn tp = (Table_PlugIn)o;

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
