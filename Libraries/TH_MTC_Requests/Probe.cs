// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using System.Xml;
using System.Data;
using System.Collections.Generic;

using TH_Global;
using TH_Global.Web;
using TH_MTC_Data;
using TH_MTC_Data.Components;


namespace TH_MTC_Requests
{
    /// <summary>
    /// Used to perform a Probe command on the MTConnect Agent and return a DataSet object and Device class object with the information recieved.
    /// Use the "ProbeFinished" event to get the DataSet.
    /// </summary>
    public class Probe
    {

        #region "Public"

        public MTC_Stream_Status Status;

        public event Connection_Handler Started;
        public event Connection_Handler Stopped;

        public bool Verbose;

        public string Address { get; set; }
        public int Port { get; set; }
        public string DeviceName { get; set; }

        public string Url { get; set; }

        public delegate void ProbeFinishedDelly(TH_MTC_Data.Components.ReturnData returnData, Probe sender);
        public event ProbeFinishedDelly ProbeFinished;

        public class ErrorData
        {
            public Probe probe { get; set; }
            public string message { get; set; }
        }

        public delegate void ProbeError_Handler(ErrorData errorData);
        public event ProbeError_Handler ProbeError;

        public Probe()
        {
            Status = MTC_Stream_Status.Stopped;
        }

        Thread worker;

        public void Start()
        {
            //worker = new Thread(new ThreadStart(Stream_Start));
            worker = new Thread(new ThreadStart(Run));
            worker.Start();
        }

        public void Stop()
        {
            if (worker != null) worker.Abort();
        }

        #endregion

        #region "Run"

        public void Run()
        {
            string url = GetUrl();
            Url = url;

            if (url != null)
            {
                string response = HTTP.GetData(url);

                if (response != null)
                {
                    ReturnData rd = ProcessData(response);
                    if (rd != null && ProbeFinished != null) ProbeFinished(rd, this);
                }
                else
                {
                    ErrorData error = new ErrorData();
                    error.message = "Connection Failed @ " + url;
                    error.probe = this;

                    if (ProbeError != null) ProbeError(error);
                }
            }
        }

        string GetUrl()
        {
            if (Url != null)
            {
                if (!Url.StartsWith("http://")) Url = "http://" + Url;

                return Url;
            }
            else
            {
                if (Address != null)
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

                    return url + "probe";
                }
                else return null;
            }
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

                        // Get Header_Devices object from XmlDocument
                        Header_Devices header = ProcessHeader(Root);

                        // Get Device object from RootNode
                        List<Device> devices = ProcessDevices(Root);

                        // Create ReturnData object to send as Event argument
                        result = new TH_MTC_Data.Components.ReturnData();
                        result.devices = devices;
                        result.header = header;
                    }
                }
                catch (Exception ex) { if (Verbose) Logger.Log("stream_ResponseReceived() :: Exception :: " + ex.Message); }
            }

            return result;
        }

        List<Device> ProcessDevices(XmlElement Root)
        {
            List<Device> Result = new List<Device>();

            XmlNodeList DeviceNodes = Root.GetElementsByTagName("Device");

            if (DeviceNodes != null)
            {
                foreach (XmlElement deviceNode in DeviceNodes)
                {
                    Device device = ProcessDevice(deviceNode);
                    if (device != null) Result.Add(device);
                }
            }
            return Result;
        }

        Device ProcessDevice(XmlElement element)
        {
            Device Result = null;

            XmlNode DeviceNode = element;

            Device device = TH_MTC_Data.Components.Tools.GetDeviceFromXML(DeviceNode);

            DataItemCollection DIC = TH_MTC_Data.Components.Tools.GetDataItemsFromDevice(device);

            Result = device;

            return Result;
        }

        Header_Devices ProcessHeader(XmlElement Root)
        {
            Header_Devices Result = null;

            XmlNodeList HeaderNodes = Root.GetElementsByTagName("Header");

            if (HeaderNodes != null)
            {
                if (HeaderNodes.Count > 0)
                {
                    XmlNode HeaderNode = HeaderNodes[0];

                    Header_Devices header = new Header_Devices(HeaderNode);

                    Result = header;
                }
            }
            return Result;
        }

        #endregion







        #region "Stream Management"

        //Stream stream;

        //private void ProbeRecieved(TH_MTC_Data.Components.ReturnData returnData)
        //{
        //    ProbeFinishedDelly handler = ProbeFinished;
        //    if (null != handler) handler(returnData, this);
        //}

        //private void Stream_Start()
        //{
        //    stream = new Stream();

        //    if (configuration.Agent.IP_Address != String.Empty && configuration.Agent.IP_Address != null)
        //    {
        //        string url = "http://";

        //        // Add Ip Address
        //        string ip = configuration.Agent.IP_Address;
                
        //        // Add Port
        //        string port = null;
        //        // If port is in ip address
        //        if (ip.Contains(":"))
        //        {
        //            int colonindex = ip.LastIndexOf(':');
        //            int slashindex = -1;

        //            // Get index of last forward slash
        //            if (ip.Contains("/")) slashindex = ip.IndexOf('/', colonindex);

        //            // Get port based on indexes
        //            if (slashindex > colonindex) port = ":" + ip.Substring(colonindex + 1, slashindex - colonindex - 1) + "/";
        //            else port = ":" + ip.Substring(colonindex + 1) + "/";
                        
        //            ip = ip.Substring(0, colonindex);
        //        }
        //        else
        //        {
        //            if (configuration.Agent.Port > 0) port = ":" + configuration.Agent.Port.ToString() + "/";
        //        }

        //        url += ip;
        //        url += port;

        //        // Add Device Name
        //        string deviceName = null;
        //        if (configuration.Agent.Device_Name != String.Empty)
        //        {
        //            if (port != null) deviceName = configuration.Agent.Device_Name;
        //            else deviceName = "/" + configuration.Agent.Device_Name;
        //            deviceName += "/";
        //        }
        //        url += deviceName;

        //        if (url[url.Length - 1] != '/') url += "/";

        //        //stream.url = new Uri(url + "probe");
        //        stream.url = url + "probe";
        //        stream.interval = 0;
        //        stream.HttpTimeout = 3000;
        //        stream.InsureDelivery = false;
        //        stream.ResponseReceived += stream_ResponseReceived;
        //        stream.ResponseError += stream_ResponseError;

        //        URL = stream.url.ToString();
        //        if (Verbose) Console.WriteLine("Connecting Probe @ : " + URL);
        //        stream.Start();
        //    }
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

        //                // Get Header_Devices object from XmlDocument
        //                Header_Devices header = ProcessHeader(Root);

        //                // Get Device object from RootNode
        //                List<Device> devices = ProcessDevices(Root);

        //                //Device device = ProcessDevice(Root);

        //                // Create ReturnData object to send as Event argument
        //                TH_MTC_Data.Components.ReturnData returnData = new TH_MTC_Data.Components.ReturnData();
        //                returnData.devices = devices;
        //                //returnData.device = device;
        //                //returnData.xmlDocument = Document;
        //                returnData.header = header;

        //                // Raise ProbeRecieved Event
        //                ProbeRecieved(returnData);
        //            }
        //        }
        //        catch (Exception ex) { if (Verbose) Logger.Log("stream_ResponseReceived() :: Exception :: " + ex.Message); }            
        //    }
        //}

        //void stream_ResponseError(Error error)
        //{
        //    ErrorData data = new ErrorData();
        //    data.message = error.message;
        //    data.probe = this;

        //    if (ProbeError != null) ProbeError(data);
        //}

        //List<Device> ProcessDevices(XmlElement Root)
        //{
        //    List<Device> Result = new List<Device>();

        //    XmlNodeList DeviceNodes = Root.GetElementsByTagName("Device");

        //    if (DeviceNodes != null)
        //    {
        //        foreach (XmlElement deviceNode in DeviceNodes)
        //        {
        //            Device device = ProcessDevice(deviceNode);
        //            if (device != null) Result.Add(device);
        //        }


        //        //if (DeviceNodes.Count > 0)
        //        //{
        //        //    XmlNode DeviceNode = DeviceNodes[0];

        //        //    Device device = TH_MTC_Data.Components.Tools.GetDeviceFromXML(DeviceNode);

        //        //    DataItemCollection DIC = TH_MTC_Data.Components.Tools.GetDataItemsFromDevice(device);

        //        //    Result = device;
        //        //}
        //    }
        //    return Result;
        //}

        //Device ProcessDevice(XmlElement element)
        //{
        //    Device Result = null;

        //    //XmlNodeList DeviceNodes = Root.GetElementsByTagName("Device");

        //    XmlNode DeviceNode = element;

        //    Device device = TH_MTC_Data.Components.Tools.GetDeviceFromXML(DeviceNode);

        //    DataItemCollection DIC = TH_MTC_Data.Components.Tools.GetDataItemsFromDevice(device);

        //    Result = device;

        //    //if (DeviceNodes != null)
        //    //{
        //    //    if (DeviceNodes.Count > 0)
        //    //    {
        //    //        XmlNode DeviceNode = DeviceNodes[0];

        //    //        Device device = TH_MTC_Data.Components.Tools.GetDeviceFromXML(DeviceNode);

        //    //        DataItemCollection DIC = TH_MTC_Data.Components.Tools.GetDataItemsFromDevice(device);

        //    //        Result = device;
        //    //    }
        //    //}
        //    return Result;
        //}

        //Header_Devices ProcessHeader(XmlElement Root)
        //{
        //    Header_Devices Result = null;

        //    XmlNodeList HeaderNodes = Root.GetElementsByTagName("Header");

        //    if (HeaderNodes != null)
        //    {
        //        if (HeaderNodes.Count > 0)
        //        {
        //            XmlNode HeaderNode = HeaderNodes[0];

        //            Header_Devices header = new Header_Devices(HeaderNode);

        //            Result = header;
        //        }
        //    }
        //    return Result;
        //}

        #endregion

    }
}
