// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Streams;
using System;

using TrakHound.Configurations;
using TrakHound.Logging;
using TrakHound.Plugins;

namespace TrakHound_Server.Plugins.MTConnectData
{
    public partial class Plugin
    {

        private ReturnData GetCurrent(Configuration config)
        {
            ReturnData result = null;

            string address = config.Address;
            int port = config.Port;
            string deviceName = config.DeviceName;

            // Set Proxy Settings
            var proxy = new HTTP.ProxySettings();
            proxy.Address = config.ProxyAddress;
            proxy.Port = config.ProxyPort;

            DateTime requestTimestamp = DateTime.Now;

            string url = HTTP.GetUrl(address, port, deviceName) + "current";

            result = Requests.Get(url, proxy, 2000, 1);
            if (result != null)
            {
                Logger.Log("Current Successful : " + url + " @ " + requestTimestamp.ToString("o"), LogLineType.Console);
            }
            else
            {
                Logger.Log("Current Error : " + url + " @ " + requestTimestamp.ToString("o"));
            }

            return result;
        }

        private void SendCurrentData(ReturnData returnData, DeviceConfiguration config)
        {
            //if (returnData != null)
            //{
                var data = new EventData();
                data.Id = "MTCONNECT_CURRENT";
                data.Data01 = config;
                data.Data02 = returnData;

                SendData?.Invoke(data);
            //}
        }

    }
}
