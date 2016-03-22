using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;

using TH_Configuration;

namespace TH_InstanceTable
{
    public partial class InstanceTable
    {
        public class InstanceConfiguration
        {
            public InstanceConfiguration() { DataItems = new InstanceConfiguration_DataItems(); }

            public int number { get; set; }

            public InstanceConfiguration_DataItems DataItems;

            public static InstanceConfiguration Read(XmlDocument xml)
            {

                InstanceConfiguration result = new InstanceConfiguration();

                XmlNodeList nodes = xml.SelectNodes("/Settings/InstanceTable");

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
                                                                result.DataItems.Conditions = Convert.ToBoolean(DataItemNode.InnerText);
                                                                break;

                                                            case "events":
                                                                result.DataItems.Events = Convert.ToBoolean(DataItemNode.InnerText);
                                                                break;

                                                            case "samples":
                                                                result.DataItems.Samples = Convert.ToBoolean(DataItemNode.InnerText);
                                                                break;

                                                            case "omit":

                                                                foreach (XmlNode OmitNode in DataItemNode.ChildNodes)
                                                                {
                                                                    if (OmitNode.NodeType == XmlNodeType.Element)
                                                                    {
                                                                        result.DataItems.Omit.Add(OmitNode.Name.ToUpper());
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

            public static InstanceConfiguration Get(Configuration config)
            {
                InstanceConfiguration Result = null;

                var customClass = config.CustomClasses.Find(x => x.GetType() == typeof(InstanceConfiguration));
                if (customClass != null) Result = (InstanceConfiguration)customClass;

                return Result;

            }
        }

        public class InstanceConfiguration_DataItems
        {
            public InstanceConfiguration_DataItems() { Omit = new List<string>(); }

            public bool Conditions { get; set; }
            public bool Events { get; set; }
            public bool Samples { get; set; }

            public List<string> Omit;
        }

    }
}
