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

namespace TH_GeneratedData.SnapshotData
{
    public partial class Plugin : IServerPlugin
    {

        public string Name { get { return "TH_SnapshotData"; } }

        public void Initialize(Configuration config)
        {
            var sdc = SnapshotDataConfiguration.Read(config.ConfigurationXML);
            if (sdc != null)
            {
                config.CustomClasses.Add(sdc);

                configuration = config;

                Database.CreateTable(config);
                Database.IntializeRows(config);
            }
        }

        public void GetSentData(EventData data)
        {
            if (data != null && data.Id != null && configuration != null)
            {
                var sdc = SnapshotDataConfiguration.Get(configuration);
                if (sdc != null)
                {
                    if (data.Id.ToLower() == "currentinstancedata")
                    {
                        var currentInstanceData = (CurrentInstanceData)data.Data02;

                        var info = new Snapshot.ProcessInfo();
                        info.CurrentData = currentInstanceData.CurrentData;
                        info.CurrentInstanceData = currentInstanceData.Data;
                        info.PreviousItems = previousSSI;

                        Snapshot.Process(configuration, info);

                        Database.UpdateRows(configuration, sdc.Snapshots);

                        // Send Table of SnapShotItems to other Plugins--------
                        SendSnapShotTable(sdc.Snapshots);
                        // ----------------------------------------------------

                        // Send List of SnapShotItems to other Plugins--------
                        SendSnapShotItems(sdc.Snapshots);
                        // ----------------------------------------------------
                    }
                }
            }
        }

        public event SendData_Handler SendData;

        public event Status_Handler StatusChanged;

        public event Status_Handler ErrorOccurred;

        public void Closing() { }

        public Type[] ConfigurationPageTypes
        {
            get
            {
                return new Type[] { typeof(ConfigurationPage.Page) };
            }
        }



        private Configuration configuration;

        private List<Snapshot> previousSSI;


        void SendSnapShotTable(List<Snapshot> snapshots)
        {
            var dt = new DataTable();
            dt.Columns.Add("Timestamp");
            dt.Columns.Add("Name");
            dt.Columns.Add("Value");
            dt.Columns.Add("Previous_Value");

            foreach (var snapshot in snapshots)
            {
                var row = dt.NewRow();
                row["Timestamp"] = snapshot.Timestamp;
                row["Name"] = snapshot.Name;
                row["Value"] = snapshot.Value;
                row["Previous_Value"] = snapshot.PreviousValue;

                dt.Rows.Add(row);
            }

            var data = new EventData();
            data.Id = "SnapShotTable";
            data.Data01 = configuration;
            data.Data02 = dt;
            if (SendData != null) SendData(data);
        }

        void SendSnapShotItems(List<Snapshot> snapshots)
        {
            var data = new EventData();
            data.Id = "SnapShotItems";
            data.Data01 = configuration;
            data.Data02 = snapshots;
            if (SendData != null) SendData(data);
        }

    }
}
