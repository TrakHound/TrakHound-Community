// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

//using TH_Configuration;
using TH_Global.TrakHound.Configurations;
using TH_Database;
using TH_Global;
using TH_Plugins;

namespace TH_Device_Server
{
    public partial class Device_Server
    {

        public Device_Server(DeviceConfiguration config)
        {
            Configuration = config;
        }

        public DeviceConfiguration Configuration { get; set; }

        public string ConfigurationPath { get; set; }

        public bool UpdateConfigurationFile { get; set; }

        private System.Timers.Timer connectionTimer;

        public void Start()
        {
            if (Configuration.Databases_Server.Databases.Count == 0)
            {
                Logger.Log("No Server Databases Configured", Logger.LogLineType.Warning);
            }

            Logger.Log("Device Server Started :: " + Configuration.Description.Description + " [" + Configuration.Description.Device_ID + "]", Logger.LogLineType.Notification);

            Initialize(Configuration);

            connectionTimer = new System.Timers.Timer();
            connectionTimer.Interval = 100;
            connectionTimer.Elapsed += ConnectionTimer_Elapsed;
            connectionTimer.Enabled = true;
        }

        public void Stop()
        {
            if (connectionTimer != null) connectionTimer.Enabled = false;

            Plugins_Closing();

            Logger.Log("Device Server Stopped :: " + Configuration.Description.Description + " [" + Configuration.Description.Device_ID + "]", Logger.LogLineType.Notification);
        }

        public void SendPluginsData(string id, string message)
        {
            var data = new EventData();
            data.Id = id;
            data.Data01 = message;

            Plugins_Update_SendData(data);
        }

        public void SendPluginsData(string id, object obj)
        {
            var data = new EventData();
            data.Id = id;
            data.Data01 = obj;

            Plugins_Update_SendData(data);
        }

        private void Initialize(DeviceConfiguration config)
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

                Logger.Log("Error in Database Connection... Retrying in " + interval.ToString() + "ms", Logger.LogLineType.Warning);
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

        DatabaseConnectionStatus CheckDatabaseConnection(DeviceConfiguration config)
        {
            // Ping Database connection for each Database Configuration
            bool dbsuccess = true;
            string msg = null;

            if (Global.Plugins.Count > 0)
            {
                foreach (Database_Configuration db_config in config.Databases_Server.Databases)
                {
                    if (!Global.Ping(db_config, out msg))
                    {
                        dbsuccess = false;
                        break;
                    }
                }
            }           

            var status = new DatabaseConnectionStatus();
            status.Connected = dbsuccess;
            status.Message = msg;
            return status;
        }

        private void SendDatabaseStatus(bool connected, DeviceConfiguration config)
        {
            var data = new EventData();
            data.Id = "DatabaseStatus";
            data.Data01 = config;
            data.Data02 = connected;

            Plugins_Update_SendData(data);
        }

        void InitializeVariablesTables()
        {
            string tablePrefix;
            if (Configuration.DatabaseId != null) tablePrefix = Configuration.DatabaseId + "_";
            else tablePrefix = "";

            TH_Database.Tables.Variables.CreateTable(Configuration.Databases_Server, tablePrefix);
        }

        void PrintDeviceHeader(DeviceConfiguration config)
        {
            Logger.Log("Device [" + config.Index.ToString() + "] ---------------------------------------", Logger.LogLineType.Console);

            Logger.Log("Description ----------------------------", Logger.LogLineType.Console);
            if (config.Description.Description != null) Logger.Log(config.Description.Description, Logger.LogLineType.Console);
            if (config.Description.Manufacturer != null) Logger.Log(config.Description.Manufacturer, Logger.LogLineType.Console);
            if (config.Description.Model != null) Logger.Log(config.Description.Model, Logger.LogLineType.Console);
            if (config.Description.Serial != null) Logger.Log(config.Description.Serial, Logger.LogLineType.Console);

            Logger.Log("--------------------------------------------------", Logger.LogLineType.Console);
        }

    }
}
