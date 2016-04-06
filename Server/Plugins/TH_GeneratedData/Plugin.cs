// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using TH_Configuration;
using TH_InstanceTable;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_GeneratedData
{
    public partial class GeneratedData : IServerPlugin
    {

        public string Name { get { return "TH_GeneratedData"; } }

        public void Initialize(Configuration config)
        {
            var gdc = GeneratedDataConfiguration.Read(config.ConfigurationXML);
            if (gdc != null)
            {
                config.CustomClasses.Add(gdc);

                configuration = config;

                // Snapshot 
                if (gdc.SnapshotsConfiguration.UploadToMySQL)
                {
                    Database.Snapshots.CreateTable(config);
                    Database.Snapshots.IntializeRows(config);
                }

                // Generated Events
                if (gdc.GeneratedEventsConfiguration.UploadToMySQL)
                {
                    Database.GeneratedEvents.CreateValueTable(config, gdc.GeneratedEventsConfiguration.Events);

                    foreach (var e in gdc.GeneratedEventsConfiguration.Events) Database.GeneratedEvents.CreateEventTable(config, e);
                }
            }
        }

        public void GetSentData(EventData data)
        {
            if (data != null && data.Id != null && configuration != null)
            {
                var gdc = GeneratedDataConfiguration.Get(configuration);
                if (gdc != null)
                {
                    switch (data.Id.ToLower())
                    {
                        // InstanceTable data after Sample received
                        case "instancedata":

                            var instanceDatas = (List<InstanceData>)data.Data02;

                            List<GeneratedEventItem> geis = GeneratedEventItem.Process(configuration, instanceDatas);

                            // Send List of GeneratedEventItems to other Plugins--------
                            SendGeneratedEventItems(geis);
                            // ----------------------------------------------------

                            if (gdc.GeneratedEventsConfiguration.UploadToMySQL) Database.GeneratedEvents.InsertEventItems(configuration, geis);

                            break;

                        // InstanceData object after current received
                        case "currentinstancedata":

                            var currentInstanceData = (CurrentInstanceData)data.Data02;

                            var info = new SnapShotItem.ProcessInfo();
                            info.CurrentData = currentInstanceData.CurrentData;
                            info.CurrentInstanceData = currentInstanceData.Data;
                            info.PreviousItems = previousSSI;

                            List<SnapShotItem> snapShots = SnapShotItem.Process(configuration, info);

                            if (gdc.SnapshotsConfiguration.UploadToMySQL) Database.Snapshots.UpdateRows(configuration, snapShots);

                            previousSSI = snapShots;

                            // Send Table of SnapShotItems to other Plugins--------
                            SendSnapShotTable(snapShots);
                            // ----------------------------------------------------

                            // Send List of SnapShotItems to other Plugins--------
                            SendSnapShotItems(snapShots);
                            // ----------------------------------------------------

                            break;
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing() { }

        public Type[] ConfigurationPageTypes { get { return new Type[] { typeof(ConfigurationPage.Page) }; } }

        //public bool UseDatabases { get; set; }

        //public string TablePrefix { get; set; }



        private Configuration configuration;

        private List<SnapShotItem> previousSSI;


        void SendGeneratedEventItems(List<GeneratedEventItem> items)
        {
            var data = new EventData();
            data.Id = "GeneratedEventItems";
            data.Data01 = configuration;
            data.Data02 = items.ToList();
            if (SendData != null) SendData(data);
        }

        void SendSnapShotTable(List<SnapShotItem> items)
        {
            var dt = new DataTable();
            dt.Columns.Add("Timestamp");
            dt.Columns.Add("Name");
            dt.Columns.Add("Value");
            dt.Columns.Add("Previous_Value");

            foreach (SnapShotItem item in items)
            {
                var row = dt.NewRow();
                row["Timestamp"] = item.timestamp;
                row["Name"] = item.name;
                row["Value"] = item.value;
                row["Previous_Value"] = item.previous_value;

                dt.Rows.Add(row);
            }

            var data = new EventData();
            data.Id = "SnapShotTable";
            data.Data01 = configuration;
            data.Data02 = dt;
            if (SendData != null) SendData(data);
        }

        void SendSnapShotItems(List<SnapShotItem> items)
        {
            var data = new EventData();
            data.Id = "SnapShotItems";
            data.Data01 = configuration;
            data.Data02 = items;
            if (SendData != null) SendData(data);
        }

    }
}
