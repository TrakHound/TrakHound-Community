// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System;
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

                if (!string.IsNullOrEmpty(device.Description.Manufacturer))
                {
                    string logoUrl = new Uri("http://dev.trakhound.com/api/images/device_images/download?manufacturer=" + device.Description.Manufacturer).ToString();
                    DeviceConfiguration.EditTable(table, "/Description/LogoUrl", logoUrl, null);

                    if (!string.IsNullOrEmpty(device.Description.Model))
                    {
                        string imageUrl = new Uri("http://dev.trakhound.com/api/images/device_images/download?manufacturer=" + device.Description.Manufacturer + "&model=" + device.Description.Model).ToString();
                        DeviceConfiguration.EditTable(table, "/Description/ImageUrl", imageUrl, null);
                    }
                }
                
                
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
