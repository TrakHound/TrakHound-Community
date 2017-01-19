// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

namespace TrakHound.Tools.Web
{
    public class ProxySettings
    {
        public ProxySettings() { }
        public ProxySettings(string address, int port)
        {
            Address = address;
            Port = port;
        }

        public string Address { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
