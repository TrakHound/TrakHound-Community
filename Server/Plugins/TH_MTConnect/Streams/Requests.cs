// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Xml;

using TH_Global;

namespace TH_MTConnect.Streams
{
    public static class Requests
    {
        //public static ReturnData Get(string url, int timeout = 10000, int maxattempts = 3)
        public static ReturnData Get(string url, HTTP.ProxySettings proxySettings, int timeout = 10000, int maxattempts = 3)
        {
            ReturnData result = null;

            string response = HTTP.GetData(url, proxySettings, timeout, maxattempts);

            if (response != null)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response);

                    if (doc.DocumentElement != null)
                    {
                        // Get Root Element from Xml Document
                        XmlElement root = doc.DocumentElement;

                        // Get Header_Streams object from Root node
                        Header_Streams header = GetHeader(root);

                        // Get DeviceStream object from Root node
                        List<DeviceStream> deviceStreams = GetDeviceStream(root);

                        if (deviceStreams != null)
                        {
                            result = new ReturnData();
                            result.Header = header;
                            result.DeviceStreams = deviceStreams;
                        }
                    }
                }
                catch (XmlException ex)
                {
                    Logger.Log("XmlException :: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception :: " + ex.Message);
                }
            }

            return result;
        }

        static List<DeviceStream> GetDeviceStream(XmlElement root)
        {
            List<DeviceStream> result = null;

            XmlNodeList deviceStreamNodes = root.GetElementsByTagName("DeviceStream");

            if (deviceStreamNodes != null)
            {
                if (deviceStreamNodes.Count > 0)
                {
                    result = new List<DeviceStream>();

                    foreach (XmlElement deviceNode in deviceStreamNodes)
                    {
                        DeviceStream deviceStream = Tools.GetDeviceStreamFromXML(deviceNode);

                        deviceStream.DataItems = Tools.GetDataItemsFromDeviceStream(deviceStream);

                        result.Add(deviceStream);
                    }
                }
            }

            return result;
        }

        static Header_Streams GetHeader(XmlElement root)
        {
            Header_Streams result = null;

            XmlNodeList headerNodes = root.GetElementsByTagName("Header");

            if (headerNodes != null)
            {
                if (headerNodes.Count > 0)
                {
                    XmlNode headerNode = headerNodes[0];

                    Header_Streams header = new Header_Streams(headerNode);

                    result = header;
                }
            }

            return result;
        }
    }
}
