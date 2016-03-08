using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using TH_MTConnect.Components;

namespace TH_DeviceManager.AddDevice
{
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
                if (Device != null) return Device.description;
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
