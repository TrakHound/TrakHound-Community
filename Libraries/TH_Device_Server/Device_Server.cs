// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TH_Configuration;
using TH_Database;
using TH_Database.Tables;
using TH_Global;
using TH_Plugins;

namespace TH_Device_Server
{
    public partial class Device_Server
    {

        public Device_Server(Configuration config)
        {
            Configuration = config;
        }

        public Configuration Configuration { get; set; }

        public string ConfigurationPath { get; set; }

        public bool UpdateConfigurationFile { get; set; }

        private System.Timers.Timer connectionTimer;

        public void Start()
        {
            if (Configuration.Databases_Server.Databases.Count > 0)
            {
                Logger.Log("Device Server Started :: " + Configuration.Description.Description + " [" + Configuration.Description.Device_ID + "]");

                Initialize(Configuration);

                connectionTimer = new System.Timers.Timer();
                connectionTimer.Interval = 100;
                connectionTimer.Elapsed += ConnectionTimer_Elapsed;
                connectionTimer.Enabled = true;
            }
            else
            {
                Logger.Log("No Server Databases Configured");
            }
        }

        public void Stop()
        {
            if (connectionTimer != null) connectionTimer.Enabled = false;

            Plugins_Closing();

            Logger.Log("Device Server Stopped :: " + Configuration.Description.Description + " [" + Configuration.Description.Device_ID + "]");
        }


        private void Initialize(Configuration config)
        {
            Configuration = config;

            Global.Initialize(config.Databases_Server);

            Database.Create(config.Databases_Server);

            InitializeVariablesTables();

            LoadPlugins();
            Plugins_Initialize(config);
        }

        const int INTERVAL_MIN = 5000;
        const int INTERVAL_MAX = 60000;
        int interval = 5000;

        private void ConnectionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var timer = (System.Timers.Timer)sender;
            timer.Enabled = false;

            // Get Connection Status for Databases
            var status = CheckDatabaseConnection(Configuration);

            // Send Database Connection Status to plugins
            SendDatabaseStatus(status.Connected, Configuration);

            if (!status.Connected)
            {
                // Increase the interval by 25% until interval == interval_max
                interval = Math.Min(Convert.ToInt32(interval + (interval * 0.25)), INTERVAL_MAX);

                Logger.Log("Error in Database Connection... Retrying in " + interval.ToString() + "ms");
                if (status.Message != null) Logger.Log(status.Message);
            }
            else interval = INTERVAL_MIN;

            if (timer != null)
            {
                timer.Interval = interval;
                timer.Enabled = true;
            }
        }


        private class DatabaseConnectionStatus
        {
            public bool Connected { get; set; }
            public string Message { get; set; }
        }

        DatabaseConnectionStatus CheckDatabaseConnection(Configuration config)
        {
            // Ping Database connection for each Database Configuration
            bool dbsuccess = true;
            string msg = null;

            foreach (Database_Configuration db_config in config.Databases_Server.Databases)
            {
                if (!Global.Ping(db_config, out msg))
                {
                    dbsuccess = false;
                    break;
                }
            }

            var status = new DatabaseConnectionStatus();
            status.Connected = dbsuccess;
            status.Message = msg;
            return status;
        }

        private void SendDatabaseStatus(bool connected, Configuration config)
        {
            var data = new EventData();
            data.id = "DatabaseStatus";
            data.data01 = config;
            data.data02 = connected;

            Plugins_Update_SendData(data);
        }

        void InitializeVariablesTables()
        {
            string tablePrefix;
            if (Configuration.DatabaseId != null) tablePrefix = Configuration.DatabaseId + "_";
            else tablePrefix = "";

            Variables.CreateTable(Configuration.Databases_Server, tablePrefix);
        }

        #region "Exception Handler"

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Log("UnhandledException : " + e.ExceptionObject.ToString());
        }

        #endregion

        #region "Header"

        void PrintDeviceHeader(Configuration config)
        {
            Logger.Log("Device [" + config.Index.ToString() + "] ---------------------------------------");

            Logger.Log("Description ----------------------------");
            if (config.Description.Description != null) Logger.Log(config.Description.Description);
            if (config.Description.Manufacturer != null) Logger.Log(config.Description.Manufacturer);
            if (config.Description.Model != null) Logger.Log(config.Description.Model);
            if (config.Description.Serial != null) Logger.Log(config.Description.Serial);

            Logger.Log("Agent ----------------------------------");
            if (config.Agent.IP_Address != null) Logger.Log(config.Agent.IP_Address);
            if (config.Agent.Port > 0) Logger.Log(config.Agent.Port.ToString());
            if (config.Agent.Device_Name != null) Logger.Log(config.Agent.Device_Name);

            Logger.Log("--------------------------------------------------");
        }

        #endregion



        /// <summary>
        /// Check Database Connections to insure they are connected before proceeding.
        /// This method will run continously until the database connections are established.
        /// </summary>
        //void CheckDatabaseConnection(Configuration config)
        //{
        //    bool dbsuccess = false;

        //    int interval_min = 3000;
        //    int interval_max = 60000;
        //    int interval = interval_min;

        //    bool first = true;

        //    while (!dbsuccess)
        //    {
        //        // Ping Database connection for each Database Configuration
        //        dbsuccess = true;
        //        string msg = null;

        //        foreach (Database_Configuration db_config in config.Databases_Server.Databases)
        //        {
        //            if (!Global.Ping(db_config, out msg))
        //            {
        //                dbsuccess = false;
        //                break;
        //            }
        //        }

        //        // Send Database Connection Status to plugins
        //        SendDatabaseStatus(dbsuccess, config);

        //        if (!dbsuccess)
        //        {
        //            // Increase the interval by 25% until interval == interval_max
        //            if (!first) interval = Math.Min(Convert.ToInt32(interval + (interval * 0.25)), interval_max);
        //            first = false;

        //            Logger.Log("Error in Database Connection... Retrying in " + interval.ToString() + "ms");
        //            if (msg != null) Logger.Log(msg);

        //            // Sleep the current thread for the calculated interval
        //            //Thread.Sleep(interval);

        //        }
        //    }
        //}
































        #region "Public"

        //public Device_Server(Configuration config, bool useDatabases = true)
        //{
        //    //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        //    UseDatabases = useDatabases;

        //    configuration = config;

        //    //TablePrefix = config.DatabaseId + "_";

        //    RunningTime_Initialize();
        //}

        //bool usedatabases = true;
        //public bool UseDatabases 
        //{
        //    get { return usedatabases; }
        //    set { usedatabases = value; }
        //}

        //string TablePrefix { get; set; }

        #region "Methods"

        //public void Initialize()
        //{
        //    Global.Initialize(configuration.Databases_Server);

        //    if (UseDatabases) Database.Create(configuration.Databases_Server);

        //    LoadPlugins();

        //    // Initialize any aux tables such as Agent info or variables
        //    InitializeTables();

        //    // Initialize each Table Plugin with the current Configuration 
        //    Plugins_Initialize(configuration, UseDatabases);
        //}

        ///// <summary>
        ///// Check Database Connections to insure they are connected before proceeding.
        ///// This method will run continously until the database connections are established.
        ///// </summary>
        //void CheckDatabaseConnection()
        //{
        //    bool dbsuccess = false;

        //    int interval_min = 3000;
        //    int interval_max = 60000;
        //    int interval = interval_min;

        //    bool first = true;

        //    if (UseDatabases)
        //    {
        //        while (!dbsuccess)
        //        {
        //            // Ping Database connection for each Database Configuration
        //            dbsuccess = true;
        //            string msg = null;

        //            foreach (Database_Configuration db_config in configuration.Databases_Server.Databases)
        //            {
        //                if (!Global.Ping(db_config, out msg))
        //                {
        //                    dbsuccess = false;
        //                    break;
        //                }
        //            }

        //            if (dbsuccess) UpdateProcessingStatus("Database Connections Established");
        //            else
        //            {
        //                // Increase the interval by 25% until interval == interval_max
        //                if (!first) interval = Math.Min(Convert.ToInt32(interval + (interval * 0.25)), interval_max);
        //                first = false;

        //                Logger.Log("Error in Database Connection... Retrying in " + interval.ToString() + "ms");
        //                if (msg != null) WriteToConsole(msg, ConsoleOutputType.Error);

        //                // Sleep the current thread for the calculated interval
        //                Thread.Sleep(interval);
        //            }
        //        }
        //    } 
        //}


        //Thread worker;

        //public void Start()
        //{
        //    PrintDeviceHeader(configuration);

        //    //worker = new Thread(new ThreadStart(Worker_Start));
        //    //worker.Start();
        //}

        //public void Stop()
        //{
        //    RunningTimeSTPW.Stop();
        //    RunningTime_TIMER.Enabled = false;

        //    //if (worker != null)
        //    //{
        //    //    Worker_Stop();
        //    //    worker.Join(5000);
        //    //    if (worker != null) worker.Abort();
        //    //    worker = null;
        //    //}

        //    Logger.Log("Device Server (" + configuration.Index.ToString() + ") Stopped");
        //}

        //public void Close()
        //{
        //    //Stop();
        //    Plugins_Closing();
        //    FSW_Stop();

        //    Logger.Log("Device (" + configuration.Index.ToString() + ") Closed");
        //}

        #endregion

        #region "Properties"

        //public Configuration configuration { get; set; }

        //public string configurationPath { get; set; }

        //public bool updateConfigurationFile { get; set; }

        #endregion

        #endregion

        #region "Worker Thread"

        //void Worker_Start() 
        //{
        //    Thread.Sleep(2000);

        //    Initialize();

        //    Requests_Start();
        //}

        //void Worker_Stop()
        //{
        //    Requests_Stop();

        //    RunningTimeSTPW.Stop();
        //}

        #endregion

        #region "Connection"

        //bool connected = false;
        //public bool Connected 
        //{
        //    get { return connected; }
        //    set
        //    {
        //        bool prev_val = connected;

        //        connected = value;

        //        if (prev_val != connected)
        //        {
        //            if (connected && AgentConnected != null) AgentConnected(this);
        //            else if (AgentDisconnected != null) AgentDisconnected(this);
        //        }
        //    }
        //}

        //public delegate void AgentConnection_Handler(Device_Server server);

        //public event AgentConnection_Handler AgentConnected;
        //public event AgentConnection_Handler AgentDisconnected;

        #endregion

        #region "Processing Status"

        //string processingstatus;
        //public string ProcessingStatus
        //{
        //    get { return processingstatus; }
        //    set
        //    {
        //        processingstatus = value;

        //        if (ProcessingStatusChanged != null && configuration != null)
        //        {
        //            ProcessingStatusChanged(configuration.Index, processingstatus);
        //        }       
        //    }
        //}

        //string prev_processingstatus;

        //void UpdateProcessingStatus(string status)
        //{
        //    prev_processingstatus = ProcessingStatus;
        //    ProcessingStatus = status;

        //    WriteToConsole(processingstatus, ConsoleOutputType.Status);
        //}

        //void ClearProcessingStatus()
        //{
        //    ProcessingStatus = prev_processingstatus;
        //    prev_processingstatus = "";
        //}

        //public delegate void ProcessingStatusChanged_Handler(int index, string status);
        //public event ProcessingStatusChanged_Handler ProcessingStatusChanged;

        #endregion

        #region "Console Output"

        //static bool DEBUG = true;

        //string previousLine = null;
        //ConsoleOutputType previousType;
        //DateTime previousErrorTimestamp;

        //enum ConsoleOutputType
        //{
        //    Normal = 0,
        //    Status = 1,
        //    Error = 2
        //}

        //void WriteToConsole(string line, ConsoleOutputType type)
        //{
        //    switch (type)
        //    {
        //        case ConsoleOutputType.Normal:

        //            Logger.Log(line);

        //            break;

        //        case ConsoleOutputType.Status:
        //            if (configuration != null)
        //            {
        //                if (DEBUG) Logger.Log("[Status] [" + configuration.Description.Description + " : " + configuration.Description.Device_ID + "] " + line);
        //            }

        //            break;

        //        case ConsoleOutputType.Error:

        //            previousErrorTimestamp = DateTime.Now;

        //            //Console.BackgroundColor = ConsoleColor.Red;
        //            //Console.ForegroundColor = ConsoleColor.White;
        //            //Console.Write("[Error]");
        //            //Console.ResetColor();

        //            Logger.Log("[Error] [" + configuration.Description.Description + " : " + configuration.Description.Device_ID + "] [" + previousErrorTimestamp.ToString() + " - " + DateTime.Now.ToString() + "] " + line);

        //            break;
        //    }

        //    previousType = type;
        //    previousLine = line;
        //}

        //public void Log(string line)
        //{
        //    if (DEBUG) Logger.Log(line);
        //}

        #endregion



        #region "Running Time"

        //System.Diagnostics.Stopwatch RunningTimeSTPW;
        //System.Timers.Timer RunningTime_TIMER;
        //public TimeSpan RunningTime;

        //void RunningTime_Initialize()
        //{
        //    RunningTimeSTPW = new System.Diagnostics.Stopwatch();
        //    RunningTimeSTPW.Start();
        //    RunningTime_TIMER = new System.Timers.Timer();
        //    RunningTime_TIMER.Interval = 500;
        //    RunningTime_TIMER.Elapsed += RunningTime_TIMER_Elapsed;
        //    RunningTime_TIMER.Enabled = true;
        //}

        //void RunningTime_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    RunningTime = RunningTimeSTPW.Elapsed;
        //}

        #endregion

        #region "Settings File Watcher"
        /// <summary>
        /// This has been mostly replaced by TH_UserManagement.Monitor but still may be needed for local files
        /// Needs some work though..
        /// </summary>

        //FileSystemWatcher FSW;

        //bool ReadNewSettingsFile;

        //void FSW_Initialize()
        //{
        //    if (configurationPath != null)
        //    {

        //        FSW = new FileSystemWatcher();
        //        FSW.Path = Path.GetDirectoryName(configurationPath);
        //        FSW.Filter = Path.GetFileName(configurationPath);

        //        Log("FSW.Path = " + FSW.Path);
        //        Log("FSW.Filter = " + FSW.Filter);

        //        FSW.Changed += FSW_Changed;

        //    }
        //}

        //void FSW_Start()
        //{
        //    if (FSW != null) FSW.EnableRaisingEvents = true;
        //}

        //void FSW_Stop()
        //{
        //    if (FSW != null) FSW.EnableRaisingEvents = false;
        //}

        //void FSW_Changed(object sender, FileSystemEventArgs e)
        //{
        //    Log(configuration.Description.Description + " Settings File Changed!");
        //    Log("Reloading Settings File from \"" + configurationPath + "\"");

        //    FSW_Stop();

        //    ReadNewSettingsFile = true;

        //    Stop();
        //}

        //void FSW_WaitForStop()
        //{
        //    System.Diagnostics.Stopwatch Timeout = new System.Diagnostics.Stopwatch();
        //    Timeout.Start();

        //    Timeout.Stop();
        //}

        #endregion

        #region "Table Management"



        #endregion

    }
}
