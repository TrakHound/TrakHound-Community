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
using TH_Global.Web;
using TH_MTC_Data;
//using TH_MTC_Requests;

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

            while (true)
            {
                var probeData = RunProbe();
                if (probeData != null)
                {
                    TablePlugIns_Update_Probe(probeData);

                    while (true)
                    {
                        var currentData = RunCurrent();
                        if (currentData != null)
                        {
                            TablePlugIns_Update_Current(currentData);

                            var sampleData = RunSample(currentData.header);
                            if (sampleData != null)
                            {
                                TablePlugIns_Update_Sample(sampleData);
                            }
                            else
                            {
                                TablePlugIns_Update_Sample(null);
                            }
                        }

                        Thread.Sleep(configuration.Agent.Current_Heartbeat);
                    }
                }
                else
                {
                    Thread.Sleep(configuration.Agent.Current_Heartbeat);
                }
            }
        }

        void Requests_Stop()
        {
            if (stopRequests != null) stopRequests.Set();
        }

        TH_MTC_Data.Components.ReturnData RunProbe()
        {
            TH_MTC_Data.Components.ReturnData result = null;

            string address = configuration.Agent.IP_Address;
            int port = configuration.Agent.Port;
            string deviceName = configuration.Agent.Device_Name;

            string url = MTC_Tools.GetUrl(address, port, deviceName) + "probe";

            result = TH_MTC_Data.Components.Requests.Get(url);
            if (result != null)
            {
                Console.WriteLine("Probe Successful : " + url);
            }
            else
            {
                Console.WriteLine("Probe Error : " + url);
            }

            return result;
        }

        TH_MTC_Data.Streams.ReturnData RunCurrent()
        {
            TH_MTC_Data.Streams.ReturnData result = null;

            string address = configuration.Agent.IP_Address;
            int port = configuration.Agent.Port;
            string deviceName = configuration.Agent.Device_Name;

            string url = MTC_Tools.GetUrl(address, port, deviceName) + "current";

            result = TH_MTC_Data.Streams.Requests.Get(url);
            if (result != null)
            {
                Console.WriteLine("Current Successful : " + url);
            }
            else
            {
                Console.WriteLine("Current Error : " + url);
            }

            return result;
        }

        Int64 lastSequenceSampled = -1;
        Int64 agentInstanceId = -1;
        Int64 lastInstanceId = -1;
        const Int64 MaxSampleCount = 10000;

        TH_MTC_Data.Streams.ReturnData RunSample(TH_MTC_Data.Header_Streams header)
        {
            TH_MTC_Data.Streams.ReturnData result = null;

            string address = configuration.Agent.IP_Address;
            int port = configuration.Agent.Port;
            string deviceName = configuration.Agent.Device_Name;

            UpdateAgentInstanceID(header);

            SampleInfo info = GetSampleInfo(header);
            if (info != null)
            {
                if (info.Count > 0)
                {
                    string url = MTC_Tools.GetUrl(address, port, deviceName) + "sample?from=" + info.From.ToString() + "&count=" + info.Count.ToString();

                    result = TH_MTC_Data.Streams.Requests.Get(url);
                    if (result != null)
                    {
                        Console.WriteLine("Sample Successful : " + url);
                    }
                    else
                    {
                        Console.WriteLine("Sample Error : " + url);
                    }
                }
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

        void UpdateAgentInstanceID(TH_MTC_Data.Header_Streams header)
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

        SampleInfo GetSampleInfo(TH_MTC_Data.Header_Streams header)
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





        #region "Probe"

        //TH_MTC_Data.Components.ReturnData ProbeData;

        //void Probe_Run()
        //{
        //    Probe p = new Probe();
        //    p.Address = configuration.Agent.IP_Address;
        //    p.Port = configuration.Agent.Port;
        //    p.DeviceName = configuration.Agent.Device_Name;

        //    p.ProbeFinished += p_ProbeFinished;
        //    p.ProbeError += p_ProbeError;

        //    p.Run();
        //}

        //void p_ProbeFinished(TH_MTC_Data.Components.ReturnData returnData, Probe probe)
        //{
        //    UpdateProcessingStatus("Probe Received");

        //    ProbeData = returnData;

        //    TablePlugIns_Update_Probe(returnData);

        //    ClearProcessingStatus();
        //}

        //void p_ProbeError(Probe.ErrorData errorData)
        //{
        //    UpdateProcessingStatus("Probe Error");
        //}

        #endregion

        #region "Current"

        //int sampleInterval;
        //int sampleCounter = -1;

        //void Current_Run()
        //{
        //    Current c = new Current();

        //    c.Address = configuration.Agent.IP_Address;
        //    c.Port = configuration.Agent.Port;
        //    c.DeviceName = configuration.Agent.Device_Name;

        //    c.CurrentFinished += c_CurrentFinished;
        //    c.CurrentError += c_CurrentError;

        //    // Calculate interval to use for when to run a Sample
        //    if (configuration.Agent.Current_Heartbeat > 0)
        //    {
        //        sampleInterval = configuration.Agent.Sample_Heartbeat / configuration.Agent.Current_Heartbeat;
        //    }

        //    c.Run();
        //}

        //void c_CurrentFinished(TH_MTC_Data.Streams.ReturnData returnData)
        //{
        //    UpdateProcessingStatus("Current Received");

        //    CheckDatabaseConnection();

        //    // Update Agent_Info
        //    UpdateAgentInfo(returnData.header);

        //    // Update all of the PlugIns with the ReturnData object
        //    TablePlugIns_Update_Current(returnData);

        //    // Run Sample if sampleCounter is over sampleInterval
        //    sampleCounter += 1;
        //    if (sampleCounter >= sampleInterval || (sampleCounter <= 0))
        //    {
        //        Sample_Run(returnData.header);
        //        sampleCounter = 0;
        //    }

        //    returnData.Dispose();

        //    Thread.Sleep(configuration.Agent.Current_Heartbeat);

        //    // Run the current again after Sleeping for the Current_Heartbeat that is configured
        //    Current_Run();
        //}

        //void c_CurrentError(Current.ErrorData errorData)
        //{
        //    UpdateProcessingStatus("Current Error");

        //    Thread.Sleep(configuration.Agent.Current_Heartbeat);

        //    Current_Run();
        //}

        ///// <summary>
        ///// Check Database Connections to insure they are connected before proceeding.
        ///// This method will run continously until the database connections are established.
        ///// </summary>
        //void CheckDatabaseConnection()
        //{
        //    bool dbsuccess = false;

        //    int interval_min = 3000;
        //    int interval_max = 60000;
        //    int interval = interval_min;

        //    bool first = true;

        //    if (UseDatabases)
        //    while (!dbsuccess)
        //    {
        //        // Ping Database connection for each Database Configuration
        //        dbsuccess = true;
        //        foreach (Database_Configuration db_config in configuration.Databases_Server.Databases)
        //        {
        //            if (!TH_Database.Global.Ping(db_config)) { dbsuccess = false; break; }
        //        }

        //        if (dbsuccess) UpdateProcessingStatus("Database Connections Established");
        //        else
        //        {
        //            // Write to console that there was an error
        //            if (first) WriteToConsole("Error in Database Connection... Retrying in " + interval.ToString() + "ms", ConsoleOutputType.Error);
        //            first = false;

        //            // Increase the interval by 10% until interval == interval_max
        //            interval = Math.Min(Convert.ToInt32(interval + (interval * 0.1)), interval_max);

        //            // Sleep the current thread for the calculated interval
        //            System.Threading.Thread.Sleep(interval);
        //        }
        //    }
        //}


        //Int64 Agent_ID;
        //DateTime Agent_First;
        //DateTime Agent_Last;

        ///// <summary>
        ///// Update the Agent Info for First Sequence and Last Sequence variables.
        ///// Resets First and Last if a new Instance ID is found for the Agent
        ///// </summary>
        ///// <param name="header"></param>
        //void UpdateAgentInfo(Header_Streams header)
        //{
        //    if (Agent_ID != header.instanceId)
        //    {
        //        Agent_First = header.creationTime;
        //    }
        //    Agent_Last = header.creationTime;
        //    if (Agent_Last < Agent_First) Agent_Last = Agent_First;
        //    Agent_ID = header.instanceId;
        //}

        #endregion

        #region "Sample"

        bool startFromFirst = true;

                 //Int64 lastSequenceSampled = -1;
        //Int64 agentInstanceId = -1;

        //void Sample_Initialize()
        //{
        //    lastSequenceSampled = GetLastSequenceFromMySQL();
        //    agentInstanceId = GetAgentInstanceIdFromMySQL();
        //}

        //Int64 GetLastSequenceFromMySQL()
        //{
        //    Int64 Result = -1;

        //    Variables.VariableData vd = Variables.Get(configuration.Databases_Server, "last_sequence_sampled", TablePrefix);
        //    if (vd != null)
        //    {
        //        Int64.TryParse(vd.value, out Result);
        //    }

        //    return Result;
        //}

        //Int64 GetAgentInstanceIdFromMySQL()
        //{
        //    Int64 Result = -1;

        //    Variables.VariableData vd = Variables.Get(configuration.Databases_Server, "agent_instanceid", TablePrefix);
        //    if (vd != null)
        //    {
        //        Int64.TryParse(vd.value, out Result);
        //    }

        //    return Result;
        //}

        //const Int64 MaxSampleCount = 10000;

        //void Sample_Run(TH_MTC_Data.Header_Streams header)
        //{
        //    Sample s = new Sample();
        //    s.Address = configuration.Agent.IP_Address;
        //    s.Port = configuration.Agent.Port;
        //    s.DeviceName = configuration.Agent.Device_Name;

        //    s.SampleFinished += s_SampleFinished;
        //    s.SampleError += s_SampleError;

        //    // Check/Update Agent Instance Id -------------------
        //    Int64 lastInstanceId = agentInstanceId;
        //    agentInstanceId = header.instanceId;
        //    Variables.Update(configuration.Databases_Server, "Agent_InstanceID", agentInstanceId.ToString(), header.creationTime, TablePrefix);
        //    // --------------------------------------------------

        //    // Get Sequence Number to use -----------------------
        //    Int64 First = header.firstSequence;
        //    if (!startFromFirst)
        //    {
        //        First = header.lastSequence;
        //        startFromFirst = true;
        //    }
        //    else if (lastInstanceId == agentInstanceId && lastSequenceSampled > 0 && lastSequenceSampled >= header.firstSequence)
        //    {
        //        First = lastSequenceSampled + 1;
        //    }
        //    else if (First > 0)
        //    {
        //        Int64 first = First;

        //        // Increment some sequences since the Agent might change the first sequence
        //        // before the Sample request gets read
        //        // (should be fixed in Agent to automatically read the first 'available' sequence
        //        // instead of returning an error)
        //        First += 20;
        //    }

        //    // Get Last Sequence Number available from Header
        //    Int64 Last = header.lastSequence;

        //    // Calculate Sample count
        //    Int64 SampleCount = Last - First;
        //    if (SampleCount > MaxSampleCount)
        //    {
        //        SampleCount = MaxSampleCount;
        //        Last = First + MaxSampleCount;
        //    }

        //    // Update Last Sequence Sampled for the subsequent samples
        //    lastSequenceSampled = Last;
        //    Variables.Update(configuration.Databases_Server, "Last_Sequence_Sampled", Last.ToString(), header.creationTime, TablePrefix);

        //    if (SampleCount > 0)
        //    {
        //        UpdateProcessingStatus("Running Sample @ " + s.Address + ":" + s.Port.ToString() + " Count = " + SampleCount.ToString());
        //        s.Run(null, First, SampleCount);
        //    }
        //    else
        //    {
        //        // Update all of the Table Plugins with a Null ReturnData
        //        TablePlugIns_Update_Sample(null);
        //    }

        //}

        //void s_SampleFinished(TH_MTC_Data.Streams.ReturnData returnData)
        //{
        //    UpdateProcessingStatus("Sample Received..");

        //    // Update all of the Table Plugins with the ReturnData
        //    TablePlugIns_Update_Sample(returnData);

        //    ClearProcessingStatus();

        //    returnData.Dispose();
        //}

        //void s_SampleError(Sample.ErrorData errorData)
        //{
            
        //}

        #endregion

    }

    static class MTC_Tools
    {
        public static string GetUrl(string address, int port, string deviceName)
        {
            string url = "http://";

            // Add Ip Address
            string ip = address;

            // Add Port
            string lport = null;
            // If port is in ip address
            if (ip.Contains(":"))
            {
                int colonindex = ip.LastIndexOf(':');
                int slashindex = -1;

                // Get index of last forward slash
                if (ip.Contains("/")) slashindex = ip.IndexOf('/', colonindex);

                // Get port based on indexes
                if (slashindex > colonindex) lport = ":" + ip.Substring(colonindex + 1, slashindex - colonindex - 1) + "/";
                else lport = ":" + ip.Substring(colonindex + 1) + "/";

                ip = ip.Substring(0, colonindex);
            }
            else
            {
                if (port > 0) lport = ":" + port.ToString() + "/";
            }

            url += ip;
            url += lport;

            // Add Device Name
            string ldeviceName = null;
            if (deviceName != String.Empty)
            {
                if (lport != null) ldeviceName = deviceName;
                else ldeviceName = "/" + deviceName;
                ldeviceName += "/";
            }
            url += ldeviceName;

            if (url[url.Length - 1] != '/') url += "/";

            return url;
        }
    }

}
