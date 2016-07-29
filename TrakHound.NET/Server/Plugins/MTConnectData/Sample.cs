// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

//using TrakHound.Configurations;
using TrakHound.Configurations;
using TrakHound.Databases.Tables;
using TrakHound.Plugins;

using TrakHound.Logging;

using MTConnect;
using MTConnect.Application.Streams;
using MTConnect.Application.Headers;

namespace TrakHound.Server.Plugins.MTConnectData
{
    public partial class Plugin
    {
        // Implement a more efficient way of collecting samples using xpath
        // agent.mtconnect.org/current?path=//*[@id='p2' or @id='x2']


        private long lastSequenceSampled = -1;
        private long agentInstanceId = -1;
        private long lastInstanceId = -1;
        private const long MaxSampleCount = 10000;
        private bool startFromFirst = false;

        private ReturnData GetSample(Streams header, AgentConfiguration ac, DeviceConfiguration config)
        {
            ReturnData result = null;

            string address = ac.IP_Address;
            int port = ac.Port;
            string deviceName = ac.Device_Name;

            // Set Proxy Settings
            var proxy = new HTTP.ProxySettings();
            proxy.Address = ac.ProxyAddress;
            proxy.Port = ac.ProxyPort;

            UpdateAgentInstanceID(header, config);

            SampleInfo info = GetSampleInfo(header, config);
            if (info != null)
            {
                if (info.Count > 0)
                {
                    DateTime requestTimestamp = DateTime.Now;

                    string url = HTTP.GetUrl(address, port, deviceName) + "sample?from=" + info.From.ToString() + "&count=" + info.Count.ToString();

                    result = Requests.Get(url, proxy);
                    if (result != null)
                    {
                        Logger.Log("Sample Successful : " + url + " @ " + requestTimestamp.ToString("o"));
                    }
                    else
                    {
                        Logger.Log("Sample Error : " + url + " @ " + requestTimestamp.ToString("o"));
                    }
                }
                else Logger.Log("No New Data :: Sample Skipped");
            }

            return result;
        }

        private void SendSampleData(ReturnData returnData, DeviceConfiguration config)
        {
            if (returnData != null)
            {
                var data = new EventData();
                data.Id = "MTConnect_Sample";
                data.Data01 = config;
                data.Data02 = returnData;

                if (SendData != null) SendData(data);
            }
        }


        private long GetLastSequenceFromMySQL(DeviceConfiguration config)
        {
            long result = -1;

            string tablePrefix;
            if (config.DatabaseId != null) tablePrefix = config.DatabaseId + "_";
            else tablePrefix = "";

            Variables.VariableData vd = Variables.Get(config.Databases_Server, "last_sequence_sampled", tablePrefix);
            if (vd != null)
            {
                long.TryParse(vd.Value, out result);
            }

            return result;
        }

        private long GetAgentInstanceIdFromMySQL(DeviceConfiguration config)
        {
            long result = -1;

            string tablePrefix;
            if (config.DatabaseId != null) tablePrefix = config.DatabaseId + "_";
            else tablePrefix = "";

            Variables.VariableData vd = Variables.Get(config.Databases_Server, "agent_instanceid", tablePrefix);
            if (vd != null)
            {
                long.TryParse(vd.Value, out result);
            }

            return result;
        }

        private void UpdateAgentInstanceID(Streams header, DeviceConfiguration config)
        {
            lastInstanceId = agentInstanceId;
            agentInstanceId = header.InstanceId;
            if (lastInstanceId != agentInstanceId)
            {
                string tablePrefix;
                if (config.DatabaseId != null) tablePrefix = config.DatabaseId + "_";
                else tablePrefix = "";

                Variables.Update(config.Databases_Server, "Agent_InstanceID", agentInstanceId.ToString(), header.CreationTime, tablePrefix);
            }
        }

        private class SampleInfo
        {
            public long From { get; set; }
            public long Count { get; set; }
        }

        private SampleInfo GetSampleInfo(Streams header, DeviceConfiguration config)
        {
            var result = new SampleInfo();

            //Get Sequence Number to use -----------------------
            long first = header.FirstSequence;
            if (!startFromFirst)
            {
                first = header.LastSequence;
                startFromFirst = true;
            }
            else if (lastInstanceId == agentInstanceId && lastSequenceSampled > 0 && lastSequenceSampled >= header.FirstSequence)
            {
                //first = lastSequenceSampled + 1;
                first = lastSequenceSampled;
            }
            else if (first > 0)
            {
                // Increment some sequences since the Agent might change the first sequence
                // before the Sample request gets read
                // (should be fixed in Agent to automatically read the first 'available' sequence
                // instead of returning an error)
                first += 20;
            }

            result.From = first;

            // Get Last Sequence Number available from Header
            long last = header.LastSequence;

            // Calculate Sample count
            long sampleCount = last - first;
            if (sampleCount > MaxSampleCount)
            {
                sampleCount = MaxSampleCount;
                last = first + MaxSampleCount;
            }

            result.Count = sampleCount;

            // Update Last Sequence Sampled for the subsequent samples
            if (lastSequenceSampled != last)
            {
                string tablePrefix;
                if (config.DatabaseId != null) tablePrefix = config.DatabaseId + "_";
                else tablePrefix = "";

                Variables.Update(config.Databases_Server, "Last_Sequence_Sampled", last.ToString(), header.CreationTime, tablePrefix);
            }
            lastSequenceSampled = last;

            return result;
        }

    }
}
