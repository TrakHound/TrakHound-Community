// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;
using TH_Global.Functions;

namespace TH_AutoGenerate.GeneratedEvents
{
    public static class CycleExecution
    {
        public static void Add(DataTable dt, List<DataItem> probeItems)
        {
            // Add Event
            string eventPrefix = "/GeneratedData/GeneratedEvents/Event||02";
            DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix, "attributes", "id||02;name||cycle_execution;");

            AddProgramStartedValue(eventPrefix, dt, probeItems);
            AddProgramPausedValue(eventPrefix, dt, probeItems);
            AddCaptureItems(eventPrefix, dt, probeItems);

            // Add Default
            DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/Default", "value", "Stopped");
            DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/Default", "attributes", "numval||0;");
        }

        private static void AddProgramStartedValue(string eventPrefix, DataTable dt, List<DataItem> probeItems)
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

            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "PROGRAM");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||05", "attributes", "id||05;link||" + item.Id + ";link_type||ID;modifier||not;value||;");

            // Add Result
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "value", "Started");
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "attributes", "numval||2;");
        }

        private static void AddProgramPausedValue(string eventPrefix, DataTable dt, List<DataItem> probeItems)
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

            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "CONTROLLER_MODE");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||03", "attributes", "id||03;link||" + item.Id + ";link_type||ID;value||AUTOMATIC;");

            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EXECUTION");
            if (item != null)
            {
                DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/MultiTrigger||00", "attributes", "id||00;");
                DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/MultiTrigger||00/Trigger||00", "attributes", "id||00;link||" + item.Id + ";link_type||ID;value||INTERRUPTED;");
                DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/MultiTrigger||00/Trigger||01", "attributes", "id||01;link||" + item.Id + ";link_type||ID;value||FEED_HOLD;");
                DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/MultiTrigger||00/Trigger||02", "attributes", "id||02;link||" + item.Id + ";link_type||ID;value||OPTIONAL_STOP;");
                DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/MultiTrigger||00/Trigger||03", "attributes", "id||03;link||" + item.Id + ";link_type||ID;value||PROGRAM_STOPPED;");
            }

            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "PROGRAM");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||05", "attributes", "id||05;link||" + item.Id + ";link_type||ID;modifier||not;value||;");

            // Add Result
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "value", "Paused");
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "attributes", "numval||1;");
        }

        private static void AddCaptureItems(string eventPrefix, DataTable dt, List<DataItem> probeItems)
        {
            string capturePrefix = eventPrefix + "/Capture";

            var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "PROGRAM");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", capturePrefix + "/Item||00", "attributes", "id||00;name||program_name;link||" + item.Id + ";");

            item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EXECUTION");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", capturePrefix + "/Item||00", "attributes", "id||00;name||execution_mode;link||" + item.Id + ";");
        }

    }
}
