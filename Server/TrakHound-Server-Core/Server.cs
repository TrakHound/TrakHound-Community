// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

using TH_Configuration;
using TH_Database;
using TH_Device_Server;
using TH_Global;
using TH_UserManagement;
using TH_UserManagement.Management;

namespace TrakHound_Server_Core
{
    public partial class Server
    {

        public delegate void StatusChanged_Handler();
        public event StatusChanged_Handler Started;
        public event StatusChanged_Handler Stopped;

        public void Start()
        {
            System.Threading.ThreadPool.SetMaxThreads(10, 10);

            PrintHeader();

            Global.UseMultithreading = false;
            DatabasePluginReader dpr = new DatabasePluginReader();

            LoadDevices();

            if (Started != null) Started();
        }

        public void Stop()
        {
            foreach (Device_Server device in Devices)
            {
                if (device != null) device.Close();
            }

            if (devicesmonitor_THREAD != null) devicesmonitor_THREAD.Abort();

            if (Stopped != null) Stopped();
        }

    }
}
