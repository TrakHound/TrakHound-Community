// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

using TrakHound.Configurations;
using TrakHound.Databases;
using TrakHound.Plugins.Database;

namespace TrakHound.Server.Plugins.Parts
{
    public static class Database
    {
        private static string[] primaryKey = { "SHIFT_ID", "ID" };

        public static void CreatePartsTable(DeviceConfiguration config)
        {
            var columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("SHIFT_ID", DataType.SmallText));
            columns.Add(new ColumnDefinition("ID", DataType.MediumText));
            columns.Add(new ColumnDefinition("TIMESTAMP", DataType.DateTime));
            columns.Add(new ColumnDefinition("COUNT", DataType.Long));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases_Server, GetTableName(config), ColArray, primaryKey);
        }

        public static void AddRows(DeviceConfiguration config, List<PartInfo> infos)
        {
            var columns = new List<string>();
            columns.Add("SHIFT_ID");
            columns.Add("ID");
            columns.Add("TIMESTAMP");
            columns.Add("COUNT");

            var tableValues = new List<List<object>>();

            foreach (var info in infos)
            {
                var rowValues = new List<object>();
                rowValues.Add(info.ShiftId);
                rowValues.Add(info.Id);
                rowValues.Add(info.Timestamp);
                rowValues.Add(info.Count);

                tableValues.Add(rowValues);
            }

            Row.Insert(config.Databases_Server, GetTableName(config), columns.ToArray(), tableValues, primaryKey, true);
        }

        private static string GetTableName(DeviceConfiguration config)
        {
            if (config.DatabaseId != null) return config.DatabaseId + "_" + TableNames.Parts;
            else return TableNames.Parts;
        }

    }
}
