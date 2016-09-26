// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Reflection;
using System.Xml;

using TrakHound.Configurations;

namespace TrakHound_Server.Plugins.Parts
{
    public enum CalculationType
    {
        /// <summary>
        /// Incrementally increased as each event is added to the total
        /// </summary>
        Incremental,

        /// <summary>
        /// Event carries total value
        /// </summary>
        Total,
    }

    public class Configuration
    {

        public string PartsEventName { get; set; }

        public string PartsEventValue { get; set; }

        public CalculationType CalculationType { get; set; }

        public static Configuration Read(XmlDocument xml)
        {
            var result = new Configuration();

            XmlNodeList nodes = xml.SelectNodes("//Parts");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {
                    XmlNode node = nodes[0];

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            if (child.Name.ToLower() == "calculationtype")
                            {
                                switch (child.InnerText.ToLower())
                                {
                                    case "incremental": result.CalculationType = CalculationType.Incremental; break;
                                    case "total": result.CalculationType = CalculationType.Total; break;
                                }
                            }
                            else
                            {
                                Type Setting = typeof(Configuration);
                                PropertyInfo info = Setting.GetProperty(child.Name);

                                if (info != null)
                                {
                                    Type t = info.PropertyType;
                                    info.SetValue(result, Convert.ChangeType(child.InnerText, t), null);
                                }
                            } 
                        }
                    }
                }
            }

            return result;
        }

        public static Configuration Get(DeviceConfiguration configuration)
        {
            Configuration result = null;

            var customClass = configuration.CustomClasses.Find(x => x.GetType() == typeof(Configuration));
            if (customClass != null) result = (Configuration)customClass;

            return result;
        }

    }
}
