using System;
using System.Linq;

using TrakHound.Configurations;
using TrakHound.Plugins;

namespace TrakHound_Overview.Pages.Dashboard.OverrideStatus
{
    public partial class Plugin
    {

        void Update(EventData data)
        {
            if (data != null && data.Id != null && data.Data01 != null)
            {
                var config = data.Data01 as DeviceConfiguration;
                if (config != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        int index = Rows.ToList().FindIndex(x => x.Configuration.UniqueId == config.UniqueId);
                        if (index >= 0)
                        {
                            var row = Rows[index];
                            row.UpdateData(data);   
                        }
                    }));
                }
            }
        }

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    var config = (DeviceConfiguration)data.Data01;
                    AddRow(config);
                }
            }
        }

        void UpdateDeviceUpdated(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_UPDATED" && data.Data01 != null)
                {
                    var config = (DeviceConfiguration)data.Data01;

                    int index = Rows.ToList().FindIndex(x => GetUniqueIdFromDeviceInfo(x) == config.UniqueId);
                    if (index >= 0)
                    {
                        Rows.RemoveAt(index);
                        AddRow(config, index);
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
                    var config = (DeviceConfiguration)data.Data01;

                    int index = Rows.ToList().FindIndex(x => GetUniqueIdFromDeviceInfo(x) == config.UniqueId);
                    if (index >= 0) Rows.RemoveAt(index);
                }
            }
        }

    }
}
