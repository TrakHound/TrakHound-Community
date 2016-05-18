// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace TH_Global.Web
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
