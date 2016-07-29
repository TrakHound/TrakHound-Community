// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Plugins;

namespace TrakHound_Overview
{
    public partial class Overview
    {

        void UpdateCurrentUser(EventData data)
        {
            if (data != null && data.Id != null)
            {
                if (data.Id.ToLower() == "userloggedin")
                {
                    var userConfig = data.Data01 as UserConfiguration;
                    if (userConfig != null) CurrentUser = userConfig;
                }
            }
        }

        void UpdateData(EventData data)
        {
            if (data != null)
            {
                var config = data.Data01 as DeviceConfiguration;
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
            data.Id = "DeviceSelected";
            data.Data01 = Devices[index];
            if (SendData != null) SendData(data);
        }

    }
}
