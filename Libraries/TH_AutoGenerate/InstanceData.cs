using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;

using TH_Configuration;
using TH_Global.Functions;

using MTConnect.Application.Components;

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
