// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins.Server;
using TrakHound_Server.Plugins.GeneratedEvents;
using TrakHound_Server.Plugins.Overrides;

namespace TrakHound_Server.Plugins.OEE
{
    public class Plugin : IServerPlugin
    {
        private List<OverrideInstance> overrideInstancesQueue = new List<OverrideInstance>();
        private List<GeneratedEvent> gEventsQueue = new List<GeneratedEvent>();

        private object _lock = new object();

        private DeviceConfiguration configuration { get; set; }

        public string Name { get { return "OEE"; } }

        public void Initialize(DeviceConfiguration config)
        {
            var oc = Configuration.Read(config.Xml);
            if (oc != null)
            {
                config.CustomClasses.Add(oc);
                configuration = config;
            }
        }

        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                bool found = false;

                if (data.Id == "OVERRIDE_INSTANCES")
                {
                    if (data.Data02.GetType() == typeof(List<OverrideInstance>))
                    {
                        var instances = (List<OverrideInstance>)data.Data02;
                        if (instances != null)
                        {
                            lock (_lock)
                            {
                                overrideInstancesQueue.AddRange(instances);
                            }
                            found = true;
                        }
                    }
                }

                if (data.Id == "GENERATED_EVENTS")
                {
                    if (data.Data02.GetType() == typeof(List<GeneratedEvent>))
                    {
                        var gEvents = (List<GeneratedEvent>)data.Data02;
                        if (gEvents != null)
                        {
                            var oc = Configuration.Get(configuration);
                            if (oc != null)
                            {
                                var operatingEvents = gEvents.FindAll(o => o.EventName == oc.OperatingEventName && o.CurrentValue != null);
                                if (operatingEvents != null)
                                {
                                    lock (_lock)
                                    {
                                        gEventsQueue.AddRange(operatingEvents);
                                    }
                                    found = true;
                                }
                            }
                        }
                    }
                }

                if (found)
                {
                    lock (_lock)
                    {
                        ProcessQueue();
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing() { }

        private void ProcessQueue()
        {
            // Get Lists of just the Sequence numbers
            var gEventSequences = gEventsQueue.Select(o => o.CurrentValue.Sequence);
            var overrideSequences = overrideInstancesQueue.Select(o => o.Sequence);

            var oc = Configuration.Get(configuration);
            if (oc != null)
            {
                bool useOverrides = oc.Overrides.Count > 0;

                List<long> sequences = null;

                // Find intersecting Sequences in queues
                if (useOverrides) sequences = gEventSequences.Intersect(overrideSequences).ToList();
                else sequences = gEventSequences.ToList();

                if (sequences != null)
                {
                    // Create list of OeeData objects
                    var oeeDatas = new List<OEEData>();

                    foreach (var sequence in sequences)
                    {
                        // Find Sequence in queue lists
                        var gEvent = gEventsQueue.Find(o => o.CurrentValue.Sequence == sequence);
                        var overrideInstance = overrideInstancesQueue.Find(o => o.Sequence == sequence);

                        if (gEvent != null && (overrideInstance != null || !useOverrides))
                        {
                            // Create new OeeData object
                            var oeeData = new OEEData();
                            oeeData.Timestamp = gEvent.CurrentValue.Timestamp;
                            oeeData.Sequence = gEvent.CurrentValue.Sequence;

                            double duration = gEvent.Duration.TotalSeconds;

                            // Set Planned Production Time to entire duration
                            oeeData.PlannedProductionTime = duration;

                            // Test if Event's Value is equal to OEE's configured value
                            if (gEvent.CurrentValue.Value == oc.OperatingEventValue)
                            {
                                // Set Operating Time to entire duration
                                oeeData.OperatingTime = duration;

                                // Set Ideal OperatingTime to adjusted duration using Feedrate Overrdie average
                                if (useOverrides) oeeData.IdealOperatingTime = duration * (overrideInstance.FeedrateAverage / 100);
                                else oeeData.IdealOperatingTime = duration;
                            }

                            // Add to list
                            oeeDatas.Add(oeeData);
                        }

                        // Remove processed items from queue
                        gEventsQueue.RemoveAll(o => o.CurrentValue.Sequence == sequence);
                        overrideInstancesQueue.RemoveAll(o => o.Sequence == sequence);
                    }

                    // Send OeeData objects
                    if (oeeDatas.Count > 0) SendOeeData(oeeDatas);
                }
            }
        }

        private void SendOeeData(List<OEEData> oeeDatas)
        {
            var data = new EventData(this);
            data.Id = "OEE";
            data.Data01 = configuration;
            data.Data02 = oeeDatas;
            SendData?.Invoke(data);
        }
    }

}
