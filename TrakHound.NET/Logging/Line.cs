using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
//  Copyright 2016 Feenux LLC
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

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
