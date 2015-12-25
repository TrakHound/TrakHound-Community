using System;
using System.Xml;
using System.Collections.Generic;

using TH_Global.Web;
using TH_MTC_Data;
using TH_MTC_Data.Streams;

namespace TH_MTC_Data.Streams
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

                    // Get Header_Streams object from Root node
                    Header_Streams header = GetHeader(root);

                    // Get DeviceStream object from Root node
                    List<DeviceStream> deviceStreams = GetDeviceStream(root);

                    if (deviceStreams != null)
                    {
                        result = new ReturnData();
                        result.header = header;
                        result.deviceStreams = deviceStreams;
                    }
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
                    Console.WriteLine("deviceStreamNodes.Count = " + deviceStreamNodes.Count.ToString());

                    result = new List<DeviceStream>();

                    foreach (XmlElement deviceNode in deviceStreamNodes)
                    {
                        DeviceStream deviceStream = Tools.GetDeviceStreamFromXML(deviceNode);

                        deviceStream.dataItems = Tools.GetDataItemsFromDeviceStream(deviceStream);

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
