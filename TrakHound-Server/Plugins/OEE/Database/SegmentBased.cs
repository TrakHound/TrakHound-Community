// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;

using TrakHound.Configurations;
using TrakHound.DataManagement;
using TrakHound.Tables;
using TrakHound.Shifts;
using TrakHound.Plugins.Database;

namespace TrakHound_Server.Plugins.OEE.Database
{
    public static class SegmentBased
    {

        public const string TABLENAME = Names.OEE_Segments;
        public static string[] primaryKey = { "SHIFT_ID", "START", "END" };

        public static void CreateTable(DeviceConfiguration config)
        {
            var columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("SHIFT_ID", DataType.SmallText, true));
            columns.Add(new ColumnDefinition("START", DataType.DateTime));
            columns.Add(new ColumnDefinition("END", DataType.DateTime));

            columns.Add(new ColumnDefinition("OEE", DataType.Double));
            columns.Add(new ColumnDefinition("AVAILABILITY", DataType.Double));
            columns.Add(new ColumnDefinition("PERFORMANCE", DataType.Double));
            columns.Add(new ColumnDefinition("QUALITY", DataType.Double));

            columns.Add(new ColumnDefinition("OPERATING_TIME", DataType.Double));
            columns.Add(new ColumnDefinition("PLANNED_PRODUCTION_TIME", DataType.Double));
            columns.Add(new ColumnDefinition("IDEAL_OPERATING_TIME", DataType.Double));
            columns.Add(new ColumnDefinition("IDEAL_CYCLE_TIME", DataType.Long));
            columns.Add(new ColumnDefinition("TOTAL_PIECES", DataType.Long));
            columns.Add(new ColumnDefinition("GOOD_PIECES", DataType.Long));

            ColumnDefinition[] ColArray = columns.ToArray();

            Table.Create(DataManagement.Database.Configuration, Table.GetName(TABLENAME, config.DatabaseId), ColArray, primaryKey);
        }

        public static List<OEEData> GetPrevious(DeviceConfiguration config)
        {
            var d = DateTime.UtcNow;
            var start = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
            var end = start.AddDays(1);

            string filter = "WHERE start >= '" + start.ToString("yyyy-MM-dd HH:mm:ss") + "' AND end < '" + end.ToString("yyyy-MM-dd HH:mm:ss") + "'";

            var result = new List<OEEData>();

            var dt = Table.Get(DataManagement.Database.Configuration, Table.GetName(TABLENAME, config.DatabaseId), filter);
            //var dt = Table.Get(DataManagement.Database.Configuration, Table.GetName(TABLENAME, config.DatabaseId), "WHERE shift_id='" + shiftId + "'");
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows) result.Add(OEEData.FromSegmentDataRow(row));
            }

            return result;
        }

        public static void UpdateRows(DeviceConfiguration config, List<OEEData> datas)
        {
            var rowValues = new List<List<object>>();

            List<string> columns = new List<string>();
            columns.Add("shift_id");
            columns.Add("start");
            columns.Add("end");

            columns.Add("oee");
            columns.Add("availability");
            columns.Add("performance");
            columns.Add("quality");

            columns.Add("operating_time");
            columns.Add("planned_production_time");
            columns.Add("ideal_operating_time");

            columns.Add("ideal_cycle_time");
            columns.Add("total_pieces");

            columns.Add("good_pieces");

            foreach (var data in datas)
            {
                var values = new List<object>();

                values.Add(data.ShiftId.Id);
                values.Add(data.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
                values.Add(data.EndTime.ToString("yyyy-MM-dd HH:mm:ss"));

                values.Add(data.Oee);
                values.Add(data.Availability);
                values.Add(data.Performance);
                values.Add(data.Quality);

                values.Add(data.OperatingTime);
                values.Add(data.PlannedProductionTime);
                values.Add(data.IdealOperatingTime);

                values.Add(data.IdealCycleTime);

                values.Add(data.TotalPieces);
                values.Add(data.GoodPieces);

                rowValues.Add(values);
            }

            string query = Row.CreateInsertQuery(Table.GetName(TABLENAME, config.DatabaseId), columns.ToArray(), rowValues, true);
            TrakHound.Servers.ProcessServer.Server.DatabaseQueue.AddToQueue(query);
        }

    }
}
