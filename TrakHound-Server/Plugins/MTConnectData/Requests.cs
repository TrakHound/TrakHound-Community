// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MTConnect;
using MTConnect.Application.Headers;

using System;
using System.Threading;

using TrakHound;
using TrakHound.API;
using TrakHound.Configurations;
using TrakHound.Logging;

namespace TrakHound_Server.Plugins.MTConnectData
{
    public partial class Plugin
    {
        private DeviceConfiguration configuration;

        private bool started = false;
        private bool verbose = true;
        private bool first = true;
        private object _lock = new object();

        private long lastSequenceSampled = -1;
        private long agentInstanceId = -1;
        private long lastInstanceId = -1;
        private const long MaxSampleCount = 5000;
        private bool startFromFirst = false;

        private ManualResetEvent stop;

        private MTConnect.Application.Components.ReturnData probeData;
        private MTConnect.Application.Streams.ReturnData currentData;

        private void Start(DeviceConfiguration config)
        {
            if (!started)
            {
                stop = new ManualResetEvent(false);
                started = true;
                probeData = null;

                var requestsThread = new Thread(new ParameterizedThreadStart(Worker));
                requestsThread.Start(config);
            }
        }

        private void Worker(object o)
        {
            var config = (DeviceConfiguration)o;
            var ac = config.Agent;

            do
            {
                RunRequests(config);
                lock (_lock) first = false;

            } while (!stop.WaitOne(ac.Heartbeat, true));

            Console.WriteLine("Requests Worker Stopped");
        }

        private void Stop()
        {
            if (stop != null) stop.Set();

            started = false;
        }
        
        private void RunRequests(DeviceConfiguration config)
        {
            var ac = config.Agent;
            if (ac != null)
            {
                // Run a Probe request and get the returned data
                if (probeData == null)
                {
                    probeData = GetProbe(ac);

                    // Send the Probe data to other plugins
                    SendProbeData(probeData, config);
                }

                if (probeData != null)
                {
                    // Run a Current request and get the returned data
                    currentData = GetCurrent(ac);
                    if (currentData != null && currentData.Header != null)
                    {
                        // Send the Current data to other plugins
                        SendCurrentData(currentData, config);

                        //Run a Sample request and get the returned data
                        var sampleData = GetSample(currentData.Header, ac, config);

                        //Send the Sample data to other plugins
                        if (sampleData != null) SendSampleData(sampleData, config);

                        UpdateAgentData(currentData.Header.InstanceId, currentData.Header.LastSequence);
                    }
                    else
                    {
                        probeData = null;
                        SendAvailability(false, ac.Heartbeat, config);
                    }
                } else SendAvailability(false, ac.Heartbeat, config);
            }
        }


        #region "MTConnect Requests"

        private MTConnect.Application.Components.ReturnData GetProbe(Data.AgentInfo config)
        {
            MTConnect.Application.Components.ReturnData result = null;

            string address = config.Address;
            int port = config.Port;
            string deviceName = config.DeviceName;

            // Set Proxy Settings
            var proxy = new HTTP.ProxySettings();
            proxy.Address = config.ProxyAddress;
            proxy.Port = config.ProxyPort;

            DateTime requestTimestamp = DateTime.Now;

            string url = HTTP.GetUrl(address, port, deviceName) + "probe";

            result = MTConnect.Application.Components.Requests.Get(url, proxy, 2000, 1);
            if (result != null)
            {
                if (verbose) Logger.Log("Probe Successful : " + url + " @ " + requestTimestamp.ToString("o"), LogLineType.Console);
            }
            else
            {
                Logger.Log("Probe Error : " + url + " @ " + requestTimestamp.ToString("o") + " : Retrying in " + (config.Heartbeat / 1000) + " sec..");
            }

            return result;
        }

        private MTConnect.Application.Streams.ReturnData GetCurrent(Data.AgentInfo config)
        {
            MTConnect.Application.Streams.ReturnData result = null;

            string address = config.Address;
            int port = config.Port;
            string deviceName = config.DeviceName;

            // Set Proxy Settings
            var proxy = new HTTP.ProxySettings();
            proxy.Address = config.ProxyAddress;
            proxy.Port = config.ProxyPort;

            DateTime requestTimestamp = DateTime.Now;

            string url = HTTP.GetUrl(address, port, deviceName) + "current";

            result = MTConnect.Application.Streams.Requests.Get(url, proxy, 2000, 1);
            if (result != null)
            {
                if (verbose) Logger.Log("Current Successful : " + url + " @ " + requestTimestamp.ToString("o") + " : " + result.Header.LastSequence, LogLineType.Console);
            }
            else
            {
                Logger.Log("Current Error : " + url + " @ " + requestTimestamp.ToString("o") + " : Retrying in " + (config.Heartbeat / 1000) + " sec..");
            }

            return result;
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

            return result;
        }

        private MTConnect.Application.Streams.ReturnData GetSample(Streams header, Data.AgentInfo ac, DeviceConfiguration config)
        {
            MTConnect.Application.Streams.ReturnData result = null;

            string address = ac.Address;
            int port = ac.Port;
            string deviceName = ac.DeviceName;

            // Set Proxy Settings
            var proxy = new HTTP.ProxySettings();
            proxy.Address = ac.ProxyAddress;
            proxy.Port = ac.ProxyPort;

            SampleInfo info = GetSampleInfo(header, config);
            if (info != null)
            {
                if (info.Count > 0)
                {
                    DateTime requestTimestamp = DateTime.Now;

                    string url = HTTP.GetUrl(address, port, deviceName) + "sample?from=" + info.From.ToString() + "&count=" + info.Count.ToString();

                    result = MTConnect.Application.Streams.Requests.Get(url, proxy, ac.Heartbeat / 2, 2);
                    if (result != null)
                    {
                        UpdateAgentData(header.InstanceId, info.From + info.Count);

                        if (verbose) Logger.Log("Sample Successful : " + url + " @ " + requestTimestamp.ToString("o"), LogLineType.Console);
                    }
                    else
                    {
                        Logger.Log("Sample Error : " + url + " @ " + requestTimestamp.ToString("o"));
                    }
                }
                else UpdateAgentData(header.InstanceId, header.LastSequence);
            }

            return result;
        }

        #endregion

        #region "Send Data"

        private void SendAgentReset(DeviceConfiguration config)
        {
            Console.WriteLine(config.UniqueId + " :: Agent Reset");

            var data = new EventData(this);
            data.Id = "MTCONNECT_AGENT_RESET";
            data.Data01 = config;

            SendData?.Invoke(data);
        }

        private void SendProbeData(MTConnect.Application.Components.ReturnData returnData, DeviceConfiguration config)
        {
            if (returnData != null && returnData.Header != null)
            {
                if (returnData.Header.InstanceId != agentInstanceId)
                {
                    SendAgentReset(config);
                }

                agentInstanceId = returnData.Header.InstanceId;
            }

            var data = new EventData(this);
            data.Id = "MTCONNECT_PROBE";
            data.Data01 = config;
            data.Data02 = returnData;

            SendData?.Invoke(data);
        }

        private void SendCurrentData(MTConnect.Application.Streams.ReturnData returnData, DeviceConfiguration config)
        {
            var data = new EventData(this);
            data.Id = "MTCONNECT_CURRENT";
            data.Data01 = config;
            data.Data02 = returnData;

            SendData?.Invoke(data);
        }

        private void SendSampleData(MTConnect.Application.Streams.ReturnData returnData, DeviceConfiguration config)
        {
            var data = new EventData(this);
            data.Id = "MTCONNECT_SAMPLE";
            data.Data01 = config;
            data.Data02 = returnData;

            SendData?.Invoke(data);
        }

        private void SendAvailability(bool available, int delay, DeviceConfiguration config)
        {
            var data = new EventData(this);
            data.Id = "DEVICE_AVAILABILITY";
            data.Data01 = config;
            data.Data02 = available;
            data.Data03 = delay;
            SendData?.Invoke(data);
        }

        #endregion

        #region "Saved Agent Data"

        private void UpdateAgentData(long instanceId)
        {
            if (instanceId != agentInstanceId)
            {
                SendAgentReset(configuration);
                AgentData.Save(configuration, instanceId);

                agentInstanceId = instanceId;
            }
        }

        private void UpdateAgentData(long instanceId, long sequence)
        {
            if (instanceId != agentInstanceId || sequence != lastSequenceSampled)
            {
                AgentData.Save(configuration, instanceId, sequence);
                agentInstanceId = instanceId;
                lastSequenceSampled = sequence;
                if (instanceId != agentInstanceId) SendAgentReset(configuration);
            }
        }

        #endregion
    }
}
