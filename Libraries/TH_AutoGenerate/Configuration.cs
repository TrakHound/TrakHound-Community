using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

using TH_Configuration;
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
            GeneratedEvents.DeviceStatus.Add(dt, items);
            GeneratedEvents.ProductionStatus.Add(dt, items);
            GeneratedEvents.ProgramExecution.Add(dt, items);
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
