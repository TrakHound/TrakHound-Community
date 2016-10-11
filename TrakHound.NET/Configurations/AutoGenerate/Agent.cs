// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Data;

namespace TrakHound.Configurations.AutoGenerate
{
    public static class Agent
    {
        public static void Add(DataTable table, string address, string port, string deviceName)
        {
            DeviceConfiguration.EditTable(table, "/Agent/Address", address, null);
            DeviceConfiguration.EditTable(table, "/Agent/Port", port, null);
            DeviceConfiguration.EditTable(table, "/Agent/DeviceName", deviceName, null);
            DeviceConfiguration.EditTable(table, "/Agent/Heartbeat", 5000, null);
        }
    }
}
