// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using System.Collections;
using System.Xml;
using System.Data;
using System.Collections.Generic;

using TH_Configuration;
using TH_Global;
using TH_Global.Web;
using TH_MTC_Data;
using TH_MTC_Data.Streams;

namespace TH_MTC_Requests
{

    public class Sample
    {

        #region "Public"

        public event Connection_Handler Started;
        public event Connection_Handler Stopped;

        public bool Verbose;

        public string Address { get; set; }
        public int Port { get; set; }
        public string DeviceName { get; set; }

        public MTC_Stream_Status Status;

        //public Configuration configuration { get; set; }

        public SQL_Settings SQL { get; set; }

        public delegate void SampleFinishedDelly(ReturnData returnData);
        public event SampleFinishedDelly SampleFinished;

        public class ErrorData
        {
            public Sample current { get; set; }
            public string message { get; set; }
        }

        public delegate void SampleError_Handler(ErrorData errorData);
        public event SampleError_Handler SampleError;

        public Sample()
        {
            Status = MTC_Stream_Status.Stopped;
        }

        public void Run(string Variable, Int64 BeginSeq, Int64 Count)
        {
            if (Address != null)
            {
                string url = GetURL(Variable, BeginSeq, Count);

                string response = HTTP.GetData(url);

                if (response != null)
                {
                    ReturnData rd = ProcessData(response);
                    if (rd != null && SampleFinished != null) SampleFinished(rd);
                }
                else
                {
                    ErrorData error = new ErrorData();
                    error.message = "Connection Failed @ " + url;
                    error.current = this;

                    if (SampleError != null) SampleError(error);
                }
            }
        }

        string GetURL(string Variable, Int64 BeginSeq, Int64 Count)
        {
            string port;
            if (Port > 0)
                port = ":" + Port.ToString();
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

            return "http://" + Address + port + "/" + DeviceName + "/sample" + FullAddress;
        }

        ReturnData ProcessData(string xml)
        {
            ReturnData result = null;

            if (xml != null)
            {
                try
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
                        List<DeviceStream> deviceStreams = ProcessDeviceStream(Root);

                        // Create ReturnData object to send as Event argument
                        result = new ReturnData();
                        result.deviceStreams = deviceStreams;
                        result.header = header;
                    }
                    else if (Verbose) Console.WriteLine("DocumentElement == null");
                }
                catch (Exception ex) { if (Verbose) Logger.Log("ProcessData() :: Exception :: " + ex.Message); }
            }

            return result;
        }

        List<DeviceStream> ProcessDeviceStream(XmlElement Root)
        {
            List<DeviceStream> result = null;

            XmlNodeList DeviceStreamNodes = Root.GetElementsByTagName("DeviceStream");

            if (DeviceStreamNodes != null)
            {
                result = new List<DeviceStream>();

                foreach (XmlElement deviceNode in DeviceStreamNodes)
                {
                    DeviceStream deviceStream = Tools.GetDeviceStreamFromXML(deviceNode);

                    deviceStream.dataItems = Tools.GetDataItemsFromDeviceStream(deviceStream);

                    result.Add(deviceStream);
                }
            }

            return result;
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

        #region "OLD 12-7-15"

        //public void Start(string Variable, Int64 BeginSeq, Int64 Count)
        //{
        //    Status = MTC_Stream_Status.Started;

        //    Connection_Handler handler = Started;
        //    if (handler != null) Started();

        //    StartParameters startParameters = new StartParameters();
        //    startParameters.variable = Variable;
        //    startParameters.beginSeq = BeginSeq;
        //    startParameters.count = Count;

        //    worker = new Thread(new ParameterizedThreadStart(start));
        //    worker.Start(startParameters);
        //}

        //// Simulation using File
        //public void Start(string XMLFilePath)
        //{
        //    if (System.IO.File.Exists(XMLFilePath))
        //    {
        //        System.IO.StreamReader SimReader = new System.IO.StreamReader(XMLFilePath);
        //        Stream_Process(SimReader.ReadToEnd());
        //    }
        //}

        //Thread worker;

        //public class StartParameters
        //{
        //    public string variable { get; set; }
        //    public Int64 beginSeq { get; set; }
        //    public Int64 count { get; set; }
        //}

        //void start(object startParameters)
        //{
        //    StartParameters sp = (StartParameters)startParameters;

        //    Stream_Start(sp.variable, sp.beginSeq, sp.count);

        //    //// Allow for local file for Simulation ------------------------------------------------
        //    //if (configuration.Agent.Simulation_Sample_Files.Count > 0)
        //    //{
        //    //    foreach (string filePath in configuration.Agent.Simulation_Sample_Files)
        //    //    {
        //    //        if (System.IO.File.Exists(filePath))
        //    //        {
        //    //            System.IO.StreamReader SimReader = new System.IO.StreamReader(filePath);
        //    //            Stream_Process(SimReader.ReadToEnd());
        //    //            Thread.Sleep(2000);
        //    //        }
        //    //    }
        //    //}

        //    ////if (configuration.Agent.Simulation_Sample_Path != null)
        //    ////{
        //    ////    if (System.IO.File.Exists(configuration.Agent.Simulation_Sample_Path))
        //    ////    {
        //    ////        System.IO.StreamReader SimReader = new System.IO.StreamReader(configuration.Agent.Simulation_Sample_Path);
        //    ////        Stream_Process(SimReader.ReadToEnd());
        //    ////    }
        //    ////}
        //    //// ------------------------------------------------------------------------------------
        //    //else Stream_Start(sp.variable, sp.beginSeq, sp.count);
        //}


        //public void Stop()
        //{
        //    Status = MTC_Stream_Status.Stopped;

        //    Connection_Handler handler = Stopped;
        //    if (handler != null) Stopped();

        //    if (worker != null) worker.Abort();
        //}

        #endregion

        #region "Stream Management"

        //Stream stream;

        //private void SampleRecieved(ReturnData returnData)
        //{
        //    if (SampleFinished != null) SampleFinished(returnData);
        //}

        //private void Stream_Start(string Variable, Int64 BeginSeq, Int64 Count)
        //{
        //    stream = new Stream();
        //    stream.failureRetryInterval = 3000;

        //    string port;
        //    if (configuration.Agent.Port > 0)
        //        port = ":" + configuration.Agent.Port.ToString();
        //    else
        //        port = null;

        //    //Get Full path of variable for source
        //    string ParentAddress = "";

        //    string FullAddress = "?from=" + BeginSeq.ToString() + "&count=" + Count.ToString();

        //    if (Variable != null)
        //    {
        //        FullAddress = ParentAddress + "[@name=\"" + Variable + "\"]";
        //        FullAddress = "?path=" + FullAddress;
        //        FullAddress += "&from=" + BeginSeq.ToString() + "&count=" + Count.ToString();
        //    }

        //    //stream.uri = new Uri("http://" + configuration.Agent.IP_Address + port + "/" + configuration.Agent.Device_Name + "/sample" + FullAddress);
        //    stream.url = "http://" + configuration.Agent.IP_Address + port + "/" + configuration.Agent.Device_Name + "/sample" + FullAddress;
        //    stream.InsureDelivery = true;
        //    stream.ResponseReceived -= stream_ResponseReceived;
        //    stream.ResponseReceived += stream_ResponseReceived;

        //    if (Verbose) Console.WriteLine("Attempting Sample @ : " + stream.url);
        //    stream.Start();
        //}

        //private void Stream_Stop()
        //{
        //    if (stream != null) stream.Stop();
        //}


        //void Stream_Started()
        //{
        //    Status = MTC_Stream_Status.Started;

        //    if (Started != null) Started();
        //}

        //void Stream_Stopped()
        //{
        //    Status = MTC_Stream_Status.Stopped;

        //    if (Stopped != null) Stopped();
        //}


        //void stream_ResponseReceived(string responseString)
        //{
        //    if (responseString != null) Stream_Process(responseString);
        //}
        

        //void Stream_Process(string xml)
        //{
        //    XmlDocument Document = new XmlDocument();

        //    Document.LoadXml(xml);

        //    if (Document.DocumentElement != null)
        //    {
        //        // Get Root Element from Xml Document
        //        XmlElement Root = Document.DocumentElement;

        //        // Get Header_Streams object from Root node
        //        Header_Streams header = ProcessHeader(Root);

        //        // Get DeviceStream object from Root node
        //        List<DeviceStream> deviceStreams = ProcessDeviceStream(Root);

        //        // Create ReturnData object to send as Event argument
        //        ReturnData returnData = new ReturnData();
        //        returnData.deviceStreams = deviceStreams;
        //        //returnData.xmlDocument = Document;
        //        returnData.header = header;

        //        // Raise SampleRecieved Event
        //        SampleRecieved(returnData);
        //    }
        //    else if (Verbose) Console.WriteLine("DocumentElement == null");
        //}

        //List<DeviceStream> ProcessDeviceStream(XmlElement Root)
        //{
        //    List<DeviceStream> result = null;

        //    XmlNodeList DeviceStreamNodes = Root.GetElementsByTagName("DeviceStream");

        //    if (DeviceStreamNodes != null)
        //    {
        //        foreach (XmlElement deviceNode in DeviceStreamNodes)
        //        {
        //            DeviceStream deviceStream = Tools.GetDeviceStreamFromXML(deviceNode);

        //            deviceStream.dataItems = Tools.GetDataItemsFromDeviceStream(deviceStream);

        //            result.Add(deviceStream);
        //        }
        //    }

        //    return result;
        //}

        //Header_Streams ProcessHeader(XmlElement Root)
        //{
        //    Header_Streams Result = null;

        //    XmlNodeList HeaderNodes = Root.GetElementsByTagName("Header");

        //    if (HeaderNodes != null)
        //    {
        //        if (HeaderNodes.Count > 0)
        //        {
        //            XmlNode HeaderNode = HeaderNodes[0];

        //            Header_Streams header = new Header_Streams(HeaderNode);

        //            Result = header;
        //        }
        //    }
        //    return Result;
        //}

        #endregion

        #endregion

    }

}
