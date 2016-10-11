// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TrakHound;
using TrakHound.Configurations;

namespace TrakHound_Server.Plugins.Instances
{
    public partial class Plugin
    {

        private DeviceConfiguration configuration;

        private MTConnect.Application.Streams.ReturnData currentData;

        // Before ProcessInstances()
        private Instance previousInstanceDataOld;
        // After ProcessInstances()
        private Instance previousInstanceDataNew;

        private DateTime lastTimestamp = DateTime.MinValue;

        private List<Instance> bufferedInstances = new List<Instance>();


        public void Update_Current(MTConnect.Application.Streams.ReturnData returnData)
        {
            currentData = returnData;

            Instance instance = ProcessInstance(returnData);

            previousInstanceDataOld = previousInstanceDataNew;

            var cid = new CurrentInstance();
            cid.CurrentData = returnData;
            cid.Instance = instance;

            var instances = new List<Instance>();
            if (bufferedInstances != null && bufferedInstances.Count > 0)
            {
                instances.AddRange(bufferedInstances);
            }

            // Only return new instances
            instances = instances.FindAll(o => o.Timestamp > lastTimestamp);

            // Sort instances ASC by timestamp
            instances = instances.OrderBy(o => o.Timestamp).ToList();

            // update lastTimestamp
            if (instances.Count > 0)
            {
                lastTimestamp = instances[instances.Count - 1].Timestamp;
            }

            // Clear the send buffer
            bufferedInstances.Clear();
            bufferedInstances.Add(instance);

            SendCurrentInstanceData(configuration, cid);
            SendInstanceData(configuration, instances);
        }

        public void Update_Sample(MTConnect.Application.Streams.ReturnData returnData)
        {
            List<Instance> instances = ProcessInstances(currentData, returnData);

            bufferedInstances.AddRange(instances);

            previousInstanceDataOld = previousInstanceDataNew;
        }


        private void SendInstanceData(DeviceConfiguration config, List<Instance> instances)
        {
            var data = new EventData(this);
            data.Id = "INSTANCES";
            data.Data01 = config;
            data.Data02 = instances;

            SendData?.Invoke(data);
        }

        private void SendCurrentInstanceData(DeviceConfiguration config, CurrentInstance instance)
        {
            var data = new EventData(this);
            data.Id = "CURRENT_INSTANCE";
            data.Data01 = configuration;
            data.Data02 = instance;

            SendData?.Invoke(data);
        }


        private static List<string> GetDataItemIds(MTConnect.Application.Components.ReturnData returnData)
        {
            var result = new List<string>();

            var dataItems = returnData.Devices[0].GetAllDataItems();

            // Conditions -------------------------------------------------------------------------
            foreach (var dataItem in dataItems.FindAll(x => x.Category == MTConnect.DataItemCategory.CONDITION))
            {
                string name = dataItem.Id.ToUpper();
                if (!result.Contains(name)) result.Add(name);
            }
            // ------------------------------------------------------------------------------------

            // Events -----------------------------------------------------------------------------
            foreach (var dataItem in dataItems.FindAll(x => x.Category == MTConnect.DataItemCategory.EVENT))
            {
                string name = dataItem.Id.ToUpper();
                if (!result.Contains(name)) result.Add(name);
            }
            // ------------------------------------------------------------------------------------

            // Samples ----------------------------------------------------------------------------
            foreach (var dataItem in dataItems.FindAll(x => x.Category == MTConnect.DataItemCategory.SAMPLE))
            {
                string name = dataItem.Id.ToUpper();
                if (!result.Contains(name)) result.Add(name);
            }
            // ------------------------------------------------------------------------------------

            return result;
        }


        // Process instance table after receiving Sample Data
        private List<Instance> ProcessInstances(MTConnect.Application.Streams.ReturnData currentData, MTConnect.Application.Streams.ReturnData sampleData)
        {
            var stpw = new System.Diagnostics.Stopwatch();
            stpw.Start();

            var result = new List<Instance>();

            Instance previousData = previousInstanceDataOld;

            if (currentData != null && sampleData != null)
            {
                if (sampleData.DeviceStreams != null && currentData.DeviceStreams != null)
                {
                    // Get all of the DataItems from the DeviceStream object
                    var dataItems = sampleData.DeviceStreams[0].GetAllDataItems();

                    // Convert the DataItems to a List of VariableData objects
                    List<VariableData> values = VariableData.Get(dataItems);

                    // Get List of Distinct Timestamps
                    IEnumerable<DateTime> timestamps = values.Select(x => x.Timestamp).Distinct();

                    // Sort timestamps by DateTime ASC
                    List<DateTime> sortedTimestamps = timestamps.ToList();
                    sortedTimestamps.Sort();

                    // Get List of Variables used
                    IEnumerable<string> usedVariables = values.Select(x => x.Id).Distinct();

                    var currentDataItems = currentData.DeviceStreams[0].GetAllDataItems();

                    foreach (DateTime timestamp in sortedTimestamps.ToList())
                    {
                        if (previousData == null || timestamp > previousData.Timestamp)
                        {
                            var data = new Instance();

                            // Preset previous values into new InstanceData object
                            if (previousData != null) data = previousData;
                            // Fill unused variables with the values from the CurrentData object
                            else FillInstanceDataWithCurrentData(usedVariables.ToList(), data, currentDataItems);

                            // Set timestamp for InstanceData object
                            data.Timestamp = timestamp;

                            data.AgentInstanceId = currentData.Header.InstanceId;

                            // Get List of Values at this timestamp
                            var valuesAtTimestamp = values.FindAll(x => x.Timestamp == timestamp);

                            foreach (var ivd in valuesAtTimestamp)
                            {
                                Instance.DataItemValue oldval = data.Values.Find(x => x.Id == ivd.Id);
                                // if value with id is already in data.values then overwrite the value
                                if (oldval != null)
                                {
                                    string s = null;
                                    if (ivd.Value != null) s = ivd.Value.ToString();

                                    if (oldval.Value != s)
                                    {
                                        oldval.Value = s;
                                        oldval.ChangedSequence = ivd.Sequence;
                                    }
                                }
                                // if not already in data.values then create new InstanceData.Value object and add it
                                else
                                {
                                    var newval = new Instance.DataItemValue();
                                    newval.Id = ivd.Id;
                                    newval.Type = ivd.Type;
                                    newval.SubType = ivd.SubType;
                                    newval.ChangedSequence = ivd.Sequence;

                                    if (ivd.Value != null) newval.Value = ivd.Value.ToString();
                                    data.Values.Add(newval);
                                }

                                data.Sequence = ivd.Sequence;
                            }

                            previousData = data.Copy();

                            result.Add(data);
                        }
                    }
                }
            }
            else if (currentData != null)
            {
                Instance instanceData = ProcessInstance(currentData);

                if (previousData == null || instanceData.Timestamp > previousData.Timestamp)
                {
                    result.Add(instanceData);

                    previousData = instanceData.Copy();
                }
            }

            previousInstanceDataNew = previousData;

            return result;
        }

        // Process InstanceData after receiving Current Data
        public static Instance ProcessInstance(MTConnect.Application.Streams.ReturnData currentData)
        {
            var result = new Instance(); ;
            result.Timestamp = currentData.Header.CreationTime; // Agent.MTConnect.org only outputs to the nearest second (not fractional seconds), check if issue with Open Source Agent
            result.AgentInstanceId = currentData.Header.InstanceId;
            result.Sequence = currentData.Header.LastSequence;

            var dataItems = currentData.DeviceStreams[0].GetAllDataItems();
            FillInstanceDataWithCurrentData(new List<string>(), result, dataItems);

            return result;
        }

        static void FillInstanceDataWithCurrentData(List<string> usedVariables, Instance data, List<MTConnect.Application.Streams.DataItem> dataItems)
        {
            foreach (var item in dataItems)
            {
                if (!usedVariables.Contains(item.DataItemId))
                {
                    var value = new Instance.DataItemValue();
                    value.Id = item.DataItemId;

                    value.Type = item.Type;
                    value.SubType = item.SubType;
                    value.ChangedSequence = item.Sequence;

                    if (item.Category == MTConnect.DataItemCategory.CONDITION)
                    {
                        value.Value = ((MTConnect.Application.Streams.Condition)item).ConditionValue.ToString();
                    }
                    else value.Value = item.CDATA;

                    data.Values.Add(value);
                    usedVariables.Add(value.Id);
                }
            }
        }

    }
}
