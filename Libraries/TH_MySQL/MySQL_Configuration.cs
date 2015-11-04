using System;
using System.Xml;
using System.Reflection;

namespace TH_MySQL
{
    public class MySQL_Configuration
    {

        public string Server { get; set; }
        public int Port { get; set; }

        public string Database { get; set; }
        public string Database_Prefix { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        // PHP Settings
        public bool UsePHP { get; set; }
        public string PHP_Server { get; set; }
        public string PHP_Directory { get; set; }

        public static MySQL_Configuration ReadXML(XmlNode node)
        {
            MySQL_Configuration result = new MySQL_Configuration();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    if (child.InnerText != "")
                    {
                        Type Settings = typeof(MySQL_Configuration);
                        PropertyInfo info = Settings.GetProperty(child.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(result, Convert.ChangeType(child.InnerText, t), null);
                        }
                    }
                }
            }

            return result;
        }

        public static MySQL_Configuration Get(object o)
        {
            MySQL_Configuration result = null;

            if (o.GetType() == typeof(MySQL_Configuration))
            {
                result = (MySQL_Configuration)o;
            }

            return result;
        }

    }

}
