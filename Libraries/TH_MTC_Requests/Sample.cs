// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using System.Collections;
using System.Xml;
using System.Data;

using TH_Configuration;
using TH_MTC_Data;
using TH_MTC_Data.Streams;

namespace TH_MTC_Requests
{

    public class Sample
    {

        #region "Public"

        public event Connection_Handler Started;
        public event Connection_Handler Stopped;

        public MTC_Stream_Status Status;

        public Configuration configuration { get; set; }

        public SQL_Settings SQL { get; set; }

        public delegate void SampleFinishedDelly(ReturnData returnData);
        public event SampleFinishedDelly SampleFinished;

        public Sample()
        {
            Status = MTC_Stream_Status.Stopped;
        }

        public void Start(string Variable, Int64 BeginSeq, Int64 Count)
        {
            Status = MTC_Stream_Status.Started;

            Connection_Handler handler = Started;
            if (handler != null) Started();

            StartParameters startParameters = new StartParameters();
            startParameters.variable = Variable;
            startParameters.beginSeq = BeginSeq;
            startParameters.count = Count;

            worker = new Thread(new ParameterizedThreadStart(start));
            worker.Start(startParameters);
        }

        Thread worker;

        public class StartParameters
        {
            public string variable { get; set; }
            public Int64 beginSeq { get; set; }
            public Int64 count { get; set; }
        }

        void start(object startParameters)
        {
            StartParameters sp = (StartParameters)startParameters;

            //Allow for local file for Simulation ------------------------------------------------
            if (configuration.Agent.Simulation_Sample_Path != null)
            {
                if (System.IO.File.Exists(configuration.Agent.Simulation_Sample_Path))
                {
                    System.IO.StreamReader SimReader = new System.IO.StreamReader(configuration.Agent.Simulation_Sample_Path);
                    Stream_Process(SimReader.ReadToEnd());
                }
            }
            //------------------------------------------------------------------------------------
            else Stream_Start(sp.variable, sp.beginSeq, sp.count);
        }


        public void Stop()
        {
            Status = MTC_Stream_Status.Stopped;

            Connection_Handler handler = Stopped;
            if (handler != null) Stopped();

            if (worker != null) worker.Abort();
            //Stream_Stop();
        }

        //public void Start(string Variable, Int64 BeginSeq, Int64 Count)
        //{
        //    Status = MTC_Stream_Status.Started;

        //    Connection_Handler handler = Started;
        //    if (handler != null) Started();

        //    //Allow for local file for Simulation ------------------------------------------------
        //    if (configuration.Agent.Simulation_Sample_Path != null)
        //    {
        //        if (System.IO.File.Exists(configuration.Agent.Simulation_Sample_Path))
        //        {
        //            System.IO.StreamReader SimReader = new System.IO.StreamReader(configuration.Agent.Simulation_Sample_Path);
        //            Stream_Process(SimReader.ReadToEnd());
        //        }
        //    }
        //    //------------------------------------------------------------------------------------
        //    else Stream_Start(Variable, BeginSeq, Count);
        //}

        //public void Stop()
        //{
        //    Status = MTC_Stream_Status.Stopped;

        //    Connection_Handler handler = Stopped;
        //    if (handler != null) Stopped();

        //    Stream_Stop();
        //}

        #endregion

        #region "Stream Management"

        Stream stream;

        private void SampleRecieved(ReturnData returnData)
        {
            if (SampleFinished != null) SampleFinished(returnData);
        }

        private void Stream_Start(string Variable, Int64 BeginSeq, Int64 Count)
        {
            stream = new Stream();
            stream.failureRetryInterval = 3000;

            string port;
            if (configuration.Agent.Port > 0)
                port = ":" + configuration.Agent.Port.ToString();
            else
                port = null;

            //Get Full path of variable for source
            string ParentAddress = "";

            string FullAddress = "?from=" + BeginSeq.ToString() + "&count=" + Count.ToString();

            if (Variable != null)
            {
                FullAddress = ParentAddress + "[@name=\"" + Variable + "\"]";
                FullAddress = "?path=" + FullAddress;
                FullAddress += "&from=" + BeginSeq.ToString() + "&count=" + Count.ToString();
            }

            stream.uri = new Uri("http://" + configuration.Agent.IP_Address + port + "/" + configuration.Agent.Device_Name + "/sample" + FullAddress);

            stream.ResponseReceived -= stream_ResponseReceived;
            stream.ResponseReceived += stream_ResponseReceived;

            Console.WriteLine("Attempting Sample @ : " + stream.uri);
            stream.Start();
        }

        private void Stream_Stop()
        {
            if (stream != null) stream.Stop();
        }


        void Stream_Started()
        {
            Status = MTC_Stream_Status.Started;

            if (Started != null) Started();
        }

        void Stream_Stopped()
        {
            Status = MTC_Stream_Status.Stopped;

            if (Stopped != null) Stopped();
        }


        void stream_ResponseReceived(string responseString)
        {
            if (responseString != null) Stream_Process(responseString);
        }
        

        void Stream_Process(string xml)
        {
            XmlDocument Document = new XmlDocument();

            Document.LoadXml(xml);

            if (Document.DocumentElement != null)
            {
                // Get Root Element from Xml Document
                XmlElement Root = Document.DocumentElement;

                // Get Header_Streams object from Root node
                Header_Streams header = ProcessHeader(Root);

                // Get DeviceStream object from Root node
                DeviceStream deviceStream = ProcessDeviceStream(Root);

                // Create ReturnData object to send as Event argument
                ReturnData returnData = new ReturnData();
                returnData.deviceStream = deviceStream;
                //returnData.xmlDocument = Document;
                returnData.header = header;

                // Raise SampleRecieved Event
                SampleRecieved(returnData);
            }
            else Console.WriteLine("DocumentElement == null");
        }

        DeviceStream ProcessDeviceStream(XmlElement Root)
        {
            DeviceStream Result = null;

            XmlNodeList DeviceStreamNodes = Root.GetElementsByTagName("DeviceStream");

            if (DeviceStreamNodes != null)
            {
                if (DeviceStreamNodes.Count > 0)
                {
                    XmlNode DeviceStreamNode = DeviceStreamNodes[0];

                    DeviceStream deviceStream = Tools.GetDeviceStreamFromXML(DeviceStreamNode);

                    DataItemCollection dataItems = Tools.GetDataItemsFromDeviceStream(deviceStream);

                    Console.WriteLine("Conditions.Count = " + dataItems.Conditions.Count.ToString());
                    Console.WriteLine("Events.Count = " + dataItems.Events.Count.ToString());
                    Console.WriteLine("Samples.Count = " + dataItems.Samples.Count.ToString());

                    Result = deviceStream;
                }
            }

            return Result;
        }

        Header_Streams ProcessHeader(XmlElement Root)
        {
            Header_Streams Result = null;

            XmlNodeList HeaderNodes = Root.GetElementsByTagName("Header");

            if (HeaderNodes != null)
            {
                if (HeaderNodes.Count > 0)
                {
                    XmlNode HeaderNode = HeaderNodes[0];

                    Header_Streams header = new Header_Streams(HeaderNode);

                    Result = header;
                }
            }
            return Result;
        }

        #endregion

    }

}
