// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows.Media;

using TH_MTConnect.Components;

namespace TH_DeviceManager.AddDevice
{
    /// <summary>
    /// Class containing the data for each device found using Pages.AutoDetect
    /// </summary>
    public class DeviceInfo
    {
        public string Id { get; set; }

        public bool Loading { get; set; }

        public string IPAddress { get; set; }
        public int Port { get; set; }
        public Device Device { get; set; }

        public TH_MTConnect.Components.Description DeviceDescription
        {
            get
            {
                if (Device != null) return Device.Description;
                else return null;
            }
        }

        public string DeviceType
        {
            get
            {
                if (SharedListItem != null) return SharedListItem.device_type;
                return null;
            }
        }

        public string Controller
        {
            get
            {
                if (SharedListItem != null) return SharedListItem.controller;
                return null;
            }
        }

        public TH_UserManagement.Management.Shared.SharedListItem SharedListItem { get; set; }

        public ImageSource Image { get; set; }
    }
}
