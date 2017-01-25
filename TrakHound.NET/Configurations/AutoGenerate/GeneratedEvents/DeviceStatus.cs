// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using MTConnect;
using MTConnectDevices = MTConnect.MTConnectDevices;
using System.Collections.Generic;
using System.Data;

namespace TrakHound.Configurations.AutoGenerate.GeneratedEvents
{
    public static class DeviceStatus
    {
        public static void Add(DataTable table, List<MTConnectDevices.DataItem> dataItems)
        {
            // Add Event
            string eventPrefix = "/GeneratedEvents/Event||00";
            //string eventPrefix = "/GeneratedData/GeneratedEvents/Event||00";
            DeviceConfiguration.EditTable(table, eventPrefix, null, "id||00;name||device_status;");

            AddActiveValue(eventPrefix, table, dataItems);
            AddIdleValue(eventPrefix, table, dataItems);

            // Add Default
            DeviceConfiguration.EditTable(table, eventPrefix + "/Default", "Alert", "numval||0;");
        }

        private static void AddActiveValue(string prefix, DataTable table, List<MTConnectDevices.DataItem> dataItems)
        {
            // Add Value
            string valuePrefix = prefix + "/Value||00";
            DeviceConfiguration.EditTable(table, valuePrefix, null, "id||00;");

            // Add Triggers
            string triggerPrefix = valuePrefix + "/Triggers";

            int i = 0;

            // Availability
            var item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "AVAILABILITY");
            if (item != null)
            {
                DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||" + i.ToString("00"), null, "id||" + i.ToString("00") + ";link||" + item.Id + ";link_type||ID;value||AVAILABLE;");
                i++;
            }

            // Emergency Stop
            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EMERGENCY_STOP");
            if (item != null)
            {
                DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||" + i.ToString("00"), null, "id||" + i.ToString("00") + ";link||" + item.Id + ";link_type||ID;value||ARMED;");
                i++;
            }

            // Add any Controller/Path Faults
            var items = dataItems.FindAll(x => x.Category == DataItemCategory.CONDITION && 
            (x.Type == "SYSTEM" || x.Type == "LOGIC_PROGRAM" || x.Type == "MOTION_PROGRAM") &&
            (x.TypePath.ToLower().Contains("controller") || x.TypePath.ToLower().Contains("path")));
            if (items != null)
            {
                foreach (var item1 in items)
                {
                    DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||" + i.ToString("00"), null, "id||" + i.ToString("00") + ";link||" + item1.Id + ";link_type||ID;modifier||NOT;value||Fault;");
                    i++;
                }
            }

            // Controller Mode
            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "CONTROLLER_MODE");
            if (item != null)
            {
                DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||" + i.ToString("00"), null, "id||" + i.ToString("00") + ";link||" + item.Id + ";link_type||ID;value||AUTOMATIC;");
                i++;
            }

            // Execution Mode
            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EXECUTION");
            if (item != null)
            {
                DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||" + i.ToString("00"), null, "id||" + i.ToString("00") + ";link||" + item.Id + ";link_type||ID;value||ACTIVE;");
                i++;
            }

            // Add Result
            DeviceConfiguration.EditTable(table, valuePrefix + "/Result", "Active", "numval||2;");
        }

        private static void AddIdleValue(string prefix, DataTable table, List<MTConnectDevices.DataItem> dataItems)
        {
            // Add Value
            string valuePrefix = prefix + "/Value||01";
            DeviceConfiguration.EditTable(table, valuePrefix, null, "id||01;");

            // Add Triggers
            string triggerPrefix = valuePrefix + "/Triggers";

            int i = 0;

            var item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "AVAILABILITY");
            if (item != null)
            {
                DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||" + i.ToString("00"), null, "id||" + i.ToString("00") + ";link||" + item.Id + ";link_type||ID;value||AVAILABLE;");
                i++;
            }

            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EMERGENCY_STOP");
            if (item != null)
            {
                DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||" + i.ToString("00"), null, "id||" + i.ToString("00") + ";link||" + item.Id + ";link_type||ID;value||ARMED;");
                i++;
            }

            // Add any Controller/Path Faults
            var items = dataItems.FindAll(x => x.Category == DataItemCategory.CONDITION &&
            (x.Type == "SYSTEM" || x.Type == "LOGIC_PROGRAM" || x.Type == "MOTION_PROGRAM") &&
            (x.TypePath.ToLower().Contains("controller") || x.TypePath.ToLower().Contains("path")));
            if (items != null)
            {
                foreach (var item1 in items)
                {
                    DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||" + i.ToString("00"), null, "id||" + i.ToString("00") + ";link||" + item1.Id + ";link_type||ID;modifier||NOT;value||Fault;");
                    i++;
                }
            }

            // Add Result
            DeviceConfiguration.EditTable(table, valuePrefix + "/Result", "Idle", "numval||1;");
        }
    }
}
