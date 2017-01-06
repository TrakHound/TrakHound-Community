// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using TrakHound.Configurations;

namespace TrakHound_Server.Plugins.SnapshotData
{
    public class Configuration
    {
        public Configuration()
        {
            Snapshots = new List<Snapshot>();
        }

        public List<Snapshot> Snapshots { get; set; }

        public static Configuration Read(XmlDocument configXML)
        {
            var result = new Configuration();

            XmlNodeList nodes = configXML.SelectNodes("//SnapshotData");

            // Test old version of 'SnapShot' instead of 'Snapshot'
            if (nodes != null && nodes.Count == 0) nodes = configXML.SelectNodes("//GeneratedData/SnapShotData");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {

                    XmlNode node = nodes[0];

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {

                            Type snapshotSetting = typeof(Configuration);
                            PropertyInfo snapshotinfo = snapshotSetting.GetProperty(child.Name);

                            if (snapshotinfo != null)
                            {
                                Type t = snapshotinfo.PropertyType;
                                snapshotinfo.SetValue(result, Convert.ChangeType(child.InnerText, t), null);
                            }
                            else if (child.Attributes != null)
                            {
                                if (child.Attributes["name"] != null && child.Attributes["link"] != null)
                                {
                                    var item = new Snapshot();

                                    string type = child.Name.ToLower();
                                    switch (type)
                                    {
                                        case "collected": item.Type = SnapshotType.Collected; break;
                                        case "generated": item.Type = SnapshotType.Generated; break;
                                        case "variable": item.Type = SnapshotType.Variable; break;
                                    }

                                    item.Name = child.Attributes["name"].Value;
                                    item.Link = child.Attributes["link"].Value;

                                    result.Snapshots.Add(item);
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
            Configuration Result = null;

            var customClass = configuration.CustomClasses.Find(x => x.GetType() == typeof(Configuration));
            if (customClass != null) Result = (Configuration)customClass;

            return Result;
        }

    }
}
