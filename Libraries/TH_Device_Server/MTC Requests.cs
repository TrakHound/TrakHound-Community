// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

using TH_Configuration;
using TH_Database.Tables;
using TH_MTConnect;

namespace TH_Device_Server
{
    public partial class Device_Server
    {
        ManualResetEvent stopRequests;

        void Requests_Start()
        {
            lastSequenceSampled = GetLastSequenceFromMySQL();
            agentInstanceId = GetAgentInstanceIdFromMySQL();

            stopRequests = new ManualResetEvent(false);

            while (!stopRequests.WaitOne(0, true))
            {
                var probeData = RunProbe();
                if (probeData != null)
                {
                    Plugins_Update_Probe(probeData);

                    while (!stopRequests.WaitOne(0, true))
                    {
                        var currentData = RunCurrent();
                        if (currentData != null)
                        {
                            Plugins_Update_Current(currentData);

                            var sampleData = RunSample(currentData.header);
                            if (sampleData != null)
                            {
                                Plugins_Update_Sample(sampleData);
                            }
                            else
                            {
                                Plugins_Update_Sample(null);
                            }
                        }

                        Thread.Sleep(configuration.Agent.Heartbeat);
                    }
                }
                else
                {
                    Thread.Sleep(configuration.Agent.Heartbeat);
                }
            }
        }

        void Requests_Stop()
        {
            if (stopRequests != null) stopRequests.Set();
        }


        TH_MTConnect.Components.ReturnData RunProbe()
        {
            TH_MTConnect.Components.ReturnData result = null;

            string address = configuration.Agent.IP_Address;
            int port = configuration.Agent.Port;
            string deviceName = configuration.Agent.Device_Name;

            // Set Proxy Settings
            var proxy = new HTTP.ProxySettings();
            proxy.Address = configuration.Agent.ProxyAddress;
            proxy.Port = configuration.Agent.ProxyPort;

            string url = HTTP.GetUrl(address, port, deviceName) + "probe";

            result = TH_MTConnect.Components.Requests.Get(url, proxy);
            if (result != null)
            {
                WriteToConsole("Probe Successful : " + url, ConsoleOutputType.Status);
            }
            else
            {
                WriteToConsole("Probe Error : " + url, ConsoleOutputType.Error);
            }

            return result;
        }

        TH_MTConnect.Streams.ReturnData RunCurrent()
        {
            TH_MTConnect.Streams.ReturnData result = null;

            string address = configuration.Agent.IP_Address;
            int port = configuration.Agent.Port;
            string deviceName = configuration.Agent.Device_Name;

            // Set Proxy Settings
            var proxy = new HTTP.ProxySettings();
            proxy.Address = configuration.Agent.ProxyAddress;
            proxy.Port = configuration.Agent.ProxyPort;

            string url = HTTP.GetUrl(address, port, deviceName) + "current";

            result = TH_MTConnect.Streams.Requests.Get(url, proxy);
            if (result != null)
            {
                WriteToConsole("Current Successful : " + url, ConsoleOutputType.Status);
            }
            else
            {
                WriteToConsole("Current Error : " + url, ConsoleOutputType.Error);
            }

            return result;
        }

        Int64 lastSequenceSampled = -1;
        Int64 agentInstanceId = -1;
        Int64 lastInstanceId = -1;
        const Int64 MaxSampleCount = 10000;
        bool startFromFirst = true;

        TH_MTConnect.Streams.ReturnData RunSample(TH_MTConnect.Header_Streams header)
        {
            TH_MTConnect.Streams.ReturnData result = null;

            string address = configuration.Agent.IP_Address;
            int port = configuration.Agent.Port;
            string deviceName = configuration.Agent.Device_Name;

            // Set Proxy Settings
            var proxy = new HTTP.ProxySettings();
            proxy.Address = configuration.Agent.ProxyAddress;
            proxy.Port = configuration.Agent.ProxyPort;

            UpdateAgentInstanceID(header);

            SampleInfo info = GetSampleInfo(header);
            if (info != null)
            {
                if (info.Count > 0)
                {
                    string url = HTTP.GetUrl(address, port, deviceName) + "sample?from=" + info.From.ToString() + "&count=" + info.Count.ToString();

                    result = TH_MTConnect.Streams.Requests.Get(url, proxy);
                    if (result != null)
                    {
                        WriteToConsole("Sample Successful : " + url, ConsoleOutputType.Status);
                    }
                    else
                    {
                        WriteToConsole("Sample Error : " + url, ConsoleOutputType.Error);
                    }
                }
                else WriteToConsole("Sample Skipped", ConsoleOutputType.Status);
            }

            return result;
        }

        Int64 GetLastSequenceFromMySQL()
        {
            Int64 Result = -1;

            Variables.VariableData vd = Variables.Get(configuration.Databases_Server, "last_sequence_sampled", TablePrefix);
            if (vd != null)
            {
                Int64.TryParse(vd.value, out Result);
            }

            return Result;
        }

        Int64 GetAgentInstanceIdFromMySQL()
        {
            Int64 Result = -1;

            Variables.VariableData vd = Variables.Get(configuration.Databases_Server, "agent_instanceid", TablePrefix);
            if (vd != null)
            {
                Int64.TryParse(vd.value, out Result);
            }

            return Result;
        }

        void UpdateAgentInstanceID(TH_MTConnect.Header_Streams header)
        {
            lastInstanceId = agentInstanceId;
            agentInstanceId = header.instanceId;
            if (lastInstanceId != agentInstanceId)
            {
                Variables.Update(configuration.Databases_Server, "Agent_InstanceID", agentInstanceId.ToString(), header.creationTime, TablePrefix);
            }      
        }

        class SampleInfo
        {
            public Int64 From { get; set; }
            public Int64 Count { get; set; }
        }

        SampleInfo GetSampleInfo(TH_MTConnect.Header_Streams header)
        {
            SampleInfo result = new SampleInfo();

            //Get Sequence Number to use -----------------------
            Int64 first = header.firstSequence;
            if (!startFromFirst)
            {
                first = header.lastSequence;
                startFromFirst = true;
            }
            else if (lastInstanceId == agentInstanceId && lastSequenceSampled > 0 && lastSequenceSampled >= header.firstSequence)
            {
                first = lastSequenceSampled + 1;
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
            Int64 last = header.lastSequence;

            // Calculate Sample count
            Int64 sampleCount = last - first;
            if (sampleCount > MaxSampleCount)
            {
                sampleCount = MaxSampleCount;
                last = first + MaxSampleCount;
            }

            result.Count = sampleCount;

            // Update Last Sequence Sampled for the subsequent samples
            if (lastSequenceSampled != last)
            {
                Variables.Update(configuration.Databases_Server, "Last_Sequence_Sampled", last.ToString(), header.creationTime, TablePrefix);
            }
            lastSequenceSampled = last;

            return result;
        }

    }

}
