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

namespace TH_GeneratedData
{
    public class SnapShotItem
    {
        public DateTime timestamp { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public DateTime previous_timestamp { get; set; }
        public string previous_value { get; set; }

        public SnapShotItem Copy()
        {
            SnapShotItem Result = new SnapShotItem();
            Result.timestamp = timestamp;
            Result.name = name;
            Result.value = value;
            Result.previous_timestamp = previous_timestamp;
            Result.previous_value = previous_value;
            return Result;
        }


        public class ProcessInfo
        {
            public List<SnapShotItem> PreviousItems { get; set; }
            public TH_MTConnect.Streams.ReturnData CurrentData { get; set; }
            public InstanceData CurrentInstanceData { get; set; }
        }

        public static List<SnapShotItem> Process(Configuration config, ProcessInfo info)
        {
            var Result = new List<SnapShotItem>();

            if (info.CurrentInstanceData != null)
            {
                var gdc = GeneratedDataConfiguration.Get(config);
                if (gdc != null)
                {
                    DataTable variables_DT = null;

                    // Get Variables Table from MySQL (if any snapshots are set to "Variable")
                    if (gdc.SnapshotsConfiguration.Items.FindAll(x => x.type.ToLower() == "variable").Count > 0)
                    {
                        string tablename;
                        if (config.DatabaseId != null) tablename = config.DatabaseId + "_" + TableNames.Variables;
                        else tablename = TableNames.Variables;

                        variables_DT = Table.Get(config.Databases_Server, tablename);
                    }

                    foreach (var item in gdc.SnapshotsConfiguration.Items)
                    {
                        // Get ssi in previousSSI list
                        SnapShotItem ssi = null;
                        if (info.PreviousItems != null) ssi = info.PreviousItems.Find(x => x.name == item.name);
                        if (ssi == null)
                        {
                            ssi = new SnapShotItem();
                            ssi.name = item.name;
                        }

                        switch (item.type.ToLower())
                        {
                            case "collected":

                                ProcessCollected(item, ssi, info.CurrentData);
                                Result.Add(ssi);

                                break;

                            case "generated":

                                ProcessGenerated(item, ssi, gdc, info.CurrentInstanceData);
                                Result.Add(ssi);

                                break;

                            case "variable":

                                ProcessVariables(item, ssi, variables_DT);
                                Result.Add(ssi);

                                break;
                        }
                    }
                }
            }

            return Result;
        }

        private static bool ProcessCollected(SnapshotsConfiguration.Item item, SnapShotItem ssi, TH_MTConnect.Streams.ReturnData currentData)
        {
            bool result = false;

            if (currentData.DeviceStreams != null)
            {
                TH_MTConnect.Streams.DataItemCollection dataItems = TH_MTConnect.Streams.Tools.GetDataItemsFromDeviceStream(currentData.DeviceStreams[0]);

                bool found = false;

                // Seach Conditions
                found = ProcessCollectedCondtion(item, ssi, dataItems);

                // Search Events
                if (!found) found = ProcessCollectedEvent(item, ssi, dataItems);

                // Search Samples
                if (!found) found = ProcessCollectedSample(item, ssi, dataItems);

                result = found;
            }

            return result;
        }

        private static bool ProcessCollectedCondtion(SnapshotsConfiguration.Item item, SnapShotItem ssi, TH_MTConnect.Streams.DataItemCollection dataItems)
        {
            bool result = false;

            TH_MTConnect.Streams.Condition dataItem = dataItems.Conditions.Find(x => x.DataItemId.ToLower() == item.link.ToLower());
            if (dataItem != null)
            {
                if (ssi.value != dataItem.CDATA)
                {
                    ssi.previous_timestamp = ssi.timestamp;
                    ssi.previous_value = ssi.value;

                    ssi.value = dataItem.CDATA;
                    ssi.timestamp = dataItem.Timestamp;

                    result = true;
                }
            }

            return result;
        }

        private static bool ProcessCollectedEvent(SnapshotsConfiguration.Item item, SnapShotItem ssi, TH_MTConnect.Streams.DataItemCollection dataItems)
        {
            bool result = false;

            TH_MTConnect.Streams.Event dataItem = dataItems.Events.Find(x => x.DataItemId.ToLower() == item.link.ToLower());
            if (dataItem != null)
            {
                if (ssi.value != dataItem.CDATA)
                {
                    ssi.previous_timestamp = ssi.timestamp;
                    ssi.previous_value = ssi.value;

                    ssi.value = dataItem.CDATA;
                    ssi.timestamp = dataItem.Timestamp;

                    result = true;
                }
            }

            return result;
        }

        private static bool ProcessCollectedSample(SnapshotsConfiguration.Item item, SnapShotItem ssi, TH_MTConnect.Streams.DataItemCollection dataItems)
        {
            bool result = false;

            TH_MTConnect.Streams.Sample dataItem = dataItems.Samples.Find(x => x.DataItemId.ToLower() == item.link.ToLower());
            if (dataItem != null)
            {
                if (ssi.value != dataItem.CDATA)
                {
                    ssi.previous_timestamp = ssi.timestamp;
                    ssi.previous_value = ssi.value;

                    ssi.value = dataItem.CDATA;
                    ssi.timestamp = dataItem.Timestamp;

                    result = true;
                }
            }

            return result;
        }


        private static void ProcessGenerated(SnapshotsConfiguration.Item item, SnapShotItem ssi, GeneratedDataConfiguration gdc, InstanceData currentInstanceData)
        {
            var e = gdc.GeneratedEventsConfiguration.Events.Find(x => x.Name.ToLower() == item.link.ToLower());
            if (e != null)
            {
                Return returnValue = e.Process(currentInstanceData);

                if (ssi.value != returnValue.Value)
                {
                    ssi.previous_timestamp = ssi.timestamp;
                    ssi.previous_value = ssi.value;

                    ssi.value = returnValue.Value;
                    ssi.timestamp = returnValue.TimeStamp;
                }
            }
        }


        static bool ProcessVariables(SnapshotsConfiguration.Item item, SnapShotItem ssi, DataTable dt)
        {
            bool result = false;

            if (dt != null)
            {
                DataRow[] rows = dt.Select("variable = '" + item.link + "'");
                if (rows != null)
                {
                    if (rows.Length > 0)
                    {
                        var val = rows[0]["value"].ToString();

                        if (ssi.value != val)
                        {
                            ssi.previous_timestamp = ssi.timestamp;
                            ssi.previous_value = ssi.value;

                            ssi.value = val;

                            DateTime timestamp = DateTime.MinValue;
                            DateTime.TryParse(rows[0]["timestamp"].ToString(), out timestamp);
                            if (timestamp > DateTime.MinValue) ssi.timestamp = timestamp;

                            result = true;
                        }
                    }
                }
            }

            return result;
        }
    }
}
