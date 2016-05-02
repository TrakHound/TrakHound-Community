// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_InstanceTable;

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
            public TH_MTConnect.Streams.ReturnData CurrentData { get; set; }
            public InstanceData CurrentInstanceData { get; set; }
        }

        public static void Process(Configuration config, ProcessInfo info)
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
                                    ProcessGenerated(snapshot, gec, info.CurrentInstanceData);
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

        private static bool ProcessCollected(Snapshot snapshot, TH_MTConnect.Streams.ReturnData currentData)
        {
            bool result = false;

            if (currentData.DeviceStreams != null)
            {
                TH_MTConnect.Streams.DataItemCollection dataItems = TH_MTConnect.Streams.Tools.GetDataItemsFromDeviceStream(currentData.DeviceStreams[0]);

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

        private static bool ProcessCollectedCondtion(Snapshot snapshot, TH_MTConnect.Streams.DataItemCollection dataItems)
        {
            bool result = false;

            TH_MTConnect.Streams.Condition dataItem = dataItems.Conditions.Find(x => x.DataItemId.ToLower() == snapshot.Link.ToLower());
            if (dataItem != null)
            {
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

        private static bool ProcessCollectedEvent(Snapshot snapshot, TH_MTConnect.Streams.DataItemCollection dataItems)
        {
            bool result = false;

            TH_MTConnect.Streams.Event dataItem = dataItems.Events.Find(x => x.DataItemId.ToLower() == snapshot.Link.ToLower());
            if (dataItem != null)
            {
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

        private static bool ProcessCollectedSample(Snapshot snapshot, TH_MTConnect.Streams.DataItemCollection dataItems)
        {
            bool result = false;

            TH_MTConnect.Streams.Sample dataItem = dataItems.Samples.Find(x => x.DataItemId.ToLower() == snapshot.Link.ToLower());
            if (dataItem != null)
            {
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


        private static void ProcessGenerated(Snapshot snapshot, GeneratedEvents.GeneratedEventsConfiguration gec, InstanceData currentInstanceData)
        {
            var e = gec.Events.Find(x => x.Name.ToLower() == snapshot.Link.ToLower());
            if (e != null)
            {
                Return returnValue = e.Process(currentInstanceData);

                if (snapshot.Value != returnValue.Value)
                {
                    snapshot.PreviousTimestamp = snapshot.Timestamp;
                    snapshot.PreviousValue = snapshot.Value;

                    snapshot.Value = returnValue.Value;
                }

                snapshot.Timestamp = returnValue.TimeStamp;
            }
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
