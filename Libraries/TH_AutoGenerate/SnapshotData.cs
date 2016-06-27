// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Data;
using TH_Global.Functions;

namespace TH_AutoGenerate
{
    public static class SnapshotData
    {
        public static void Add(DataTable dt)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/GeneratedData/SnapShotData/Generated||00", "attributes", "id||00;name||Device Status;link||device_status;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/GeneratedData/SnapShotData/Generated||01", "attributes", "id||01;name||Production Status;link||production_status;");
            DataTable_Functions.UpdateTableValue(dt, "address", "/GeneratedData/SnapShotData/Generated||02", "attributes", "id||02;name||Program Execution;link||program_execution;");
        }

    }
}
