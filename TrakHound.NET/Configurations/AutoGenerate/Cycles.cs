// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;
using TrakHound.Tools;

namespace TrakHound.Configurations.AutoGenerate
{
    public static class Cycles
    {
        public static void Add(DataTable dt, List<DataItem> probeItems)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/CycleEventName", "value", "cycle_execution");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/StoppedEventValue", "value", "Stopped");

            var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "PROGRAM");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/CycleNameLink", "value", item.Id);

            // Add Production Types
            DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/ProductionTypes/Value||00", "attributes", "id||00;name||Started;type||IN_PRODUCTION;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/ProductionTypes/Value||01", "attributes", "id||01;name||Paused;type||PAUSED;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/ProductionTypes/Value||02", "attributes", "id||02;name||Stopped;type||STOPPED;");

            // Add Override Values
            var ovrItems = probeItems.FindAll(x => x.Category == DataItemCategory.EVENT && x.Type == "PATH_FEEDRATE_OVERRIDE" && x.SubType != "JOG");
            if (ovrItems != null)
            {
                for (var x = 0; x < ovrItems.Count; x++)
                {
                    DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/OverrideLinks/Value||" + x.ToString("00"), "attributes", "id||" + x.ToString("00"));
                    DataTable_Functions.UpdateTableValue(dt, "address", "/Cycles/OverrideLinks/Value||" + x.ToString("00"), "value", ovrItems[x].Id);
                }
            }
        }
    }
}
