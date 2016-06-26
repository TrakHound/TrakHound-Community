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
    public static class Parts
    {
        public static void Add(DataTable dt)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/Parts/PartsEventName", "value", "parts_count");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Parts/PartsEventValue", "value", "parts_produced");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Parts/PartsCaptureItemLink", "value", "part_count");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Parts/CalculationType", "value", "Total");
        }
    }
}
