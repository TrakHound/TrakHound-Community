using System;
using System.Xml;
using System.Reflection;

namespace TH_SQLite
{
    public class SQLite_Configuration
    {
        public string DatabasePath { get; set; }

        public static SQLite_Configuration ReadXML(XmlNode node)
        {
            var result = new SQLite_Configuration();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    if (child.InnerText != "")
                    {
                        Type Settings = typeof(SQLite_Configuration);
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

        public static SQLite_Configuration Get(object o)
        {
            SQLite_Configuration result = null;

            if (o != null)
                if (o.GetType() == typeof(SQLite_Configuration))
                {
                    result = (SQLite_Configuration)o;
                }

            return result;
        }
    }
}
