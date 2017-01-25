// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System.Collections.Generic;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;

using MTConnectDevices = MTConnect.MTConnectDevices;
using MTConnectStreams = MTConnect.MTConnectStreams;

namespace TrakHound_Server.Plugins.Status
{
    public class Plugin : IServerPlugin
    {

        public string Name { get { return "MTConnect Status"; } }

        private DeviceConfiguration configuration;

        private List<StatusInfo> statusInfos = new List<StatusInfo>();


        public void Initialize(DeviceConfiguration config)
        {
            configuration = config;
        }

        public void GetSentData(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "MTCONNECT_PROBE")
                {
                    if (data.Data02 != null)
                    {
                        var infos = StatusInfo.GetList((MTConnectDevices.Document)data.Data02);
                        if (infos.Count > 0)
                        {
                            statusInfos = infos;
                        }
                    }
                    else
                    {
                        SendStatusData(null);
                    }
                }
                else if (data.Id == "MTCONNECT_CURRENT" && data.Data02 != null)
                {
                    StatusInfo.ProcessList((MTConnectStreams.Document)data.Data02, statusInfos);

                    SendStatusData(statusInfos);
                }
            }
        }

        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing() { }


        private void SendStatusData(List<StatusInfo> infos)
        {
            var data = new EventData(this);
            data.Id = "MTCONNECT_STATUS";
            data.Data01 = configuration;
            data.Data02 = infos;

            SendData?.Invoke(data);
        }

    }
}
