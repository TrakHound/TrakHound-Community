// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;
using TrakHound_Server.Plugins.Instances;

namespace TrakHound_Server.Plugins.Cycles
{
    public class Cycles //Disabled
    {

        public string Name { get { return "Cycles"; } }

        public void Initialize(DeviceConfiguration config)
        {
            var cc = Configuration.Read(config.Xml);
            if (cc != null)
            {
                config.CustomClasses.Add(cc);

                configuration = config;
            }
        }

        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                switch (data.Id)
                {
                    case "INSTANCE_DATA":

                        var instances = (List<Instance>)data.Data02;

                        List<CycleData> cycles = ProcessCycles(instances);
                        if (cycles != null && cycles.Count > 0)
                        {
                            SendCycleData(cycles);
                        }

                        break;
                }
            }
        }

        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing() { }


        // Local variables
        private DeviceConfiguration configuration;
        private CycleData storedCycle;
        private DateTime lastTimestamp = DateTime.MinValue;

        private List<CycleData> ProcessCycles(List<Instance> data)
        {
            var result = new List<CycleData>();

            if (data != null)
            {
                if (data.Count > 0)
                {
                    // Filter out data that has already been processed
                    var latestData = data.FindAll(x => x.Timestamp > lastTimestamp);

                    // Insure that InstanceData list is sorted by Timestamp ASC
                    var orderedData = latestData.OrderBy(x => x.Timestamp).ToList();

                    if (configuration != null)
                    {
                        foreach (var instanceData in orderedData)
                        {
                            // Get list of new / updated CycleData objects
                            var cycleDatas = Process(instanceData);

                            // Update last timestamp for filtering
                            lastTimestamp = instanceData.Timestamp;

                            // Add new CycleData objects to returned list
                            result.AddRange(cycleDatas);
                        }
                    }
                }             
            }
            else
            {
                if (storedCycle == null || storedCycle.ProductionType != CycleProductionType.STOPPED)
                {
                    storedCycle = new CycleData();
                    storedCycle.CycleId = Guid.NewGuid().ToString();
                    storedCycle.InstanceId = Guid.NewGuid().ToString();
                    storedCycle.ProductionType = CycleProductionType.STOPPED;
                    storedCycle.Name = "UNAVAILABLE";
                    storedCycle.Event = "UNAVAILABLE";
                    storedCycle.Start = DateTime.UtcNow;
                }

                storedCycle.Stop = DateTime.UtcNow;
                result.Add(storedCycle.Copy());
            }

            return result;
        }


        private List<CycleData> Process(Instance instanceData)
        {
            var result = new List<CycleData>();

            var cc = Configuration.Get(configuration);
            var gc = GeneratedEvents.Configuration.Get(configuration);

            if (cc != null && cc.CycleEventName != null && gc != null)
            {
                // Find the CycleEventName in the Generated Events Configuration
                var cycleEvent = gc.Events.Find(x => x.Name.ToLower() == cc.CycleEventName.ToLower());
                if (cycleEvent != null)
                {
                    // Search for cycle name link in InstanceData
                    var instanceValue = instanceData.Values.Find(x => x.Id == cc.CycleNameLink);
                    if (instanceValue != null)
                    {
                        var cycleOverrides = new List<CycleOverride>();

                        // Get CycleOverride values from InstanceData
                        foreach (var ovr in cc.OverrideLinks)
                        {
                            var cycleOverride = GetOverrideFromInstanceData(ovr, instanceData);
                            if (cycleOverride != null) cycleOverrides.Add(cycleOverride);
                        }

                        // Process Cycle Event using instanceData
                        var eventReturn = cycleEvent.Process(instanceData);
                        if (eventReturn != null)
                        {
                            // Get the name of the cycle
                            string cycleName = instanceValue.Value;

                            // Get the name of the cycleEvent (cycleEvent.Value)
                            string cycleEventValue = eventReturn.Value;

                            // Set cycle to stored cycleData object
                            CycleData cycle = storedCycle;
                            if (cycle != null)
                            {
                                // Update Stop Time (running total so that items such as OEE (which is based on cycles) can be updated)
                                cycle.Stop = instanceData.Timestamp;

                                if (cycle.Name != cycleName || cycle.Event != cycleEventValue || !CompareOverrideLists(cycle.CycleOverrides, cycleOverrides))
                                {
                                    cycle.Completed = true;

                                    // Add a copy of the current cycle to the list
                                    result.Add(cycle.Copy());

                                    // Check if new cycle has been started
                                    if (cycle.Name != cycleName || cycleEventValue == cc.StoppedEventValue)
                                    {
                                        // Set cycle to new instance
                                        cycle = new CycleData();
                                        cycle.CycleId = Guid.NewGuid().ToString();
                                        cycle.InstanceId = Guid.NewGuid().ToString();
                                        cycle.Name = cycleName;
                                        cycle.Event = cycleEventValue;
                                        cycle.Completed = false;

                                        // Set Production Type
                                        var productionType = cc.ProductionTypes.Find(x => x.EventValue == cycleEventValue);
                                        if (productionType != null) cycle.ProductionType = productionType.ProductionType;
                                        else cycle.ProductionType = CycleProductionType.UNCATEGORIZED;

                                        cycle.Start = instanceData.Timestamp;
                                        cycle.Stop = instanceData.Timestamp;
                                        cycle.CycleOverrides = cycleOverrides.ToList();
                                    }
                                    else
                                    {
                                        // Set cycle to new Event
                                        cycle.InstanceId = Guid.NewGuid().ToString();
                                        cycle.Event = cycleEventValue;
                                        cycle.Completed = false;

                                        // Set Production Type
                                        var productionType = cc.ProductionTypes.Find(x => x.EventValue == cycleEventValue);
                                        if (productionType != null) cycle.ProductionType = productionType.ProductionType;
                                        else cycle.ProductionType = CycleProductionType.UNCATEGORIZED;

                                        cycle.Start = instanceData.Timestamp;
                                        cycle.Stop = instanceData.Timestamp;
                                        cycle.CycleOverrides = cycleOverrides.ToList();
                                    }
                                }

                                result.Add(cycle);
                            }
                            else
                            {
                                // Set cycle to new instance
                                cycle = new CycleData();
                                cycle.CycleId = Guid.NewGuid().ToString();
                                cycle.InstanceId = Guid.NewGuid().ToString();
                                cycle.Name = cycleName;
                                cycle.Event = cycleEventValue;

                                // Set Production Type
                                var productionType = cc.ProductionTypes.Find(x => x.EventValue == cycleEventValue);
                                if (productionType != null) cycle.ProductionType = productionType.ProductionType;
                                else cycle.ProductionType = CycleProductionType.UNCATEGORIZED;

                                cycle.Start = instanceData.Timestamp;
                                cycle.Stop = instanceData.Timestamp;
                                cycle.CycleOverrides = cycleOverrides.ToList();

                                result.Add(cycle);
                            }

                            storedCycle = cycle.Copy();
                        }
                    }
                }     
            }

            return result;
        }

        private static bool CompareOverrideLists(List<CycleOverride> l1, List<CycleOverride> l2)
        {
            // Check if one is null and the other isn't
            if (l1 == null && l2 != null) return false;
            if (l1 != null && l2 == null) return false;

            // Check count
            if (l1.Count != l2.Count) return false;

            //Check Values and Id's
            for (var x = 0; x <= l1.Count - 1; x++)
            {
                if (l1[x].Id != l2[x].Id ||
                    l1[x].Value != l2[x].Value)
                {
                    return false;
                }
            }

            return true;
        }

        private static CycleOverride GetOverrideFromInstanceData(string overrideId, Instance instanceData)
        {
            CycleOverride result = null;

            var val = instanceData.Values.Find(x => x.Id == overrideId);
            if (val != null)
            {
                double d = -1;
                if (double.TryParse(val.Value, out d))
                {
                    result = new CycleOverride();
                    result.Id = overrideId;
                    result.Value = d;
                }
            }

            return result;
        }
        
        void SendCycleData(List<CycleData> cycleData)
        {
            var data = new EventData(this);
            data.Id = "CYCLES";
            data.Data01 = configuration;
            data.Data02 = cycleData;
            SendData?.Invoke(data);
        }
    }
    
}
