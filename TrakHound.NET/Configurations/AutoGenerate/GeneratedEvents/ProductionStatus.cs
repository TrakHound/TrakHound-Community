// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using MTConnect;
using MTConnectDevices = MTConnect.MTConnectDevices;
using System.Collections.Generic;
using System.Data;

namespace TrakHound.Configurations.AutoGenerate.GeneratedEvents
{
    public static class ProductionStatus
    {
        public static void Add(DataTable table, List<MTConnectDevices.DataItem> dataItems)
        {
            // Add Event
            string eventPrefix = "/GeneratedEvents/Event||01";
            DeviceConfiguration.EditTable(table, eventPrefix, null, "id||01;name||production_status;");

            AddSetupValue(eventPrefix, table, dataItems);
            AddTeardownValue(eventPrefix, table, dataItems);
            AddMaintenanceValue(eventPrefix, table, dataItems);
            AddProcessDevelopmentValue(eventPrefix, table, dataItems);

            // Add Default
            DeviceConfiguration.EditTable(table, eventPrefix + "/Default", "Production", "numval||0;");
        }

        private static void AddSetupValue(string prefix, DataTable table, List<MTConnectDevices.DataItem> dataItems)
        {
            // Only add if there is a Functional Mode tag available
            if (dataItems.Exists(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE"))
            {
                // Add Value
                string valuePrefix = prefix + "/Value||00";
                DeviceConfiguration.EditTable(table, valuePrefix, null, "id||00;");

                // Add Triggers
                string triggerPrefix = valuePrefix + "/Triggers";

                var item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE");
                if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||00", null, "id||00;link||" + item.Id + ";link_type||ID;value||SETUP;");

                // Add Result
                DeviceConfiguration.EditTable(table, valuePrefix + "/Result", "Setup", "numval||1;");
            }
        }

        private static void AddTeardownValue(string prefix, DataTable table, List<MTConnectDevices.DataItem> dataItems)
        {
            // Only add if there is a Functional Mode tag available
            if (dataItems.Exists(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE"))
            {
                // Add Value
                string valuePrefix = prefix + "/Value||01";
                DeviceConfiguration.EditTable(table, valuePrefix, null, "id||01;");

                // Add Triggers
                string triggerPrefix = valuePrefix + "/Triggers";

                var item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE");
                if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||00", null, "id||00;link||" + item.Id + ";link_type||ID;value||TEARDOWN;");

                // Add Result
                DeviceConfiguration.EditTable(table, valuePrefix + "/Result", "Teardown", "numval||2;");
            }
        }

        private static void AddMaintenanceValue(string prefix, DataTable table, List<MTConnectDevices.DataItem> dataItems)
        {
            // Only add if there is a Functional Mode tag available
            if (dataItems.Exists(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE"))
            {
                // Add Value
                string valuePrefix = prefix + "/Value||02";
                DeviceConfiguration.EditTable(table, valuePrefix, null, "id||02;");

                // Add Triggers
                string triggerPrefix = valuePrefix + "/Triggers";

                var item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE");
                if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||00", null, "id||00;link||" + item.Id + ";link_type||ID;value||MAINTENANCE;");

                // Add Result
                DeviceConfiguration.EditTable(table, valuePrefix + "/Result", "Maintenance", "numval||3");
            }
        }

        private static void AddProcessDevelopmentValue(string prefix, DataTable table, List<MTConnectDevices.DataItem> dataItems)
        {
            // Only add if there is a Functional Mode tag available
            if (dataItems.Exists(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE"))
            {
                // Add Value
                string valuePrefix = prefix + "/Value||03";
                DeviceConfiguration.EditTable(table, valuePrefix, null, "id||03;");

                // Add Triggers
                string triggerPrefix = valuePrefix + "/Triggers";

                var item = dataItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE");
                if (item != null) DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||00", null, "id||00;link||" + item.Id + ";link_type||ID;value||PROCESS_DEVELOPMENT;");

                // Add Result
                DeviceConfiguration.EditTable(table, valuePrefix + "/Result", "Process Development", "numval||4;");
            }
        }
    }
}