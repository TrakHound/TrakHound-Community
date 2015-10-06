using System;
using System.Xml;
using System.Reflection;

namespace TH_MySQL
{
    //public class Configuration
    //{

    //    public string Server { get; set; }
    //    public int Port { get; set; }

    //    public string Database { get; set; }
    //    public string Database_Prefix { get; set; }

    //    public string Username { get; set; }
    //    public string Password { get; set; }

    //    // PHP Settings
    //    public string PHP_Server { get; set; }
    //    public string PHP_Directory { get; set; }
    //    public string Database_Prefix { get; set; }

    //    public static Configuration ReadXML(XmlNode node)
    //    {
    //        Configuration result = new Configuration();

    //        foreach (XmlNode child in node.ChildNodes)
    //        {
    //            if (child.NodeType == XmlNodeType.Element)
    //            {
    //                if (child.InnerText != "")
    //                {
    //                    Type Settings = typeof(Configuration);
    //                    PropertyInfo info = Settings.GetProperty(child.Name);

    //                    if (info != null)
    //                    {
    //                        Type t = info.PropertyType;
    //                        info.SetValue(result, Convert.ChangeType(child.InnerText, t), null);
    //                    }
    //                }
    //            }
    //        }

    //        return result;
    //    }

    //    public static Configuration Get(object o)
    //    {
    //        Configuration result = null;

    //        if (o.GetType() == typeof(Configuration))
    //        {
    //            result = (Configuration)o;
    //        }

    //        return result;
    //    }

    //}



}
