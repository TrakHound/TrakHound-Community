// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;

using TH_Configuration;
using TH_Database;
using TH_Database.Tables;
using TH_Global;
using TH_UserManagement.Management;

namespace TH_Device_Server
{
    public partial class Device_Server
    {







        #region "Public"

        public Device_Server(Configuration config, bool useDatabases = true)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            UseDatabases = useDatabases;

            LoadPlugins();

            configuration = config;

            RunningTime_Initialize();
        }

        bool usedatabases = true;
        public bool UseDatabases 
        {
            get { return usedatabases; }
            set { usedatabases = value; }
        }

        string TablePrefix { get; set; }

        #region "Methods"

        public void Initialize()
        {
            CheckDatabaseConnection();

            TH_Database.Database.Create(configuration.Databases_Server);

            // Initialize any aux tables such as Agent info or variables
            InitializeTables();

            // Initialize each Table Plugin with the current Configuration 
            TablePlugIns_Initialize(configuration, UseDatabases);
        }

        /// <summary>
        /// Check Database Connections to insure they are connected before proceeding.
        /// This method will run continously until the database connections are established.
        /// </summary>
        void CheckDatabaseConnection()
        {
            bool dbsuccess = false;

            int interval_min = 3000;
            int interval_max = 60000;
            int interval = interval_min;

            bool first = true;

            if (UseDatabases)
                while (!dbsuccess)
                {
                    // Ping Database connection for each Database Configuration
                    dbsuccess = true;
                    foreach (Database_Configuration db_config in configuration.Databases_Server.Databases)
                    {
                        //if (!TH_Database.Global.Ping(db_config)) { dbsuccess = false; break; }
                        if (!TH_Database.Global.CheckPermissions(db_config, Application_Type.Server))
                        {
                            dbsuccess = false;
                            break;
                        }
                    }

                    if (dbsuccess) UpdateProcessingStatus("Database Connections Established");
                    else
                    {
                        // Write to console that there was an error
                        //if (first) WriteToConsole("Error in Database Connection... Retrying in " + interval.ToString() + "ms", ConsoleOutputType.Error);
                        //first = false;

                        // Increase the interval by 25% until interval == interval_max
                        if (!first) interval = Math.Min(Convert.ToInt32(interval + (interval * 0.25)), interval_max);
                        first = false;

                        WriteToConsole("Error in Database Connection... Retrying in " + interval.ToString() + "ms", ConsoleOutputType.Error);

                        // Sleep the current thread for the calculated interval
                        System.Threading.Thread.Sleep(interval);
                    }
                }
        }

        public void Start() { start(); }

        public void Start(bool startSampleFromFirst)
        {
            startFromFirst = startSampleFromFirst;
            start();
        }

        Thread worker;

        void start()
        {
            PrintDeviceHeader(configuration);

            if (UseDatabases) Database.Create(configuration.Databases_Server);

            worker = new Thread(new ThreadStart(Worker_Start));
            worker.Start();
        }    

        public void Stop()
        {
            RunningTimeSTPW.Stop();
            RunningTime_TIMER.Enabled = false;

            if (worker != null)
            {
                worker.Join(5000);
                if (worker != null) worker.Abort();
                worker = null;
            }

            Log("Device Server (" + configuration.Index.ToString() + ") Stopped");
        }

        public void Close()
        {
            Stop();
            TablePlugIns_Closing();
            FSW_Stop();

            Log("Device (" + configuration.Index.ToString() + ") Closed");
        }

        #endregion

        #region "Properties"

        public Configuration configuration { get; set; }

        public string configurationPath { get; set; }

        public bool updateConfigurationFile { get; set; }

        #endregion

        #endregion

        #region "Worker Thread"

        void Worker_Start() 
        {
            Initialize();

            Requests_Start();
        }

        void Worker_Stop()
        {
            RunningTimeSTPW.Stop();
        }

        #endregion

        #region "Connection"

        bool connected = false;
        public bool Connected 
        {
            get { return connected; }
            set
            {
                bool prev_val = connected;

                connected = value;

                if (prev_val != connected)
                {
                    if (connected && AgentConnected != null) AgentConnected(this);
                    else if (AgentDisconnected != null) AgentDisconnected(this);
                }
            }
        }

        public delegate void AgentConnection_Handler(Device_Server server);

        public event AgentConnection_Handler AgentConnected;
        public event AgentConnection_Handler AgentDisconnected;

        #endregion

        #region "Processing Status"

        string processingstatus;
        public string ProcessingStatus
        {
            get { return processingstatus; }
            set
            {
                processingstatus = value;

                if (ProcessingStatusChanged != null && configuration != null)
                {
                    ProcessingStatusChanged(configuration.Index, processingstatus);
                }       
            }
        }

        string prev_processingstatus;

        void UpdateProcessingStatus(string status)
        {
            prev_processingstatus = ProcessingStatus;
            ProcessingStatus = status;

            WriteToConsole(processingstatus, ConsoleOutputType.Status);
        }

        void ClearProcessingStatus()
        {
            ProcessingStatus = prev_processingstatus;
            prev_processingstatus = "";
        }

        public delegate void ProcessingStatusChanged_Handler(int index, string status);
        public event ProcessingStatusChanged_Handler ProcessingStatusChanged;

        #endregion

        #region "Console Output"

        static bool DEBUG = true;

        string previousLine = null;
        ConsoleOutputType previousType;
        DateTime previousErrorTimestamp;

        enum ConsoleOutputType
        {
            Normal = 0,
            Status = 1,
            Error = 2
        }

        void WriteToConsole(string line, ConsoleOutputType type)
        {
            switch (type)
            {
                case ConsoleOutputType.Normal:

                    Console.WriteLine(line);

                    break;

                case ConsoleOutputType.Status:
                    if (configuration != null)
                    {
                        if (DEBUG) Console.Write("[Status] [" + configuration.Description.Description + " : " + configuration.Description.Device_ID + "] " + line + Environment.NewLine);
                    }

                    break;

                case ConsoleOutputType.Error:

                    previousErrorTimestamp = DateTime.Now;

                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("[Error]");
                    Console.ResetColor();

                    Console.Write(" [" + previousErrorTimestamp.ToString() + " - " + DateTime.Now.ToString() + "] " + line + Environment.NewLine);

                    break;
            }

            previousType = type;
            previousLine = line;
        }

        public void Log(string line)
        {
            if (DEBUG) Logger.Log(line);
        }

        #endregion

        #region "Exception Handler"

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log("UnhandledException : " + e.ExceptionObject.ToString());
        }

        #endregion

        #region "Header"

        void PrintDeviceHeader(Configuration config)
        {
            Console.WriteLine("Device [" + config.Index.ToString() + "] ---------------------------------------");

            Console.WriteLine("Description ----------------------------");
            if (config.Description.Description != null) Console.WriteLine(config.Description.Description);
            if (config.Description.Manufacturer != null) Console.WriteLine(config.Description.Manufacturer);
            if (config.Description.Model != null) Console.WriteLine(config.Description.Model);
            if (config.Description.Serial != null) Console.WriteLine(config.Description.Serial);

            Console.WriteLine("Agent ----------------------------------");
            if (config.Agent.IP_Address != null) Console.WriteLine(config.Agent.IP_Address);
            if (config.Agent.Port > 0) Console.WriteLine(config.Agent.Port.ToString());
            if (config.Agent.Device_Name != null) Console.WriteLine(config.Agent.Device_Name);

            Console.WriteLine("--------------------------------------------------");
        }

        #endregion

        #region "Running Time"

        System.Diagnostics.Stopwatch RunningTimeSTPW;
        System.Timers.Timer RunningTime_TIMER;
        public TimeSpan RunningTime;

        void RunningTime_Initialize()
        {
            RunningTimeSTPW = new System.Diagnostics.Stopwatch();
            RunningTimeSTPW.Start();
            RunningTime_TIMER = new System.Timers.Timer();
            RunningTime_TIMER.Interval = 500;
            RunningTime_TIMER.Elapsed += RunningTime_TIMER_Elapsed;
            RunningTime_TIMER.Enabled = true;
        }

        void RunningTime_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RunningTime = RunningTimeSTPW.Elapsed;
        }

        #endregion

        #region "Settings File Watcher"
        /// <summary>
        /// This has been mostly replaced by TH_UserManagement.Monitor but still may be needed for local files
        /// Needs some work though..
        /// </summary>

        FileSystemWatcher FSW;

        bool ReadNewSettingsFile;

        void FSW_Initialize()
        {
            if (configurationPath != null)
            {

                FSW = new FileSystemWatcher();
                FSW.Path = Path.GetDirectoryName(configurationPath);
                FSW.Filter = Path.GetFileName(configurationPath);

                Log("FSW.Path = " + FSW.Path);
                Log("FSW.Filter = " + FSW.Filter);

                FSW.Changed += FSW_Changed;

            }
        }

        void FSW_Start()
        {
            if (FSW != null) FSW.EnableRaisingEvents = true;
        }

        void FSW_Stop()
        {
            if (FSW != null) FSW.EnableRaisingEvents = false;
        }

        void FSW_Changed(object sender, FileSystemEventArgs e)
        {
            Log(configuration.Description.Description + " Settings File Changed!");
            Log("Reloading Settings File from \"" + configurationPath + "\"");

            FSW_Stop();

            ReadNewSettingsFile = true;

            Stop();
        }

        void FSW_WaitForStop()
        {
            System.Diagnostics.Stopwatch Timeout = new System.Diagnostics.Stopwatch();
            Timeout.Start();

            Timeout.Stop();
        }

        #endregion

        #region "Table Management"

        void InitializeTables()
        {
            if (UseDatabases) Variables.CreateTable(configuration.Databases_Server, TablePrefix);
        }

        #endregion

    }
}
