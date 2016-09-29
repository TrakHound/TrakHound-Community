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
            int i = 1;
            int e = 3;

            var partCountItems = probeItems.FindAll(x => x.Category == DataItemCategory.EVENT && x.Type == "PART_COUNT");
            foreach (var partCountItem in partCountItems)
            {
                // Add Event
                // Add Event
                string eventPrefix = "/GeneratedData/GeneratedEvents/Event||" + e.ToString("00");
                DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix, "attributes", "id||" + e.ToString("00") + ";name||parts_count_" + i.ToString() + ";");

                AddPartsProducedValue(eventPrefix, dt, partCountItem);
                AddCaptureItems(eventPrefix, dt, partCountItem);

                // Add Default
                DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/Default", "value", "No Parts Produced");
                DataTable_Functions.UpdateTableValue(dt, "address", eventPrefix + "/Default", "attributes", "numval||0;");

                i++;
                e++;
            }
        }

        private static void AddPartsProducedValue(string eventPrefix, DataTable dt, DataItem probeItem)
        {
            // Add Value
            string valuePrefix = eventPrefix + "/Value||00";
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix, "attributes", "id||00;");

            // Add Triggers
            string triggerPrefix = valuePrefix + "/Triggers";

            DataTable_Functions.UpdateTableValue(dt, "address", triggerPrefix + "/Trigger||00", "attributes", "id||00;link||" + probeItem.Id + ";link_type||ID;value||UNAVAILABLE;modifier||not;");

            // Add Result
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "value", "Parts Produced");
            DataTable_Functions.UpdateTableValue(dt, "address", valuePrefix + "/Result", "attributes", "numval||1;");
        }

        private static void AddCaptureItems(string eventPrefix, DataTable dt, DataItem probeItem)
        {
            string capturePrefix = eventPrefix + "/Capture";

            DataTable_Functions.UpdateTableValue(dt, "address", capturePrefix + "/Item||00", "attributes", "id||00;name||part_count;link||" + probeItem.Id + ";");
        }

    }
}
