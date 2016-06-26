// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System;
using System.Xml;
using System.Reflection;

using TH_Configuration;

namespace TH_GeneratedData.SnapshotData
{
    public class SnapshotDataConfiguration
    {
        public SnapshotDataConfiguration()
        {
            Snapshots = new List<Snapshot>();
        }

        public List<Snapshot> Snapshots { get; set; }

        public static SnapshotDataConfiguration Read(XmlDocument configXML)
        {
            var result = new SnapshotDataConfiguration();

            XmlNodeList nodes = configXML.SelectNodes("/Settings/GeneratedData/SnapshotData");

            // Test old version of 'SnapShot' instead of 'Snapshot'
            if (nodes != null && nodes.Count == 0) nodes = configXML.SelectNodes("/Settings/GeneratedData/SnapShotData");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {

                    XmlNode node = nodes[0];

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {

                            Type snapshotSetting = typeof(SnapshotDataConfiguration);
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

        public static SnapshotDataConfiguration Get(DeviceConfiguration configuration)
        {
            SnapshotDataConfiguration Result = null;

            var customClass = configuration.CustomClasses.Find(x => x.GetType() == typeof(SnapshotDataConfiguration));
            if (customClass != null) Result = (SnapshotDataConfiguration)customClass;

            return Result;
        }

    }
}
