// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TH_Configuration;
using TH_Global;
using TH_Plugins;

using TH_MTConnect.Streams;

namespace TH_MTConnect.Plugin
{
    public partial class MTConnect
    {

        private ReturnData GetCurrent(AgentConfiguration config)
        {
            ReturnData result = null;

            string address = config.IP_Address;
            int port = config.Port;
            string deviceName = config.Device_Name;

            // Set Proxy Settings
            var proxy = new HTTP.ProxySettings();
            proxy.Address = config.ProxyAddress;
            proxy.Port = config.ProxyPort;

            string url = HTTP.GetUrl(address, port, deviceName) + "current";

            result = Requests.Get(url, proxy);
            if (result != null)
            {
                Logger.Log("Current Successful : " + url);
            }
            else
            {
                Logger.Log("Current Error : " + url);
            }

            return result;
        }

        private void SendCurrentData(ReturnData returnData, Configuration config)
        {
            if (returnData != null)
            {
                var data = new EventData();
                data.Id = "MTConnect_Current";
                data.Data01 = config;
                data.Data02 = returnData;

                if (SendData != null) SendData(data);
            }
        }

    }
}
