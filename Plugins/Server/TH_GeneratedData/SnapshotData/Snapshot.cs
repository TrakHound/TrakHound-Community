// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;

using MTConnect.Application.Streams;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_InstanceData;

using TH_GeneratedData.GeneratedEvents;

namespace TH_GeneratedData.SnapshotData
{
    public enum SnapshotType
    {
        Collected,
        Generated,
        Variable,
    }

    public class Snapshot
    {
        public string Name { get; set; }
        public SnapshotType Type { get; set; }
        public string Link { get; set; }

        public string Value { get; set; }
        public string PreviousValue { get; set; }

        public DateTime Timestamp { get; set; }
        public DateTime PreviousTimestamp { get; set; }

        public Snapshot()
        {
            Timestamp = DateTime.UtcNow;
            PreviousTimestamp = DateTime.UtcNow;
        }


        public Snapshot Copy()
        {
            var result = new Snapshot();
            result.Timestamp = Timestamp;
            result.Name = Name;
            result.Value = Value;
            result.PreviousTimestamp = PreviousTimestamp;
            result.PreviousValue = PreviousValue;
            return result;
        }


        public class ProcessInfo
        {
            public List<Snapshot> PreviousItems { get; set; }
            public ReturnData CurrentData { get; set; }
            public InstanceData CurrentInstanceData { get; set; }
        }

        public static void Process(DeviceConfiguration config, ProcessInfo info)
        {
            if (info.CurrentInstanceData != null)
            {
                var sdc = SnapshotDataConfiguration.Get(config);
                if (sdc != null)
                {
                    DataTable variables_DT = null;

                    // Get Variables Table from MySQL (if any snapshots are set to "Variable")
                    if (sdc.Snapshots.FindAll(x => x.Type == SnapshotType.Variable).Count > 0)
                    {
                        string tablename;
                        if (config.DatabaseId != null) tablename = config.DatabaseId + "_" + TableNames.Variables;
                        else tablename = TableNames.Variables;

                        variables_DT = Table.Get(config.Databases_Server, tablename);
                    }

                    foreach (var snapshot in sdc.Snapshots)
                    {
                        switch (snapshot.Type)
                        {
                            case SnapshotType.Collected:

                                ProcessCollected(snapshot, info.CurrentData);
                                break;

                            case SnapshotType.Generated:

                                var gec = GeneratedEvents.GeneratedEventsConfiguration.Get(config);
                                if (gec != null)
                                {
                                    ProcessGenerated(snapshot, gec, info.CurrentInstanceData, info.CurrentData);
                                }
                                
                                break;

                            case SnapshotType.Variable:

                                ProcessVariables(snapshot, variables_DT);
                                break;
                        }
                    }
                }
            }
        }

        private static bool ProcessCollected(Snapshot snapshot, ReturnData currentData)
        {
            bool result = false;

            if (currentData.DeviceStreams != null)
            {
                var dataItems = currentData.DeviceStreams[0].GetAllDataItems();

                bool found = false;

                // Seach Conditions
                found = ProcessCollectedCondtion(snapshot, dataItems);

                // Search Events
                if (!found) found = ProcessCollectedEvent(snapshot, dataItems);

                // Search Samples
                if (!found) found = ProcessCollectedSample(snapshot, dataItems);

                result = found;
            }

            return result;
        }

        private static bool ProcessCollectedCondtion(Snapshot snapshot, List<DataItem> dataItems)
        {
            bool result = false;

            var dataItem = dataItems.Find(x => x.DataItemId.ToLower() == snapshot.Link.ToLower());
            if (dataItem != null)
            {
                // If first pass
                if (snapshot.PreviousValue == null)
                {
                    snapshot.PreviousTimestamp = dataItem.Timestamp;
                }

                if (snapshot.Value != dataItem.CDATA)
                {
                    snapshot.PreviousTimestamp = snapshot.Timestamp;
                    snapshot.PreviousValue = snapshot.Value;

                    snapshot.Value = dataItem.CDATA;
                }

                snapshot.Timestamp = dataItem.Timestamp;

                result = true;
            }

            return result;
        }

        private static bool ProcessCollectedEvent(Snapshot snapshot, List<DataItem> dataItems)
        {
            bool result = false;

            var dataItem = dataItems.Find(x => x.DataItemId.ToLower() == snapshot.Link.ToLower());
            if (dataItem != null)
            {
                // If first pass
                if (snapshot.PreviousValue == null)
                {
                    snapshot.PreviousTimestamp = dataItem.Timestamp;
                }

                if (snapshot.Value != dataItem.CDATA)
                {
                    snapshot.PreviousTimestamp = snapshot.Timestamp;
                    snapshot.PreviousValue = snapshot.Value;

                    snapshot.Value = dataItem.CDATA; 
                }

                snapshot.Timestamp = dataItem.Timestamp;

                result = true;
            }

            return result;
        }

        private static bool ProcessCollectedSample(Snapshot snapshot, List<DataItem> dataItems)
        {
            bool result = false;

            var dataItem = dataItems.Find(x => x.DataItemId.ToLower() == snapshot.Link.ToLower());
            if (dataItem != null)
            {
                // If first pass
                if (snapshot.PreviousValue == null)
                {
                    snapshot.PreviousTimestamp = dataItem.Timestamp;
                }

                if (snapshot.Value != dataItem.CDATA)
                {
                    snapshot.PreviousTimestamp = snapshot.Timestamp;
                    snapshot.PreviousValue = snapshot.Value;

                    snapshot.Value = dataItem.CDATA;
                }

                snapshot.Timestamp = dataItem.Timestamp;

                result = true;
            }

            return result;
        }


        private static void ProcessGenerated(Snapshot snapshot, GeneratedEventsConfiguration gec, InstanceData currentInstanceData, ReturnData currentData)
        {
            var e = gec.Events.Find(x => x.Name.ToLower() == snapshot.Link.ToLower());
            if (e != null)
            {
                Return returnValue = e.Process(currentInstanceData);

                if (snapshot.Value != returnValue.Value)
                {
                    if (returnValue != null)
                    {
                        var value = e.Values.Find(x => x.Result.NumVal == returnValue.NumVal);
                        if (value != null)
                        {
                            snapshot.PreviousTimestamp = GetTimestampFromCurrent(value, currentData);
                        }
                    }

                    //snapshot.PreviousTimestamp = snapshot.Timestamp;
                    snapshot.PreviousValue = snapshot.Value;

                    snapshot.Value = returnValue.Value;
                }

                snapshot.Timestamp = returnValue.TimeStamp;
            }
        }

        static DateTime GetTimestampFromCurrent(Value value, ReturnData currentData)
        {
            var result = DateTime.MinValue;

            var dataItems = currentData.DeviceStreams[0].GetAllDataItems();

            foreach (var trigger in value.Triggers)
            {
                var timestamp = GetTimestampFromTrigger(trigger, dataItems);
                if (timestamp > result) result = timestamp;
            }

            foreach (var multitrigger in value.MultiTriggers)
            {
                foreach (var trigger in multitrigger.Triggers)
                {
                    var timestamp = GetTimestampFromTrigger(trigger, dataItems);
                    if (timestamp > result) result = timestamp;
                }
            }

            return result;
        }

        static DateTime GetTimestampFromTrigger(Trigger trigger, List<DataItem> dataItems)
        {
            var result = DateTime.MinValue;

            DataItem item = dataItems.Find(x => x.DataItemId == trigger.Link);

            if (item != null) if (item.Timestamp > result) result = item.Timestamp;

            return result;
        }


        static bool ProcessVariables(Snapshot snapshot, DataTable dt)
        {
            bool result = false;

            if (dt != null)
            {
                DataRow[] rows = dt.Select("variable = '" + snapshot.Link + "'");
                if (rows != null)
                {
                    if (rows.Length > 0)
                    {
                        var val = rows[0]["value"].ToString();

                        if (snapshot.Value != val)
                        {
                            snapshot.PreviousTimestamp = snapshot.Timestamp;
                            snapshot.PreviousValue = snapshot.Value;

                            snapshot.Value = val;

                            DateTime timestamp = DateTime.MinValue;
                            DateTime.TryParse(rows[0]["timestamp"].ToString(), out timestamp);
                            if (timestamp > DateTime.MinValue) snapshot.Timestamp = timestamp;

                            result = true;
                        }
                    }
                }
            }

            return result;
        }
    }
}
