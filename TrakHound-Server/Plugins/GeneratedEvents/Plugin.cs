// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;

using TrakHound;
using TrakHound.Configurations;
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
                    if (data.Id == "INSTANCES")
                    {
                        var instances = (List<Instance>)data.Data02;

                        var events = GeneratedEvent.Process(configuration, instances);

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
            var data = new EventData(this);
            data.Id = "GENERATED_EVENTS";
            data.Data01 = configuration;
            data.Data02 = events.ToList();
            SendData?.Invoke(data);
        }
        
    }
}
