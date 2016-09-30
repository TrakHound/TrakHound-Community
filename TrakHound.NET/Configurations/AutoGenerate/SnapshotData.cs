// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;
using TrakHound.Tools;

namespace TrakHound.Configurations.AutoGenerate
{
    public static class SnapshotData
    {
        public static void Add(DataTable dt, List<DataItem> probeItems)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/GeneratedData/SnapShotData/Generated||00", "attributes", "id||00;name||Device Status;link||device_status;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/GeneratedData/SnapShotData/Generated||01", "attributes", "id||01;name||Production Status;link||production_status;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/GeneratedData/SnapShotData/Generated||02", "attributes", "id||02;name||Program Execution;link||program_execution;");

            int id = 3;

            // Feedrate Overrides
            var items = probeItems.FindAll(x => x.Category == DataItemCategory.EVENT && x.Type == "PATH_FEEDRATE_OVERRIDE" && x.SubType == "PROGRAMMED" && x.FullAddress.ToLower().Contains("controller"));
            if (items != null)
            {
                int i = 1;

                foreach (var item in items)
                {
                    DataTable_Functions.UpdateTableValue(dt, "address", "/GeneratedData/SnapShotData/Collected||" + id.ToString("00"), "attributes", "id||" + id.ToString("00") + ";name||Feedrate Override " + i.ToString("00") + ";link||" + item.Id + ";");
                    i++;
                    id++;
                }
            }

            // Rapidrate Overrides
            items = probeItems.FindAll(x => x.Category == DataItemCategory.EVENT && x.Type == "PATH_FEEDRATE_OVERRIDE" && x.SubType == "RAPID" && x.FullAddress.ToLower().Contains("controller"));
            if (items != null)
            {
                int i = 1;

                foreach (var item in items)
                {
                    DataTable_Functions.UpdateTableValue(dt, "address", "/GeneratedData/SnapShotData/Collected||" + id.ToString("00"), "attributes", "id||" + id.ToString("00") + ";name||Rapid Override " + i.ToString("00") + ";link||" + item.Id + ";");
                    i++;
                    id++;
                }
            }

            // Spindle Overrides
            items = probeItems.FindAll(x => x.Category == DataItemCategory.EVENT && x.Type == "ROTARY_VELOCITY_OVERRIDE" && x.FullAddress.ToLower().Contains("rotary"));
            if (items != null)
            {
                int i = 1;

                foreach (var item in items)
                {
                    DataTable_Functions.UpdateTableValue(dt, "address", "/GeneratedData/SnapShotData/Collected||" + id.ToString("00"), "attributes", "id||" + id.ToString("00") + ";name||Spindle Override " + i.ToString("00") + ";link||" + item.Id + ";");
                    i++;
                    id++;
                }
            }
        }


    }
}
