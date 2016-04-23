// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ServiceProcess;

using TH_Global;
using TH_Global.Functions;
using TH_Global.Updates;
using System.IO;

namespace TrakHound_Updater
{
    public partial class Service1 : ServiceBase
    {
        // Used to send/recieve messages using WCF to other applications
        MessageServer messageServer;

        public Service1()
        {
            InitializeComponent();
        }


        protected override void OnStart(string[] args)
        {
            var status = new ServiceStatus();
            status.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            status.dwWaitHint = 10000;
            SetServiceStatus(this.ServiceHandle, ref status);

            Start();

            // Update the service state to Running.
            status.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref status);
        }

        protected override void OnStop()
        {
            var status = new ServiceStatus();
            status.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            status.dwWaitHint = 10000;
            SetServiceStatus(this.ServiceHandle, ref status);

            Stop();

            // Update the service state to Stopped.
            status.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref status);
        }

        #region "Service Status"

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        #endregion


        public void Start()
        {
            ReadConfigFile();
        }

        public void Stop()
        {
            StopUpdates();

            StopMessageServer();
        }


        private void ReadConfigFile()
        {
            // Read the update_config.xml file
            configuration = UpdateConfiguration.Read();

            // Enable Message Server Communication using WCF
            if (configuration.EnableMessageServer) StartMessageServer();
            else StopMessageServer();

            StopConfigurationFileWatcher();
            UpdateConfiguration.Create(configuration);

            // Monitor update_config.xml file for any changes
            StartConfigurationFileWatcher();

            // If updates are enabled then start auto check timer
            if (configuration.Enabled) StartUpdates();
            else Logger.Log("Auto Updates Disabled", Logger.LogLineType.Notification);
        }


        private System.Timers.Timer updateTimer;

        private void StartUpdates()
        {
            Logger.Log("TrakHound-Updater Started!", Logger.LogLineType.Notification);

            StopUpdates();

            updateTimer = new System.Timers.Timer();
            updateTimer.Interval = GetMillisecondsFromMinutes(configuration.UpdateCheckInterval);
            updateTimer.Elapsed += UpdateTimer_Elapsed;
            updateTimer.Enabled = true;
        }

        private void StopUpdates()
        {
            if (updateTimer != null) updateTimer.Enabled = false;
        }

        private void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var timer = (System.Timers.Timer)sender;

            timer.Enabled = false;

            // Get available updates, download, store links in Registry
            AppInfo[] infos = GetUpdates();

            ApplyUpdates(infos);

            timer.Interval = GetMillisecondsFromMinutes(configuration.UpdateCheckInterval);
            timer.Enabled = true;
        }


        private static List<AppInfo> appInfos = new List<AppInfo>();

        public static AppInfo[] GetUpdates()
        {
            var infos = new List<AppInfo>();

            string[] names = Registry_Functions.GetKeyNames();
            if (names != null)
            {
                foreach (var name in names)
                {
                    string url = Registry_Functions.GetValue(Update.UPDATE_URL, name);
                    if (url != null) infos.Add(Update.Get(url, name));
                }
            }

            appInfos.AddRange(infos);

            return infos.ToArray();
        }

        public static AppInfo GetUpdate(string appName)
        {
            string[] names = Registry_Functions.GetKeyNames();
            if (names != null)
            {
                var name = names.ToList().Find(x => x == appName);
                if (name != null)
                {
                    string url = Registry_Functions.GetValue(Update.UPDATE_URL, name);
                    if (url != null)
                    {
                        var info = Update.Get(url, appName);

                        // Add to static list if not already in there
                        if (!appInfos.Exists(x => x.Name == info.Name)) appInfos.Add(info);

                        return info;
                    }
                }
            }

            return null;
        }

        public static void ApplyUpdates()
        {
            if (appInfos != null)
            {
                foreach (var info in appInfos)
                {
                    Update.Apply(info);
                }
            }
        }

        public static void ApplyUpdates(AppInfo[] infos)
        {
            if (infos != null)
            {
                foreach (var info in infos)
                {
                    Update.Apply(info);
                }
            }
        }

        public static void ApplyUpdate(string appName)
        {
            if (appInfos != null)
            {
                var info = appInfos.ToList().Find(x => x.Name == appName);
                if (info != null) Update.Apply(info);
            }
        }

        private UpdateConfiguration configuration;

        private FileSystemWatcher watcher;

        private void StartConfigurationFileWatcher()
        {
            if (watcher == null)
            {
                watcher = new FileSystemWatcher(FileLocations.TrakHound, UpdateConfiguration.CONFIG_FILENAME);
                watcher.Changed += Watcher_Changed;
            }

            watcher.EnableRaisingEvents = true;
        }

        private void StopConfigurationFileWatcher()
        {
            if (watcher != null) { watcher.EnableRaisingEvents = false; }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            ReadConfigFile();
        }

        private static double GetMillisecondsFromMinutes(double minutes)
        {
            var ts = TimeSpan.FromMinutes(minutes);
            return ts.TotalMilliseconds;
        }

    }
}
