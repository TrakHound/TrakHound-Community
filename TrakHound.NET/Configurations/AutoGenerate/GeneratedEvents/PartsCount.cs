// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;

using TrakHound.Tools;

namespace TrakHound.Configurations.AutoGenerate.GeneratedEvents
{
    public static class PartsCount
    {
        public static void Add(DataTable dt, List<DataItem> probeItems)
        {
            if (probeItems.Exists(x => x.Category == DataItemCategory.EVENT && x.Type == "PART_COUNT"))
            {
                // Add Event
                string eventPrefix = "/GeneratedData/GeneratedEvents/Event||03";
                DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix, "attributes", "id||03;name||parts_count;");

                AddPartsProducedValue(eventPrefix, dt, probeItems);
                AddCaptureItems(eventPrefix, dt, probeItems);

                // Add Default
                DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/Default", "value", "No Parts Produced");
                DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/Default", "attributes", "numval||0;");
            } 
        }

        private static void AddPartsProducedValue(string eventPrefix, DataTable dt, List<DataItem> probeItems)
        {
            // Add Value
            string valuePrefix = eventPrefix + "/Value||00";
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix, "attributes", "id||00;");

            // Add Triggers
            string triggerPrefix = valuePrefix + "/Triggers";

            var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "PART_COUNT");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||00", "attributes", "id||00;link||" + item.Id + ";link_type||ID;value||UNAVAILABLE;modifier||not;");

            // Add Result
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "value", "Parts Produced");
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "attributes", "numval||1;");
        }

        private static void AddCaptureItems(string eventPrefix, DataTable dt, List<DataItem> probeItems)
        {
            string capturePrefix = eventPrefix + "/Capture";

            var item = probeItems.Find(x => x.Category == DataItemCategory.EVENT && x.Type == "PART_COUNT");
            if (item != null) DataTable_Functions.UpdateTableValue(dt, "address", capturePrefix + "/Item||00", "attributes", "id||00;name||part_count;link||" + item.Id + ";");
        }

    }
}
