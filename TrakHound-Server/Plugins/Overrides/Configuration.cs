// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Xml;

using TrakHound.Configurations;
using TrakHound.Tools.XML;

namespace TrakHound_Server.Plugins.Overrides
{

    public class Configuration
    {
        public Configuration()
        {
            Overrides = new List<Override>();
        }

        public List<Override> Overrides { get; set; }


        public static Configuration Read(XmlDocument xml)
        {
            var result = new Configuration();

            var root = xml.DocumentElement;
            XmlNodeList nodes = xml.SelectNodes("/" + root.Name + "/Overrides");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {
                    XmlNode node = nodes[0];

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            if (child.Name.ToLower() == "override")
                            {
                                var o = new Override();
                                o.Name = Attributes.Get(child, "name");
                                o.Link = Attributes.Get(child, "link");

                                string type = Attributes.Get(child, "type");
                                if (!string.IsNullOrEmpty(type))
                                {
                                    switch (type)
                                    {
                                        case "feedrate_override": o.Type = OverrideType.FEEDRATE_OVERRIDE; break;
                                        case "spindle_override": o.Type = OverrideType.SPINDLE_OVERRIDE; break;
                                        case "rapid_override": o.Type = OverrideType.RAPID_OVERRIDE; break;
                                    }
                                }

                                result.Overrides.Add(o);
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
