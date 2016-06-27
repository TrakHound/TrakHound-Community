// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System.Collections.Generic;
using System.Data;

using TH_Global.Functions;

namespace TH_AutoGenerate
{
    public static class Shifts
    {

        public static void Add(DataTable dt, List<DataItem> probeItems)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/Shifts/Shift||00", "attributes", "id||00;name||1st Shift;begintime||7:00:00;endtime||15:00:00;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Shifts/Shift||01", "attributes", "id||01;name||2nd Shift;begintime||15:00:00;endtime||23:00:00;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/Shifts/Shift||02", "attributes", "id||02;name||3rd Shift;begintime||23:00:00;endtime||7:00:00;");
        }

    }
}
