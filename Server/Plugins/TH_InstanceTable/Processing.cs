// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

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
    public partial class InstanceTable
    {

        bool firstPass = true;

        Configuration config { get; set; }

        bool AddDatabases = false;

        List<string> ColumnNames { get; set; }

        TH_MTConnect.Streams.ReturnData CurrentData { get; set; }

        //void UpdateStatus(string status)
        //{
        //    if (StatusChanged != null) StatusChanged(status);
        //}

        public void Update_Probe(TH_MTConnect.Components.ReturnData returnData)
        {

            ColumnNames = GetVariablesFromProbeData(returnData);

            if (AddDatabases) CreateInstanceTable(ColumnNames);

        }

        public void Update_Current(TH_MTConnect.Streams.ReturnData returnData)
        {
            CurrentData = returnData;

            InstanceData instanceData = ProcessSingleInstance(returnData);

            PreviousInstanceData_old = PreviousInstanceData_new;

            CurrentInstanceData cid = new CurrentInstanceData();
            cid.currentData = returnData;
            cid.data = instanceData;

            // Send InstanceData object to other Plugins --
            var data = new EventData();
            data.id = "CurrentInstanceData";
            data.data01 = config;
            data.data02 = cid;

            if (SendData != null) SendData(data);
            //--------------------------------------------         
        }

        public void Update_Sample(TH_MTConnect.Streams.ReturnData returnData)
        {
            List<InstanceData> instanceDatas = ProcessInstances(CurrentData, returnData);

            if (AddDatabases) AddRowsToDatabase(ColumnNames, instanceDatas);

            PreviousInstanceData_old = PreviousInstanceData_new;

            // Send instanceDatas to other Plugins --------
            var data = new EventData();
            data.id = "InstanceData";
            data.data01 = config;
            data.data02 = instanceDatas;

            if (SendData != null) SendData(data);
        }


        List<string> GetVariablesFromProbeData(TH_MTConnect.Components.ReturnData returnData)
        {

            List<string> Result = new List<string>();

            TH_MTConnect.Components.DataItemCollection dataItems = TH_MTConnect.Components.Tools.GetDataItemsFromDevice(returnData.devices[0]);

            var ic = InstanceConfiguration.Get(config);
            if (ic != null)
            {
                if (ic.DataItems.Conditions)
                {
                    // Conditions -------------------------------------------------------------------------
                    foreach (TH_MTConnect.Components.DataItem dataItem in dataItems.Conditions)
                    {
                        string name = dataItem.id.ToUpper();
                        if (!Result.Contains(name)) Result.Add(name);
                    }
                    // ------------------------------------------------------------------------------------
                }

                if (ic.DataItems.Events)
                {
                    // Events -----------------------------------------------------------------------------
                    foreach (TH_MTConnect.Components.DataItem dataItem in dataItems.Events)
                    {
                        string name = dataItem.id.ToUpper();
                        if (!Result.Contains(name)) Result.Add(name);
                    }
                    // ------------------------------------------------------------------------------------
                }

                if (ic.DataItems.Samples)
                {
                    // Samples ----------------------------------------------------------------------------
                    foreach (TH_MTConnect.Components.DataItem dataItem in dataItems.Samples)
                    {
                        string name = dataItem.id.ToUpper();
                        if (!Result.Contains(name)) Result.Add(name);
                    }
                    // ------------------------------------------------------------------------------------
                }
            }

            return Result;

        }

        // Before ProcessInstances()
        InstanceData PreviousInstanceData_old;
        // After ProcessInstances()
        InstanceData PreviousInstanceData_new;

        // Process instance table after receiving Sample Data
        List<InstanceData> ProcessInstances(TH_MTConnect.Streams.ReturnData currentData, TH_MTConnect.Streams.ReturnData sampleData)
        {
            List<InstanceData> Result = new List<InstanceData>();

            InstanceData previousData = PreviousInstanceData_old;

            if (currentData != null && sampleData != null)
            {
                if (sampleData.deviceStreams != null && currentData.deviceStreams != null)
                {
                    // Get all of the DataItems from the DeviceStream object
                    TH_MTConnect.Streams.DataItemCollection dataItems = TH_MTConnect.Streams.Tools.GetDataItemsFromDeviceStream(sampleData.deviceStreams[0]);

                    // Convert the DataItems to a List of InstanceVariableData objects
                    List<InstanceVariableData> values = GetVariableDataFromDataItemCollection(dataItems);

                    // Get List of Distinct Timestamps
                    IEnumerable<DateTime> timestamps = values.Select(x => x.timestamp).Distinct();

                    // Sort timestamps by DateTime ASC
                    List<DateTime> sortedTimestamps = timestamps.ToList();
                    sortedTimestamps.Sort();

                    // Get List of Variables used
                    IEnumerable<string> usedVariables = values.Select(x => x.id).Distinct();

                    bool anyChanged = false;

                    var ic = InstanceConfiguration.Get(config);

                    foreach (string variable in usedVariables.ToList())
                    {
                        if (ic.DataItems.Omit.Find(x => x.ToLower() == variable.ToLower()) == null)
                        {
                            anyChanged = true;
                            break;
                        }
                    }

                    if (anyChanged)
                    {
                        foreach (DateTime timestamp in sortedTimestamps.ToList())
                        {
                            InstanceData data = new InstanceData();

                            // Preset previous values into new InstanceData object
                            if (previousData != null) data = previousData.Copy();
                            // Fill unused variables with the values from the CurrentData object
                            else FillInstanceDataWithCurrentData(usedVariables.ToList(), data, currentData);

                            // Set timestamp for InstanceData object
                            data.timestamp = timestamp;

                            data.agentInstanceId = currentData.header.instanceId;

                            // Get List of Values at this timestamp
                            List<InstanceVariableData> valuesAtTimestamp = values.FindAll(x => x.timestamp == timestamp);

                            foreach (InstanceVariableData ivd in valuesAtTimestamp)
                            {
                                InstanceData.Value oldval = data.values.Find(x => x.id == ivd.id);
                                // if value with id is already in data.values then overwrite the value
                                if (oldval != null)
                                {
                                    if (oldval.value != ivd.value.ToString())
                                    {
                                        oldval.value = ivd.value.ToString();
                                    }
                                }
                                // if not already in data.values then create new InstanceData.Value object and add it
                                else
                                {
                                    InstanceData.Value newval = new InstanceData.Value();
                                    newval.id = ivd.id;
                                    newval.value = ivd.value.ToString();
                                    data.values.Add(newval);
                                }

                                data.sequence = ivd.sequence;
                            }

                            previousData = data.Copy();


                            bool changed = false;

                            foreach (var value in valuesAtTimestamp)
                            {
                                if (ic.DataItems.Omit.Find(x => x.ToLower() == value.id.ToLower()) == null)
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
                        InstanceData instanceData = ProcessSingleInstance(currentData);

                        Result.Add(instanceData);

                        previousData = instanceData.Copy();
                    }
                }
            }
            else if (currentData != null)
            {
                InstanceData instanceData = ProcessSingleInstance(currentData);

                Result.Add(instanceData);

                previousData = instanceData.Copy();
            }

            PreviousInstanceData_new = previousData;

            return Result;
        }

        // Process InstanceData after receiving Current Data
        InstanceData ProcessSingleInstance(TH_MTConnect.Streams.ReturnData currentData)
        {
            InstanceData Result = new InstanceData(); ;

            Result.timestamp = currentData.header.creationTime;
            Result.agentInstanceId = currentData.header.instanceId;
            Result.sequence = currentData.header.lastSequence;

            FillInstanceDataWithCurrentData(new List<string>(), Result, currentData);

            return Result;
        }

        void FillInstanceDataWithCurrentData(List<string> usedVariables, InstanceData data, TH_MTConnect.Streams.ReturnData currentData)
        {

            // Get all of the DataItems from the DeviceStream object
            TH_MTConnect.Streams.DataItemCollection dataItems = TH_MTConnect.Streams.Tools.GetDataItemsFromDeviceStream(currentData.deviceStreams[0]);

            // Set Conditions
            foreach (TH_MTConnect.Streams.Condition condition_DI in dataItems.Conditions)
            {
                if (!usedVariables.Contains(condition_DI.dataItemId))
                {
                    InstanceData.Value value = new InstanceData.Value();
                    value.id = condition_DI.dataItemId;
                    value.value = condition_DI.value;
                    data.values.Add(value);
                }
            }

            // Set Events
            foreach (TH_MTConnect.Streams.Event event_DI in dataItems.Events)
            {
                if (!usedVariables.Contains(event_DI.dataItemId))
                {
                    InstanceData.Value value = new InstanceData.Value();
                    value.id = event_DI.dataItemId;
                    value.value = event_DI.CDATA;
                    data.values.Add(value);
                }
            }

            // Set Samples
            foreach (TH_MTConnect.Streams.Sample sample_DI in dataItems.Samples)
            {
                if (!usedVariables.Contains(sample_DI.dataItemId))
                {
                    InstanceData.Value value = new InstanceData.Value();
                    value.id = sample_DI.dataItemId;
                    value.value = sample_DI.CDATA;
                    data.values.Add(value);
                }
            }

        }

        List<InstanceVariableData> GetVariableDataFromDataItemCollection(TH_MTConnect.Streams.DataItemCollection dataItems)
        {

            List<InstanceVariableData> Result = new List<InstanceVariableData>();

            // Get Conditions
            foreach (TH_MTConnect.Streams.Condition condition_DI in dataItems.Conditions)
            {
                InstanceVariableData instanceData = new InstanceVariableData();

                instanceData.id = condition_DI.dataItemId;
                instanceData.value = condition_DI.value;
                instanceData.timestamp = condition_DI.timestamp;
                instanceData.sequence = condition_DI.sequence;

                Result.Add(instanceData);
            }

            // Get Events
            foreach (TH_MTConnect.Streams.Event event_DI in dataItems.Events)
            {
                InstanceVariableData instanceData = new InstanceVariableData();

                instanceData.id = event_DI.dataItemId;
                instanceData.value = event_DI.CDATA;
                instanceData.timestamp = event_DI.timestamp;
                instanceData.sequence = event_DI.sequence;

                Result.Add(instanceData);
            }

            // Get Samples
            foreach (TH_MTConnect.Streams.Sample sample_DI in dataItems.Samples)
            {
                InstanceVariableData instanceData = new InstanceVariableData();

                instanceData.id = sample_DI.dataItemId;
                instanceData.value = sample_DI.CDATA;
                instanceData.timestamp = sample_DI.timestamp;
                instanceData.sequence = sample_DI.sequence;

                Result.Add(instanceData);
            }

            // Sort List by timestamp ASC
            Result.Sort((x, y) => x.timestamp.Second.CompareTo(y.timestamp.Second));

            return Result;

        }

    }
}
