// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

using TrakHound.Configurations;

namespace TrakHound_Server.Plugins.GeneratedEvents
{
    public class Configuration
    {
        public Configuration()
        {
            Events = new List<Event>();
            UploadToMySQL = true;
        }

        public bool UploadToMySQL { get; set; }

        public List<Event> Events;

        public static Configuration Read(XmlDocument configXML)
        {
            var result = new Configuration();

            //XmlNodeList nodes = configXML.SelectNodes("//GeneratedData/GeneratedEvents");
            XmlNodeList nodes = configXML.SelectNodes("//GeneratedEvents");

            if (nodes != null)
            {
                if (nodes.Count > 0)
                {
                    XmlNode node = nodes[0];

                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            if (child.InnerText != null)
                            {
                                Type Setting = typeof(Configuration);
                                PropertyInfo info = Setting.GetProperty(child.Name);

                                if (info != null)
                                {
                                    Type t = info.PropertyType;
                                    info.SetValue(result, Convert.ChangeType(child.InnerText, t), null);
                                }
                                else if (child.Name.ToLower() == "event")
                                {
                                    if (child.NodeType == XmlNodeType.Element && child.Attributes != null)
                                    {
                                        if (child.Attributes["name"] != null)
                                        {

                                            var ev = new Event();
                                            ev.Values = new List<Value>();

                                            ev.Name = child.Attributes["name"].Value.ToString();

                                            foreach (XmlNode Childnode in child.ChildNodes)
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
                                                            d.Id = "DEFAULT_ID";
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

                                            ev.PreviousValue = ev.Default;

                                            result.Events.Add(ev);

                                        }
                                    }
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
            Configuration Result = null;

            var customClass = configuration.CustomClasses.Find(x => x.GetType() == typeof(Configuration));
            if (customClass != null) Result = (Configuration)customClass;

            return Result;
        }

    }
}
