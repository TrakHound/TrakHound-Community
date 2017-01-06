// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Xml;

using TrakHound.Tools.XML;

namespace TrakHound.Logging
{
    public class Line
    {
        public long Row { get; set; }

        public LogLineType Type { get; set; }

        public string Text { get; set; }
        public DateTime Timestamp { get; set; }

        public string Assembly { get; set; }
        public string Filename { get; set; }
        public string Member { get; set; }
        public int LineNumber { get; set; }

        public static Line FromXmlNode(XmlNode lineNode)
        {
            var line = new Line();

            line.Text = lineNode.InnerText;

            DateTime ts = DateTime.MinValue;
            DateTime.TryParse(Attributes.Get(lineNode, "timestamp"), out ts);

            line.Timestamp = ts;
            line.Assembly = Attributes.Get(lineNode, "assembly");
            line.Filename = Attributes.Get(lineNode, "filename");
            line.Member = Attributes.Get(lineNode, "member");

            string lineNumber = Attributes.Get(lineNode, "line");
            if (lineNumber != null)
            {
                int n = -1;
                int.TryParse(lineNumber, out n);
                line.LineNumber = n;
            }

            return line;
        }

        public override string ToString()
        {
            string format = "{0} {1}.{2} @ Line={3} :: {4}";
            return string.Format(format, Timestamp.ToString("o"), Assembly, Member, LineNumber, Text);
        }
    }
}
