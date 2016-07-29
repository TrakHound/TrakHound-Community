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

using System.Linq;
using System.Xml;

namespace TrakHound.Tools.XML
{
    public static class Nodes
    {

        public static XmlNode Add(XmlDocument doc, string xPath)
        {
            XmlNode result = null;

            if (doc != null)
            {
                XmlElement element = doc.DocumentElement;

                if (doc.DocumentElement != null)
                {
                    var node = CreatePath(doc, doc.DocumentElement as XmlNode, xPath);
                    if (node != null) result = node;
                }
            }

            return result;
        }

        private static XmlNode CreatePath(XmlDocument doc, XmlNode parent, string xPath)
        {
            // grab the next node name in the xpath; or return parent if empty
            string[] partsOfXPath = xPath.Trim('/').Split('/');

            string nextNodeInXPath = partsOfXPath.First();
            if (string.IsNullOrEmpty(nextNodeInXPath)) return parent;

            // get or create the node from the name
            XmlNode node = parent.SelectSingleNode(nextNodeInXPath);
            if (node == null) node = parent.AppendChild(doc.CreateElement(nextNodeInXPath));

            // rejoin the remainder of the array as an xpath expression and recurse
            string rest = string.Join("/", partsOfXPath.Skip(1).ToArray());

            return CreatePath(doc, node, rest);
        }


        public static void Delete(XmlDocument doc, string xPath)
        {
            if (doc != null)
            {
                XmlElement element = doc.DocumentElement;

                if (doc.DocumentElement != null)
                {
                    XmlNode node = doc.SelectSingleNode(xPath);
                    doc.RemoveChild(node);
                }
            }
        }

    }
}
