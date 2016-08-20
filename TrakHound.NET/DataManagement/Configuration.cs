// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Xml;
using System.Reflection;

namespace TrakHound.DataManagement
{
    public class Configuration
    {
        public const string FILENAME = "database_config.xml";

        public Configuration()
        {
            DatabasePath = Path.Combine(FileLocations.Databases, "trakhound.db");
        }
        
        public string DatabasePath { get; set; }

        public static Configuration ReadXML(XmlNode node)
        {
            var result = new Configuration();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    if (child.InnerText != "")
                    {
                        Type Settings = typeof(Configuration);
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

        public static Configuration Get(object o)
        {
            Configuration result = null;

            if (o != null)
                if (o.GetType() == typeof(Configuration))
                {
                    result = (Configuration)o;
                }

            return result;
        }
    }
}
