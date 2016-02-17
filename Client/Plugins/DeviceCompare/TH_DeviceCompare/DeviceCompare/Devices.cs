// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TH_Configuration;

namespace TH_DeviceCompare
{
    public partial class DeviceCompare
    {
        /// <summary>
        /// Create the DeviceDisplays and RowHeaders based on the Devices set for DeviceCompare
        /// </summary>
        /// <param name="devices"></param>
        void UpdateDevices(List<Configuration> devices)
        {
            if (devices != null)
            {
                DeviceDisplays = new List<DeviceDisplay>();
                Headers.Clear();
                Columns.Clear();
                Overlays.Clear();

                var category = SubCategories.Find(x => x.Name == "Components");
                if (category != null)
                {
                    // Add the RowHeaders
                    AddRowHeaders(Plugins, category.PluginConfigurations);

                    foreach (Configuration device in devices)
                    {
                        var display = new DeviceDisplay(device, Plugins, category.PluginConfigurations);
                        display.CellSizeChanged += display_CellSizeChanged;

                        DeviceDisplays.Add(display);
                        if (display.Group.Header != null) Headers.Add(display.Group.Header);
                        if (display.Group.Column != null) Columns.Add(display.Group.Column);
                        if (display.Group.Overlay != null) Overlays.Add(display.Group.Overlay);
                    }

                    SortDataItems();
                    LoadHeaderView();
                }
            }
        }

    }
}
