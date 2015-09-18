// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using System.Xml;
using System.Data;

using TH_Configuration;
using TH_MTC_Data;
using TH_MTC_Data.Streams;

namespace TH_MTC_Requests
{
    public class Current
    {

        #region "Public"

        public event Connection_Handler Started;
        public event Connection_Handler Stopped;

        public MTC_Stream_Status Status;

        public Configuration configuration { get; set; }

        public delegate void CurrentFinishedDelly(ReturnData returnData);
        public event CurrentFinishedDelly CurrentFinished;

        public Current()
        {
            Status = MTC_Stream_Status.Stopped;
        }

        Thread worker;

        public void Start(int Heartbeat)
        {
            worker = new Thread(new ParameterizedThreadStart(Stream_Start));
            worker.Start(Heartbeat);

            //Stream_Start(Heartbeat);
        }

        public void Stop()
        {
            Stream_Stop();
        }

        #endregion

        #region "Stream Management"

        Stream stream;

        private void CurrentRecieved(ReturnData returnData)
        {
            CurrentFinishedDelly handler = CurrentFinished;
            if (null != handler) handler(returnData);
        }

        private void Stream_Start(object heartbeat)
        {
            int Heartbeat = (int)heartbeat;

            stream = new Stream();

            string port;
            if (configuration.Agent.Port > 0)
                port = ":" + configuration.Agent.Port.ToString();
            else
                port = null;

            stream.uri = new Uri("http://" + configuration.Agent.IP_Address + port + "/" + configuration.Agent.Device_Name + "/current");
            stream.interval = Heartbeat;

            stream.ResponseReceived += stream_ResponseReceived;

            Console.WriteLine("Connecting Current @ : " + stream.uri);
            stream.Start();
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

        private void Stream_Stop()
        {
            if (stream != null) stream.Stop();
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

                    // Get Header_Streams object from Root node
                    Header_Streams header = ProcessHeader(Root);

                    // Get DeviceStream object from Root node
                    DeviceStream deviceStream = ProcessDeviceStream(Root);

                    // Create ReturnData object to send as Event argument
                    ReturnData returnData = new ReturnData();
                    returnData.deviceStream = deviceStream;
                    //returnData.xmlDocument = Document;
                    returnData.header = header;

                    // Raise CurrentReceived Event
                    CurrentRecieved(returnData);
                }
            }
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
