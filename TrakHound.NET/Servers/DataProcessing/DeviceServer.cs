// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Threading;

using TrakHound.Configurations;
using TrakHound.Logging;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;
using TrakHound.Tables;

namespace TrakHound.Servers.DataProcessing
{
    public partial class DeviceServer
    {

        public DeviceServer(DeviceConfiguration config, List<IServerPlugin> serverPlugins)
        {
            Configuration = config;
            plugins = serverPlugins;
        }

        public DeviceConfiguration Configuration { get; set; }

        public string ConfigurationPath { get; set; }

        public bool UpdateConfigurationFile { get; set; }

        public delegate void StatusChanged_Handler(DeviceServer server);
        public event StatusChanged_Handler Started;
        public event StatusChanged_Handler Stopped;

        private bool _isRunning = false;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                if (_isRunning && Started != null) Started(this);
                else if (Stopped != null) Stopped(this);
            }
        }

        private System.Timers.Timer connectionTimer;

        private ManualResetEvent stop;

        public void Start()
        {
            IsRunning = true;
            stop = new ManualResetEvent(false);

            Initialize(Configuration);

            Plugins_Starting();

            SendDatabaseStatus(true, Configuration);
        }

        public void Stop()
        {
            stop.Set();

            if (connectionTimer != null) connectionTimer.Enabled = false;

            SendDatabaseStatus(false, Configuration);

            Plugins_Closing();

            IsRunning = false;

            Logger.Log("Device Server Stopped :: " + Configuration.Description.Description + " [" + Configuration.Description.DeviceId + "]", LogLineType.Notification);
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

            //InitializeVariablesTables();

            Plugins_Initialize(config);
        }

        private void SendDatabaseStatus(bool connected, DeviceConfiguration config)
        {
            var data = new EventData();
            data.Id = "DatabaseStatus";
            data.Data01 = config;
            data.Data02 = connected;

            Plugins_Update_SendData(data);
        }

        //void InitializeVariablesTables()
        //{
        //    string tablePrefix;
        //    if (Configuration.DatabaseId != null) tablePrefix = Configuration.DatabaseId + "_";
        //    else tablePrefix = "";

        //    Variables.CreateTable(tablePrefix);
        //}

        void PrintDeviceHeader(DeviceConfiguration config)
        {
            Logger.Log("Device [" + config.Index.ToString() + "] ---------------------------------------", LogLineType.Console);

            Logger.Log("Description ----------------------------", LogLineType.Console);
            if (config.Description.Description != null) Logger.Log(config.Description.Description, LogLineType.Console);
            if (config.Description.Manufacturer != null) Logger.Log(config.Description.Manufacturer, LogLineType.Console);
            if (config.Description.Model != null) Logger.Log(config.Description.Model, LogLineType.Console);
            if (config.Description.Serial != null) Logger.Log(config.Description.Serial, LogLineType.Console);

            Logger.Log("--------------------------------------------------", LogLineType.Console);
        }

        public event SendData_Handler SendData;


        List<IServerPlugin> plugins { get; set; }

        void Plugins_Initialize(DeviceConfiguration config)
        {
            if (plugins != null && config != null)
            {
                foreach (var plugin in plugins)
                {
                    try
                    {
                        plugin.SendData -= Plugins_Update_SendData;
                        plugin.SendData += Plugins_Update_SendData;
                        plugin.Initialize(config);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Initialize :: Exception :: " + plugin.Name + " :: " + ex.Message, LogLineType.Error);
                    }
                }
            }
        }

        void Plugins_Update_SendData(EventData data)
        {
            if (plugins != null)
            {
                foreach (var plugin in plugins)
                {
                    try
                    {
                        plugin.GetSentData(data);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Plugin :: Exception : " + plugin.Name + " :: " + ex.Message, LogLineType.Error);
                    }
                }
            }

            if (SendData != null) SendData(data);
        }

        void Plugins_Starting()
        {
            if (plugins != null)
            {
                foreach (var plugin in plugins)
                {
                    try
                    {
                        plugin.Starting();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Starting :: Exception :: " + plugin.Name + " :: " + ex.Message, LogLineType.Error);
                    }
                }
            }
        }

        void Plugins_Closing()
        {
            if (plugins != null)
            {
                foreach (var plugin in plugins)
                {
                    try
                    {
                        plugin.Closing();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Plugin :: Exception :: " + plugin.Name + " :: " + ex.Message);
                    }
                }
            }
        }

    }
}
