// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Threading;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Logging;

namespace TrakHound_Server.Plugins.MTConnectData
{
    public partial class Plugin
    {

        private System.Timers.Timer requestTimer;

        private ManualResetEvent stop;

        private MTConnect.Application.Components.ReturnData probeData;
        private MTConnect.Application.Streams.ReturnData currentData;

        private void Start(DeviceConfiguration config)
        {
            if (!started)
            {
                stop = new ManualResetEvent(false);

                started = true;

                if (requestTimer == null)
                {
                    probeData = null;

                    var ac = config.Agent;
                    if (ac != null)
                    {
                        requestTimer = new System.Timers.Timer();
                        requestTimer.Interval = ac.Heartbeat;
                        requestTimer.Elapsed += RequestTimer_Elapsed;
                        requestTimer.Enabled = true;
                    }
                }
            }
        }

        private void RequestTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var timer = (System.Timers.Timer)sender;

            timer.Enabled = false;
            RunRequests(configuration);
            if (stop != null && !stop.WaitOne(0, true)) timer.Enabled = true;
        }

        private void RunRequests(DeviceConfiguration config)
        {
            //var ac = Configuration.Get(config);
            var ac = config.Agent;
            if (ac != null)
            {
                // Run a Probe request and get the returned data
                if (probeData == null)
                {
                    probeData = GetProbe(ac);

                    if (probeData != null) UpdateAgentData(probeData.Header);

                    // Send the Probe data to other plugins
                    SendProbeData(probeData, config);
                }
                if (probeData != null)
                {
                    // Run a Current request and get the returned data
                    currentData = GetCurrent(ac);
                    if (currentData != null)
                    {
                        // Send the Current data to other plugins
                        SendCurrentData(currentData, config);

                        // Run a Sample request and get the returned data
                        var sampleData = GetSample(currentData.Header, ac, config);

                        // Send the Sample data to other plugins
                        SendSampleData(sampleData, config);

                        // Update the 'device_available' variable in the Variables table
                        bool available = GetAvailability(currentData);
                        UpdateAvailability(available, config);
                    }
                    else
                    {
                        probeData = null;
                        UpdateAvailability(false, config);
                    }
                }
                else
                {
                    UpdateAvailability(false, config);
                }
            }
        }

        private void Stop()
        {
            stop.Set();
            started = false;

            Logger.Log("MTConnect Requests Stopped");

            if (requestTimer != null) requestTimer.Enabled = false;
            requestTimer = null;
        }


        private void UpdateAvailability(bool available, DeviceConfiguration config)
        {
            var data = new EventData(this);
            data.Id = "DEVICE_AVAILABILITY";
            data.Data01 = config;
            data.Data02 = available;
            SendData?.Invoke(data);
        }

        private bool GetAvailability(MTConnect.Application.Streams.ReturnData data)
        {
            if (data.DeviceStreams.Count > 0)
            {
                var deviceStream = data.DeviceStreams[0];
                var avail = deviceStream.GetAllDataItems().Find(x => x.Type.ToLower() == "availability");
                if (avail != null)
                {
                    if (avail.CDATA == "AVAILABLE") return true;
                }
            }

            return false;
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
