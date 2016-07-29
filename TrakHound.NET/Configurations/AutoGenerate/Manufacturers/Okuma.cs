using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using MTConnect.Application.Components;

using TrakHound.Tools;

namespace TrakHound.Configurations.AutoGenerate.Manufacturers
{
    public static class Okuma
    {

        public static void Process(DataTable table, Device probeDevice)
        {
            if (probeDevice != null && !string.IsNullOrEmpty(probeDevice.Description.Manufacturer))
            {
                if (probeDevice.Description.Manufacturer.ToLower() == "okuma")
                {
                    RemoveDescription(table);
                }
            }
        }

        private static void RemoveDescription(DataTable table)
        {
            DataTable_Functions.UpdateTableValue(table, "address", "/Description/Description", "value", "");
        }

    }
}
