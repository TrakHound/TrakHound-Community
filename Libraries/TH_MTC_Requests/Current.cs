// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
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
    public class Current
    {

        #region "Public"

        public event Connection_Handler Started;
        public event Connection_Handler Stopped;

        public bool Verbose;

        public string Address { get; set; }
        public int Port { get; set; }
        public string DeviceName { get; set; }

        public MTC_Stream_Status Status;

        public Configuration configuration { get; set; }

        public delegate void CurrentFinishedDelly(ReturnData returnData);
        public event CurrentFinishedDelly CurrentFinished;

        public class ErrorData
        {
            public Current current { get; set; }
            public string message { get; set; }
        }

        public delegate void CurrentError_Handler(ErrorData errorData);
        public event CurrentError_Handler CurrentError;

        public Current()
        {
            Status = MTC_Stream_Status.Stopped;
        }

        Thread worker;

        public void Start()
        {
            if (Started != null) Started();

            worker = new Thread(new ThreadStart(Run));
            worker.Start();
        }

        public void Start(int Heartbeat)
        {
            if (Started != null) Started();

            //worker = new Thread(new ParameterizedThreadStart(Stream_Start));
            //worker.Start(Heartbeat);

            //Stream_Start(Heartbeat);
        }

        public void Stop()
        {
            //Stream_Stop();
            if (worker != null) worker.Abort();

            if (Stopped != null) Stopped();
        }

        #endregion

        #region "Run"

        public void Run()
        {
            if (Address != null)
            {
                string url = GetUrl();

                string response = HTTP.GetData(url);

                if (response != null)
                {
                    ReturnData rd = ProcessData(response);
                    if (rd != null && CurrentFinished != null) CurrentFinished(rd);
                }
                else
                {
                    ErrorData error = new ErrorData();
                    error.message = "Connection Failed @ " + url;
                    error.current = this;

                    if (CurrentError != null) CurrentError(error);
                }
            }
        }

        string GetUrl()
        {
            string url = "http://";

            // Add Ip Address
            string ip = Address;

            // Add Port
            string port = null;
            // If port is in ip address
            if (ip.Contains(":"))
            {
                int colonindex = ip.LastIndexOf(':');
                int slashindex = -1;

                // Get index of last forward slash
                if (ip.Contains("/")) slashindex = ip.IndexOf('/', colonindex);

                // Get port based on indexes
                if (slashindex > colonindex) port = ":" + ip.Substring(colonindex + 1, slashindex - colonindex - 1) + "/";
                else port = ":" + ip.Substring(colonindex + 1) + "/";

                ip = ip.Substring(0, colonindex);
            }
            else
            {
                if (Port > 0) port = ":" + Port.ToString() + "/";
            }

            url += ip;
            url += port;

            // Add Device Name
            string deviceName = null;
            if (DeviceName != String.Empty)
            {
                if (port != null) deviceName = DeviceName;
                else deviceName = "/" + DeviceName;
                deviceName += "/";
            }
            url += deviceName;

            if (url[url.Length - 1] != '/') url += "/";

            return url + "current";
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
                }
                catch (Exception ex) { if (Verbose) Logger.Log("stream_ResponseReceived() :: Exception :: " + ex.Message); }
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

        #endregion

        #region "Stream Management"

        //Stream stream;

        //private void CurrentRecieved(ReturnData returnData)
        //{
        //    CurrentFinishedDelly handler = CurrentFinished;
        //    if (null != handler) handler(returnData);
        //}

        //private void Stream_Start(object heartbeat)
        //{
        //    int Heartbeat = (int)heartbeat;

        //    stream = new Stream();

        //    //string port;
        //    //if (configuration.Agent.Port > 0)
        //    //    port = ":" + configuration.Agent.Port.ToString();
        //    //else
        //    //    port = null;

        //    //stream.uri = new Uri("http://" + configuration.Agent.IP_Address + port + "/" + configuration.Agent.Device_Name + "/current");


        //    string url = "http://";

        //    // Add Ip Address
        //    string ip = configuration.Agent.IP_Address;

        //    // Add Port
        //    string port = null;
        //    // If port is in ip address
        //    if (ip.Contains(":"))
        //    {
        //        int colonindex = ip.LastIndexOf(':');
        //        int slashindex = -1;

        //        // Get index of last forward slash
        //        if (ip.Contains("/")) slashindex = ip.IndexOf('/', colonindex);

        //        // Get port based on indexes
        //        if (slashindex > colonindex) port = ":" + ip.Substring(colonindex + 1, slashindex - colonindex - 1) + "/";
        //        else port = ":" + ip.Substring(colonindex + 1) + "/";

        //        ip = ip.Substring(0, colonindex);
        //    }
        //    else
        //    {
        //        if (configuration.Agent.Port > 0) port = ":" + configuration.Agent.Port.ToString() + "/";
        //    }

        //    url += ip;
        //    url += port;

        //    // Add Device Name
        //    string deviceName = null;
        //    if (configuration.Agent.Device_Name != String.Empty)
        //    {
        //        if (port != null) deviceName = configuration.Agent.Device_Name;
        //        else deviceName = "/" + configuration.Agent.Device_Name;
        //        deviceName += "/";
        //    }
        //    url += deviceName;

        //    if (url[url.Length - 1] != '/') url += "/";

        //    //stream.uri = new Uri(url + "current");
        //    stream.url = url + "current";
        //    stream.HttpTimeout = 3000;
        //    stream.interval = Heartbeat;

        //    stream.ResponseReceived += stream_ResponseReceived;
        //    stream.ResponseError += stream_ResponseError;

        //    if (Verbose) Console.WriteLine("Connecting Current @ : " + stream.url);
        //    stream.Start();
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

        //private void Stream_Stop()
        //{
        //    if (stream != null) stream.Stop();
        //}

        //void stream_ResponseReceived(string responseString)
        //{
        //    if (responseString != null)
        //    {
        //        try
        //        {
        //            XmlDocument Document = new XmlDocument();
        //            Document.LoadXml(responseString);

        //            if (Document.DocumentElement != null)
        //            {
        //                // Get Root Element from Xml Document
        //                XmlElement Root = Document.DocumentElement;

        //                // Get Header_Streams object from Root node
        //                Header_Streams header = ProcessHeader(Root);

        //                // Get DeviceStream object from Root node
        //                DeviceStream deviceStream = ProcessDeviceStream(Root);

        //                // Create ReturnData object to send as Event argument
        //                ReturnData returnData = new ReturnData();
        //                returnData.deviceStream = deviceStream;
        //                //returnData.xmlDocument = Document;
        //                returnData.header = header;

        //                // Raise CurrentReceived Event
        //                CurrentRecieved(returnData);
        //            }
        //        }
        //        catch (Exception ex) { if (Verbose) Logger.Log("stream_ResponseReceived() :: Exception :: " + ex.Message); }
        //    }
        //}

        //DeviceStream ProcessDeviceStream(XmlElement Root)
        //{
        //    DeviceStream Result = null;

        //    XmlNodeList DeviceStreamNodes = Root.GetElementsByTagName("DeviceStream");

        //    if (DeviceStreamNodes != null)
        //    {
        //        if (DeviceStreamNodes.Count > 0)
        //        {
        //            XmlNode DeviceStreamNode = DeviceStreamNodes[0];

        //            DeviceStream deviceStream = Tools.GetDeviceStreamFromXML(DeviceStreamNode);

        //            DataItemCollection dataItems = Tools.GetDataItemsFromDeviceStream(deviceStream);

        //            Result = deviceStream;
        //        }
        //    }

        //    return Result;
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

        //void stream_ResponseError(Error error)
        //{
        //    ErrorData data = new ErrorData();
        //    data.message = error.message;
        //    data.current = this;

        //    if (CurrentError != null) CurrentError(data);
        //}

        #endregion

    }
}
