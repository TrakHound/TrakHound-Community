// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;

namespace TrakHound.Configurations.AutoGenerate
{
    public static class Parts
    {
        public static void Add(DataTable table, List<DataItem> dataItems)
        {
            int e = 0;

            //DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00"), null, "id||" + e.ToString("00") + ";");

            //DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/EventName", "device_status", null);
            //DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/EventValue", "Idle", null);
            //DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/PreviousEventValue", "Active", null);
            //DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/ValueType", "static_increment", null);
            //DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/StaticIncrementValue", 1, null);

            DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00"), null, "id||" + e.ToString("00") + ";");

            DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/EventName", "cycle_execution", null);
            DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/EventValue", "Paused", null);
            DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/PreviousEventValue", "Started", null);
            DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/ValueType", "static_increment", null);
            DeviceConfiguration.EditTable(table, "/Parts/Event||" + e.ToString("00") + "/StaticIncrementValue", 1, null);
        }
    }
}
