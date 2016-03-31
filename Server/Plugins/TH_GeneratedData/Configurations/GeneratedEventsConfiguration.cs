// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using TH_GeneratedData.GeneratedEvents;

namespace TH_GeneratedData
{
    public class GeneratedEventsConfiguration
    {
        public GeneratedEventsConfiguration()
        {
            Events = new List<Event>();
            UploadToMySQL = true;
        }

        public bool UploadToMySQL { get; set; }

        public List<Event> Events;

        public static GeneratedEventsConfiguration Read(XmlNode Node)
        {
            var result = new GeneratedEventsConfiguration();

            if (Node.HasChildNodes)
            {
                foreach (XmlNode EventNode in Node.ChildNodes)
                {
                    if (EventNode.InnerText != null)
                    {
                        Type Setting = typeof(GeneratedEventsConfiguration);
                        PropertyInfo info = Setting.GetProperty(EventNode.Name);

                        if (info != null)
                        {
                            Type t = info.PropertyType;
                            info.SetValue(result, Convert.ChangeType(EventNode.InnerText, t), null);
                        }
                        else if (EventNode.Name.ToLower() == "event")
                        {
                            if (EventNode.NodeType == XmlNodeType.Element && EventNode.Attributes != null)
                            {
                                if (EventNode.Attributes["name"] != null)
                                {

                                    var ev = new Event();
                                    ev.Values = new List<Value>();

                                    ev.Name = EventNode.Attributes["name"].Value.ToString();

                                    foreach (XmlNode Childnode in EventNode.ChildNodes)
                                    {
                                        if (Childnode.NodeType == XmlNodeType.Element)
                                        {
                                            switch (Childnode.Name.ToLower())
                                            {

                                                case "value":

                                                    var value = Value.Read(Childnode);
                                                    ev.Values.Add(value);

                                                    break;

                                                case "default":

                                                    var d = Return.Read(Childnode);
                                                    ev.Default = d;

                                                    break;

                                                case "capture":

                                                    foreach (XmlNode itemNode in Childnode.ChildNodes)
                                                    {
                                                        if (itemNode.NodeType == XmlNodeType.Element)
                                                        {
                                                            var item = CaptureItem.Read(itemNode);
                                                            if (item != null) ev.CaptureItems.Add(item);
                                                        }
                                                    }

                                                    break;
                                            }
                                        }
                                    }

                                    ev.CurrentValue = ev.Default;
                                    ev.CurrentValue.TimeStamp = DateTime.MinValue;
                                    result.Events.Add(ev);

                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

    }
}
