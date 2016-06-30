using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Reflection;

namespace TH_Global.TrakHound
{
    public class ApiConfiguration
    {
        public string Address { get; set; }


        private const string CONFIG_FILENAME = "api_config.xml";
        private static string CONFIG_FILEPATH = FileLocations.TrakHound + @"\" + CONFIG_FILENAME;

        public static ApiConfiguration Read()
        {
            var result = new ApiConfiguration();

            if (File.Exists(CONFIG_FILEPATH))
            {
                try
                {
                    var xml = new XmlDocument();
                    xml.Load(CONFIG_FILEPATH);

                    foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                    {
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            if (node.InnerText != "")
                            {
                                Type c = typeof(ApiConfiguration);
                                PropertyInfo info = c.GetProperty(node.Name);

                                if (info != null)
                                {
                                    Type t = info.PropertyType;
                                    info.SetValue(result, Convert.ChangeType(node.InnerText, t), null);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { Logger.Log("Exception :: " + ex.Message); }
            }

            return result;
        }

    }
}
