using System;
using System.Xml;
using System.Collections.Generic;

using TH_Global.Web;
using TH_MTC_Data;
using TH_MTC_Data.Streams;

namespace TH_Device_Server.Requests
{
    static class MTC_Current
    {
        public static ReturnData Run(string url)
        {
            ReturnData result = null;

            string response = HTTP.GetData(url);

            if (response != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);

                if (doc.DocumentElement != null)
                {
                    result = new ReturnData();

                    // Get Root Element from Xml Document
                    XmlElement root = doc.DocumentElement;

                    // Get Header_Streams object from Root node
                    result.header = GetHeader(root);

                    // Get DeviceStream object from Root node
                    result.deviceStreams = GetDeviceStream(root);  
                }
            }

            return result;
        }

        public static List<DeviceStream> GetDeviceStream(XmlElement root)
        {
            List<DeviceStream> result = null;

            XmlNodeList deviceStreamNodes = root.GetElementsByTagName("DeviceStream");

            if (deviceStreamNodes != null)
            {
                result = new List<DeviceStream>();

                foreach (XmlElement deviceNode in deviceStreamNodes)
                {
                    DeviceStream deviceStream = Tools.GetDeviceStreamFromXML(deviceNode);

                    deviceStream.dataItems = Tools.GetDataItemsFromDeviceStream(deviceStream);

                    result.Add(deviceStream);
                }
            }

            return result;
        }

        public static Header_Streams GetHeader(XmlElement root)
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
