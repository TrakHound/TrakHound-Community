// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

using TH_Global.TrakHound.Configurations;
using TH_Database;
using TH_Global;
using TH_Global.Shifts;
using TH_Plugins.Database;

namespace TH_OEE.Database
{
    public static class CycleBased
    {

        public const string TABLENAME = TableNames.OEE_Cycles;
        public static string[] primaryKey = { "SHIFT_ID", "CYCLE_ID", "CYCLE_INSTANCE_ID" };

        public static void CreateTable(DeviceConfiguration config)
        { 
            var columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("SHIFT_ID", DataType.SmallText, true));
            columns.Add(new ColumnDefinition("CYCLE_ID", DataType.LargeText, true));
            columns.Add(new ColumnDefinition("CYCLE_INSTANCE_ID", DataType.LargeText, true));

            columns.Add(new ColumnDefinition("START", DataType.DateTime));
            columns.Add(new ColumnDefinition("END", DataType.DateTime));
            columns.Add(new ColumnDefinition("START_UTC", DataType.DateTime));
            columns.Add(new ColumnDefinition("END_UTC", DataType.DateTime));

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

            Table.Create(config.Databases_Server, Global.GetTableName(TABLENAME, config.DatabaseId), ColArray, primaryKey);
        }

        public static OEEData GetPrevious(DeviceConfiguration config, ShiftId shiftId, string cycleId, string cycleInstanceId)
        {
            var dt = Table.Get(config.Databases_Server, Global.GetTableName(TABLENAME, config.DatabaseId), "WHERE shift_id='" + shiftId + "' AND cycle_id='" + cycleId + "' AND cycle_instance_id='" + cycleInstanceId + "'");
            if (dt != null && dt.Rows.Count > 0)
            {
                return OEEData.FromCycleDataRow(dt.Rows[0]);
            }

            return null;
        }

        public static void UpdateRows(DeviceConfiguration config, List<OEEData> datas)
        {
            var rowValues = new List<List<object>>();

            List<string> columns = new List<string>();
            columns.Add("shift_id");
            columns.Add("cycle_id");
            columns.Add("cycle_instance_id");

            columns.Add("start");
            columns.Add("end");
            columns.Add("start_utc");
            columns.Add("end_utc");

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
                values.Add(data.CycleId);
                values.Add(data.CycleInstanceId);

                values.Add(data.StartTime);
                values.Add(data.EndTime);
                values.Add(data.StartTimeUTC);
                values.Add(data.EndTimeUTC);

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

            Row.Insert(config.Databases_Server, Global.GetTableName(TABLENAME, config.DatabaseId), columns.ToArray(), rowValues, primaryKey, true);
        }

    }
}
