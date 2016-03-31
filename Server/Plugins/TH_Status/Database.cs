// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Plugins.Database;

namespace TH_Status
{
    public static class Database
    {
        private static string[] primaryKey = { "INFO_TYPE", "ADDRESS" };

        public static void CreateTable(Configuration config)
        {
            var columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("INFO_TYPE", DataType.Short));
            columns.Add(new ColumnDefinition("ADDRESS", DataType.MediumText));
            columns.Add(new ColumnDefinition("ID", DataType.MediumText));
            columns.Add(new ColumnDefinition("NAME", DataType.MediumText));
            columns.Add(new ColumnDefinition("TYPE", DataType.MediumText));
            columns.Add(new ColumnDefinition("SUB_TYPE", DataType.MediumText));
            columns.Add(new ColumnDefinition("TIMESTAMP", DataType.DateTime));
            columns.Add(new ColumnDefinition("VALUE", DataType.LargeText));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(config.Databases_Server, GetTableName(config), ColArray, primaryKey);
        }

        public static void AddRows(Configuration config, List<StatusInfo> infos)
        {
            var columns = new List<string>();
            columns.Add("INFO_TYPE");
            columns.Add("ADDRESS");
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
                rowValues.Add(info.Id);
                rowValues.Add(info.Name);
                rowValues.Add(info.Type);
                rowValues.Add(info.SubType);

                tableValues.Add(rowValues);
            }

            Row.Insert(config.Databases_Server, GetTableName(config), columns.ToArray(), tableValues, primaryKey, true);
        }

        public static void UpdateRows(Configuration config, List<StatusInfo> infos)
        {
            var columns = new List<string>();
            columns.Add("INFO_TYPE");
            columns.Add("ADDRESS");
            columns.Add("ID");
            columns.Add("NAME");
            columns.Add("TYPE");
            columns.Add("SUB_TYPE");

            columns.Add("TIMESTAMP");
            columns.Add("VALUE");

            var tableValues = new List<List<object>>();

            foreach (var info in infos)
            {
                var rowValues = new List<object>();
                rowValues.Add((int)info.InfoType);
                rowValues.Add(info.Address);
                rowValues.Add(info.Id);
                rowValues.Add(info.Name);
                rowValues.Add(info.Type);
                rowValues.Add(info.SubType);

                rowValues.Add(info.Timestamp);
                rowValues.Add(info.Value);

                tableValues.Add(rowValues);
            }

            Row.Insert(config.Databases_Server, GetTableName(config), columns.ToArray(), tableValues, primaryKey, true);
        }

        private static string GetTableName(Configuration config)
        {
            if (config.DatabaseId != null) return config.DatabaseId + "_" + TableNames.Status;
            else return TableNames.Status;
        }

    }
}
