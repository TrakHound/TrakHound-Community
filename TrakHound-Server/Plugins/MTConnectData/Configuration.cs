// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Xml;

using TrakHound.Configurations;

namespace TrakHound_Server.Plugins.MTConnectData
{
    public class Configuration
    {

        private string _address;
        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        private string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set { _deviceName = value; }
        }

        private int _heartbeat;
        public int Heartbeat
        {
            get { return _heartbeat; }
            set { _heartbeat = value; }
        }

        private string _proxyAddress;
        public string ProxyAddress
        {
            get { return _proxyAddress; }
            set { _proxyAddress = value; }
        }

        private int _proxyPort;
        public int ProxyPort
        {
            get { return _proxyPort; }
            set { _proxyPort = value; }
        }



        public static Configuration Read(XmlDocument configXML)
        {
            var result = new Configuration();

            XmlNodeList nodes = configXML.SelectNodes("//Agent");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {
                    XmlNode node = nodes[0];

                    foreach (XmlNode Child in node.ChildNodes)
                    {
                        if (Child.NodeType == XmlNodeType.Element)
                        {
                            if (Child.InnerText != "")
                            {
                                Type Settings = typeof(Configuration);
                                var info = Settings.GetProperty(Child.Name);

                                if (info != null)
                                {
                                    Type t = info.PropertyType;
                                    info.SetValue(result, Convert.ChangeType(Child.InnerText, t), null);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static Configuration Get(DeviceConfiguration configuration)
        {
            Configuration result = null;

            var customClass = configuration.CustomClasses.Find(x => x.GetType() == typeof(Configuration));
            if (customClass != null) result = (Configuration)customClass;

            return result;
        }
    }
}
