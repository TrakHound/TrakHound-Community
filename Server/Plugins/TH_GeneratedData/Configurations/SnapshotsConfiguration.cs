// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System;
using System.Xml;
using System.Reflection;

namespace TH_GeneratedData
{
    public class SnapshotsConfiguration
    {

        public SnapshotsConfiguration()
        {
            Items = new List<Item>();
            UploadToMySQL = true;
        }

        public bool UploadToMySQL { get; set; }

        public List<Item> Items;

        public class Item
        {
            public string type { get; set; }

            public string name { get; set; }
            public string link { get; set; }
        }

        public static SnapshotsConfiguration Read(XmlNode node)
        {
            var result = new SnapshotsConfiguration();

            foreach (XmlNode snapshotNode in node.ChildNodes)
            {
                if (snapshotNode.NodeType == XmlNodeType.Element)
                {
                    Type snapshotSetting = typeof(SnapshotsConfiguration);
                    PropertyInfo snapshotinfo = snapshotSetting.GetProperty(snapshotNode.Name);

                    if (snapshotinfo != null)
                    {
                        Type t = snapshotinfo.PropertyType;
                        snapshotinfo.SetValue(result, Convert.ChangeType(snapshotNode.InnerText, t), null);
                    }
                    else if (snapshotNode.Attributes != null)
                    {
                        if (snapshotNode.Attributes["name"] != null && snapshotNode.Attributes["link"] != null)
                        {
                            var item = new Item();
                            item.type = snapshotNode.Name.ToUpper();
                            item.name = snapshotNode.Attributes["name"].Value;
                            item.link = snapshotNode.Attributes["link"].Value;

                            result.Items.Add(item);
                        }
                    }
                }
            }

            return result;
        }

    }
}
