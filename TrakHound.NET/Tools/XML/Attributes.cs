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

using System.Xml;

namespace TrakHound.Tools.XML
{
    public static class Attributes
    {

        public static bool Set(XmlDocument doc, string xPath, string attributeName, string attributeValue)
        {
            bool result = false;

            if (doc != null)
            {
                XmlElement element = doc.DocumentElement;

                var node = element.SelectSingleNode(xPath);
                if (node == null) node = Nodes.Add(doc, xPath);
                if (node != null)
                {
                    var attr = doc.CreateAttribute(attributeName);
                    attr.Value = attributeValue;

                    node.Attributes.SetNamedItem(attr);

                    result = true;
                }
            }

            return result;
        }

        public static bool Set(XmlDocument doc, XmlNode node, string attributeName, string attributeValue)
        {
            bool result = false;

            if (node != null && node.Attributes != null)
            {
                var attr = doc.CreateAttribute(attributeName);
                attr.Value = attributeValue;

                node.Attributes.SetNamedItem(attr);

                result = true;
            }

            return result;
        }

        public static string Get(XmlNode node, string attributeName)
        {
            string result = null;

            if (node != null && node.Attributes != null)
            {
                var attribute = node.Attributes[attributeName];
                if (attribute != null) result = attribute.Value;
            }

            return result;
        }

    }
}
