// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Xml;

namespace TrakHound.Server.Plugins.GeneratedEvents
{
    public class Return
    {
        public Return() { CaptureItems = new List<CaptureItem>(); }

        public Int16 NumVal { get; set; }
        public string Value { get; set; }
        public DateTime TimeStamp { get; set; }
        public double Duration { get; set; }

        public List<CaptureItem> CaptureItems { get; set; }

        public static Return Read(XmlNode node)
        {
            var result = new Return();

            if (node.Attributes != null)
                if (node.Attributes["numval"] != null)
                    result.NumVal = Convert.ToInt16(node.Attributes["numval"].Value);

            result.Value = node.InnerText;

            return result;
        }
    }
}
