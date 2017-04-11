// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using NLog;
using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins.Server;
using MTConnectDevices = MTConnect.MTConnectDevices;
using MTConnectStreams = MTConnect.MTConnectStreams;

namespace TrakHound_Server.Plugins.Instances
{
    public partial class Plugin : IServerPlugin
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public string Name { get { return "Instances"; } }

        public void Initialize(DeviceConfiguration config)
        {
            configuration = config;
        }

        public void GetSentData(EventData data)
        {
            if (data != null && data.Id != null)
            {
                switch (data.Id)
                {
                    case "MTCONNECT_PROBE":

                        if (data.Data02 == null)
                        {
                            SendInstanceData(configuration, null);
                            SendCurrentInstanceData(configuration, null);
                        }
                        
                        break;

                    case "MTCONNECT_CURRENT":

                        if (data.Data02 != null) Update_Current((MTConnectStreams.Document)data.Data02);

                        break;

                    case "MTCONNECT_SAMPLE":

                        if (data.Data02 != null) Update_Sample((MTConnectStreams.Document)data.Data02);

                        break;
                }
            }
        }

        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing() { }

    }
}
