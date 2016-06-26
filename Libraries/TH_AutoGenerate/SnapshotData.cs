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
    public static class SnapshotData
    {
        public static void Add(DataTable dt)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/SnapShotData/Generated", "attributes", "id||00;name||Device Status;link||device_status;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/SnapShotData/Generated", "attributes", "id||01;name||Production Status;link||production_status;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/SnapShotData/Generated", "attributes", "id||02;name||Program Execution;link||program_execution;");
        }

    }
}
