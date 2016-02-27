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
        /// Handler for Devices.CollectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                ClearDeviceList();
            }

            if (e.NewItems != null)
            {
                foreach (Configuration newConfig in e.NewItems)
                {
                    if (newConfig != null) AddDevice(newConfig);
                }

                SortDataItems();
                LoadHeaderView();
            }

            if (e.OldItems != null)
            {
                foreach (Configuration oldConfig in e.OldItems)
                {
                    Devices.Remove(oldConfig);

                    int index = DeviceDisplays.FindIndex(x => x.UniqueId == oldConfig.UniqueId);
                    if (index >= 0)
                    {
                        var dd = DeviceDisplays[index];
                        Headers.Remove(dd.Group.Header);
                        Columns.Remove(dd.Group.Column);
                        Overlays.Remove(dd.Group.Overlay);
                        DeviceDisplays.Remove(dd);
                    }
                }
            }
        }

        /// <summary>
        /// Create the DeviceDisplays and RowHeaders based on the Devices set for DeviceCompare
        /// </summary>
        /// <param name="devices"></param>
        void UpdateDeviceList(List<Configuration> configs)
        {
            if (configs != null)
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

                    foreach (Configuration config in configs)
                    {
                        AddDevice(config);
                    }

                    SortDataItems();
                    LoadHeaderView();
                }
            }
        }

        private void ClearDeviceList()
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

                SortDataItems();
                LoadHeaderView();
            }
        }

        private void AddDevice(Configuration config)
        {
            var category = SubCategories.Find(x => x.Name == "Components");
            if (category != null)
            {
                var display = new DeviceDisplay(config, Plugins, category.PluginConfigurations);
                display.CellSizeChanged += display_CellSizeChanged;

                DeviceDisplays.Add(display);
                if (display.Group.Header != null) Headers.Add(display.Group.Header);
                if (display.Group.Column != null) Columns.Add(display.Group.Column);
                if (display.Group.Overlay != null) Overlays.Add(display.Group.Overlay);
            }
        }

    }
}
