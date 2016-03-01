using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace TH_Global.Functions
{
    public static class XML_Functions
    {

        public static bool SetInnerText(XmlDocument doc, string xPath, string text)
        {
            bool result = false;

            if (doc != null)
            {
                XmlElement element = doc.DocumentElement;

                var node = element.SelectSingleNode(xPath);
                if (node != null)
                {
                    node.InnerText = text;
                    result = true;
                }
            }

            return result;
        }

        public static string GetInnerText(XmlDocument doc, string xPath)
        {
            string result = null;

            if (doc != null)
            {
                XmlElement element = doc.DocumentElement;

                var node = element.SelectSingleNode(xPath);
                if (node != null)
                {
                    result = node.InnerText;
                }
            }

            return result;
        }

        public static XmlDocument StringToXmlDocument(string str)
        {
            XmlDocument result = null;

            try
            {
                var xml = new XmlDocument();
                xml.LoadXml(str);

                result = xml;
            }
            catch (Exception ex) { Logger.Log("XML_Functions.StringToXmlDocument :: Exception :: " + ex.Message); }

            return result;
        }

        public static string XmlDocumentToString(XmlDocument doc)
        {
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter))
            {
                doc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        


    }
}
