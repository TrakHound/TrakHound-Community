// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

using TrakHound.Configurations;
using TrakHound.DataManagement;
using TrakHound.Tables;
using TrakHound.Shifts;
using TrakHound.Plugins.Database;

namespace TrakHound_Server.Plugins.OEE.Database
{
    public static class ShiftBased
    {

        public const string TABLENAME = Names.OEE_Shifts;
        public static string[] primaryKey = { "DATE", "SHIFT" };

        public static void CreateTable(DeviceConfiguration config)
        {
            var columns = new List<ColumnDefinition>();

            columns.Add(new ColumnDefinition("DATE", DataType.SmallText, true));

            columns.Add(new ColumnDefinition("SHIFT", DataType.SmallText, true));

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

        public static OEEData GetPrevious(DeviceConfiguration config, ShiftId shiftId)
        {
            var dt = Table.Get(DataManagement.Database.Configuration, Table.GetName(TABLENAME, config.DatabaseId), "WHERE date='" + shiftId.FormattedDate + "' AND shift='" + shiftId.Shift.ToString() + "'");
            if (dt != null && dt.Rows.Count > 0)
            {
                return OEEData.FromShiftDataRow(dt.Rows[0]);
            }

            return null;
        }

        public static void UpdateRows(DeviceConfiguration config, List<OEEData> datas)
        {
            var rowValues = new List<List<object>>();

            List<string> columns = new List<string>();
            columns.Add("date");
            columns.Add("shift");

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

                values.Add(data.ShiftId.FormattedDate);
                values.Add(data.ShiftId.Shift);

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

            //Row.Insert(DataManagement.Database.Configuration, Table.GetName(TABLENAME, config.DatabaseId), columns.ToArray(), rowValues, primaryKey, true);
        }

    }
}
