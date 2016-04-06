// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ServiceProcess;

using TH_Global;

namespace TrakHound_Updater
{
    public partial class Service1 : ServiceBase
    {
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

            StartUpdates();

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

            StopUpdates();

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


        System.Timers.Timer updateTimer;

        private const double INITIAL_UPDATE_INTERVAL = 60000; // 1 Minute
        private const double CONTINUOUS_UPDATE_INTERVAL = 3600000; // 1 Hour

        public void StartUpdates()
        {
            Logger.Log("TrakHound-Updater Started!");

            updateTimer = new System.Timers.Timer();
            updateTimer.Interval = INITIAL_UPDATE_INTERVAL;
            updateTimer.Elapsed += UpdateTimer_Elapsed;
            updateTimer.Enabled = true;
        }

        public void StopUpdates()
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

            timer.Interval = CONTINUOUS_UPDATE_INTERVAL;
            timer.Enabled = true;
        }


        private AppInfo[] GetUpdates()
        {
            var infos = new List<AppInfo>();

            string[] names = Registry.GetValueNames("Update_Urls");
            if (names != null)
            {
                foreach (var name in names)
                {
                    string url = Registry.GetValue(name, "Update_Urls");
                    if (url != null) infos.Add(Update.Get(url));
                }
            }

            return infos.ToArray();
        }

        private void ApplyUpdates(AppInfo[] infos)
        {
            if (infos != null)
            {
                foreach (var info in infos)
                {
                    Update.Apply(info);
                }
            }
        }
    }
}
