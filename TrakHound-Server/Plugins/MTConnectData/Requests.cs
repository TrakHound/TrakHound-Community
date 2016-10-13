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
                        if (currentData != null) UpdateAgentData(currentData.Header);

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

        private void UpdateAgentData(MTConnect.Application.Headers.Devices header)
        {
            if (header != null)
            {
                if (header.InstanceId != agentInstanceId)
                {
                    SendAgentReset(configuration);
                    AgentData.Save(header, configuration);
                }

                agentInstanceId = header.InstanceId;
            }
        }

        private void UpdateAgentData(MTConnect.Application.Headers.Streams header)
        {
            if (header != null)
            {
                if (header.InstanceId != agentInstanceId)
                {
                    SendAgentReset(configuration);
                    AgentData.Save(header, configuration);
                }

                agentInstanceId = header.InstanceId;
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
