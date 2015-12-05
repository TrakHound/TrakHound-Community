// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TH_Database.Tables;
using TH_MTC_Data;
using TH_MTC_Requests;

namespace TH_Device_Server
{
    public partial class Device_Server
    {

        void Requests_Initialize()
        {
            Probe_Initialize();
            Current_Initialize();
            Sample_Initialize();
        }

        void Requests_Start()
        {
            Probe_Start();
            Current_Start();
        }

        void Requests_Stop()
        {
            Probe_Stop();
            Current_Stop();
            Sample_Stop();
        }

        #region "Probe"

        Probe probe;

        TH_MTC_Data.Components.ReturnData ProbeData;

        void Probe_Initialize()
        {
            probe = new Probe();
            probe.configuration = configuration;
            probe.ProbeFinished -= probe_ProbeFinished;
            probe.ProbeFinished += probe_ProbeFinished;
        }

        void Probe_Start()
        {
            UpdateProcessingStatus("Running Probe Request..");
            if (probe != null) probe.Start();
        }

        void Probe_Stop()
        {
            if (probe != null) probe.Stop();
        }

        void probe_ProbeFinished(TH_MTC_Data.Components.ReturnData returnData, Probe probe)
        {
            UpdateProcessingStatus("Probe Received");

            ProbeData = returnData;

            if (configuration.Server.Tables.MTConnect.Sample)
            {
                //CreateSampleTables_MySQL(returnData.DS);
            }

            TablePlugIns_Update_Probe(returnData);

            ClearProcessingStatus();
        }

        #endregion

        #region "Current"

        Current current;

        int sampleInterval;
        int sampleCounter = -1;

        Int64 Agent_ID;
        DateTime Agent_First;
        DateTime Agent_Last;

        bool currentStopped;

        void Current_Initialize()
        {
            Agent_First = DateTime.MinValue;
            Agent_Last = DateTime.MinValue;

            current = new Current();
            current.configuration = configuration;
            current.CurrentFinished -= current_CurrentFinished;
            current.CurrentFinished += current_CurrentFinished;
            current.CurrentError += current_CurrentError;

            // Calculate interval to use for when to run a Sample
            sampleInterval = configuration.Agent.Sample_Heartbeat / configuration.Agent.Current_Heartbeat;
        }

        void current_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Current_Start()
        {
            currentStopped = false;

            UpdateProcessingStatus("Running Current Request..");
            if (current != null) current.Start(configuration.Agent.Current_Heartbeat); 
        }

        void Current_Stop()
        {
            if (current != null) current.Stop();

            currentStopped = true;
            Connected = false;

            sampleCounter = -1;
        }

        void Current_Restart()
        {
            if (current != null) current.Stop();

            System.Threading.Thread.Sleep(2000);

            Current_Start();
        }

        void current_CurrentFinished(TH_MTC_Data.Streams.ReturnData returnData)
        {
            Connected = true;

            UpdateProcessingStatus("Current Received");

            // Check Database Connections (if any) and don't continue till connection established
            bool dbsuccess = false;
            bool error = false;

            while (!dbsuccess)
            {
                dbsuccess = CheckDatabaseConnections(configuration);

                if (dbsuccess) UpdateProcessingStatus("Database Connections Established");
                else
                {
                    current.Stop();
                    WriteToConsole("Error in Database Connection... Retrying in 1000ms", ConsoleOutputType.Error);
                    System.Threading.Thread.Sleep(1000);
                    error = true;
                }
            }

            // If there was an error then restart Current requests
            if (error) current.Start(configuration.Agent.Current_Heartbeat);

            
            // Update Agent_Info
            if (Agent_ID != returnData.header.instanceId)
            {
                Agent_First = returnData.header.creationTime;
            }
            Agent_Last = returnData.header.creationTime;
            if (Agent_Last < Agent_First) Agent_Last = Agent_First;
            Agent_ID = returnData.header.instanceId;


            // Update all of the PlugIns with the ReturnData object
            TablePlugIns_Update_Current(returnData);

            // Run Sample if sampleCounter is over sampleInterval
            sampleCounter += 1;


            if (configuration.Agent.Simulation_Sample_Files.Count > 0)
            {
                Sample_Start();
            }
            else
            {
                if (sampleCounter >= sampleInterval || (sampleCounter <= 0))
                {
                    if (!inProgress)
                    {
                        Sample_Start(returnData.header);
                        sampleCounter = 0;
                    }
                }
            }

            ClearProcessingStatus();
        }

        void current_CurrentError(Current.ErrorData errorData)
        {
            Connected = false;

            //Log(errorData.message);
            WriteToConsole("Error Connecting to Current", ConsoleOutputType.Error);

            //Current_Restart();

            //current.Stop();
            
            //if (!currentStopped) current.Start(configuration.Agent.Current_Heartbeat); 
        }

        #endregion

        #region "Sample"

        bool startFromFirst = true;

        Sample sample;

        Int64 lastSequenceSampled = -1;
        Int64 agentInstanceId = -1;

        bool inProgress = false;

        void Sample_Initialize()
        {
            lastSequenceSampled = GetLastSequenceFromMySQL();

            agentInstanceId = GetAgentInstanceIdFromMySQL();
        }

        Int64 GetLastSequenceFromMySQL()
        {
            Int64 Result = -1;

            Variables.VariableData vd = Variables.Get(configuration.Databases_Server, "last_sequence_sampled");
            if (vd != null)
            {
                Int64.TryParse(vd.value, out Result);
            }

            return Result;
        }

        Int64 GetAgentInstanceIdFromMySQL()
        {
            Int64 Result = -1;

            Variables.VariableData vd = Variables.Get(configuration.Databases_Server, "agent_instanceid");
            if (vd != null)
            {
                Int64.TryParse(vd.value, out Result);
            }

            return Result;
        }

        const Int64 MaxSampleCount = 10000;

        void Sample_Start(TH_MTC_Data.Header_Streams header)
        {
            UpdateProcessingStatus("Running Sample Request..");

            sample = new Sample();
            sample.configuration = configuration;
            sample.SampleFinished -= sample_SampleFinished;
            sample.SampleFinished += sample_SampleFinished;

            if (sample != null)
            {
                // Check/Update Agent Instance Id -------------------
                Int64 lastInstanceId = agentInstanceId;
                agentInstanceId = header.instanceId;
                Variables.Update(configuration.Databases_Server, "Agent_InstanceID", agentInstanceId.ToString(), header.creationTime);
                // --------------------------------------------------

                // Get Sequence Number to use -----------------------
                Int64 First = header.firstSequence;
                if (!startFromFirst)
                {
                    First = header.lastSequence;
                    startFromFirst = true;
                }
                else if (lastInstanceId == agentInstanceId && lastSequenceSampled > 0 && lastSequenceSampled >= header.firstSequence)
                {
                    First = lastSequenceSampled + 1;
                }
                else if (First > 0)
                {
                    Int64 first = First;

                    // Increment some sequences since the Agent might change the first sequence
                    // before the Sample request gets read
                    // (should be fixed in Agent to automatically read the first 'available' sequence
                    // instead of returning an error)
                    First += 20;
                }

                // Get Last Sequence Number available from Header
                Int64 Last = header.lastSequence;

                // Calculate Sample count
                Int64 SampleCount = Last - First;
                if (SampleCount > MaxSampleCount)
                {
                    SampleCount = MaxSampleCount;
                    Last = First + MaxSampleCount;
                }

                // Update Last Sequence Sampled for the subsequent samples
                // lastSequenceSampled_temp = Last;
                lastSequenceSampled = Last;
                Variables.Update(configuration.Databases_Server, "Last_Sequence_Sampled", Last.ToString(), header.creationTime);


                //if (configuration.Agent.Simulation_Sample_Path != null)
                //{
                //    Log("Sample_Start() : Simulation File : " + configuration.Agent.Simulation_Sample_Path);
                //    sample.Start(null, First, SampleCount);
                //}
                //else if (SampleCount > 0)
                if (SampleCount > 0 && configuration.Agent.Sample_Heartbeat >= 0)
                {
                    //Log("Sample_Start() : " + First.ToString() + " to " + Last.ToString());
                    sample.Start(null, First, SampleCount);
                }
                else
                {
                    //Log("Sample Skipped : No New Data! : " + First.ToString() + " to " + Last.ToString());

                    // Update all of the Table Plugins with a Null ReturnData
                    TablePlugIns_Update_Sample(null);
                }

            }
        }

        void Sample_Start()
        {
            Probe_Stop();
            Current_Stop();


            SampleSimulation_Start();

            //if (configuration.Agent.Simulation_Sample_Files.Count > 0)
            //{
            //    foreach (string filePath in configuration.Agent.Simulation_Sample_Files)
            //    {
            //        Sample simSample = new Sample();
            //        simSample.configuration = configuration;
            //        //simSample.SampleFinished -= sample_SampleFinished;
            //        simSample.SampleFinished += sample_SampleFinished;
            //        simSample.Start(filePath);
            //    }
            //}




            //Log("Sample_Start() : Simulation File : " + configuration.Agent.Simulation_Sample_Path);

            //sample = new Sample();
            //sample.configuration = configuration;
            //sample.SampleFinished -= sample_SampleFinished;
            //sample.SampleFinished += sample_SampleFinished;
            //sample.Start(null, 0, 0);
        }

        int simulationIndex = 0;

        System.Timers.Timer SampleSimulation_TIMER;

        void SampleSimulation_Start()
        {
            SampleSimulation_TIMER = new System.Timers.Timer();
            SampleSimulation_TIMER.Interval = 2000;
            SampleSimulation_TIMER.Elapsed += SampleSimulation_TIMER_Elapsed;
            SampleSimulation_TIMER.Enabled = true;
        }

        void SampleSimulation_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (configuration.Agent.Simulation_Sample_Files.Count > simulationIndex)
            {
                string filePath = configuration.Agent.Simulation_Sample_Files[simulationIndex];

                Sample simSample = new Sample();
                simSample.configuration = configuration;
                simSample.SampleFinished += sample_SampleFinished;
                simSample.Start(filePath);

                simulationIndex += 1;
            }
            else
            {
                SampleSimulation_TIMER.Enabled = false;
            }
        }



        void Sample_Stop()
        {
            if (sample != null) sample.Stop();
        }

        void sample_SampleFinished(TH_MTC_Data.Streams.ReturnData returnData)
        {
            UpdateProcessingStatus("Sample Received..");

            // Update all of the Table Plugins with the ReturnData
            TablePlugIns_Update_Sample(returnData);

            ClearProcessingStatus();
        }

        #endregion

    }
}
