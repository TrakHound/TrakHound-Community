using System;
using System.Xml;
using System.Collections.Generic;

using TH_Global.Web;
using TH_MTC_Data;
using TH_MTC_Data.Components;

namespace TH_MTC_Data.Components
{
    public static class Requests
    {
        public static ReturnData Get(string url)
        {
            ReturnData result = null;

            string response = HTTP.GetData(url);

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
                        result.header = header;
                        result.devices = devices;
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

                        device.dataItems = Tools.GetDataItemsFromDevice(device);

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

                    Header_Devices header = new Header_Devices(headerNode);

                    result = header;
                }
            }

            return result;
        }

    }
}
