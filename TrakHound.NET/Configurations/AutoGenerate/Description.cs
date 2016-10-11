// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System.Data;

namespace TrakHound.Configurations.AutoGenerate
{
    public static class Description
    {
        public static void Add(DataTable table, Device device)
        {
            if (device.Description != null)
            {
                DeviceConfiguration.EditTable(table, "/Description/Manufacturer", Trim(device.Description.Manufacturer), null);
                DeviceConfiguration.EditTable(table, "/Description/Model", Trim(device.Description.Model), null);
                DeviceConfiguration.EditTable(table, "/Description/Serial", Trim(device.Description.SerialNumber), null);
                DeviceConfiguration.EditTable(table, "/Description/Description", Trim(device.Description.CDATA), null);
            }
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
