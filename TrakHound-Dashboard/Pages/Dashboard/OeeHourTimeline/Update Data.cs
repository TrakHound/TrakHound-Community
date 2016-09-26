// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

using TrakHound;
using TrakHound.API;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.Dashboard.OeeHourTimeline
{
    public partial class Page
    {

        void Update(EventData data)
        {
            if (data != null && data.Id == "STATUS_STATUS" && data.Data02 != null && data.Data02.GetType() == typeof(Data.StatusInfo))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (Data.StatusInfo)data.Data02;

                    int index = Rows.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var row = Rows[index];
                        row.UpdateData(info);
                    }
                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }

            if (data != null && data.Id == "STATUS_HOURS" && data.Data02 != null && data.Data02.GetType() == typeof(List<Data.HourInfo>))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    string uniqueId = data.Data01.ToString();
                    var info = (List<Data.HourInfo>)data.Data02;

                    int index = Rows.ToList().FindIndex(x => x.Device.UniqueId == uniqueId);
                    if (index >= 0)
                    {
                        var row = Rows[index];
                        row.UpdateData(info);
                    }
                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }
        }

        void UpdateDevicesLoading(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "LOADING_DEVICES")
                {
                    ClearRows();
                }
            }
        }

        private void ClearRows()
        {
            foreach (var row in Rows) row.Clicked -= Row_Clicked;
            Rows.Clear();
        }

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;
                    AddRow(device);
                }
            }
        }

        void UpdateDeviceUpdated(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_UPDATED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;

                    int index = Rows.ToList().FindIndex(x => GetUniqueIdFromDeviceInfo(x) == device.UniqueId);
                    if (index >= 0)
                    {
                        var row = Rows[index];
                        row.Device = device;
                        Rows.Sort();
                    }
                }
            }
        }

        void UpdateDeviceRemoved(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_REMOVED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;

                    int index = Rows.ToList().FindIndex(x => GetUniqueIdFromDeviceInfo(x) == device.UniqueId);
                    if (index >= 0)
                    {
                        // Remove Event Handlers
                        var row = Rows[index];
                        row.Clicked -= Row_Clicked;

                        Rows.RemoveAt(index);
                    }
                }
            }
        }

    }
}
