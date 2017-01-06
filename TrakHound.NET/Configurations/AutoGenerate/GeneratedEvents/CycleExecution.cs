// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;

namespace TrakHound.Configurations.AutoGenerate.GeneratedEvents
{
    public static class CycleExecution
    {
        public static void Add(DataTable table, List<DataItem> dataItems)
        {
            // Add Event
            string eventPrefix = "/GeneratedEvents/Event||02";
            DeviceConfiguration.EditTable(table, eventPrefix, null, "id||02;name||cycle_execution;");

            AddProgramStartedValue(eventPrefix, table, dataItems);
            AddProgramPausedValue(eventPrefix, table, dataItems);
            AddCaptureItems(eventPrefix, table, dataItems);

            // Add Default
            DeviceConfiguration.EditTable(table, eventPrefix + "/Default", "Stopped", "numval||0;");
        }

        private static void AddProgramStartedValue(string prefix, DataTable table, List<DataItem> dataItems)
        {
            // Add Value
            string valuePrefix = prefix + "/Value||00";
            DeviceConfiguration.EditTable(table, valuePrefix, null, "id||00;");

            // Add Triggers
            string triggerPrefix = valuePrefix + "/Triggers";

            // Availability
            var item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "AVAILABILITY");
            if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||00", null, "id||00;link||" + item.Id + ";link_type||ID;value||AVAILABLE;");

            // Emergency Stop
            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EMERGENCY_STOP");
            if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||01", null, "id||01;link||" + item.Id + ";link_type||ID;value||ARMED;");

            // System
            item = dataItems.Find(x => x.Category == DataItemCategory.CONDITION && x.Type == "SYSTEM" && x.FullAddress.ToLower().Contains("controller"));
            if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||02", null, "id||02;link||" + item.Id + ";link_type||ID;modifier||NOT;value||Fault;");

            // Controller Mode
            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "CONTROLLER_MODE");
            if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||03", null, "id||03;link||" + item.Id + ";link_type||ID;value||AUTOMATIC;");

            // Execution
            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EXECUTION");
            if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||04", null, "id||04;link||" + item.Id + ";link_type||ID;value||ACTIVE;");

            // Program
            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "PROGRAM");
            if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||05", null, "id||05;link||" + item.Id + ";link_type||ID;modifier||not;value||;");

            // Add Result
            DeviceConfiguration.EditTable(table, valuePrefix + "/Result", "Started", "numval||2;");
        }

        private static void AddProgramPausedValue(string prefix, DataTable table, List<DataItem> dataItems)
        {
            // Add Value
            string valuePrefix = prefix + "/Value||01";
            DeviceConfiguration.EditTable(table, valuePrefix, null, "id||01;");

            // Add Triggers
            string triggerPrefix = valuePrefix + "/Triggers";

            // Availability
            var item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "AVAILABILITY");
            if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||00", null, "id||00;link||" + item.Id + ";link_type||ID;value||AVAILABLE;");

            // Emergency Stop
            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EMERGENCY_STOP");
            if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||01", null, "id||01;link||" + item.Id + ";link_type||ID;value||ARMED;");

            // System
            item = dataItems.Find(x => x.Category == DataItemCategory.CONDITION && x.Type == "SYSTEM" && x.FullAddress.ToLower().Contains("controller"));
            if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||02", null, "id||02;link||" + item.Id + ";link_type||ID;modifier||NOT;value||Fault;");

            // Controller Mode
            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "CONTROLLER_MODE");
            if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||03", null, "id||03;link||" + item.Id + ";link_type||ID;value||AUTOMATIC;");

            // Execution Mode
            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EXECUTION");
            if (item != null)
            {
                DeviceConfiguration.EditTable(table, triggerPrefix + "/MultiTrigger||00", null, "id||00;");
                DeviceConfiguration.EditTable(table, triggerPrefix + "/MultiTrigger||00/Trigger||00", null, "id||00;link||" + item.Id + ";link_type||ID;value||INTERRUPTED;");
                DeviceConfiguration.EditTable(table, triggerPrefix + "/MultiTrigger||00/Trigger||01", null, "id||01;link||" + item.Id + ";link_type||ID;value||FEED_HOLD;");
                DeviceConfiguration.EditTable(table, triggerPrefix + "/MultiTrigger||00/Trigger||02", null, "id||02;link||" + item.Id + ";link_type||ID;value||OPTIONAL_STOP;");
                DeviceConfiguration.EditTable(table, triggerPrefix + "/MultiTrigger||00/Trigger||03", null, "id||03;link||" + item.Id + ";link_type||ID;value||PROGRAM_STOPPED;");
            }

            // Program Name
            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "PROGRAM");
            if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||05", null, "id||05;link||" + item.Id + ";link_type||ID;modifier||not;value||;");

            // Add Result
            DeviceConfiguration.EditTable(table, valuePrefix + "/Result", "Paused", "numval||1;");
        }

        private static void AddCaptureItems(string prefix, DataTable table, List<DataItem> dataItems)
        {
            string capturePrefix = prefix + "/Capture";

            var item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "PROGRAM");
            if (item != null) DeviceConfiguration.EditTable(table, capturePrefix + "/Item||00", null, "id||00;name||program_name;link||" + item.Id + ";");

            item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "EXECUTION");
            if (item != null) DeviceConfiguration.EditTable(table, capturePrefix + "/Item||01", null, "id||01;name||execution_mode;link||" + item.Id + ";");
        }        
    }
}
