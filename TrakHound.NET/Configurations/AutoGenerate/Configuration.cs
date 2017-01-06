// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System.Data;

namespace TrakHound.Configurations.AutoGenerate
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
            if (probeData != null && probeData.Device != null)
            {
                var table = new DataTable();
                table.Columns.Add("address");
                table.Columns.Add("name");
                table.Columns.Add("value");
                table.Columns.Add("attributes");

                var items = probeData.Device.GetAllDataItems();

                SetIds(table);
                SetEnabled(table);
                Description.Add(table, probeData.Device);
                Agent.Add(table, probeData.Address, probeData.Port, probeData.Device.Name);
                GeneratedEvents.DeviceStatus.Add(table, items);
                GeneratedEvents.ProductionStatus.Add(table, items);
                GeneratedEvents.CycleExecution.Add(table, items);
                GeneratedEvents.PartsCount.Add(table, items);
                Cycles.Add(table, items);
                Parts.Add(table, items);
                Overrides.Add(table, items);
                Oee.Add(table, items);

                // Manufacturer Specific Processing
                Manufacturers.Okuma.Process(table, probeData.Device);

                //var xml = Converters.DeviceConfigurationConverter.TableToXML(dt);
                var xml = DeviceConfiguration.TableToXml(table);

                return DeviceConfiguration.Read(xml);
            }

            return null;
        }

        private static void SetIds(DataTable table)
        {
            DeviceConfiguration.EditTable(table, "/UniqueId", DeviceConfiguration.GenerateUniqueID(), null);
            DeviceConfiguration.EditTable(table, "/UpdateId", DeviceConfiguration.GenerateUniqueID(), null);
        }

        private static void SetEnabled(DataTable table)
        {
            DeviceConfiguration.EditTable(table, "/Enabled", true, null);
        }

    }
}
