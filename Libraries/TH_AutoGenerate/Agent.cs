// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Data;
using TH_Global.Functions;

namespace TH_AutoGenerate
{
    public static class Agent
    {

        public static void Add(DataTable dt, string address, string port, string deviceName)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/Agent/Address", "value", address);
            DataTable_Functions.UpdateTableValue(dt, "address", "/Agent/Port", "value", port);
            DataTable_Functions.UpdateTableValue(dt, "address", "/Agent/DeviceName", "value", deviceName);
            DataTable_Functions.UpdateTableValue(dt, "address", "/Agent/Heartbeat", "value", "5000");
        }

    }
}
