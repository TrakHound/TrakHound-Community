// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Components;
using System.Data;
using System.Collections.Generic;
using TrakHound.Tools;

namespace TrakHound.Configurations.AutoGenerate
{
    public static class Parts
    {
        public static void Add(DataTable dt, List<DataItem> probeItems)
        {
            int i = 1;
            int e = 0;

            var partCountItems = probeItems.FindAll(x => x.Category == DataItemCategory.EVENT && x.Type == "PART_COUNT");
            foreach (var partCountItem in partCountItems)
            {
                DataTable_Functions.UpdateTableValue(dt, "address", "/Parts/Event||" + e.ToString("00"), "attributes", "id||" + e.ToString("00") + ";");

                DataTable_Functions.UpdateTableValue(dt, "address", "/Parts/Event||" + e.ToString("00") + "/EventName", "value", "parts_count_" + i.ToString());
                DataTable_Functions.UpdateTableValue(dt, "address", "/Parts/Event||" + e.ToString("00") + "/EventValue", "value", "Parts Produced");
                DataTable_Functions.UpdateTableValue(dt, "address", "/Parts/Event||" + e.ToString("00") + "/CaptureItemLink", "value", "part_count");
                DataTable_Functions.UpdateTableValue(dt, "address", "/Parts/Event||" + e.ToString("00") + "/CalculationType", "value", "Total");

                i++;
                e++;
            }
        }
    }
}
