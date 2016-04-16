// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Plugins.Database;

namespace TH_GeneratedData.SnapshotData
{
    public static class Database
    {

        static string[] snapshotsPrimaryKey = { "NAME" };

        public static void CreateTable(Configuration config)
        {
            var columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition("TIMESTAMP", DataType.DateTime),
                new ColumnDefinition("NAME", DataType.LargeText),
                new ColumnDefinition("VALUE", DataType.LargeText),
                new ColumnDefinition("PREVIOUS_TIMESTAMP", DataType.DateTime),
                new ColumnDefinition("PREVIOUS_VALUE", DataType.LargeText)
            };

            Table.Replace(config.Databases_Server, GetTableName(config), columns.ToArray(), snapshotsPrimaryKey);
        }

        public static void IntializeRows(Configuration config)
        {
            var sdc = SnapshotDataConfiguration.Get(config);
            if (sdc != null)
            {
                var columns = new List<string>();
                columns.Add("Name");

                var rowValues = new List<List<object>>();

                foreach (var snapshot in sdc.Snapshots)
                {
                    List<object> values = new List<object>();

                    values.Add(snapshot.Name);

                    rowValues.Add(values);
                }

                Row.Insert(config.Databases_Server, GetTableName(config), columns.ToArray(), rowValues, snapshotsPrimaryKey, true);
            }
        }

        public static void UpdateRows(Configuration config, List<Snapshot> snapshots)
        {
            // Set Columns to Update (include Name so that it can Update the row instead of creating a new one)
            var columns = new List<string>();
            columns.Add("NAME");
            columns.Add("TIMESTAMP");
            columns.Add("VALUE");
            columns.Add("PREVIOUS_TIMESTAMP");
            columns.Add("PREVIOUS_VALUE");

            var rowValues = new List<List<object>>();

            foreach (var snapshot in snapshots)
            {
                var values = new List<object>();

                values.Add(snapshot.Name);
                values.Add(snapshot.Timestamp);
                values.Add(snapshot.Value);

                DateTime prev_timestamp = snapshot.PreviousTimestamp;
                if (prev_timestamp > DateTime.MinValue) values.Add(prev_timestamp);
                else values.Add(null);

                values.Add(snapshot.PreviousValue);

                // only add to query if different (no need sending more info than needed)
                if (snapshot.Value != snapshot.PreviousValue) rowValues.Add(values);
            }

            Row.Insert(config.Databases_Server, GetTableName(config), columns.ToArray(), rowValues, snapshotsPrimaryKey, true);
        }

        private static string GetTableName(Configuration config)
        {
            if (config.DatabaseId != null) return config.DatabaseId + "_" + TableNames.SnapShots;
            else return TableNames.SnapShots;
        }

    }
}
