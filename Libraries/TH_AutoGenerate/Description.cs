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
    public static class Description
    {

        public static void Add(DataTable dt, Device probeDevice)
        {
            DataTable_Functions.UpdateTableValue(dt, "address", "/Description/Manufacturer", "value", Trim(probeDevice.Description.Manufacturer));
            DataTable_Functions.UpdateTableValue(dt, "address", "/Description/Model", "value", Trim(probeDevice.Description.Model));
            DataTable_Functions.UpdateTableValue(dt, "address", "/Description/Serial", "value", Trim(probeDevice.Description.SerialNumber));
            DataTable_Functions.UpdateTableValue(dt, "address", "/Description/Description", "value", Trim(probeDevice.Description.CDATA));         
        }

        private static string Trim(string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                return s.Trim();
            }

            return null;
        }
        
    }
}
