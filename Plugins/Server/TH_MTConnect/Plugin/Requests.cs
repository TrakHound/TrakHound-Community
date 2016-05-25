// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using TH_Configuration;
using TH_Database.Tables;
using TH_Plugins;

using MTConnect.Application;

namespace TH_MTConnect.Plugin
{
    public partial class Plugin
    {

        private System.Timers.Timer requestTimer;

        MTConnect.Application.Components.ReturnData probeData;
        MTConnect.Application.Streams.ReturnData currentData;

        private void Start(Configuration config)
        {
            if (!started)
            {
                started = true;

                if (requestTimer == null)
                {
                    probeData = null;

                    lastSequenceSampled = GetLastSequenceFromMySQL(config);
                    agentInstanceId = GetAgentInstanceIdFromMySQL(config);

                    var ac = AgentConfiguration.Get(config);
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
            timer.Enabled = true;
        }

        private void RunRequests(Configuration config)
        {
            var ac = AgentConfiguration.Get(config);
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
            started = false;

            TH_Global.Logger.Log("MTConnect Requests Stopped");

            if (requestTimer != null) requestTimer.Enabled = false;
            requestTimer = null;
        }


        private void UpdateAvailability(bool available, Configuration config)
        {
            string tablePrefix;
            if (config.DatabaseId != null) tablePrefix = config.DatabaseId + "_";
            else tablePrefix = "";

            Variables.Update(config.Databases_Server, "device_available", available.ToString(), DateTime.Now, tablePrefix);

            var data = new EventData();
            data.Id = "DeviceAvailability";
            data.Data01 = config;
            data.Data02 = available;
            if (SendData != null) SendData(data);
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

    }
}
