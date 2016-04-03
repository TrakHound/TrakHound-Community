// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Plugins.Database;

namespace TH_GeneratedData.Database
{
    public static class Snapshots
    {
        static string[] snapshotsPrimaryKey = { "NAME" };

        public static void CreateTable(Configuration config)
        {
            List<ColumnDefinition> columns = new List<ColumnDefinition>()
            {
                new ColumnDefinition("TIMESTAMP", DataType.DateTime),
                new ColumnDefinition("NAME", DataType.LargeText),
                new ColumnDefinition("VALUE", DataType.LargeText),
                new ColumnDefinition("PREVIOUS_TIMESTAMP", DataType.DateTime),
                new ColumnDefinition("PREVIOUS_VALUE", DataType.LargeText)
            };

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases_Server, GetTableName(config), ColArray, snapshotsPrimaryKey);
        }

        public static void IntializeRows(Configuration config)
        {
            var gdc = GeneratedDataConfiguration.Get(config);
            if (gdc != null)
            {
                List<string> Columns = new List<string>();
                Columns.Add("Name");

                List<List<object>> rowValues = new List<List<object>>();

                foreach (var item in gdc.SnapshotsConfiguration.Items)
                {
                    List<object> values = new List<object>();

                    values.Add(item.name);

                    rowValues.Add(values);
                }

                Row.Insert(config.Databases_Server, GetTableName(config), Columns.ToArray(), rowValues, snapshotsPrimaryKey, true);
            }
        }

        public static void UpdateRows(Configuration config, List<SnapShotItem> items)
        {
            // Set Columns to Update (include Name so that it can Update the row instead of creating a new one)
            List<string> columns = new List<string>();
            columns.Add("NAME");
            columns.Add("TIMESTAMP");
            columns.Add("VALUE");
            columns.Add("PREVIOUS_TIMESTAMP");
            columns.Add("PREVIOUS_VALUE");

            List<List<object>> rowValues = new List<List<object>>();

            foreach (var item in items)
            {
                List<object> values = new List<object>();

                values.Add(item.name);
                values.Add(item.timestamp);
                values.Add(item.value);

                DateTime prev_timestamp = item.previous_timestamp;
                if (prev_timestamp > DateTime.MinValue) values.Add(prev_timestamp);
                else values.Add(null);

                values.Add(item.previous_value);

                // only add to query if different (no need sending more info than needed)
                if (item.value != item.previous_value) rowValues.Add(values);
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
