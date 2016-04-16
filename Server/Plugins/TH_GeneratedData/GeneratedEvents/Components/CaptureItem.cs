// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Xml;

namespace TH_GeneratedData.GeneratedEvents
{
    public class CaptureItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string link { get; set; }
        public string value { get; set; }
        public string previous_value { get; set; }

        public static CaptureItem Read(XmlNode node)
        {
            if (node.Attributes != null)
            {
                if (node.Attributes["id"] != null && node.Attributes["link"] != null)
                {
                    var item = new CaptureItem();
                    item.id = node.Attributes["id"].Value;
                    item.name = node.Attributes["name"].Value;
                    item.link = node.Attributes["link"].Value;
                    return item;
                }
            }

            return null;
        }
    }
}
