// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using MTConnect;
using MTConnectDevices = MTConnect.MTConnectDevices;
using System.Collections.Generic;
using System.Data;

namespace TrakHound.Configurations.AutoGenerate.Manufacturers
{
    public static class Okuma
    {
        public static void Process(DataTable table, MTConnectDevices.Device device)
        {
            if (device != null && device.Description != null && !string.IsNullOrEmpty(device.Description.Manufacturer))
            {
                if (device.Description.Manufacturer.ToLower() == "okuma")
                {
                    SetDescription(table);

                    var items = device.GetDataItems();

                    AddTimers(table, items);
                }
            }
        }

        private static void SetDescription(DataTable table)
        {
            // Read IP Address
            string ip = DeviceConfiguration.GetTableValue(table, "/Agent/Address");
            if (!string.IsNullOrEmpty(ip))
            {
                DeviceConfiguration.EditTable(table, "/Description/Description", ip, null);
            }
            else
            {
                DeviceConfiguration.EditTable(table, "/Description/Description", string.Empty, null);
            }
        }

        private static void AddTimers(DataTable table, List<MTConnectDevices.DataItem> dataItems)
        {
            // Run Time
            var item = dataItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:RUNNING_TIME");
            if (item != null) DeviceConfiguration.EditTable(table, "/GeneratedData/SnapShotData/Collected||90", null, "id||90;name||Day Run Time;link||" + item.Id + ";");

            item = dataItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:TOTAL_RUNNING_TIME");
            if (item != null) DeviceConfiguration.EditTable(table, "/GeneratedData/SnapShotData/Collected||91", null, "id||91;name||Total Run Time;link||" + item.Id + ";");

            // Operating Time
            item = dataItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:OPERATING_TIME");
            if (item != null) DeviceConfiguration.EditTable(table, "/GeneratedData/SnapShotData/Collected||92", null, "id||92;name||Day Operating Time;link||" + item.Id + ";");

            item = dataItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:TOTAL_OPERATING_TIME");
            if (item != null) DeviceConfiguration.EditTable(table, "/GeneratedData/SnapShotData/Collected||93", null, "id||93;name||Total Operating Time;link||" + item.Id + ";");

            // Cutting Time
            item = dataItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:CUTTING_TIME");
            if (item != null) DeviceConfiguration.EditTable(table, "/GeneratedData/SnapShotData/Collected||94", null, "id||94;name||Day Cutting Time;link||" + item.Id + ";");

            item = dataItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:TOTAL_CUTTING_TIME");
            if (item != null) DeviceConfiguration.EditTable(table, "/GeneratedData/SnapShotData/Collected||95", null, "id||95;name||Total Cutting Time;link||" + item.Id + ";");

            // Spindle Run Time
            item = dataItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:SPINDLE_RUN_TIME");
            if (item != null) DeviceConfiguration.EditTable(table, "/GeneratedData/SnapShotData/Collected||96", null, "id||96;name||Day Spindle Time;link||" + item.Id + ";");

            item = dataItems.Find(x => x.Category == DataItemCategory.SAMPLE && x.Type == "ACCUMULATED_TIME" && x.SubType == "x:TOTAL_SPINDLE_RUN_TIME");
            if (item != null) DeviceConfiguration.EditTable(table, "/GeneratedData/SnapShotData/Collected||97", null, "id||97;name||Total Spindle Time;link||" + item.Id + ";");
        }
    }
}

