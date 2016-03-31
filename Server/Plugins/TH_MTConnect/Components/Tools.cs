// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using System.Xml;

namespace TH_MTConnect.Components
{
    public static class Tools
    {
        public static Device GetDeviceFromXML(XmlNode DeviceNode)
        {

            Device Result = null;

            if (DeviceNode != null)
            {

                Device device = new Device(DeviceNode);

                foreach (XmlNode ChildNode in DeviceNode.ChildNodes)
                {
                    if (ChildNode.NodeType == XmlNodeType.Element)
                    {
                        switch (ChildNode.Name.ToLower())
                        {
                            case "components":

                                device.Components.AddRange(ProcessComponents(ChildNode));

                                break;

                            case "dataitems":

                                device.DataItems = ProcessDataItems(ChildNode);

                                break;

                            case "description":

                                device.Description = new Description(ChildNode);

                                break;
                        }
                    }
                }

                Result = device;

            }

            return Result;

        }

        static List<Component> ProcessComponents(XmlNode ComponentsNode)
        {
            List<Component> Result = new List<Component>();

            foreach (XmlNode ChildNode in ComponentsNode.ChildNodes)
            {
                Component component = new Component(ChildNode);

                foreach (XmlNode ChildChildNode in ChildNode.ChildNodes)
                {
                    if (ChildChildNode.NodeType == XmlNodeType.Element)
                    {
                        switch (ChildChildNode.Name.ToLower())
                        {
                            case "components":

                                component.Components.AddRange(ProcessComponents(ChildChildNode));

                                break;

                            case "dataitems":

                                component.DataItems = ProcessDataItems(ChildChildNode);

                                break;

                            case "description":

                                component.Description = new Description(ChildChildNode);

                                break;
                        }
                    }
                }

                Result.Add(component);
            }

            return Result;
        }

        static DataItemCollection ProcessDataItems(XmlNode DataItemsNode)
        {
            DataItemCollection Result = new DataItemCollection();

            foreach (XmlNode ChildNode in DataItemsNode.ChildNodes)
            {
                if (ChildNode.NodeType == XmlNodeType.Element)
                {
                    if (ChildNode.Attributes != null)
                    {
                        if (ChildNode.Attributes["category"] != null)
                        {

                            var dataItem = new DataItem(ChildNode);

                            foreach (XmlNode ChildChildNode in ChildNode.ChildNodes)
                            {
                                if (ChildChildNode.NodeType == XmlNodeType.Element)
                                {
                                    switch (ChildChildNode.Name.ToLower())
                                    {
                                        case "source":

                                            dataItem.Source = new Source(ChildChildNode);
                                            break;

                                        case "constraints":

                                            dataItem.Constraints = ProcessConstraints(ChildChildNode);

                                            break;
                                    }
                                }
                            }

                            // Add to corresponding List in DataItemCollection object
                            if (dataItem.Category != null)
                            {
                                switch (dataItem.Category.ToLower())
                                {
                                    case "condition":

                                        Result.Conditions.Add(dataItem);

                                        break;

                                    case "event":

                                        Result.Events.Add(dataItem);

                                        break;

                                    case "sample":

                                        Result.Samples.Add(dataItem);

                                        break;
                                }
                            }                         
                        }
                    }
                }
            }

            return Result;
        }

        static Constraints ProcessConstraints(XmlNode ConstraintsNode)
        {
            var result = new Constraints();

            foreach (XmlNode ChildNode in ConstraintsNode.ChildNodes)
            {
                if (ChildNode.NodeType == XmlNodeType.Element)
                {
                    switch (ChildNode.Name.ToLower())
                    {
                        case "value": result.Value = ChildNode.InnerText; break;
                        case "minimum": result.Minimum = ChildNode.InnerText; break;
                        case "maximum": result.Maximum = ChildNode.InnerText; break;

                        case "filter": result.Filter = new Filter(ChildNode); break;
                    }
                }
            }

            return result;
        }

        public static DataItemCollection GetDataItemsFromDevice(Device device)
        {
            var result = new DataItemCollection();

            foreach (DataItem item in device.DataItems.Conditions) result.Conditions.Add(item);
            foreach (DataItem item in device.DataItems.Events) result.Events.Add(item);
            foreach (DataItem item in device.DataItems.Samples) result.Samples.Add(item);

            foreach (Component component in device.Components)
            {
                DataItemCollection collection = GetDataItemsFromComponent(component);

                foreach (DataItem item in collection.Conditions) result.Conditions.Add(item);
                foreach (DataItem item in collection.Events) result.Events.Add(item);
                foreach (DataItem item in collection.Samples) result.Samples.Add(item);
            }

            return result;
        }

        static DataItemCollection GetDataItemsFromComponent(Component component)
        {
            var result = new DataItemCollection();

            foreach (DataItem item in component.DataItems.Conditions) result.Conditions.Add(item);
            foreach (DataItem item in component.DataItems.Events) result.Events.Add(item);
            foreach (DataItem item in component.DataItems.Samples) result.Samples.Add(item);

            foreach (Component childcomponent in component.Components)
            {
                DataItemCollection collection = GetDataItemsFromComponent(childcomponent);

                foreach (DataItem item in collection.Conditions) result.Conditions.Add(item);
                foreach (DataItem item in collection.Events) result.Events.Add(item);
                foreach (DataItem item in collection.Samples) result.Samples.Add(item);
            }

            return result;
        }
    }
}
