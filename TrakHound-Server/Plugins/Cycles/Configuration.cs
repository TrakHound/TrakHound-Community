// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using TrakHound.Configurations;

namespace TrakHound_Server.Plugins.Cycles
{
    public class Configuration
    {
        public Configuration()
        {
            OverrideLinks = new List<string>();
            ProductionTypes = new List<ProductionTypeItem>();
        }

        public string CycleEventName { get; set; }

        public string StoppedEventValue { get; set; }

        public List<string> OverrideLinks { get; set; }

        public class ProductionTypeItem
        {
            public string EventValue { get; set; }
            public CycleProductionType ProductionType { get; set; }
        }

        public List<ProductionTypeItem> ProductionTypes { get; set; }

        public string CycleNameLink { get; set; }

        public static Configuration Read(XmlDocument configXML)
        {
            var result = new Configuration();

            XmlNodeList nodes = configXML.SelectNodes("//Cycles");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {
                    XmlNode node = nodes[0];

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            if (child.Name.ToLower() == "productiontypes" )
                            {
                                foreach (XmlNode productionTypeChild in child.ChildNodes)
                                {
                                    if (productionTypeChild.Attributes["name"] != null && productionTypeChild.Attributes["type"] != null)
                                    {
                                        string name = productionTypeChild.Attributes["name"].Value.ToString();

                                        CycleProductionType type;
                                        Enum.TryParse(productionTypeChild.Attributes["type"].Value.ToString(), out type);

                                        var typeItem = new ProductionTypeItem();
                                        typeItem.EventValue = name;
                                        typeItem.ProductionType = type;

                                        result.ProductionTypes.Add(typeItem);
                                    }
                                }
                            }
                            else if (child.Name.ToLower() == "overridelinks")
                            {
                                foreach (XmlNode overrideLinkChild in child.ChildNodes)
                                {
                                    if (overrideLinkChild.InnerText != null) result.OverrideLinks.Add(overrideLinkChild.InnerText);
                                }  
                            }
                            else
                            {
                                var info = typeof(Configuration).GetProperty(child.Name);
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
