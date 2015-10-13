using System;
using System.Collections.Generic;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Threading;

using TH_Configuration;
using TH_PlugIns_Client_Data;
using TH_Global;

namespace TH_Device_Client
{
    public class Device_Client
    {

        public Device_Client(Configuration config)
        {

            configuration = config;

            returnData = new ReturnData();

            LoadPlugins();

            DataPlugIns_Initialize(config);

            Update_TIMER = new System.Timers.Timer();
            Update_TIMER.Interval = Math.Max(1000, UpdateInterval);
            Update_TIMER.Elapsed += Update_TIMER_Elapsed;
            Update_TIMER.Enabled = true;

        }

        void Update_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Update_TIMER.Enabled = false;
            Update_TIMER.Interval = Math.Max(1000, UpdateInterval);

            Update();

            Update_TIMER.Enabled = true;
        }

        #region "Properties"

        public Configuration configuration { get; set; }

        public int UpdateInterval { get; set; }

        private int index = -1;
        public int Index 
        {
            get { return index; }
            set
            {
                index = value;
                if (returnData != null) returnData.index = index;
            }
        }

        #endregion

        #region "Update"

        System.Timers.Timer Update_TIMER;

        ReturnData returnData;

        void Update()
        {
            if (Data_Plugins != null)
            {
                foreach (Lazy<Data_PlugIn> ldp in Data_Plugins.ToList())
                {
                    try
                    {
                        Data_PlugIn dp = ldp.Value;
                        dp.Run();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Update : Exception : " + ex.Message);
                    }
                }
            }

            ReturnData rd = returnData.Copy();

            rd.configuration = configuration;

            // Raise DataUpdated event and send ReturnData object as parameter
            if (DataUpdated != null) DataUpdated(rd);

        }

        public delegate void DataUpdated_Handler(ReturnData rd);
        public event DataUpdated_Handler DataUpdated;

        #endregion

        #region "Plugins"

        public List<Lazy<Data_PlugIn>> Data_Plugins { get; set; }

        DataPlugs TPLUGS;

        class DataPlugs
        {
            [ImportMany(typeof(Data_PlugIn))]
            public IEnumerable<Lazy<Data_PlugIn>> PlugIns { get; set; }
        }

        void LoadPlugins()
        {
            Data_Plugins = new List<Lazy<Data_PlugIn>>();

            string pluginsPath;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            pluginsPath = TH_Global.FileLocations.Plugins + @"\Data\";
            //pluginsPath = TH_Global.FileLocations.TrakHound + @"\Plugins\Data\";
            if (Directory.Exists(pluginsPath)) LoadDataPlugins(pluginsPath);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            pluginsPath = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";
            if (Directory.Exists(pluginsPath)) LoadDataPlugins(pluginsPath);
        }

        void LoadDataPlugins(string Path)
        {
            Console.WriteLine("Searching for Data Plugins in '" + Path + "'");
            if (Directory.Exists(Path))
            {
                TPLUGS = new DataPlugs();

                var PageCatalog = new DirectoryCatalog(Path);
                var PageContainer = new CompositionContainer(PageCatalog);
                PageContainer.SatisfyImportsOnce(TPLUGS);

                List<Lazy<Data_PlugIn>> dataPlugins = TPLUGS.PlugIns.ToList();

                foreach (Lazy<Data_PlugIn> DP in dataPlugins.ToList())
                {
                    bool found = false;
                    foreach (Lazy<Data_PlugIn> dp in Data_Plugins.ToList())
                    {
                        if (dp.IsValueCreated) if (dp.Value.Name == DP.Value.Name) found = true;       
                    }
                    if (!found)
                    {
                        Data_Plugins.Add(DP);
                        Console.WriteLine("Data Plugin - " + DP.Value.Name + " : Loaded");
                    }
                }

                // Search Subdirectories
                foreach (string directory in Directory.GetDirectories(Path))
                {
                    LoadDataPlugins(directory);
                }
            }
            else Console.WriteLine("Data PlugIns Directory Doesn't Exist (" + Path + ")");
        }

        void DataPlugIns_Initialize(Configuration Config)
        {
            if (Data_Plugins != null)
            {
                foreach (Lazy<Data_PlugIn> ldp in Data_Plugins)
                {
                    try
                    {
                        Data_PlugIn dp = ldp.Value;
                        dp.DataEvent -= DataPlugIns_Update;
                        dp.DataEvent += DataPlugIns_Update;
                        dp.Initialize(Config);
                        Logger.Log(dp.Name + " Initialized!");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("DataPlugIns_Initialize : Exception : " + ex.Message);
                    }
                }
            }
        }

        void DataPlugIns_Update(DataEvent_Data de_d)
        {
            if (Data_Plugins != null)
            {
                foreach (Lazy<Data_PlugIn> ldp in Data_Plugins)
                {
                    try
                    {
                        Data_PlugIn dp = ldp.Value;
                        UpdateWorkerInfo info = new UpdateWorkerInfo();
                        info.dataPlugin = dp;
                        info.de_d = de_d;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(DataPlugin_Update_Worker), info);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("DataPlugIns_Initialize : Exception : " + ex.Message);
                    }
                }
            }
        }

        class UpdateWorkerInfo
        {
            public Data_PlugIn dataPlugin { get; set; }
            public DataEvent_Data de_d { get; set; }
        }

        void DataPlugin_Update_Worker(object o)
        {
            UpdateWorkerInfo info = (UpdateWorkerInfo)o;

            try
            {
                Data_PlugIn DP = info.dataPlugin;
                DP.Update_DataEvent(info.de_d);

                lock (returnData)
                {
                    // Add or Update new data in ReturnData
                    if (returnData.data.ContainsKey(info.de_d.id))
                    {
                        returnData.data[info.de_d.id] = info.de_d.data;
                    }
                    else
                    {
                        returnData.data.Add(info.de_d.id, info.de_d.data);
                    }
                }  
            }
            catch (Exception ex)
            {
                Console.WriteLine("Data Plugin Exception : " + ex.Message);
            }
        }

        #endregion

    }


    public class ReturnData
    {
        public ReturnData()
        {
            data = new Dictionary<string, object>();
        }

        public bool connected { get; set; }

        public int index { get; set; }

        public Configuration configuration { get; set; }

        public Dictionary<string, object> data { get; set; }

        public ReturnData Copy()
        {
            ReturnData Result = new ReturnData();
            Result.connected = connected;
            Result.configuration = configuration;
            Result.data = data;
            return Result;
        }
    }

}
