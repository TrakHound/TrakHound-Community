using System;
using System.Threading;

using TH_Configuration;
using TH_Database.Tables;
using TH_Global;
using TH_Global.Functions;

namespace TH_MTConnect.Plugin
{
    public partial class MTConnect
    {

        private System.Timers.Timer requestTimer;

        Components.ReturnData probeData;

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

                    var ac = Plugin.AgentConfiguration.Get(config);
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
            requestTimer.Enabled = false;
            RunRequests(configuration);
            if (requestTimer != null) requestTimer.Enabled = true;
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
                    var currentData = GetCurrent(ac);
                    if (currentData != null)
                    {
                        // Run a Sample request and get the returned data
                        var sampleData = GetSample(currentData.Header, ac, config);

                        // Send the Sample data to other plugins
                        SendSampleData(sampleData, config);

                        // Update the 'device_available' variable in the Variables table
                        bool available = GetAvailability(currentData);
                        UpdateAvailability(available, config);

                        // Send the Current data to other plugins
                        SendCurrentData(currentData, config);
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

            if (requestTimer != null) requestTimer.Enabled = false;
            requestTimer = null;
        }


        private void UpdateAvailability(bool available, Configuration config)
        {
            string tablePrefix;
            if (config.DatabaseId != null) tablePrefix = config.DatabaseId + "_";
            else tablePrefix = "";

            Variables.Update(config.Databases_Server, "device_available", available.ToString(), DateTime.Now, tablePrefix);
        }

        private bool GetAvailability(Streams.ReturnData data)
        {
            if (data.DeviceStreams.Count > 0)
            {
                var deviceStream = data.DeviceStreams[0];
                var avail = deviceStream.DataItems.Events.Find(x => x.Type.ToLower() == "availability");
                if (avail != null)
                {
                    if (avail.CDATA == "AVAILABLE") return true;
                }
            }

            return false;
        }

    }
}
