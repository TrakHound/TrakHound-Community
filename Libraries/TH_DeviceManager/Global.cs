// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TH_Configuration;

namespace TH_DeviceManager
{

    public delegate void DeviceSelected_Handler(DeviceConfiguration config);
    public delegate void PageSelected_Handler();

    public enum ManagementType
    {
        Client = 0,
        Server = 1
    }

}
