// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


using System.Data;

using TH_Global.TrakHound.Configurations;
using TH_Global.Functions;
using MTConnect.Application.Components;

namespace TH_AutoGenerate
{
    public static class Configuration
    {
        public class ProbeData
        {
            public string Address { get; set; }
            public string Port { get; set; }
            public Device Device { get; set; }
        }

        public static DeviceConfiguration Create(ProbeData probeData)
        {
            var dt = new DataTable();
            dt.Columns.Add("address");
            dt.Columns.Add("name");
            dt.Columns.Add("value");
            dt.Columns.Add("attributes");

            var items = probeData.Device.GetAllDataItems();

            SetIds(dt);
            SetEnabled(dt);
            Description.Add(dt, probeData.Device);
            Agent.Add(dt, probeData.Address, probeData.Port, probeData.Device.Name);
            Databases.Add(dt);
            InstanceData.Add(dt, items);
            SnapshotData.Add(dt);
            GeneratedEvents.DeviceStatus.Add(dt, items);
            GeneratedEvents.ProductionStatus.Add(dt, items);
            GeneratedEvents.CycleExecution.Add(dt, items);
            GeneratedEvents.PartsCount.Add(dt, items);
            Shifts.Add(dt, items);
            Cycles.Add(dt, items);
            Parts.Add(dt);          


            var xml = Converter.TableToXML(dt);

            return DeviceConfiguration.Read(xml);
        }

        private static void SetIds(DataTable dt)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/UniqueId", "value", DeviceConfiguration.GenerateUniqueID());
            DataTable_Functions.UpdateTableValue(dt, "address", "/ClientUpdateId", "value", DeviceConfiguration.GenerateUniqueID());
            DataTable_Functions.UpdateTableValue(dt, "address", "/ServerUpdateId", "value", DeviceConfiguration.GenerateUniqueID());
        }

        private static void SetEnabled(DataTable dt)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/ClientEnabled", "value", "True");
            DataTable_Functions.UpdateTableValue(dt, "address", "/ServerEnabled", "value", "True");
        }

    }
}
