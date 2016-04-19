using System;

using TH_Configuration;
using TH_Database.Tables;
//using TH_Global;
using TH_Plugins;

using TH_MTConnect.Streams;

namespace TH_MTConnect.Plugin
{
    public partial class MTConnect
    {
        // Implement a more efficient way of collecting samples using xpath
        // agent.mtconnect.org/current?path=//*[@id='p2' or @id='x2']


        private Int64 lastSequenceSampled = -1;
        private Int64 agentInstanceId = -1;
        private Int64 lastInstanceId = -1;
        private const Int64 MaxSampleCount = 10000;
        private bool startFromFirst = false;

        private ReturnData GetSample(Header_Streams header, AgentConfiguration ac, Configuration config)
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
                    string url = HTTP.GetUrl(address, port, deviceName) + "sample?from=" + info.From.ToString() + "&count=" + info.Count.ToString();

                    result = Requests.Get(url, proxy);
                    if (result != null)
                    {
                        TH_Global.Logger.Log("Sample Successful : " + url);
                    }
                    else
                    {
                        TH_Global.Logger.Log("Sample Error : " + url);
                    }
                }
                else TH_Global.Logger.Log("Sample Skipped");
            }

            return result;
        }

        private void SendSampleData(ReturnData returnData, Configuration config)
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



        private Int64 GetLastSequenceFromMySQL(Configuration config)
        {
            Int64 Result = -1;

            string tablePrefix;
            if (config.DatabaseId != null) tablePrefix = config.DatabaseId + "_";
            else tablePrefix = "";

            Variables.VariableData vd = Variables.Get(config.Databases_Server, "last_sequence_sampled", tablePrefix);
            if (vd != null)
            {
                Int64.TryParse(vd.Value, out Result);
            }

            return Result;
        }

        private Int64 GetAgentInstanceIdFromMySQL(Configuration config)
        {
            Int64 Result = -1;

            string tablePrefix;
            if (config.DatabaseId != null) tablePrefix = config.DatabaseId + "_";
            else tablePrefix = "";

            Variables.VariableData vd = Variables.Get(config.Databases_Server, "agent_instanceid", tablePrefix);
            if (vd != null)
            {
                Int64.TryParse(vd.Value, out Result);
            }

            return Result;
        }

        private void UpdateAgentInstanceID(Header_Streams header, Configuration config)
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
            public Int64 From { get; set; }
            public Int64 Count { get; set; }
        }

        private SampleInfo GetSampleInfo(Header_Streams header, Configuration config)
        {
            var result = new SampleInfo();

            //Get Sequence Number to use -----------------------
            Int64 first = header.FirstSequence;
            if (TH_Global.Variables.SIMULATION_MODE)
            {
                Console.WriteLine("Sample Simulation Enabled");
            }
            else
            {
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
            }           

            result.From = first;

            // Get Last Sequence Number available from Header
            Int64 last = header.LastSequence;

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
