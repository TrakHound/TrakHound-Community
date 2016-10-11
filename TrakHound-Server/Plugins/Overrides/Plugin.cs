// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins.Server;
using TrakHound_Server.Plugins.Instances;

namespace TrakHound_Server.Plugins.Overrides
{
    public class Plugin : IServerPlugin
    {
        private long lastItemSequence = 0;
        private long lastInstanceSequence = 0;
        private DateTime lastTimestamp = DateTime.MinValue;
        private object _lock = new object();

        private DeviceConfiguration configuration;


        public string Name { get { return "Overrides"; } }

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
                if (data.Id == "INSTANCES")
                {
                    var instances = (List<Instance>)data.Data02;
                    if (instances != null)
                    {
                        var oc = Configuration.Get(configuration);
                        if (oc != null)
                        {
                            lock (_lock)
                            {
                                // Create Lists
                                var overrideItems = new List<OverrideItem>();
                                var overrideInstanceItems = new List<OverrideItem>();
                                var overrideInstances = new List<OverrideInstance>();

                                foreach (var instance in instances)
                                {
                                    // Loop through each configured Override
                                    foreach (var x in oc.Overrides)
                                    {
                                        // Find Override Link in instance values
                                        var match = instance.Values.Find(o => o.Id == x.Link);
                                        if (match != null)
                                        {
                                            // Create new OverrideItem object
                                            var item = new OverrideItem(x);
                                            item.Timestamp = instance.Timestamp;
                                            item.Sequence = match.ChangedSequence;

                                            // Read Value
                                            double val = -1;
                                            double.TryParse(match.Value, out val);
                                            item.Value = val;

                                            // Add item to lists
                                            overrideInstanceItems.Add(item);
                                            if (match.ChangedSequence > lastItemSequence)
                                            {
                                                overrideItems.Add(item);
                                                lastItemSequence = match.ChangedSequence;
                                            }
                                        }
                                    }

                                    // Create new OverrideInstance object
                                    var overrideInstance = new OverrideInstance(overrideInstanceItems);
                                    overrideInstance.Timestamp = instance.Timestamp;
                                    overrideInstance.Sequence = instance.Sequence;

                                    // Calculate duration based on stored timestamp
                                    if (lastTimestamp > DateTime.MinValue) overrideInstance.Duration = (instance.Timestamp - lastTimestamp);
                                    else overrideInstance.Duration = TimeSpan.Zero;

                                    // Add to list
                                    overrideInstances.Add(overrideInstance);

                                    // Set new stored values
                                    lastTimestamp = instance.Timestamp;
                                    lastInstanceSequence = instance.Sequence;
                                }

                                if (overrideItems.Count > 0)
                                {
                                    SendOverrideItems(overrideItems);
                                }

                                // Send lists to other plugins
                                SendOverrideInstances(overrideInstances);
                            }
                        }
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing() { }

        private void SendOverrideItems(List<OverrideItem> items)
        {
            var data = new EventData(this);
            data.Id = "OVERRIDE_ITEMS";
            data.Data01 = configuration;
            data.Data02 = items;
            SendData?.Invoke(data);
        }

        private void SendOverrideInstances(List<OverrideInstance> instances)
        {
            var data = new EventData(this);
            data.Id = "OVERRIDE_INSTANCES";
            data.Data01 = configuration;
            data.Data02 = instances;
            SendData?.Invoke(data);
        }

    }
}

