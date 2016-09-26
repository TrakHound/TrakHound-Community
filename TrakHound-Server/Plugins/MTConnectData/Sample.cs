// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Headers;
using MTConnect.Application.Streams;
using System;

using TrakHound;
using TrakHound.API;
using TrakHound.Configurations;
using TrakHound.Logging;
using TrakHound.Plugins;

namespace TrakHound_Server.Plugins.MTConnectData
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

        private ReturnData GetSample(Streams header, Data.AgentInfo ac, DeviceConfiguration config)
        {
            ReturnData result = null;

            string address = ac.Address;
            int port = ac.Port;
            string deviceName = ac.DeviceName;

            // Set Proxy Settings
            var proxy = new HTTP.ProxySettings();
            proxy.Address = ac.ProxyAddress;
            proxy.Port = ac.ProxyPort;

            lastInstanceId = agentInstanceId;
            agentInstanceId = header.InstanceId;

            SampleInfo info = GetSampleInfo(header, config);
            if (info != null)
            {
                if (info.Count > 0)
                {
                    DateTime requestTimestamp = DateTime.Now;

                    string url = HTTP.GetUrl(address, port, deviceName) + "sample?from=" + info.From.ToString() + "&count=" + info.Count.ToString();

                    result = Requests.Get(url, proxy, 5000, 2);
                    if (result != null)
                    {
                        Logger.Log("Sample Successful : " + url + " @ " + requestTimestamp.ToString("o"), LogLineType.Console);
                    }
                    else
                    {
                        Logger.Log("Sample Error : " + url + " @ " + requestTimestamp.ToString("o"));
                    }
                }
                else Logger.Log("No New Data :: Sample Skipped", LogLineType.Console);
            }

            return result;
        }

        private void SendSampleData(ReturnData returnData, DeviceConfiguration config)
        {
            //if (returnData != null)
            //{
                var data = new EventData(this);
                data.Id = "MTCONNECT_SAMPLE";
                data.Data01 = config;
                data.Data02 = returnData;

                SendData?.Invoke(data);
            //}
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

            lastSequenceSampled = last;

            return result;
        }

    }
}
