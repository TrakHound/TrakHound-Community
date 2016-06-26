// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TH_Configuration;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_MTConnect.Plugin
{
    public partial class Plugin : IServerPlugin
    {
        public string Name { get { return "TH_MTConnect"; } }

        public void Initialize(DeviceConfiguration config)
        {
            var ac = AgentConfiguration.Read(config.ConfigurationXML);
            if (ac != null)
            {
                config.CustomClasses.Add(ac);

                configuration = config;
            }
        }

        public void GetSentData(EventData data)
        {
            if (data != null && data.Id != null && data.Data02 != null)
            {
                UpdateDatabaseStatus(data);
            }
        }

        public event SendData_Handler SendData;


        public void Closing()
        {
            Stop();
        }

        //public Type[] ConfigurationPageTypes { get { return new Type[] { typeof(ConfigurationPage.Page) }; } }


        DeviceConfiguration configuration;

        private bool started = false;

        private void UpdateDatabaseStatus(EventData data)
        {
            if (data.Id.ToLower() == "databasestatus")
            {
                if (data.Data02.GetType() == typeof(bool))
                {
                    var connected = (bool)data.Data02;
                    if (connected) Start(configuration);
                    else if (!connected) Stop();
                }
            }
        }

    }
}
