using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Reflection;

using TH_Global.TrakHound.Configurations;

namespace TH_Global.TrakHound.Configurations.Converter_Sub_Classes
{
    public static class ObjectToXml
    {

        public static XmlDocument Create(DeviceConfiguration obj)
        {
            XmlDocument result = new XmlDocument();

            // Insert XML Declaration
            XmlDeclaration xmlDeclaration = result.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = result.DocumentElement;
            result.InsertBefore(xmlDeclaration, root);

            //XmlElement configuration = result.CreateElement(string.Empty, "Settings", string.Empty);
            XmlElement configuration = result.CreateElement(string.Empty, "DeviceConfiguration", string.Empty);
            result.AppendChild(configuration);

            foreach (PropertyInfo info in typeof(DeviceConfiguration).GetProperties())
            {
                XmlNode propertyNode = result.CreateElement(string.Empty, info.Name, string.Empty);
                object value = info.GetValue(obj, null);
                if (value != null) propertyNode.InnerText = value.ToString();
                configuration.AppendChild(propertyNode);
            }

            return result;
        }

    }
}
