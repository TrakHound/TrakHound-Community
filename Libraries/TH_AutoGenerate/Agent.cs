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
