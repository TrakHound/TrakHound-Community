// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Threading;

using TrakHound;
using TrakHound.Configurations;

namespace TrakHound_Server.Plugins.MTConnectData
{
    public partial class Plugin
    {
        private DeviceConfiguration configuration;

        private bool started = false;
        private bool verbose = true;
        private bool first = true;
        private object _lock = new object();

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

                var ac = config.Agent;

                ThreadPool.QueueUserWorkItem((o) =>
                {
                    do
                    {
                         RunRequests(config);
                        lock (_lock) first = false;

                    } while (!stop.WaitOne(ac.Heartbeat, true));
                });
            }
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
                    if (currentData != null)
                    {
                        if (currentData != null && currentData.Header != null)
                        {
                            if (first) UpdateAgentData(currentData.Header.InstanceId, currentData.Header.LastSequence);
                            else UpdateAgentData(currentData.Header.InstanceId);
                        }

                        // Send the Current data to other plugins
                        SendCurrentData(currentData, config);

                        // Run a Sample request and get the returned data
                        var sampleData = GetSample(currentData.Header, ac, config);

                        // Send the Sample data to other plugins
                        SendSampleData(sampleData, config);
                    }
                    else
                    {
                        probeData = null;
                        UpdateAvailability(false, config);
                    }
                } else UpdateAvailability(false, config);
            } else UpdateAvailability(false, config);
        }

        private void UpdateAvailability(bool available, DeviceConfiguration config)
        {
            var data = new EventData(this);
            data.Id = "DEVICE_AVAILABILITY";
            data.Data01 = config;
            data.Data02 = available;
            SendData?.Invoke(data);
        }

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

        private void SendAgentReset(DeviceConfiguration config)
        {
            System.Console.WriteLine(config.UniqueId + " :: Agent Reset");

            var data = new EventData(this);
            data.Id = "MTCONNECT_AGENT_RESET";
            data.Data01 = config;

            SendData?.Invoke(data);
        }

    }
}
