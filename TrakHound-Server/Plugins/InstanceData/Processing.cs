// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TrakHound.Configurations;
using TrakHound.Plugins;

namespace TrakHound_Server.Plugins.Instances
{
    public partial class Plugin
    {

        private DeviceConfiguration configuration;

        private MTConnect.Application.Streams.ReturnData currentData;

        // Before ProcessInstances()
        private InstanceData previousInstanceDataOld;
        // After ProcessInstances()
        private InstanceData previousInstanceDataNew;

        private DateTime lastTimestamp = DateTime.MinValue;

        private List<InstanceData> bufferedInstanceDatas = new List<InstanceData>();


        public void Update_Current(MTConnect.Application.Streams.ReturnData returnData)
        {
            currentData = returnData;

            InstanceData instanceData = ProcessInstance(returnData);

            previousInstanceDataOld = previousInstanceDataNew;

            var cid = new CurrentInstanceData();
            cid.CurrentData = returnData;
            cid.Data = instanceData;

            var instanceDatas = new List<InstanceData>();
            if (bufferedInstanceDatas != null && bufferedInstanceDatas.Count > 0)
            {
                instanceDatas.AddRange(bufferedInstanceDatas);
            }

            // Only return new instances
            instanceDatas = instanceDatas.FindAll(o => o.Timestamp > lastTimestamp);

            // Sort instances ASC by timestamp
            instanceDatas = instanceDatas.OrderBy(o => o.Timestamp).ToList();

            // update lastTimestamp
            if (instanceDatas.Count > 0)
            {
                lastTimestamp = instanceDatas[instanceDatas.Count - 1].Timestamp;
            }

            // Clear the send buffer
            bufferedInstanceDatas.Clear();
            bufferedInstanceDatas.Add(instanceData);

            SendCurrentInstanceData(configuration, cid);
            SendInstanceData(configuration, instanceDatas);
        }

        public void Update_Sample(MTConnect.Application.Streams.ReturnData returnData)
        {
            List<InstanceData> instanceDatas = ProcessInstances(currentData, returnData);

            bufferedInstanceDatas.AddRange(instanceDatas);

            previousInstanceDataOld = previousInstanceDataNew;
        }


        private void SendInstanceData(DeviceConfiguration config, List<InstanceData> instanceDatas)
        {
            var data = new EventData();
            data.Id = "INSTANCE_DATA";
            data.Data01 = config;
            data.Data02 = instanceDatas;

            SendData?.Invoke(data);
        }

        private void SendCurrentInstanceData(DeviceConfiguration config, CurrentInstanceData instanceData)
        {
            var data = new EventData();
            data.Id = "CURRENT_INSTANCE_DATA";
            data.Data01 = configuration;
            data.Data02 = instanceData;

            SendData?.Invoke(data);    
        }


        private static List<string> GetDataItemIds(MTConnect.Application.Components.ReturnData returnData)
        {
            var result = new List<string>();

            var dataItems = returnData.Devices[0].GetAllDataItems();

            // Conditions -------------------------------------------------------------------------
            foreach (var dataItem in dataItems.FindAll(x => x.Category == MTConnect.Application.Components.DataItemCategory.CONDITION))
            {
                string name = dataItem.Id.ToUpper();
                if (!result.Contains(name)) result.Add(name);
            }
            // ------------------------------------------------------------------------------------

            // Events -----------------------------------------------------------------------------
            foreach (var dataItem in dataItems.FindAll(x => x.Category == MTConnect.Application.Components.DataItemCategory.EVENT))
            {
                string name = dataItem.Id.ToUpper();
                if (!result.Contains(name)) result.Add(name);
            }
            // ------------------------------------------------------------------------------------

            // Samples ----------------------------------------------------------------------------
            foreach (var dataItem in dataItems.FindAll(x => x.Category == MTConnect.Application.Components.DataItemCategory.SAMPLE))
            {
                string name = dataItem.Id.ToUpper();
                if (!result.Contains(name)) result.Add(name);
            }
            // ------------------------------------------------------------------------------------

            return result;
        }


        // Process instance table after receiving Sample Data
        private List<InstanceData> ProcessInstances(MTConnect.Application.Streams.ReturnData currentData, MTConnect.Application.Streams.ReturnData sampleData)
        {
            var result = new List<InstanceData>();

            InstanceData previousData = previousInstanceDataOld;

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

                    bool anyChanged = false;

                    var ic = Configuration.Get(configuration);

                    foreach (string variable in usedVariables.ToList())
                    {
                        if (ic.Omit.Find(x => x.ToLower() == variable.ToLower()) == null)
                        {
                            anyChanged = true;
                            break;
                        }
                    }

                    if (anyChanged)
                    {
                        foreach (DateTime timestamp in sortedTimestamps.ToList())
                        {
                            if (previousData == null || timestamp > previousData.Timestamp)
                            {
                                var data = new InstanceData();

                                // Preset previous values into new InstanceData object
                                if (previousData != null) data = previousData.Copy();
                                // Fill unused variables with the values from the CurrentData object
                                else FillInstanceDataWithCurrentData(usedVariables.ToList(), data, currentData);

                                // Set timestamp for InstanceData object
                                data.Timestamp = timestamp;

                                data.AgentInstanceId = currentData.Header.InstanceId;

                                // Get List of Values at this timestamp
                                var valuesAtTimestamp = values.FindAll(x => x.Timestamp == timestamp);

                                foreach (var ivd in valuesAtTimestamp)
                                {
                                    InstanceData.DataItemValue oldval = data.Values.Find(x => x.Id == ivd.Id);
                                    // if value with id is already in data.values then overwrite the value
                                    if (oldval != null)
                                    {
                                        string s = null;
                                        if (ivd.Value != null) s = ivd.Value.ToString();

                                        if (oldval.Value != s)
                                        {
                                            oldval.Value = s;
                                        }
                                    }
                                    // if not already in data.values then create new InstanceData.Value object and add it
                                    else
                                    {
                                        var newval = new InstanceData.DataItemValue();
                                        newval.Id = ivd.Id;
                                        newval.Type = ivd.Type;
                                        newval.SubType = ivd.SubType;

                                        if (ivd.Value != null) newval.Value = ivd.Value.ToString();
                                        data.Values.Add(newval);
                                    }

                                    data.Sequence = ivd.Sequence;
                                }

                                previousData = data.Copy();


                                bool changed = false;

                                foreach (var value in valuesAtTimestamp)
                                {
                                    if (ic.Omit.Find(x => x.ToLower() == value.Id.ToLower()) == null)
                                    {
                                        changed = true;
                                        break;
                                    }
                                }

                                if (changed) result.Add(data);
                            }
                        }
                    }
                    else if (currentData != null)
                    {
                        InstanceData instanceData = ProcessInstance(currentData);

                        if (previousData == null || instanceData.Timestamp > previousData.Timestamp)
                        {
                            result.Add(instanceData);

                            previousData = instanceData.Copy();
                        }
                    }
                }
            }
            else if (currentData != null)
            {
                InstanceData instanceData = ProcessInstance(currentData);

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
        public static InstanceData ProcessInstance(MTConnect.Application.Streams.ReturnData currentData)
        {
            var result = new InstanceData(); ;
            result.Timestamp = currentData.Header.CreationTime; // Agent.MTConnect.org only outputs to the nearest second (not fractional seconds), check if issue with Open Source Agent
            result.AgentInstanceId = currentData.Header.InstanceId;
            result.Sequence = currentData.Header.LastSequence;

            FillInstanceDataWithCurrentData(new List<string>(), result, currentData);

            return result;
        }

        static void FillInstanceDataWithCurrentData(List<string> usedVariables, InstanceData data, MTConnect.Application.Streams.ReturnData currentData)
        {
            // Get all of the DataItems from the DeviceStream object
            var dataItems = currentData.DeviceStreams[0].GetAllDataItems();

            foreach (var item in dataItems)
            {
                if (!usedVariables.Contains(item.DataItemId))
                {
                    var value = new InstanceData.DataItemValue();
                    value.Id = item.DataItemId;

                    value.Type = item.Type;
                    value.SubType = item.SubType;

                    if (item.Category == MTConnect.Application.Streams.DataItemCategory.CONDITION)
                    {
                        value.Value = item.Value;
                    }
                    else value.Value = item.CDATA;


                    data.Values.Add(value);
                    usedVariables.Add(value.Id);
                }
            }
        }
        
    }
}
