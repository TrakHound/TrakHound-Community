// Copyright (c) 2017 TrakHound Inc, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Net;
using System.Net.NetworkInformation;

namespace TrakHound.MTConnectSniffer
{
    /// <summary>
    /// Container for MTConnect Device information
    /// </summary>
    public class MTConnectDevice
    {
        /// <summary>
        /// The IP Address of the MTConnect Agent
        /// </summary>
        public IPAddress IpAddress { get; set; }

        /// <summary>
        /// The Port of the MTConnect Agent
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The MAC Address of the device that the MTConnect Agent is operating on
        /// </summary>
        public PhysicalAddress MacAddress { get; set; }

        /// <summary>
        /// The name of the MTConnect Device
        /// </summary>
        public string DeviceName { get; set; }


        public MTConnectDevice(IPAddress ipAddress, int port, PhysicalAddress macAddress, string deviceName)
        {
            IpAddress = ipAddress;
            Port = port;
            MacAddress = macAddress;
            DeviceName = deviceName;
        }
    }
}
