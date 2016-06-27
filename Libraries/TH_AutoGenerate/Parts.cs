// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Data;
using TH_Global.Functions;

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
