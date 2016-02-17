// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TH_Configuration;
using TH_Plugins_Client;

namespace TH_DeviceCompare
{
    public partial class DeviceCompare
    {

        void UpdateData(DataEvent_Data de_d)
        {
            if (de_d != null)
            {
                Configuration config = de_d.data01 as Configuration;
                if (config != null)
                {
                    DeviceDisplay dd = DeviceDisplays.Find(x => x.UniqueId == config.UniqueId);
                    if (dd != null)
                    {
                        dd.UpdateData(de_d);
                    }
                }
            }
        }

        void DeviceSelected(int index)
        {
            DataEvent_Data de_d = new DataEvent_Data();
            de_d.id = "DeviceSelected";
            de_d.data01 = Devices[index];
            if (DataEvent != null) DataEvent(de_d);
        }

    }
}
