// Copyright(c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Xml;

using TrakHound.Configurations;

namespace TrakHound_Server.Plugins.OEE
{
    public class Configuration
    {
        public Configuration()
        {
            Overrides = new List<string>();
        }

        public string OperatingEventName { get; set; }

        public string OperatingEventValue { get; set; }

        public List<string> Overrides { get; set; }

        public static Configuration Read(XmlDocument xml)
        {
            var result = new Configuration();

            XmlNodeList nodes = xml.SelectNodes("//OEE");

            if (nodes != null && nodes.Count > 0)
            {
                XmlNode node = nodes[0];

                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        if (child.Name.ToLower() == "overrides")
                        {
                            foreach (XmlNode oChild in child.ChildNodes)
                            {
                                if (oChild.NodeType == XmlNodeType.Element)
                                {
                                    if (oChild.Name.ToLower() == "override")
                                    {
                                        var name = oChild.InnerText;
                                        if (!string.IsNullOrEmpty(name)) result.Overrides.Add(name);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var info = typeof(Configuration).GetProperty(child.Name);
                            if (info != null)
                            {
                                Type t = info.PropertyType;
                                info.SetValue(result, Convert.ChangeType(child.InnerText, t), null);
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
