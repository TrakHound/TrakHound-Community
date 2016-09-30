// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;

using TrakHound.Tools;

namespace TrakHound.Configurations.AutoGenerate.GeneratedEvents
{
    public static class DeviceStatus
    {
        public static void Add(DataTable dt, List<DataItem> probeItems)
        {
            // Add Event
            string eventPrefix = "/GeneratedData/GeneratedEvents/Event||00";
            DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix, "attributes", "id||00;name||device_status;");

            AddActiveValue(eventPrefix, dt, probeItems);
            AddIdleValue(eventPrefix, dt, probeItems);

            // Add Default
            DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/Default", "value", "Alert");
            DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/Default", "attributes", "numval||0;");
        }

        private static void AddActiveValue(string eventPrefix, DataTable dt, List<DataItem> probeItems)
        {
            // Add Value
            string valuePrefix = eventPrefix + "/Value||00";
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix, "attributes", "id||00;");

            // Add Triggers
            string triggerPrefix = valuePrefix + "/Triggers";

            int i = 0;

            var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "AVAILABILITY");
            if (item != null)
            {
                DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||" + i.ToString("00"), "attributes", "id||" + i.ToString("00") + ";link||" + item.Id + ";link_type||ID;value||AVAILABLE;");
                i++;
            }

            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EMERGENCY_STOP");
            if (item != null)
            {
                DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||" + i.ToString("00"), "attributes", "id||" + i.ToString("00") + ";link||" + item.Id + ";link_type||ID;value||ARMED;");
                i++;
            }

            // Add any Controller/Path Faults
            var items = probeItems.FindAll(x => x.Category == DataItemCategory.CONDITION && 
            (x.Type == "SYSTEM" || x.Type == "LOGIC_PROGRAM" || x.Type == "MOTION_PROGRAM") &&
            (x.FullAddress.ToLower().Contains("controller") || x.FullAddress.ToLower().Contains("path")));
            if (items != null)
            {
                foreach (var item1 in items)
                {
                    DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||" + i.ToString("00"), "attributes", "id||" + i.ToString("00") + ";link||" + item1.Id + ";link_type||ID;modifier||NOT;value||Fault;");
                    i++;
                }
            }

            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "CONTROLLER_MODE");
            if (item != null)
            {
                DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||" + i.ToString("00"), "attributes", "id||" + i.ToString("00") + ";link||" + item.Id + ";link_type||ID;value||AUTOMATIC;");
                i++;
            }
            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EXECUTION");
            if (item != null)
            {
                DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||" + i.ToString("00"), "attributes", "id||" + i.ToString("00") + ";link||" + item.Id + ";link_type||ID;value||ACTIVE;");
                i++;
            }

            // Add Result
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "value", "Active");
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "attributes", "numval||2;");
        }

        private static void AddIdleValue(string eventPrefix, DataTable dt, List<DataItem> probeItems)
        {
            // Add Value
            string valuePrefix = eventPrefix + "/Value||01";
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix, "attributes", "id||01;");

            // Add Triggers
            string triggerPrefix = valuePrefix + "/Triggers";

            int i = 0;

            var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "AVAILABILITY");
            if (item != null)
            {
                DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||" + i.ToString("00"), "attributes", "id||" + i.ToString("00") + ";link||" + item.Id + ";link_type||ID;value||AVAILABLE;");
                i++;
            }

            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EMERGENCY_STOP");
            if (item != null)
            {
                DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||" + i.ToString("00"), "attributes", "id||" + i.ToString("00") + ";link||" + item.Id + ";link_type||ID;value||ARMED;");
                i++;
            }

            // Add any Controller/Path Faults
            var items = probeItems.FindAll(x => x.Category == DataItemCategory.CONDITION &&
            (x.Type == "SYSTEM" || x.Type == "LOGIC_PROGRAM" || x.Type == "MOTION_PROGRAM") &&
            (x.FullAddress.ToLower().Contains("controller") || x.FullAddress.ToLower().Contains("path")));
            if (items != null)
            {
                foreach (var item1 in items)
                {
                    DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||" + i.ToString("00"), "attributes", "id||" + i.ToString("00") + ";link||" + item1.Id + ";link_type||ID;modifier||NOT;value||Fault;");
                    i++;
                }
            }

            // Add Result
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "value", "Idle");
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "attributes", "numval||1;");
        }

    }
}
