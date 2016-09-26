// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;

using TrakHound.Tools;

namespace TrakHound.Configurations.AutoGenerate.GeneratedEvents
{
    public static class ProductionStatus
    {

        public static void Add(DataTable dt, List<DataItem> probeItems)
        {
            // Add Event
            string eventPrefix = "/GeneratedData/GeneratedEvents/Event||01";
            DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix, "attributes", "id||01;name||production_status;");

            AddSetupValue(eventPrefix, dt, probeItems);
            AddTeardownValue(eventPrefix, dt, probeItems);
            AddMaintenanceValue(eventPrefix, dt, probeItems);
            AddProcessDevelopmentValue(eventPrefix, dt, probeItems);

            // Add Default
            DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/Default", "value", "Production");
            DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/Default", "attributes", "numval||0;");
        }

        private static void AddSetupValue(string eventPrefix, DataTable dt, List<DataItem> probeItems)
        {
            // Only add if there is a Functional Mode tag available
            if (probeItems.Exists(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE"))
            {
                // Add Value
                string valuePrefix = eventPrefix + "/Value||00";
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix, "attributes", "id||00;");

                // Add Triggers
                string triggerPrefix = valuePrefix + "/Triggers";

                var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE");
                if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||00", "attributes", "id||00;link||" + item.Id + ";link_type||ID;value||SETUP;");

                // Add Result
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "value", "Setup");
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "attributes", "numval||1;");
            }
        }

        private static void AddTeardownValue(string eventPrefix, DataTable dt, List<DataItem> probeItems)
        {
            // Only add if there is a Functional Mode tag available
            if (probeItems.Exists(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE"))
            {
                // Add Value
                string valuePrefix = eventPrefix + "/Value||01";
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix, "attributes", "id||01;");

                // Add Triggers
                string triggerPrefix = valuePrefix + "/Triggers";

                var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE");
                if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||00", "attributes", "id||00;link||" + item.Id + ";link_type||ID;value||TEARDOWN;");

                // Add Result
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "value", "Teardown");
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "attributes", "numval||2;");
            }
        }

        private static void AddMaintenanceValue(string eventPrefix, DataTable dt, List<DataItem> probeItems)
        {
            // Only add if there is a Functional Mode tag available
            if (probeItems.Exists(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE"))
            {
                // Add Value
                string valuePrefix = eventPrefix + "/Value||02";
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix, "attributes", "id||02;");

                // Add Triggers
                string triggerPrefix = valuePrefix + "/Triggers";

                var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE");
                if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||00", "attributes", "id||00;link||" + item.Id + ";link_type||ID;value||MAINTENANCE;");

                // Add Result
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "value", "Maintenance");
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "attributes", "numval||43");
            }
        }

        private static void AddProcessDevelopmentValue(string eventPrefix, DataTable dt, List<DataItem> probeItems)
        {
            // Only add if there is a Functional Mode tag available
            if (probeItems.Exists(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE"))
            {
                // Add Value
                string valuePrefix = eventPrefix + "/Value||03";
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix, "attributes", "id||03;");

                // Add Triggers
                string triggerPrefix = valuePrefix + "/Triggers";

                var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "FUNCTIONAL_MODE");
                if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||00", "attributes", "id||00;link||" + item.Id + ";link_type||ID;value||PROCESS_DEVELOPMENT;");

                // Add Result
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "value", "Process Development");
                DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "attributes", "numval||4;");
            }
        }

    }
}