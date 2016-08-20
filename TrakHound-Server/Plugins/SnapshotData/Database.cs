// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TrakHound.Configurations;
using TrakHound.DataManagement;
using TrakHound.Tables;
using TrakHound.Plugins.Database;

namespace TrakHound_Server.Plugins.SnapshotData
{
    public static class Database
    {

        static string[] snapshotsPrimaryKey = { "NAME" };

        public static void CreateTable(DeviceConfiguration config)
        {
            var columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition("TIMESTAMP", DataType.DateTime),
                new ColumnDefinition("NAME", DataType.LargeText),
                new ColumnDefinition("LINK", DataType.LargeText),
                new ColumnDefinition("VALUE", DataType.LargeText),
                new ColumnDefinition("PREVIOUS_TIMESTAMP", DataType.DateTime),
                new ColumnDefinition("PREVIOUS_VALUE", DataType.LargeText)
            };

            Table.Create(DataManagement.Database.Configuration, GetTableName(config), columns.ToArray(), snapshotsPrimaryKey);
            Table.Truncate(DataManagement.Database.Configuration, GetTableName(config));
        }

        public static void IntializeRows(DeviceConfiguration config)
        {
            var sdc = SnapshotDataConfiguration.Get(config);
            if (sdc != null)
            {
                var columns = new List<string>();
                columns.Add("NAME");
                columns.Add("LINK");

                var rowValues = new List<List<object>>();

                foreach (var snapshot in sdc.Snapshots)
                {
                    List<object> values = new List<object>();

                    values.Add(snapshot.Name);
                    values.Add(snapshot.Link);

                    rowValues.Add(values);
                }

                if (rowValues.Count > 0)
                {
                    string query = Row.CreateInsertQuery(GetTableName(config), columns.ToArray(), rowValues, true);
                    TrakHound.Servers.ProcessServer.Server.DatabaseQueue.AddToQueue(query);
                }

                //Row.Insert(DataManagement.Database.Configuration, GetTableName(config), columns.ToArray(), rowValues, snapshotsPrimaryKey, true);
            }
        }

        public static void UpdateRows(DeviceConfiguration config, List<Snapshot> snapshots)
        {
            // Set Columns to Update (include Name so that it can Update the row instead of creating a new one)
            var columns = new List<string>();
            columns.Add("TIMESTAMP");
            columns.Add("NAME");
            columns.Add("LINK");
            columns.Add("VALUE");
            columns.Add("PREVIOUS_TIMESTAMP");
            columns.Add("PREVIOUS_VALUE");

            var rowValues = new List<List<object>>();

            foreach (var snapshot in snapshots)
            {
                var values = new List<object>();

                values.Add(snapshot.Timestamp);
                values.Add(snapshot.Name);
                values.Add(snapshot.Link);
                values.Add(snapshot.Value);

                DateTime prev_timestamp = snapshot.PreviousTimestamp;
                if (prev_timestamp > DateTime.MinValue) values.Add(prev_timestamp);
                else values.Add(null);

                values.Add(snapshot.PreviousValue);

                // only add to query if different (no need sending more info than needed)
                if (snapshot.Value != snapshot.LastReadValue)
                {
                    rowValues.Add(values);
                    snapshot.LastReadValue = snapshot.Value;
                }
            }

            if (rowValues.Count > 0)
            {
                string query = Row.CreateInsertQuery(GetTableName(config), columns.ToArray(), rowValues, true);
                TrakHound.Servers.ProcessServer.Server.DatabaseQueue.AddToQueue(query);
            }

            //Row.Insert(DataManagement.Database.Configuration, GetTableName(config), columns.ToArray(), rowValues, snapshotsPrimaryKey, true);
        }

        private static string GetTableName(DeviceConfiguration config)
        {
            if (config.DatabaseId != null) return config.DatabaseId + "_" + Names.SnapShots;
            else return Names.SnapShots;
        }

    }
}
