// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using System.Xml;
using System.Data;

using TH_Configuration;
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

        public Configuration configuration { get; set; }

        public delegate void ProbeFinishedDelly(TH_MTC_Data.Components.ReturnData returnData);
        public event ProbeFinishedDelly ProbeFinished;

        public Probe()
        {
            Status = MTC_Stream_Status.Stopped;
        }

        Thread worker;

        public void Start()
        {
            worker = new Thread(new ThreadStart(Stream_Start));
            worker.Start();

            //Stream_Start();
        }

        public void Stop()
        {
            if (worker != null) worker.Abort();

            //Stream_Stop();
        }

        #endregion

        #region "Stream Management"

        Stream stream;

        private void ProbeRecieved(TH_MTC_Data.Components.ReturnData returnData)
        {
            ProbeFinishedDelly handler = ProbeFinished;
            if (null != handler) handler(returnData);
        }

        private void Stream_Start()
        {
            stream = new Stream();

            string port;
            if (configuration.Agent.Port > 0)
                port = ":" + configuration.Agent.Port.ToString();
            else
                port = null;

            stream.uri = new Uri("http://" + configuration.Agent.IP_Address + port + "/" + configuration.Agent.Device_Name + "/probe");

            stream.ResponseReceived += stream_ResponseReceived;

            Console.WriteLine("Connecting Probe @ : " + stream.uri);
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
            if (responseString != null)
            {
                XmlDocument Document = new XmlDocument();
                Document.LoadXml(responseString);

                if (Document.DocumentElement != null)
                {
                    // Get Root Element from Xml Document
                    XmlElement Root = Document.DocumentElement;

                    // Get Header_Devices object from XmlDocument
                    Header_Devices header = ProcessHeader(Root);

                    // Get Device object from RootNode
                    Device device = ProcessDevice(Root);

                    // Create ReturnData object to send as Event argument
                    TH_MTC_Data.Components.ReturnData returnData = new TH_MTC_Data.Components.ReturnData();
                    returnData.device = device;
                    //returnData.xmlDocument = Document;
                    returnData.header = header;

                    // Raise ProbeRecieved Event
                    ProbeRecieved(returnData);
                }
            }
        }

        Device ProcessDevice(XmlElement Root)
        {
            Device Result = null;

            XmlNodeList DeviceNodes = Root.GetElementsByTagName("Device");

            if (DeviceNodes != null)
            {
                if (DeviceNodes.Count > 0)
                {
                    XmlNode DeviceNode = DeviceNodes[0];

                    Device device = TH_MTC_Data.Components.Tools.GetDeviceFromXML(DeviceNode);

                    DataItemCollection DIC = TH_MTC_Data.Components.Tools.GetDataItemsFromDevice(device);

                    Result = device;
                }
            }
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

    }
}
