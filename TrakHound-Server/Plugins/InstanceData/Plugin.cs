// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;

namespace TrakHound_Server.Plugins.Instances
{
    public partial class Plugin : IServerPlugin
    {

        public string Name { get { return "Instances"; } }

        public void Initialize(DeviceConfiguration config)
        {
            var ic = Configuration.Read(config.Xml);
            if (ic != null)
            {
                config.CustomClasses.Add(ic);
            }

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

                        if (data.Data02 != null) Update_Current((MTConnect.Application.Streams.ReturnData)data.Data02);

                        break;

                    case "MTCONNECT_SAMPLE":

                        if (data.Data02 != null) Update_Sample((MTConnect.Application.Streams.ReturnData)data.Data02);

                        break;
                }
            }
        }

        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing() { }

    }
}
