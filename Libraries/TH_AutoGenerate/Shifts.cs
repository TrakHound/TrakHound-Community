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
