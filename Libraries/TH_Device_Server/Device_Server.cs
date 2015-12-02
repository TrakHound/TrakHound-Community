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

            //Ping_Agent_Initialize(config);
            //Ping_DB_Initialize(config);
            //Ping_PHP_Initialize(config);

            //Status = ConnectionStatus.Stopped;

            //ConnectionStatus_Initialize();
        }

        bool usedatabases = true;
        public bool UseDatabases 
        {
            get { return usedatabases; }
            set { usedatabases = value; }
        }

        #region "Methods"

        public void Initialize()
        {
            /// Initialize MTConnect Requests (probe, current, and sample)
            Requests_Initialize();

            // Initialize any aux tables such as Agent info or variables
            InitializeTables();

            // Initialize each Table Plugin with the current Configuration 
            TablePlugIns_Initialize(configuration, UseDatabases);
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

            if (UseDatabases) Database.Create(configuration.Databases);

            worker = new Thread(new ThreadStart(Worker_Start));
            worker.Start();
        }    

        public void Stop()
        {
            RunningTimeSTPW.Stop();
            RunningTime_TIMER.Enabled = false;

            Worker_Stop();

            if (worker != null) worker.Abort();

            //Status = ConnectionStatus.Stopped;

            Log("Device Server (" + configuration.Index.ToString() + ") Stopped");
        }

        public void Close()
        {
            Stop();

            TablePlugIns_Closing();

            //if (Connection_Timer != null) Connection_Timer.Enabled = false;

            FSW_Stop();

            Log("Device (" + configuration.Index.ToString() + ") Closed");
        }

        public void Restart()
        {

        }

        #endregion

        #region "Properties"

        public Configuration configuration { get; set; }

        public string configurationPath { get; set; }

        public bool updateConfigurationFile { get; set; }

        #endregion

        #endregion

        #region "Worker Thread"

        //void Worker_Start() { Connection_Initialize(); }

        void Worker_Start() 
        {
            Initialize();

            Requests_Start();
        }

        void Worker_Stop()
        {
            RunningTimeSTPW.Stop();

            Requests_Stop();

            Log("Device Server (" + configuration.Index.ToString() + ") Stopped");
        }

        #endregion

        #region "Connection"

        bool connected;
        public bool Connected 
        {
            get { return connected; }
            set
            {
                bool prev_val = connected;

                connected = value;

                //if (connected) AgentConnected(this);
                //else AgentDisconnected(this);

                if (prev_val != connected)
                {
                    if (connected) AgentConnected(this);
                    else AgentDisconnected(this);
                }
            }
        }

        public delegate void AgentConnection_Handler(Device_Server server);

        public event AgentConnection_Handler AgentConnected;
        public event AgentConnection_Handler AgentDisconnected;





        //public System.Timers.Timer Connection_Timer;

        //int TryCount = 1;
        //const int ConnectionAttempts = 5;

        //bool FirstAttempt = true;

        //void Connection_Initialize()
        //{
        //    Connection_Timer = new System.Timers.Timer();
        //    Connection_Timer.Elapsed += Connection_Timer_Elapsed;
        //    Connection_Timer.Interval = 1000;
        //    Connection_Timer.Enabled = true;
        //}

        //void Connection_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    Connection_Check();
        //}

        //void Connection_Check()
        //{

        //    if (MTC_PingResult)
        //    {
        //        if (FirstAttempt) Log("Device Connected...");

        //        if (TryCount > 1) Log("Connection Reestablished");

        //        TryCount = 1;

        //        Connection_Timer.Interval = 1000;

        //        if (Status == ConnectionStatus.Stopped)
        //        {
        //            Status = ConnectionStatus.Started;

        //            Initialize();

        //            RunningTimeSTPW.Start();

        //            Requests_Start();

        //            Log("Device (" + configuration.Index.ToString() + ") Started...");
        //        }

        //        FirstAttempt = false;
        //    }
        //    else
        //    {
        //        if (!MTC_PingResult) Log("Device (" + configuration.Index.ToString() + ") MTC Not Reachable!");

        //        if (Status == ConnectionStatus.Started || FirstAttempt)
        //        {
        //            FirstAttempt = false;

        //            Connection_Timer.Interval = 5000;

        //            Log("Attempting to Connect...(Attempt #" + TryCount.ToString() + ")");

        //            if (TryCount >= ConnectionAttempts)
        //            {
        //                TryCount = 1;

        //                Connection_Timer.Interval = 1000;

        //                Stop();
        //            }

        //            TryCount += 1;
        //        }
        //    }


            //if (MTC_PingResult && SQL_PingResult && PHP_PingResult)
            //{
            //    if (FirstAttempt) Log("Device Connected...");

            //    if (TryCount > 1) Log("Connection Reestablished");

            //    TryCount = 1;

            //    Connection_Timer.Interval = 1000;

            //    if (Status == ConnectionStatus.Stopped)
            //    {
            //        Status = ConnectionStatus.Started;

            //        Initialize();

            //        RunningTimeSTPW.Start();

            //        Requests_Start();

            //        Log("Device (" + configuration.Index.ToString() + ") Started...");
            //    }

            //    FirstAttempt = false;
            //}
            //else
            //{
            //    if (!MTC_PingResult && !SQL_PingResult && !PHP_PingResult) Log("Device (" + configuration.Index.ToString() + ") MTC, SQL, and PHP Not Reachable!");
            //    else if (!MTC_PingResult && !SQL_PingResult) Log("Device (" + configuration.Index.ToString() + ") MTC and SQL Not Reachable!");
            //    else if (!MTC_PingResult && !PHP_PingResult) Log("Device (" + configuration.Index.ToString() + ") MTC and PHP Not Reachable!");
            //    else if (!PHP_PingResult && !SQL_PingResult) Log("Device (" + configuration.Index.ToString() + ") PHP and SQL Not Reachable!");
            //    else if (!MTC_PingResult) Log("Device (" + configuration.Index.ToString() + ") MTC Not Reachable!");
            //    else if (!PHP_PingResult) Log("Device (" + configuration.Index.ToString() + ") PHP Not Reachable!");
            //    else Log("Device (" + configuration.Index.ToString() + ") SQL Not Reachable!");

            //    if (Status == ConnectionStatus.Started || FirstAttempt)
            //    {
            //        FirstAttempt = false;

            //        Connection_Timer.Interval = 5000;

            //        Log("Attempting to Connect...(Attempt #" + TryCount.ToString() + ")");

            //        if (TryCount >= ConnectionAttempts)
            //        {
            //            TryCount = 1;

            //            Connection_Timer.Interval = 1000;

            //            Stop();
            //        }

            //        TryCount += 1;
            //    }
            //}
        //}

        #endregion

        #region "Connection Status"

        //public enum ConnectionStatus
        //{
        //    Stopped = 0,
        //    Started = 1
        //}

        //public ConnectionStatus Status;

        //public delegate void StatusDelly(int Index, ConnectionStatus Status);
        //public event StatusDelly StatusUpdated;

        //void ConnectionStatus_Initialize()
        //{
        //    ConnectionStatus_Timer = new System.Timers.Timer();
        //    ConnectionStatus_Timer.Interval = 1000;
        //    ConnectionStatus_Timer.Elapsed += ConnectionStatus_Timer_Elapsed;
        //    ConnectionStatus_Timer.Enabled = true;
        //}

        //void ConnectionStatus_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    UpdateStatus(Status);
        //}

        //private void UpdateStatus(ConnectionStatus status)
        //{
        //    StatusDelly handler = StatusUpdated;
        //    if (handler != null) handler(configuration.Index, status);
        //}

        //System.Timers.Timer ConnectionStatus_Timer;

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
        int previousRow;

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

                    //previousRow = Console.CursorTop;

                    break;

                case ConsoleOutputType.Status:

                    if (previousType == ConsoleOutputType.Status)
                    {
                        //Console.SetCursorPosition(0, previousRow);
                        //Console.Write(new string(' ', Console.WindowWidth));
                        //Console.SetCursorPosition(0, previousRow);

                        //Console.SetCursorPosition(0, previousRow - 1);
                        //Console.Write(new string(' ', Console.WindowWidth));
                        //Console.SetCursorPosition(0, previousRow - 1);

                        //Console.SetCursorPosition(0, Console.CursorTop - 1);
                        //Console.Write(new string(' ', Console.WindowWidth));
                        //Console.SetCursorPosition(0, Console.CursorTop - 1);
                    }
                    //else previousRow = Console.CursorTop;

                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("[Status]");
                    Console.ResetColor();

                    if (configuration != null)
                    {
                        Console.Write(" [" + configuration.Index.ToString() + "] " + line + Environment.NewLine);
                    }
                    else
                    {
                        Console.Write(" " + line + Environment.NewLine);
                    }
                    

                    break;

                case ConsoleOutputType.Error:

                    if (line == previousLine)
                    {
                        //Console.SetCursorPosition(0, previousRow - 1);
                        //Console.Write(new string(' ', Console.WindowWidth));
                        //Console.SetCursorPosition(0, previousRow - 1);

                        //Console.SetCursorPosition(0, Console.CursorTop - 1);
                        //Console.Write(new string(' ', Console.WindowWidth));
                        //Console.SetCursorPosition(0, Console.CursorTop - 1);
                    }
                    else
                    {
                        previousErrorTimestamp = DateTime.Now;
                    }

                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("[Error]");
                        Console.ResetColor();

                        Console.Write(" [" + previousErrorTimestamp.ToString() + " - " + DateTime.Now.ToString() + "] " + line + Environment.NewLine);
                        //Log(line);
                    

                    break;
            }

            previousType = type;
            previousLine = line;
            //previousRow = Console.CursorTop;
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

            //Console.Title = "TrakHound Server - " + RunningTime.ToString(@"dd\.hh\:mm\:ss");
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
            if (UseDatabases) Variables.CreateTable(configuration.Databases);
        }

        #endregion

    }
}
