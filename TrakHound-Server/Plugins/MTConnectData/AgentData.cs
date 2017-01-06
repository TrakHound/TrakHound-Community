// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

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

        public long LastSequence { get; set; }


        public static void Save(DeviceConfiguration config, long instanceId)
        {
            Save(config, instanceId, -1);
        }

        public static void Save(DeviceConfiguration config, long instanceId, long sequence)
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

                    agentDatas[i].InstanceId = instanceId;
                    if (sequence >= 0) agentDatas[i].LastSequence = sequence;

                    Properties.Settings.Default.StoredAgentData = JSON.FromList<AgentData>(agentDatas);
                    Properties.Settings.Default.Save();
                }
                else System.Console.WriteLine(config.UniqueId + " :: Error Saving AgentData");
            }
            else
            {
                var agentDatas = new List<AgentData>();

                var agentData = new AgentData();
                agentData.UniqueId = config.UniqueId;
                agentData.InstanceId = instanceId;
                if (sequence >= 0) agentData.LastSequence = sequence;
                agentDatas.Add(agentData);

                Properties.Settings.Default.StoredAgentData = JSON.FromList<AgentData>(agentDatas);
                Properties.Settings.Default.Save();
            }
        }

        public static AgentData Load(DeviceConfiguration config)
        {
            string json = Properties.Settings.Default.StoredAgentData;
            if (!string.IsNullOrEmpty(json))
            {
                var agentDatas = JSON.ToType<List<AgentData>>(json);
                if (agentDatas != null)
                {
                    var agentData = agentDatas.Find(o => o.UniqueId == config.UniqueId);
                    if (agentData != null) return agentData;
                }
            }

            return null;
        }

    }
}
