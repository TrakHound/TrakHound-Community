// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TH_Configuration;
using TH_Plugins;

namespace TH_InstanceTable
{
    public partial class Plugin
    {

        private Configuration configuration;

        private bool AddDatabases = false;

        private List<string> columnNames;

        private TH_MTConnect.Streams.ReturnData currentData;

        // Before ProcessInstances()
        private InstanceData PreviousInstanceData_old;
        // After ProcessInstances()
        private InstanceData PreviousInstanceData_new;


        public void Update_Probe(TH_MTConnect.Components.ReturnData returnData)
        {
            columnNames = GetVariablesFromProbeData(returnData);

            if (AddDatabases) CreateInstanceTable(columnNames);
        }

        public void Update_Current(TH_MTConnect.Streams.ReturnData returnData)
        {
            currentData = returnData;

            if (!TH_Global.Variables.SIMULATION_MODE)
            {
                InstanceData instanceData = ProcessInstance(returnData);

                PreviousInstanceData_old = PreviousInstanceData_new;

                CurrentInstanceData cid = new CurrentInstanceData();
                cid.CurrentData = returnData;
                cid.Data = instanceData;

                SendCurrentInstanceData(configuration, cid);
                SendInstanceData(configuration, new List<InstanceData>() { instanceData });
            }
        }

        private void SendInstanceData(Configuration config, List<InstanceData> instanceDatas)
        {
            var data = new EventData();
            data.Id = "InstanceData";
            data.Data01 = config;
            data.Data02 = instanceDatas;

            if (SendData != null) SendData(data);
        }

        private void SendCurrentInstanceData(Configuration config, CurrentInstanceData instanceData)
        {
            var data = new EventData();
            data.Id = "CurrentInstanceData";
            data.Data01 = configuration;
            data.Data02 = instanceData;

            if (SendData != null) SendData(data);    
        }

        public void Update_Sample(TH_MTConnect.Streams.ReturnData returnData)
        {
            List<InstanceData> instanceDatas = ProcessInstances(currentData, returnData);

            if (AddDatabases) AddRowsToDatabase(columnNames, instanceDatas);

            PreviousInstanceData_old = PreviousInstanceData_new;

            SendInstanceData(configuration, instanceDatas);
        }


        List<string> GetVariablesFromProbeData(TH_MTConnect.Components.ReturnData returnData)
        {

            var result = new List<string>();

            var dataItems = returnData.Devices[0].GetAllDataItems();

            var ic = InstanceConfiguration.Get(configuration);
            if (ic != null)
            {
                if (ic.Conditions)
                {
                    // Conditions -------------------------------------------------------------------------
                    foreach (var dataItem in dataItems.FindAll(x => x.Category == TH_MTConnect.Components.DataItemCategory.CONDITION))
                    {
                        string name = dataItem.Id.ToUpper();
                        if (!result.Contains(name)) result.Add(name);
                    }
                    // ------------------------------------------------------------------------------------
                }

                if (ic.Events)
                {
                    // Events -----------------------------------------------------------------------------
                    foreach (var dataItem in dataItems.FindAll(x => x.Category == TH_MTConnect.Components.DataItemCategory.EVENT))
                    {
                        string name = dataItem.Id.ToUpper();
                        if (!result.Contains(name)) result.Add(name);
                    }
                    // ------------------------------------------------------------------------------------
                }

                if (ic.Samples)
                {
                    // Samples ----------------------------------------------------------------------------
                    foreach (var dataItem in dataItems.FindAll(x => x.Category == TH_MTConnect.Components.DataItemCategory.SAMPLE))
                    {
                        string name = dataItem.Id.ToUpper();
                        if (!result.Contains(name)) result.Add(name);
                    }
                    // ------------------------------------------------------------------------------------
                }
            }

            return result;
        }


        // Process instance table after receiving Sample Data
        List<InstanceData> ProcessInstances(TH_MTConnect.Streams.ReturnData currentData, TH_MTConnect.Streams.ReturnData sampleData)
        {
            List<InstanceData> Result = new List<InstanceData>();

            InstanceData previousData = PreviousInstanceData_old;

            if (currentData != null && sampleData != null)
            {
                if (sampleData.DeviceStreams != null && currentData.DeviceStreams != null)
                {
                    // Get all of the DataItems from the DeviceStream object
                    var dataItems = sampleData.DeviceStreams[0].DataItems;

                    // Convert the DataItems to a List of InstanceVariableData objects
                    List<InstanceVariableData> values = GetVariableDataFromDataItemCollection(dataItems);

                    // Get List of Distinct Timestamps
                    IEnumerable<DateTime> timestamps = values.Select(x => x.Timestamp).Distinct();

                    // Sort timestamps by DateTime ASC
                    List<DateTime> sortedTimestamps = timestamps.ToList();
                    sortedTimestamps.Sort();

                    // Get List of Variables used
                    IEnumerable<string> usedVariables = values.Select(x => x.Id).Distinct();

                    bool anyChanged = false;

                    var ic = InstanceConfiguration.Get(configuration);

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
                            var data = new InstanceData();

                            // Preset previous values into new InstanceData object
                            if (previousData != null) data = previousData.Copy();
                            // Fill unused variables with the values from the CurrentData object
                            else FillInstanceDataWithCurrentData(usedVariables.ToList(), data, currentData);

                            // Set timestamp for InstanceData object
                            data.Timestamp = timestamp;

                            data.AgentInstanceId = currentData.Header.InstanceId;

                            // Get List of Values at this timestamp
                            List<InstanceVariableData> valuesAtTimestamp = values.FindAll(x => x.Timestamp == timestamp);

                            foreach (InstanceVariableData ivd in valuesAtTimestamp)
                            {
                                InstanceData.DataItemValue oldval = data.Values.Find(x => x.Id == ivd.Id);
                                // if value with id is already in data.values then overwrite the value
                                if (oldval != null)
                                {
                                    if (oldval.Value != ivd.Value.ToString())
                                    {
                                        oldval.Value = ivd.Value.ToString();
                                    }
                                }
                                // if not already in data.values then create new InstanceData.Value object and add it
                                else
                                {
                                    var newval = new InstanceData.DataItemValue();
                                    newval.Id = ivd.Id;
                                    newval.Value = ivd.Value.ToString();
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

                            if (changed) Result.Add(data);
                        }
                    }
                    else if (currentData != null)
                    {
                        InstanceData instanceData = ProcessInstance(currentData);

                        Result.Add(instanceData);

                        previousData = instanceData.Copy();
                    }
                }
            }
            else if (currentData != null)
            {
                InstanceData instanceData = ProcessInstance(currentData);

                Result.Add(instanceData);

                previousData = instanceData.Copy();
            }

            PreviousInstanceData_new = previousData;

            return Result;
        }

        // Process InstanceData after receiving Current Data
        public static InstanceData ProcessInstance(TH_MTConnect.Streams.ReturnData currentData)
        {
            var result = new InstanceData(); ;
            result.Timestamp = currentData.Header.CreationTime;
            result.AgentInstanceId = currentData.Header.InstanceId;
            result.Sequence = currentData.Header.LastSequence;

            FillInstanceDataWithCurrentData(new List<string>(), result, currentData);

            return result;
        }

        static void FillInstanceDataWithCurrentData(List<string> usedVariables, InstanceData data, TH_MTConnect.Streams.ReturnData currentData)
        {
            // Get all of the DataItems from the DeviceStream object
            var dataItems = currentData.DeviceStreams[0].DataItems;

            foreach (var item in dataItems)
            {
                if (!usedVariables.Contains(item.DataItemId))
                {
                    var value = new InstanceData.DataItemValue();
                    value.Id = item.DataItemId;
                    value.Value = item.Value;
                    data.Values.Add(value);
                    usedVariables.Add(value.Id);
                }
            }

            //// Set Events
            //foreach (var event_DI in dataItems.Events)
            //{
            //    if (!usedVariables.Contains(event_DI.DataItemId))
            //    {
            //        var value = new InstanceData.DataItemValue();
            //        value.Id = event_DI.DataItemId;
            //        value.Value = event_DI.CDATA;
            //        data.Values.Add(value);
            //        usedVariables.Add(value.Id);
            //    }
            //}

            //// Set Samples
            //foreach (var sample_DI in dataItems.Samples)
            //{
            //    if (!usedVariables.Contains(sample_DI.DataItemId))
            //    {
            //        var value = new InstanceData.DataItemValue();
            //        value.Id = sample_DI.DataItemId;
            //        value.Value = sample_DI.CDATA;
            //        data.Values.Add(value);
            //        usedVariables.Add(value.Id);
            //    }
            //}
        }

        List<InstanceVariableData> GetVariableDataFromDataItemCollection(List<TH_MTConnect.Streams.DataItem> dataItems)
        {

            List<InstanceVariableData> Result = new List<InstanceVariableData>();

            foreach (var item in dataItems)
            {
                var instanceData = new InstanceVariableData();
                instanceData.Id = item.DataItemId;
                instanceData.Value = item.Value;
                instanceData.Timestamp = item.Timestamp;
                instanceData.Sequence = item.Sequence;

                Result.Add(instanceData);
            }

            // Get Conditions
            //foreach (var item in dataItems.FindAll(x => x.Category == TH_MTConnect.Streams.DataItemCategory.CONDITION))
            //{
            //    var instanceData = new InstanceVariableData();
            //    instanceData.Id = condition_DI.DataItemId;
            //    instanceData.Value = condition_DI.Value;
            //    instanceData.Timestamp = condition_DI.Timestamp;
            //    instanceData.Sequence = condition_DI.Sequence;

            //    Result.Add(instanceData);
            //}

            //// Get Events
            //foreach (var item in dataItems.FindAll(x => x.Category == TH_MTConnect.Streams.DataItemCategory.CONDITION))
            //{
            //    var instanceData = new InstanceVariableData();
            //    instanceData.Id = event_DI.DataItemId;
            //    instanceData.Value = event_DI.CDATA;
            //    instanceData.Timestamp = event_DI.Timestamp;
            //    instanceData.Sequence = event_DI.Sequence;

            //    Result.Add(instanceData);
            //}

            //// Get Samples
            //foreach (var item in dataItems.FindAll(x => x.Category == TH_MTConnect.Streams.DataItemCategory.CONDITION))
            //{
            //    var instanceData = new InstanceVariableData();
            //    instanceData.Id = sample_DI.DataItemId;
            //    instanceData.Value = sample_DI.CDATA;
            //    instanceData.Timestamp = sample_DI.Timestamp;
            //    instanceData.Sequence = sample_DI.Sequence;

            //    Result.Add(instanceData);
            //}

            // Sort List by timestamp ASC
            Result.Sort((x, y) => x.Timestamp.Second.CompareTo(y.Timestamp.Second));

            return Result;

        }

    }
}
