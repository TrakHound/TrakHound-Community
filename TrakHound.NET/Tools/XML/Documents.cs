//  Copyright 2017 TrakHound Inc.
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

using NLog;
using System;
using System.IO;
using System.Xml;

namespace TrakHound.Tools.XML
{
    public static class Documents
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static XmlDocument FromString(string str)
        {
            try
            {
                var xml = new XmlDocument();
                xml.LoadXml(str);

                return xml;
            }
            catch (Exception ex) { logger.Error(ex); }

            return null;
        }

        public static string ToString(XmlDocument doc)
        {
            try
            {
                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    return stringWriter.GetStringBuilder().ToString();
                }
            }
            catch (Exception ex) { logger.Error(ex); }

            return null;
        }

    }
}
