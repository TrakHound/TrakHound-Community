// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using TH_Configuration;

namespace TH_InstanceData
{
    public class InstanceConfiguration
    {
        public InstanceConfiguration()
        {
            //DataItems = new InstanceConfiguration_DataItems();
            Omit = new List<string>();
        }

        //public int Number { get; set; }

        public bool Conditions { get; set; }
        public bool Events { get; set; }
        public bool Samples { get; set; }

        public List<string> Omit { get; set; }

        //public InstanceConfiguration_DataItems DataItems;

        public static InstanceConfiguration Read(XmlDocument xml)
        {
            var result = new InstanceConfiguration();

            XmlNodeList nodes = xml.SelectNodes("//InstanceTable");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {

                    XmlNode node = nodes[0];

                    foreach (XmlNode Child in node.ChildNodes)
                    {
                        if (Child.NodeType == XmlNodeType.Element)
                        {
                            // Read Properties
                            if (Child.InnerText != "")
                            {
                                Type Setting = typeof(InstanceConfiguration);
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
                                        case "dataitems":

                                            foreach (XmlNode DataItemNode in Child.ChildNodes)
                                            {
                                                if (DataItemNode.NodeType == XmlNodeType.Element)
                                                {
                                                    switch (DataItemNode.Name.ToLower())
                                                    {
                                                        case "conditions":
                                                            result.Conditions = Convert.ToBoolean(DataItemNode.InnerText);
                                                            break;

                                                        case "events":
                                                            result.Events = Convert.ToBoolean(DataItemNode.InnerText);
                                                            break;

                                                        case "samples":
                                                            result.Samples = Convert.ToBoolean(DataItemNode.InnerText);
                                                            break;

                                                        case "omit":

                                                            foreach (XmlNode OmitNode in DataItemNode.ChildNodes)
                                                            {
                                                                if (OmitNode.NodeType == XmlNodeType.Element)
                                                                {
                                                                    result.Omit.Add(OmitNode.Name.ToUpper());
                                                                }
                                                            }

                                                            break;
                                                    }
                                                }
                                            }

                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static InstanceConfiguration Get(DeviceConfiguration config)
        {
            InstanceConfiguration Result = null;

            var customClass = config.CustomClasses.Find(x => x.GetType() == typeof(InstanceConfiguration));
            if (customClass != null) Result = (InstanceConfiguration)customClass;

            return Result;
        }
    }

    //public class InstanceConfiguration_DataItems
    //{
    //    public InstanceConfiguration_DataItems() { Omit = new List<string>(); }

    //    public bool Conditions { get; set; }
    //    public bool Events { get; set; }
    //    public bool Samples { get; set; }

    //    public List<string> Omit { get; set; }
    //}
}
