// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;

namespace TrakHound.Configurations.AutoGenerate.GeneratedEvents
{
    public static class PartsCount
    {
        public static void Add(DataTable table, List<DataItem> dataItems)
        {
            int i = 1;
            int e = 3;

            var partCountItems = dataItems.FindAll(x => x.Category == DataItemCategory.EVENT && x.Type == "PART_COUNT");
            foreach (var partCountItem in partCountItems)
            {
                // Add Event
                string eventPrefix = "/GeneratedEvents/Event||" + e.ToString("00");
                DeviceConfiguration.EditTable(table, eventPrefix, null, "id||" + e.ToString("00") + ";name||parts_count_" + i.ToString() + ";");

                AddPartsProducedValue(eventPrefix, table, partCountItem);
                AddCaptureItems(eventPrefix, table, partCountItem);

                // Add Default
                DeviceConfiguration.EditTable(table, eventPrefix + "/Default", "No Parts Produced", "numval||0;");

                i++;
                e++;
            }
        }

        private static void AddPartsProducedValue(string prefix, DataTable table, DataItem dataItem)
        {
            // Add Value
            string valuePrefix = prefix + "/Value||00";
            DeviceConfiguration.EditTable(table, valuePrefix, null, "id||00;");

            // Add Triggers
            string triggerPrefix = valuePrefix + "/Triggers";

            DeviceConfiguration.EditTable(table, triggerPrefix + "/Trigger||00", null, "id||00;link||" + dataItem.Id + ";link_type||ID;value||UNAVAILABLE;modifier||not;");

            // Add Result
            DeviceConfiguration.EditTable(table, valuePrefix + "/Result", "Parts Produced", "numval||1;");
        }

        private static void AddCaptureItems(string prefix, DataTable table, DataItem dataItem)
        {
            string capturePrefix = prefix + "/Capture";

            DeviceConfiguration.EditTable(table, capturePrefix + "/Item||00", null, "id||00;name||part_count;link||" + dataItem.Id + ";");
        }
    }
}
