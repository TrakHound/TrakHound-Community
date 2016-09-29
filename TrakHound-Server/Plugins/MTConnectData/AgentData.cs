// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

using TrakHound.Configurations;
using TrakHound.Tools.Web;

namespace TrakHound_Server.Plugins.MTConnectData
{
    class AgentData
    {
        public string UniqueId { get; set; }

        public long InstanceId { get; set; }

        public static void Save(MTConnect.Application.Headers.Devices header, DeviceConfiguration config)
        {
            string json = Properties.Settings.Default.StoredAgentData;
            if (!string.IsNullOrEmpty(json))
            {
                var agentDatas = JSON.ToType<List<AgentData>>(json);
                if (agentDatas != null)
                {
                    int i = agentDatas.FindIndex(o => o.UniqueId == config.UniqueId);
                    if (i < 0)
                    {
                        var agentData = new AgentData();
                        agentData.UniqueId = config.UniqueId;
                        agentDatas.Add(agentData);
                        i = agentDatas.FindIndex(o => o.UniqueId == config.UniqueId);
                    }

                    agentDatas[i].InstanceId = header.InstanceId;

                    Properties.Settings.Default.StoredAgentData = JSON.FromList<AgentData>(agentDatas);
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                var agentDatas = new List<AgentData>();

                var agentData = new AgentData();
                agentData.UniqueId = config.UniqueId;
                agentData.InstanceId = header.InstanceId;
                agentDatas.Add(agentData);

                Properties.Settings.Default.StoredAgentData = JSON.FromList<AgentData>(agentDatas);
                Properties.Settings.Default.Save();
            }
        }

        public static void Save(MTConnect.Application.Headers.Streams header, DeviceConfiguration config)
        {
            string json = Properties.Settings.Default.StoredAgentData;
            if (!string.IsNullOrEmpty(json))
            {
                var agentDatas = JSON.ToType<List<AgentData>>(json);
                if (agentDatas != null)
                {
                    int i = agentDatas.FindIndex(o => o.UniqueId == config.UniqueId);
                    if (i < 0)
                    {
                        var agentData = new AgentData();
                        agentData.UniqueId = config.UniqueId;
                        agentDatas.Add(agentData);
                        i = agentDatas.FindIndex(o => o.UniqueId == config.UniqueId);
                    }

                    agentDatas[i].InstanceId = header.InstanceId;

                    Properties.Settings.Default.StoredAgentData = JSON.FromList<AgentData>(agentDatas);
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                var agentDatas = new List<AgentData>();

                var agentData = new AgentData();
                agentData.UniqueId = config.UniqueId;
                agentData.InstanceId = header.InstanceId;
                agentDatas.Add(agentData);

                Properties.Settings.Default.StoredAgentData = JSON.FromList<AgentData>(agentDatas);
                Properties.Settings.Default.Save();
            }
        }

        public static long Load(DeviceConfiguration config)
        {
            string json = Properties.Settings.Default.StoredAgentData;
            if (!string.IsNullOrEmpty(json))
            {
                var agentDatas = JSON.ToType<List<AgentData>>(json);
                if (agentDatas != null)
                {
                    var agentData = agentDatas.Find(o => o.UniqueId == config.UniqueId);
                    if (agentData != null) return agentData.InstanceId;
                }
            }

            return 0;
        }
    }
}
