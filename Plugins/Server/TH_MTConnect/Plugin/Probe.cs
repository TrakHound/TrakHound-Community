// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TH_Configuration;
using TH_Global;
using TH_Plugins;

using MTConnect;
using MTConnect.Application.Components;

namespace TH_MTConnect.Plugin
{
    public partial class Plugin
    {

        private ReturnData GetProbe(AgentConfiguration config)
        {
            ReturnData result = null;

            string address = config.IP_Address;
            int port = config.Port;
            string deviceName = config.Device_Name;

            // Set Proxy Settings
            var proxy = new HTTP.ProxySettings();
            proxy.Address = config.ProxyAddress;
            proxy.Port = config.ProxyPort;

            DateTime requestTimestamp = DateTime.Now;

            string url = HTTP.GetUrl(address, port, deviceName) + "probe";

            result = Requests.Get(url, proxy);
            if (result != null)
            {
                Logger.Log("Probe Successful : " + url + " @ " + requestTimestamp.ToString("o"));
            }
            else
            {
                Logger.Log("Probe Error : " + url + " @ " + requestTimestamp.ToString("o"));
            }

            return result;
        }

        private void SendProbeData(ReturnData returnData, Configuration config)
        {
            if (returnData != null)
            {
                var data = new EventData();
                data.Id = "MTConnect_Probe";
                data.Data01 = config;
                data.Data02 = returnData;

                if (SendData != null) SendData(data);
            }
        }

    }
}
