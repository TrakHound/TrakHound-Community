// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Components;
using System.Data;
using System.Collections.Generic;
using TrakHound.Tools;

namespace TrakHound.Configurations.AutoGenerate.Manufacturers
{
    public static class Okuma
    {

        public static void Process(DataTable table, Device probeDevice)
        {
            if (probeDevice != null && probeDevice.Description != null && !string.IsNullOrEmpty(probeDevice.Description.Manufacturer))
            {
                if (probeDevice.Description.Manufacturer.ToLower() == "okuma")
                {
                    SetDescription(table);

                    var items = probeDevice.GetAllDataItems();

                    AddTimers(table, items);
                }
            }
        }


        private static void SetDescription(DataTable table)
        {
            // Read IP Address
            string ip = DataTable_Functions.GetTableValue(table, "address", "/Agent/Address", "value");
            if (!string.IsNullOrEmpty(ip))
            {
                DataTable_Functions.UpdateTableValue(table, "address", "/Description/Description", "value", ip);
            }
            else
            {
                DataTable_Functions.UpdateTableValue(table, "address", "/Description/Description", "value", "");
            }
        }

        //private static void RemoveDescription(DataTable table)
        //{
        //    DataTable_Functions.UpdateTableValue(table, "address", "/Description/Description", "value", "");
        //}

        private static void AddTimers(DataTable table, List<DataItem> probeItems)
        {
            // Run Time
            var item = probeItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:RUNNING_TIME");
            if (item != null) DataTable_Functions.UpdateTableValue(table, "address", "/GeneratedData/SnapShotData/Collected||90", "attributes", "id||90;name||Day Run Time;link||" + item.Id + ";");

            item = probeItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:TOTAL_RUNNING_TIME");
            if (item != null) DataTable_Functions.UpdateTableValue(table, "address", "/GeneratedData/SnapShotData/Collected||91", "attributes", "id||91;name||Total Run Time;link||" + item.Id + ";");

            // Operating Time
            item = probeItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:OPERATING_TIME");
            if (item != null) DataTable_Functions.UpdateTableValue(table, "address", "/GeneratedData/SnapShotData/Collected||92", "attributes", "id||92;name||Day Operating Time;link||" + item.Id + ";");

            item = probeItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:TOTAL_OPERATING_TIME");
            if (item != null) DataTable_Functions.UpdateTableValue(table, "address", "/GeneratedData/SnapShotData/Collected||93", "attributes", "id||93;name||Total Operating Time;link||" + item.Id + ";");

            // Cutting Time
            item = probeItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:CUTTING_TIME");
            if (item != null) DataTable_Functions.UpdateTableValue(table, "address", "/GeneratedData/SnapShotData/Collected||94", "attributes", "id||94;name||Day Cutting Time;link||" + item.Id + ";");

            item = probeItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:TOTAL_CUTTING_TIME");
            if (item != null) DataTable_Functions.UpdateTableValue(table, "address", "/GeneratedData/SnapShotData/Collected||95", "attributes", "id||95;name||Total Cutting Time;link||" + item.Id + ";");

            // Spindle Run Time
            item = probeItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:SPINDLE_RUN_TIME");
            if (item != null) DataTable_Functions.UpdateTableValue(table, "address", "/GeneratedData/SnapShotData/Collected||96", "attributes", "id||96;name||Day Spindle Time;link||" + item.Id + ";");

            item = probeItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:TOTAL_SPINDLE_RUN_TIME");
            if (item != null) DataTable_Functions.UpdateTableValue(table, "address", "/GeneratedData/SnapShotData/Collected||97", "attributes", "id||97;name||Total Spindle Time;link||" + item.Id + ";");
        }

    }
}
