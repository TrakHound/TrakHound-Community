// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;

namespace TrakHound.Configurations.AutoGenerate
{
    public static class Cycles
    {
        public static void Add(DataTable table, List<DataItem> dataItems)
        {
            DeviceConfiguration.EditTable(table, "/Cycles/CycleEventName", "cycle_execution", null);
            DeviceConfiguration.EditTable(table, "/Cycles/StoppedEventValue", "Stopped", null);

            var item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "PROGRAM");
            if (item != null) DeviceConfiguration.EditTable(table, "/Cycles/CycleNameLink", item.Id, null);

            // Add Production Types
            DeviceConfiguration.EditTable(table, "/Cycles/ProductionTypes/Value||00", null, "id||00;name||Started;type||IN_PRODUCTION;");
            DeviceConfiguration.EditTable(table, "/Cycles/ProductionTypes/Value||01", null, "id||01;name||Paused;type||PAUSED;");
            DeviceConfiguration.EditTable(table, "/Cycles/ProductionTypes/Value||02", null, "id||02;name||Stopped;type||STOPPED;");

            // Add Override Values
            var ovrItems = dataItems.FindAll(x => x.Category == DataItemCategory.EVENT && x.Type == "PATH_FEEDRATE_OVERRIDE" && x.SubType != "JOG");
            if (ovrItems != null)
            {
                for (var x = 0; x < ovrItems.Count; x++)
                {
                    DeviceConfiguration.EditTable(table, "/Cycles/OverrideLinks/Value||" + x.ToString("00"), ovrItems[x].Id, "id||" + x.ToString("00"));
                }
            }
        }
    }
}
