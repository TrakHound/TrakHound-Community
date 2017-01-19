// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins.Server;

namespace TrakHound_Server.Plugins.MTConnectData
{
    public partial class Plugin : IServerPlugin
    {
        public string Name { get { return "MTConnect"; } }

        public void Initialize(DeviceConfiguration config)
        {
            configuration = config;

            var agentData = AgentData.Load(config);
            if (agentData != null)
            {
                agentInstanceId = agentData.InstanceId;
                lastInstanceId = agentData.InstanceId;
                lastSequenceSampled = agentData.LastSequence;

                System.Console.WriteLine(config.UniqueId + " :: " + agentInstanceId + " : " + lastSequenceSampled);
            }

            Start(config);
        }

        public void GetSentData(EventData data) { }

        public event SendData_Handler SendData;

        public void Starting() { }

        public void Closing()
        {
            Stop();
        }

    }
}
