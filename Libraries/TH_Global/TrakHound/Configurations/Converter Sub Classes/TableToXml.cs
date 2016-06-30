// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Xml;

namespace TH_Global.TrakHound.Configurations.Converter_Sub_Classes
{
    public static class TableToXml
    {

        public static XmlDocument Create(DataTable table)
        {
            XmlDocument result = new XmlDocument();

            // Insert XML Declaration
            XmlDeclaration xmlDeclaration = result.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = result.DocumentElement;
            result.InsertBefore(xmlDeclaration, root);

            //XmlElement configuration = result.CreateElement(string.Empty, "Settings", string.Empty);
            XmlElement configuration = result.CreateElement(string.Empty, "DeviceConfiguration", string.Empty);
            result.AppendChild(configuration);

            foreach (DataRow row in table.Rows)
            {
                XmlNode node = AddAddress(result, row["address"].ToString());
                if (node != null)
                {
                    // Set Inner Text (Value) and prevent from adding closing tag by checking for ""
                    if (row["value"] != null) if (row["Value"].ToString() != "") node.InnerText = row["value"].ToString();

                    // Set Attributes
                    if (row["attributes"] != null)
                    {
                        string[] attributes = row["attributes"].ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string attribute in attributes.ToList())
                        {
                            string[] split = attribute.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                            if (split.Length > 1)
                            {
                                string name = split[0];
                                string val = split[1];

                                XmlAttribute a = result.CreateAttribute(name);
                                a.Value = val;
                                node.Attributes.Append(a);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static XmlNode AddAddress(XmlDocument doc, string address)
        {
            string[] nodes = address.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            XmlNode parent = doc.DocumentElement;

            string xpath = "/";

            for (int x = 0; x <= nodes.Length - 1; x++)
            {
                string node = nodes[x];

                string node_id = "";

                // Check for node with 'id' attribute, if so then use that in xpath expression
                if (node.Contains("||"))
                {
                    int idIndex = node.IndexOf("||");

                    node_id = @"[@id='" + node.Substring(idIndex + 2) + "']";
                    node = node.Substring(0, idIndex);
                }

                xpath += "/" + node + node_id;

                XmlNode child = parent.SelectSingleNode(xpath);

                if (child == null)
                {
                    child = doc.CreateElement(node);
                    parent.AppendChild(child);
                }
                // Make sure last node in address gets added (even though there may be another node with the same 'name')
                else if (x == nodes.Length - 1)
                {
                    child = doc.CreateElement(node);
                    parent.AppendChild(child);
                }

                parent = child;
            }

            return parent;
        }

        public static void Save(XmlDocument doc, string path)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            try
            {
                using (XmlWriter writer = XmlWriter.Create(path, settings))
                {
                    doc.Save(writer);
                }
            }
            catch (Exception ex)
            {
                TH_Global.Logger.Log(ex.Message, TH_Global.Logger.LogLineType.Warning);
            }
        }

    }
}
