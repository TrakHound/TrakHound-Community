// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

using TH_Configuration;

namespace TH_OEE
{

    public class OEEConfiguration
    {
        public OEEConfiguration()
        {
            availability = new AvailabilityConfiguration();
            performance = new PeformanceConfiguration();
            quality = new QualityConfiguration();
        }

        public AvailabilityConfiguration availability;
        public PeformanceConfiguration performance;
        public QualityConfiguration quality;

        public static OEEConfiguration Get(Configuration configuration)
        {
            OEEConfiguration result = null;

            var customClass = configuration.CustomClasses.Find(x => x.GetType() == typeof(OEEConfiguration));
            if (customClass != null) result = (OEEConfiguration)customClass;

            return result;
        }

        public static OEEConfiguration Read(XmlDocument configXML)
        {
            var result = new OEEConfiguration();

            XmlNodeList nodes = configXML.SelectNodes("/Settings/OEE");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {
                    XmlNode node = nodes[0];

                    foreach (XmlNode Child in node.ChildNodes)
                    {
                        if (Child.NodeType == XmlNodeType.Element)
                        {
                            Type Setting = typeof(OEEConfiguration);
                            PropertyInfo info = Setting.GetProperty(Child.Name);

                            if (info != null)
                            {
                                Type t = info.PropertyType;
                                info.SetValue(result, Convert.ChangeType(Child.InnerText, t), null);
                            }
                            else
                            {
                                switch (Child.Name.ToLower())
                                {
                                    case "availability": result.availability = AvailabilityConfiguration.Read(Child); break;
                                    case "performance": result.performance = PeformanceConfiguration.Read(Child); break;
                                    case "quality": result.quality = QualityConfiguration.Read(Child); break;
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }

    public class AvailabilityConfiguration
    {
        public string Event { get; set; }
        public string Value { get; set; }

        public static AvailabilityConfiguration Read(XmlNode node)
        {
            var result = new AvailabilityConfiguration();

            foreach (XmlNode Child in node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    Type Setting = typeof(AvailabilityConfiguration);
                    PropertyInfo info = Setting.GetProperty(Child.Name);

                    if (info != null)
                    {
                        Type t = info.PropertyType;
                        info.SetValue(result, Convert.ChangeType(Child.InnerText, t), null);
                    }
                }
            }

            return result;
        }
    }

    public class PeformanceConfiguration
    {
        public bool Enabled { get; set; }

        public static PeformanceConfiguration Read(XmlNode node)
        {
            var result = new PeformanceConfiguration();

            foreach (XmlNode Child in node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    Type Setting = typeof(PeformanceConfiguration);
                    PropertyInfo info = Setting.GetProperty(Child.Name);

                    if (info != null)
                    {
                        Type t = info.PropertyType;
                        info.SetValue(result, Convert.ChangeType(Child.InnerText, t), null);
                    }
                }
            }

            return result;
        }
    }

    public class QualityConfiguration
    {
        public bool Enabled { get; set; }

        public static QualityConfiguration Read(XmlNode node)
        {
            var result = new QualityConfiguration();

            foreach (XmlNode Child in node.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    Type Setting = typeof(QualityConfiguration);
                    PropertyInfo info = Setting.GetProperty(Child.Name);

                    if (info != null)
                    {
                        Type t = info.PropertyType;
                        info.SetValue(result, Convert.ChangeType(Child.InnerText, t), null);
                    }
                }
            }

            return result;
        }
    }

}
