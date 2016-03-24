// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TH_Configuration;
using TH_Plugins;

namespace TH_DeviceCompare
{
    public partial class DeviceCompare
    {

        void UpdateData(EventData data)
        {
            if (data != null)
            {
                Configuration config = data.data01 as Configuration;
                if (config != null)
                {
                    DeviceDisplay dd = DeviceDisplays.Find(x => x.UniqueId == config.UniqueId);
                    if (dd != null)
                    {
                        this.Dispatcher.BeginInvoke(new Action<EventData>(dd.UpdateData), Priority_Background, new object[] { data });
                    }
                }
            }
        }

        void DeviceSelected(int index)
        {
            var data = new EventData();
            data.id = "DeviceSelected";
            data.data01 = Devices[index];
            if (SendData != null) SendData(data);
        }

    }
}
