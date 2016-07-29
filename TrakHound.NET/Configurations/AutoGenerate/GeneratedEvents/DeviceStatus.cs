// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

            var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "AVAILABILITY");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||00", "attributes", "id||00;link||" + item.Id + ";link_type||ID;value||AVAILABLE;");

            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EMERGENCY_STOP");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||01", "attributes", "id||01;link||" + item.Id + ";link_type||ID;value||ARMED;");

            item = probeItems.Find(x => x.Category == DataItemCategory.CONDITION && x.Type == "SYSTEM" && x.FullAddress.ToLower().Contains("controller"));
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||02", "attributes", "id||02;link||" + item.Id + ";link_type||ID;modifier||NOT;value||Fault;");

            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "CONTROLLER_MODE");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||03", "attributes", "id||03;link||" + item.Id + ";link_type||ID;value||AUTOMATIC;");

            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EXECUTION");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||04", "attributes", "id||04;link||" + item.Id + ";link_type||ID;value||ACTIVE;");


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

            var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "AVAILABILITY");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||00", "attributes", "id||00;link||" + item.Id + ";link_type||ID;value||AVAILABLE;");

            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EMERGENCY_STOP");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||01", "attributes", "id||01;link||" + item.Id + ";link_type||ID;value||ARMED;");

            item = probeItems.Find(x => x.Category == DataItemCategory.CONDITION && x.Type == "SYSTEM" && x.FullAddress.ToLower().Contains("controller"));
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||02", "attributes", "id||02;link||" + item.Id + ";link_type||ID;modifier||NOT;value||Fault;");

            // Add Result
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "value", "Idle");
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "attributes", "numval||1;");
        }

    }
}
