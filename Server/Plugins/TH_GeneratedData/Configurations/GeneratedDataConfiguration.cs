// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

using System.Data;
using System.Xml;
using System.Reflection;
using System.Threading;
using System.Text.RegularExpressions;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_InstanceTable;
using TH_Plugins;
using TH_Plugins.Server;
using TH_Plugins.Database;

namespace TH_GeneratedData
{
    public class GeneratedDataConfiguration
    {
        public GeneratedDataConfiguration()
        {
            SnapshotsConfiguration = new SnapshotsConfiguration();
            GeneratedEventsConfiguration = new GeneratedEventsConfiguration();
        }

        public SnapshotsConfiguration SnapshotsConfiguration { get; set; }

        public GeneratedEventsConfiguration GeneratedEventsConfiguration { get; set; }


        public static GeneratedDataConfiguration Read(XmlDocument configXML)
        {
            var result = new GeneratedDataConfiguration();

            XmlNodeList nodes = configXML.SelectNodes("/Settings/GeneratedData");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {

                    XmlNode node = nodes[0];

                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        if (childNode.NodeType == XmlNodeType.Element)
                        {

                            Type Setting = typeof(GeneratedDataConfiguration);
                            PropertyInfo info = Setting.GetProperty(childNode.Name);

                            if (info != null)
                            {
                                Type t = info.PropertyType;
                                info.SetValue(result, Convert.ChangeType(childNode.InnerText, t), null);
                            }
                            else
                            {
                                switch (childNode.Name.ToLower())
                                {
                                    case "snapshotdata":

                                        result.SnapshotsConfiguration = SnapshotsConfiguration.Read(childNode);

                                        break;

                                    case "generatedevents":

                                        result.GeneratedEventsConfiguration = GeneratedEventsConfiguration.Read(childNode);

                                        break;

                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static GeneratedDataConfiguration Get(Configuration configuration)
        {
            GeneratedDataConfiguration Result = null;

            var customClass = configuration.CustomClasses.Find(x => x.GetType() == typeof(GeneratedDataConfiguration));
            if (customClass != null) Result = (GeneratedDataConfiguration)customClass;

            return Result;
        }
    }
}
