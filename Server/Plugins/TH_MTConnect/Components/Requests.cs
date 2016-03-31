// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Xml;
using System.Collections.Generic;

using TH_MTConnect;
using TH_MTConnect.Components;

namespace TH_MTConnect.Components
{
    public static class Requests
    {
        public static ReturnData Get(string url, HTTP.ProxySettings proxySettings, int timeout = 10000, int maxattempts = 3)
        {
            ReturnData result = null;

            string response = HTTP.GetData(url, proxySettings, timeout, maxattempts);

            if (response != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);

                if (doc.DocumentElement != null)
                {
                    // Get Root Element from Xml Document
                    XmlElement root = doc.DocumentElement;

                    // Get Header_Devices object from Root node
                    Header_Devices header = GetHeader(root);

                    // Get Device object from Root node
                    List<Device> devices = GetDevices(root);

                    if (devices != null)
                    {
                        result = new ReturnData();
                        result.Header = header;
                        result.Devices = devices;
                    }
                }
            }

            return result;
        }

        static List<Device> GetDevices(XmlElement root)
        {
            List<Device> result = null;

            XmlNodeList deviceNodes = root.GetElementsByTagName("Device");

            if (deviceNodes != null)
            {
                if (deviceNodes.Count > 0)
                {
                    result = new List<Device>();

                    foreach (XmlElement deviceNode in deviceNodes)
                    {
                        Device device = Tools.GetDeviceFromXML(deviceNode);

                        device.DataItems = Tools.GetDataItemsFromDevice(device);

                        result.Add(device);
                    }
                }              
            }

            return result;
        }

        static Header_Devices GetHeader(XmlElement root)
        {
            Header_Devices result = null;

            XmlNodeList headerNodes = root.GetElementsByTagName("Header");

            if (headerNodes != null)
            {
                if (headerNodes.Count > 0)
                {
                    XmlNode headerNode = headerNodes[0];

                    var header = new Header_Devices(headerNode);

                    result = header;
                }
            }

            return result;
        }

    }
}
