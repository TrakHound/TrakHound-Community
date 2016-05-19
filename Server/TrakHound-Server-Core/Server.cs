// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TH_Device_Server;
using TH_Global;
using TH_Global.TrakHound.Users;

namespace TrakHound_Server_Core
{
    public partial class Server
    {
        public Server()
        {
            Logger.AppicationName = "TrakHound-Server";

            FileLocations.CreateAllDirectories();

            TH_Database.DatabasePluginReader.ReadPlugins();

            //UserManagementSettings.ReadConfiguration();

            UserLoginFileMonitor_Start();
        }

        public delegate void StatusChanged_Handler();
        public event StatusChanged_Handler Started;
        public event StatusChanged_Handler Stopped;

        public bool IsRunnning;

        public void Start()
        {
            PrintHeader();

            LoadDevices();

            IsRunnning = true;

            if (Started != null) Started();
        }

        public void Stop()
        {
            foreach (Device_Server device in Devices)
            {
                if (device != null) device.Stop();
            }

            DevicesMonitor_Stop();
            if (devicesmonitor_THREAD != null) devicesmonitor_THREAD.Abort();

            IsRunnning = false;

            if (Stopped != null) Stopped();
        }

    }
}
