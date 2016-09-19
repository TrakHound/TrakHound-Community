// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;

namespace TrakHound_Server.Plugins.MTConnectData
{
    public partial class Plugin : IServerPlugin
    {
        public string Name { get { return "MTConnect"; } }

        public void Initialize(DeviceConfiguration config)
        {
            var ac = Configuration.Read(config.Xml);
            if (ac != null)
            {
                config.CustomClasses.Add(ac);

                configuration = config;

                Start(config);
            }
        }

        public void GetSentData(EventData data) { }

        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing()
        {
            Stop();
        }

        DeviceConfiguration configuration;

        private bool started = false;

    }
}
