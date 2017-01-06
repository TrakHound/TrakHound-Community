// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TrakHound.Configurations;

namespace TrakHound_Dashboard.Pages.DeviceManager
{

    public delegate void DeviceSelected_Handler(DeviceDescription config);
    public delegate void PageSelected_Handler();

    public enum ManagementType
    {
        Client = 0,
        Server = 1
    }

}
