// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace TrakHound_Server.Plugins.GeneratedEvents
{
    public class CaptureItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Value { get; set; }
        public string PreviousValue { get; set; }
        public long Sequence { get; set; }

        public static CaptureItem Read(XmlNode node)
        {
            if (node.Attributes != null)
            {
                if (node.Attributes["id"] != null && node.Attributes["link"] != null)
                {
                    var item = new CaptureItem();
                    item.Id = node.Attributes["id"].Value;
                    item.Name = node.Attributes["name"].Value;
                    item.Link = node.Attributes["link"].Value;
                    return item;
                }
            }

            return null;
        }
    }
}
