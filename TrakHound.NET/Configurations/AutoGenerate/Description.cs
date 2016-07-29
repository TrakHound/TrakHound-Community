// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System.Data;

using TrakHound.Tools;

namespace TrakHound.Configurations.AutoGenerate
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
