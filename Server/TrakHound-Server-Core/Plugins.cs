// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TH_Device_Server;

namespace TrakHound_Server_Core
{
    public partial class Server
    {
        private void SendPluginData(string id, string message)
        {
            foreach (Device_Server device in Devices)
            {
                device.SendPluginsData(id, message);
            }
        }
    }
}
