// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TrakHound.Configurations.AutoGenerate
{
    public static class Parts
    {
        public static void Add(DataTable table, List<DataItem> dataItems)
        {
            //int i = 1;
            int e = 0;

            DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00"), null, "id||" + e.ToString("00") + ";");

            DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/EventName", "device_status", null);
            DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/EventValue", "Idle", null);
            DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/PreviousEventValue", "Active", null);
            DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/ValueType", "static_increment", null);
            DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/StaticIncrementValue", 1, null);



            //int i = 1;
            //int e = 0;

            //var partCountItems = dataItems.FindAll(x => x.Category == DataItemCategory.EVENT && x.Type == "PART_COUNT");
            //if (partCountItems.Any())
            //{
            //    foreach (var partCountItem in partCountItems)
            //    {
            //        DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00"), null, "id||" + e.ToString("00") + ";");

            //        DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/EventName", "parts_count_" + i.ToString(), null);
            //        DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/EventValue", "Parts Produced", null);
            //        DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/CaptureItemLink", "part_count", null);
            //        DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/CalculationType", "Total", null);

            //        i++;
            //        e++;
            //    }
            //}
            //else
            //{
            //    DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00"), null, "id||" + e.ToString("00") + ";");

            //    DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/EventName", "device_status", null);
            //    DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/EventValue", "Idle", null);
            //    DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/PreviousEventValue", "Active", null);
            //    DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/ValueType", "static_increment", null);
            //    DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/StaticIncrementValue", 1, null);
            //}
        }
    }
}
