// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
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
        INCREMENTAL,

        /// <summary>
        /// Event carries total value
        /// </summary>
        TOTAL,
    }

    public enum ValueType
    {
        CAPTURE_ITEM,
        STATIC_INCREMENT
    }

    public class Configuration
    {
        public Configuration()
        {
            Events = new List<PartCountEvent>();
        }

        public List<PartCountEvent> Events { get; set; }


        public static Configuration Read(XmlDocument xml)
        {
            var result = new Configuration();

            XmlNodeList nodes = xml.SelectNodes("//Parts");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {
                    XmlNode node = nodes[0];

                    OldConfiguration oldConfig = null;

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            if (child.Name.ToLower() == "event")
                            {
                                var partCountEvent = new PartCountEvent();

                                foreach (XmlNode eventChild in child.ChildNodes)
                                {
                                    if (eventChild.NodeType == XmlNodeType.Element)
                                    {
                                        if (eventChild.Name.ToLower() == "calculationtype")
                                        {
                                            switch (eventChild.InnerText.ToLower())
                                            {
                                                case "incremental": partCountEvent.CalculationType = CalculationType.INCREMENTAL; break;
                                                case "total": partCountEvent.CalculationType = CalculationType.TOTAL; break;
                                            }
                                        }
                                        else if (eventChild.Name.ToLower() == "valuetype")
                                        {
                                            switch (eventChild.InnerText.ToLower())
                                            {
                                                case "capture_item": partCountEvent.ValueType = ValueType.CAPTURE_ITEM; break;
                                                case "static_increment": partCountEvent.ValueType = ValueType.STATIC_INCREMENT; break;
                                            }
                                        }
                                        else
                                        {
                                            var type = typeof(PartCountEvent);

                                            PropertyInfo info = type.GetProperty(eventChild.Name);
                                            if (info != null)
                                            {
                                                Type t = info.PropertyType;
                                                info.SetValue(partCountEvent, Convert.ChangeType(eventChild.InnerText, t), null);
                                            }
                                        }
                                    }
                                }

                                result.Events.Add(partCountEvent);
                            }
                            else
                            {
                                if (oldConfig == null) oldConfig = new OldConfiguration();

                                if (child.Name.ToLower() == "calculationtype") // Deprecated
                                {
                                    switch (child.InnerText.ToLower())
                                    {
                                        case "incremental": oldConfig.CalculationType = CalculationType.INCREMENTAL; break;
                                        case "total": oldConfig.CalculationType = CalculationType.TOTAL; break;
                                    }
                                }
                                else // Deprecated
                                {
                                    var type = typeof(OldConfiguration);

                                    PropertyInfo info = type.GetProperty(child.Name);
                                    if (info != null)
                                    {
                                        Type t = info.PropertyType;
                                        info.SetValue(oldConfig, Convert.ChangeType(child.InnerText, t), null);
                                    }
                                }
                            }
                        }
                    }

                    if (oldConfig != null)
                    {
                        var partCountEvent = new PartCountEvent();

                        partCountEvent.EventName = oldConfig.PartsEventName;
                        partCountEvent.EventValue = oldConfig.PartsEventValue;
                        partCountEvent.CaptureItemLink = oldConfig.PartsCaptureItemLink;
                        partCountEvent.CalculationType = oldConfig.CalculationType;

                        result.Events.Add(partCountEvent);
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

    /// <summary>
    /// Deprecated. Use Configuration class
    /// </summary>
    public class OldConfiguration
    {
        public string PartsEventName { get; set; }

        public string PartsEventValue { get; set; }

        public string PartsCaptureItemLink { get; set; }

        public CalculationType CalculationType { get; set; }
    }

    public class PartCountEvent
    {
        public string EventName { get; set; }

        public string EventValue { get; set; }

        public string PreviousEventValue { get; set; }


        public ValueType ValueType { get; set; }

        public CalculationType CalculationType { get; set; }


        public string CaptureItemLink { get; set; }

        public int StaticIncrementValue { get; set; }
    }
}
