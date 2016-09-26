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

namespace TrakHound.Servers.DataProcessing
{
    /// <summary>
    /// Data Processing Server for individual device. Handles plugins and events related to the device.
    /// </summary>
    public partial class DeviceServer
    {

        public DeviceConfiguration Configuration { get; set; }

        private bool _isRunning = false;
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                if (_isRunning && Started != null) Started(this);
                else Stopped?.Invoke(this);
            }
        }

        public delegate void StatusChanged_Handler(DeviceServer server);
        public event StatusChanged_Handler Started;
        public event StatusChanged_Handler Stopped;

        public event SendData_Handler SendData;

        private ManualResetEvent stop;

        private List<IServerPlugin> plugins { get; set; }


        public DeviceServer(DeviceConfiguration config, List<IServerPlugin> serverPlugins)
        {
            Configuration = config;
            plugins = serverPlugins;
        }

        public void Start()
        {
            IsRunning = true;
            stop = new ManualResetEvent(false);

            Initialize(Configuration);

            StartPlugins();
        }

        public void Stop()
        {
            stop.Set();

            ClosePlugins();

            IsRunning = false;

            Logger.Log("Device Server Stopped :: " + Configuration.Description.Description + " [" + Configuration.Description.DeviceId + "]", LogLineType.Notification);
        }


        private void Initialize(DeviceConfiguration config)
        {
            Configuration = config;

            InitializePlugins(config);
        }

        private void PrintDeviceHeader(DeviceConfiguration config)
        {
            Logger.Log("Device [" + config.Index.ToString() + "] ---------------------------------------", LogLineType.Console);

            Logger.Log("Description ----------------------------", LogLineType.Console);
            if (config.Description.Description != null) Logger.Log(config.Description.Description, LogLineType.Console);
            if (config.Description.Manufacturer != null) Logger.Log(config.Description.Manufacturer, LogLineType.Console);
            if (config.Description.Model != null) Logger.Log(config.Description.Model, LogLineType.Console);
            if (config.Description.Serial != null) Logger.Log(config.Description.Serial, LogLineType.Console);

            Logger.Log("--------------------------------------------------", LogLineType.Console);
        }

        private void InitializePlugins(DeviceConfiguration config)
        {
            if (plugins != null && config != null)
            {
                foreach (var plugin in plugins)
                {
                    try
                    {
                        plugin.SendData -= SendPluginData;
                        plugin.SendData += SendPluginData;
                        plugin.Initialize(config);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Initialize :: Exception :: " + plugin.Name + " :: " + ex.Message, LogLineType.Error);
                    }
                }
            }
        }


        private class SendDataInfo
        {
            public SendDataInfo(IServerPlugin plugin, EventData data)
            {
                Plugin = plugin;
                EventData = data;
            }

            public IServerPlugin Plugin { get; set; }
            public EventData EventData { get; set; }
        }

        public void SendPluginData(string id, string message)
        {
            var data = new EventData(this);
            data.Id = id;
            data.Data01 = message;

            SendPluginData(data);
        }

        public void SendPluginData(string id, object obj)
        {
            var data = new EventData(this);
            data.Id = id;
            data.Data01 = obj;

            SendPluginData(data);
        }

        private void SendPluginData(EventData data)
        {
            if (plugins != null)
            {
                foreach (var plugin in plugins)
                {
                    var sendDataInfo = new SendDataInfo(plugin, data);

                    ThreadPool.QueueUserWorkItem(new WaitCallback(PluginSendData), sendDataInfo); 
                }
            }

            SendData?.Invoke(data);
        }

        private void PluginSendData(object o)
        {
            if (o != null)
            {
                var sendDataInfo = (SendDataInfo)o;

                try
                {
                    sendDataInfo.Plugin.GetSentData(sendDataInfo.EventData);
                }
                catch (Exception ex)
                {
                    Logger.Log("Plugin Send Data :: Exception : " + sendDataInfo.Plugin.Name + " :: " + ex.Message, LogLineType.Error);
                }
            }  
        }


        private void StartPlugins()
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

        private void ClosePlugins()
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
