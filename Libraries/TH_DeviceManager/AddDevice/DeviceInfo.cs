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

        public ImageSource Image { get; set; }
    }
}
