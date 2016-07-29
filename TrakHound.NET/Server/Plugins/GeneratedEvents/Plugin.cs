// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TrakHound.Configurations;
using TrakHound.Server.Plugins.Instances;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;

namespace TrakHound.Server.Plugins.GeneratedEvents
{
    public partial class GeneratedData : IServerPlugin
    {

        public string Name { get { return "TH_GeneratedEvents"; } }

        public void Initialize(DeviceConfiguration config)
        {
            var gec = GeneratedEventsConfiguration.Read(config.ConfigurationXML);
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
                var gec = GeneratedEventsConfiguration.Get(configuration);
                if (gec != null)
                {
                    if (data.Id.ToLower() == "instancedata")
                    {
                        var instanceDatas = (List<InstanceData>)data.Data02;

                        List<GeneratedEvent> events = GeneratedEvent.Process(configuration, instanceDatas);

                        // Send List of GeneratedEventItems to other Plugins--------
                        SendGeneratedEvents(events);
                        // ----------------------------------------------------

                        Database.InsertEventItems(configuration, events);
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Starting()
        {
            var gec = GeneratedEventsConfiguration.Get(configuration);
            if (gec != null)
            {
                Database.CreateValueTable(configuration, gec.Events);

                foreach (var e in gec.Events) Database.CreateEventTable(configuration, e);
            }
        }

        public void Closing() { }

        //public Type[] ConfigurationPageTypes { get { return new Type[] { typeof(ConfigurationPage.Page) }; } }


        private DeviceConfiguration configuration;

        void SendGeneratedEvents(List<GeneratedEvent> events)
        {
            var data = new EventData();
            data.Id = "GeneratedEventItems";
            data.Data01 = configuration;
            data.Data02 = events.ToList();
            if (SendData != null) SendData(data);
        }
        
    }
}
