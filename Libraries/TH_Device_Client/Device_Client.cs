using System;
using System.Collections.Generic;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Threading;

using TH_Configuration;
using TH_Ping;
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

            DBPing_Initialize();

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

        #region "Ping"

        MySQLPing DBPing;

        void DBPing_Initialize()
        {
            // Start ping for MySQL database connection detection
            DBPing = new MySQLPing();
            DBPing.Settings = configuration.SQL;
            DBPing.MySQLPingResult += DBPing_MySQLPingResult;
            DBPing.Start();
        }

        void DBPing_MySQLPingResult(bool PingResult) 
        { 
            Update(PingResult);
            DBPing.SuccessInterval = Math.Max(1000, UpdateInterval);
        }

        #endregion

        #region "Update"

        ReturnData returnData;

        void Update(bool pingResult)
        {
            if (Data_Plugins != null)
            {
                foreach (Lazy<Data_PlugIn> ldp in Data_Plugins.ToList())
                {
                    Data_PlugIn dp = ldp.Value;
                    dp.Run();
                }
            }

            ReturnData rd = returnData.Copy();

            rd.connected = pingResult;
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
            pluginsPath = TH_Global.FileLocations.TrakHound + @"\Plugins\Data\";
            if (Directory.Exists(pluginsPath)) LoadDataPlugins(pluginsPath);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            pluginsPath = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\Data\";
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
                IEnumerable<int> priorities = Data_Plugins.Select(x => x.Value.Priority).Distinct();

                List<int> sortedPriorities = priorities.ToList();
                sortedPriorities.Sort();

                foreach (int priority in sortedPriorities)
                {
                    List<Lazy<Data_PlugIn>> pluginsAtPriority = Data_Plugins.FindAll(x => x.Value.Priority == priority);

                    foreach (Lazy<Data_PlugIn> LDP in pluginsAtPriority)
                    {
                        Data_PlugIn DP = LDP.Value;
                        DP.DataEvent -= DataPlugIns_Update;
                        DP.DataEvent += DataPlugIns_Update;
                        DP.Initialize(Config);
                        Console.WriteLine(DP.Name + " Initialized!");
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
                    if (ldp.IsValueCreated)
                    {
                        UpdateWorkerInfo info = new UpdateWorkerInfo();
                        info.dataPlugin = ldp.Value;
                        info.de_d = de_d;

                        ThreadPool.QueueUserWorkItem(new WaitCallback(DataPlugin_Update_Worker), info);
                    }
                }


                //IEnumerable<int> priorities = Data_Plugins.Select(x => x.Value.Priority).Distinct();

                //List<int> sortedPriorities = priorities.ToList();
                //sortedPriorities.Sort();

                //foreach (int priority in sortedPriorities)
                //{
                //    List<Lazy<Data_PlugIn>> pluginsAtPriority = Data_Plugins.FindAll(x => x.Value.Priority == priority);

                //    foreach (Lazy<Data_PlugIn> LDP in pluginsAtPriority)
                //    {
                //        Data_PlugIn DP = LDP.Value;
                //        DP.Update_DataEvent(de_d);
                //    }
                //}
            }

            //lock (returnData)
            //{
            //    // Add or Update new data in ReturnData
            //    if (returnData.data.ContainsKey(de_d.id))
            //    {
            //        returnData.data[de_d.id] = de_d.data;
            //    }
            //    else
            //    {
            //        returnData.data.Add(de_d.id, de_d.data);
            //    }
            //}  
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
