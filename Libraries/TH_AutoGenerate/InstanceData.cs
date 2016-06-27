// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;

using TH_Global.Functions;

namespace TH_AutoGenerate
{
    public static class InstanceData
    {

        public static void Add(DataTable dt, List<DataItem> probeItems)
        {
            // Set Table Defaults
            DataTable_Functions.UpdateTableValue(dt, "address", "/InstanceTable/DataItems/Conditions", "value", "False");
            DataTable_Functions.UpdateTableValue(dt, "address", "/InstanceTable/DataItems/Events", "value", "False");
            DataTable_Functions.UpdateTableValue(dt, "address", "/InstanceTable/DataItems/Samples", "value", "False");

            // Set Omit Items
            foreach (var item in probeItems)
            {
                if (item.Category == DataItemCategory.SAMPLE || item.Type == "LINE" || item.Type == "BLOCK")
                {
                    DataTable_Functions.UpdateTableValue(dt, "address", "/InstanceTable/DataItems/Omit/" + item.Id, null, null);
                }
            }
        }

    }
}
