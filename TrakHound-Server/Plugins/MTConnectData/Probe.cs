// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect.Application.Components;
using System;

using TrakHound;
using TrakHound.API;
using TrakHound.Configurations;
using TrakHound.Logging;

namespace TrakHound_Server.Plugins.MTConnectData
{
    public partial class Plugin
    {
        private ReturnData GetProbe(Data.AgentInfo config)
        {
            ReturnData result = null;

            string address = config.Address;
            int port = config.Port;
            string deviceName = config.DeviceName;

            // Set Proxy Settings
            var proxy = new MTConnect.HTTP.ProxySettings();
            proxy.Address = config.ProxyAddress;
            proxy.Port = config.ProxyPort;

            DateTime requestTimestamp = DateTime.Now;

            string url = MTConnect.HTTP.GetUrl(address, port, deviceName) + "probe";

            result = Requests.Get(url, proxy, 2000, 1);
            if (result != null)
            {
                Logger.Log("Probe Successful : " + url + " @ " + requestTimestamp.ToString("o"), LogLineType.Console);
            }
            else
            {
                Logger.Log("Probe Error : " + url + " @ " + requestTimestamp.ToString("o"));
            }

            return result;
        }

        private void SendProbeData(ReturnData returnData, DeviceConfiguration config)
        {
            SendProbeDataItems(returnData, config);

            if (returnData != null && returnData.Header != null)
            {
                if (returnData.Header.InstanceId != agentInstanceId)
                {
                    SendAgentReset(config);
                }

                agentInstanceId = returnData.Header.InstanceId;
            }
        }

        private void SendProbeDataItems(ReturnData returnData, DeviceConfiguration config)
        {
            var data = new EventData(this);
            data.Id = "MTCONNECT_PROBE";
            data.Data01 = config;
            data.Data02 = returnData;

            SendData?.Invoke(data);
        }

    }
}
