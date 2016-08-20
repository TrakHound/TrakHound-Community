// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

using TrakHound.Configurations;
using TrakHound.DataManagement;
using TrakHound.Tables;
using TrakHound.Plugins.Database;

namespace TrakHound_Server.Plugins.Status
{
    public static class Database
    {
        private static string[] primaryKey = { "INFO_TYPE", "ADDRESS" };

        public static void CreateTable(DeviceConfiguration config)
        {
            var columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("INFO_TYPE", DataType.Short));
            columns.Add(new ColumnDefinition("ADDRESS", DataType.MediumText));
            columns.Add(new ColumnDefinition("CATEGORY", DataType.MediumText));
            columns.Add(new ColumnDefinition("ID", DataType.MediumText));
            columns.Add(new ColumnDefinition("NAME", DataType.MediumText));
            columns.Add(new ColumnDefinition("TYPE", DataType.MediumText));
            columns.Add(new ColumnDefinition("SUB_TYPE", DataType.MediumText));
            columns.Add(new ColumnDefinition("TIMESTAMP", DataType.DateTime));
            columns.Add(new ColumnDefinition("VALUE1", DataType.LargeText));
            columns.Add(new ColumnDefinition("VALUE2", DataType.LargeText));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(DataManagement.Database.Configuration, GetTableName(config), ColArray, primaryKey);
            Table.Truncate(DataManagement.Database.Configuration, GetTableName(config));
        }

        public static void AddRows(DeviceConfiguration config, List<StatusInfo> infos)
        {
            var columns = new List<string>();
            columns.Add("INFO_TYPE");
            columns.Add("ADDRESS");
            columns.Add("CATEGORY");
            columns.Add("ID");
            columns.Add("NAME");
            columns.Add("TYPE");
            columns.Add("SUB_TYPE");

            var tableValues = new List<List<object>>();

            foreach (var info in infos)
            {
                var rowValues = new List<object>();
                rowValues.Add((int)info.InfoType);
                rowValues.Add(info.Address);
                rowValues.Add(info.Category.ToString());
                rowValues.Add(info.Id);
                rowValues.Add(info.Name);
                rowValues.Add(info.Type);
                rowValues.Add(info.SubType);

                tableValues.Add(rowValues);
            }

            if (tableValues.Count > 0)
            {
                string query = Row.CreateInsertQuery(GetTableName(config), columns.ToArray(), tableValues, true);
                TrakHound.Servers.ProcessServer.Server.DatabaseQueue.AddToQueue(query);
            }

            //Row.Insert(DataManagement.Database.Configuration, GetTableName(config), columns.ToArray(), tableValues, primaryKey, true);
        }

        public static void UpdateRows(DeviceConfiguration config, List<StatusInfo> infos)
        {
            var columns = new List<string>();
            columns.Add("INFO_TYPE");
            columns.Add("ADDRESS");
            columns.Add("CATEGORY");
            columns.Add("ID");
            columns.Add("NAME");
            columns.Add("TYPE");
            columns.Add("SUB_TYPE");

            columns.Add("TIMESTAMP");
            columns.Add("VALUE1");
            columns.Add("VALUE2");

            var tableValues = new List<List<object>>();

            foreach (var info in infos)
            {
                var rowValues = new List<object>();
                rowValues.Add((int)info.InfoType);
                rowValues.Add(info.Address);
                rowValues.Add(info.Category);
                rowValues.Add(info.Id);
                rowValues.Add(info.Name);
                rowValues.Add(info.Type);
                rowValues.Add(info.SubType);

                rowValues.Add(info.Timestamp);
                rowValues.Add(info.Value1);
                rowValues.Add(info.Value2);

                if (info.Value1 != info.PreviousValue1 || info.Value2 != info.PreviousValue2) tableValues.Add(rowValues);
            }

            if (tableValues.Count > 0)
            {
                string query = Row.CreateInsertQuery(GetTableName(config), columns.ToArray(), tableValues, true);
                TrakHound.Servers.ProcessServer.Server.DatabaseQueue.AddToQueue(query);
            }

            //Row.Insert(DataManagement.Database.Configuration, GetTableName(config), columns.ToArray(), tableValues, primaryKey, true);
        }

        private static string GetTableName(DeviceConfiguration config)
        {
            if (config.DatabaseId != null) return config.DatabaseId + "_" + Names.Status;
            else return Names.Status;
        }

    }
}
