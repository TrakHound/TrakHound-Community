using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Reflection;

using TH_Configuration;

namespace TH_Configuration.Converter_Sub_Classes
{
    public static class ObjectToXml
    {

        public static XmlDocument Create(Configuration obj)
        {
            XmlDocument result = new XmlDocument();

            // Insert XML Declaration
            XmlDeclaration xmlDeclaration = result.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = result.DocumentElement;
            result.InsertBefore(xmlDeclaration, root);

            XmlElement configuration = result.CreateElement(string.Empty, "Settings", string.Empty);
            result.AppendChild(configuration);

            foreach (PropertyInfo info in typeof(Configuration).GetProperties())
            {
                XmlNode propertyNode = result.CreateElement(string.Empty, info.Name, string.Empty);
                object value = info.GetValue(obj, null);
                if (value != null) propertyNode.InnerText = value.ToString();
                configuration.AppendChild(propertyNode);
            }

            return result;
        }

    //    static List<object> GetAllProperties(Configuration config)
    //    {
    //        List<object> result = new List<object>();

    //        foreach (PropertyInfo info in config.GetType().GetProperties())
    //        {
    //            string Value = GetAttribute(Node, info.Name);
    //            if (Value != "")
    //            {
    //                Type t = info.PropertyType;

    //                // Make sure DateTime gets set as UTC
    //                if (t == typeof(DateTime))
    //                {
    //                    DateTime dt = DateTime_Functions.ConvertStringToUTC(Value);
    //                    info.SetValue(obj, dt, null);
    //                }
    //                else
    //                {
    //                    info.SetValue(obj, Convert.ChangeType(Value, t), null);
    //                }


    //            }
    //        }


    //        return result;
    //    }

        

    //    public static XmlNode AddAddress(XmlDocument doc, string address)
    //    {
    //        string[] nodes = address.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

    //        XmlNode parent = doc.DocumentElement;

    //        string xpath = "/";

    //        for (int x = 0; x <= nodes.Length - 1; x++)
    //        {
    //            string node = nodes[x];

    //            string node_id = "";

    //            // Check for node with 'id' attribute, if so then use that in xpath expression
    //            if (node.Contains("||"))
    //            {
    //                int idIndex = node.IndexOf("||");

    //                node_id = @"[@id='" + node.Substring(idIndex + 2) + "']";
    //                node = node.Substring(0, idIndex);
    //            }

    //            xpath += "/" + node + node_id;

    //            XmlNode child = parent.SelectSingleNode(xpath);

    //            if (child == null)
    //            {
    //                child = doc.CreateElement(node);
    //                parent.AppendChild(child);
    //            }
    //            // Make sure last node in address gets added (even though there may be another node with the same 'name')
    //            else if (x == nodes.Length - 1)
    //            {
    //                child = doc.CreateElement(node);
    //                parent.AppendChild(child);
    //            }

    //            parent = child;
    //        }

    //        return parent;
    //    }

    //    public static void Save(XmlDocument doc, string path)
    //    {
    //        XmlWriterSettings settings = new XmlWriterSettings();
    //        settings.Indent = true;

    //        try
    //        {
    //            using (XmlWriter writer = XmlWriter.Create(path, settings))
    //            {
    //                doc.Save(writer);
    //            }
    //        }
    //        catch { }
    //    }

    }
}
