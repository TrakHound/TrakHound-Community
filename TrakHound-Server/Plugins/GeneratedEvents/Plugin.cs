// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;

using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;
using TrakHound_Server.Plugins.Instances;

namespace TrakHound_Server.Plugins.GeneratedEvents
{
    public partial class GeneratedData : IServerPlugin
    {

        public string Name { get { return "GeneratedEvents"; } }

        public void Initialize(DeviceConfiguration config)
        {
            var gec = Configuration.Read(config.Xml);
            if (gec != null)
            {
                config.CustomClasses.Add(gec);

                configuration = config;
            }
        }

        public void GetSentData(EventData data)
        {
            if (data != null && data.Id != null && configuration != null)
            {
                var gec = Configuration.Get(configuration);
                if (gec != null)
                {
                    if (data.Id == "INSTANCE_DATA")
                    {
                        var instanceDatas = (List<InstanceData>)data.Data02;

                        var events = GeneratedEvent.Process(configuration, instanceDatas);

                        // Send List of GeneratedEventItems to other Plugins
                        SendGeneratedEvents(events);
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing() { }

        private DeviceConfiguration configuration;

        void SendGeneratedEvents(List<GeneratedEvent> events)
        {
            var data = new EventData();
            data.Id = "GENERATED_EVENTS";
            data.Data01 = configuration;
            data.Data02 = events.ToList();
            if (SendData != null) SendData(data);
        }
        
    }
}
