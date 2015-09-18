// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using System.Xml;

namespace TH_MTC_Data.Components
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

                                device.components.AddRange(ProcessComponents(ChildNode));

                                break;

                            case "dataitems":

                                device.dataItems = ProcessDataItems(ChildNode);

                                break;

                            case "description":

                                device.description = new Description(ChildNode);

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

                                component.components.AddRange(ProcessComponents(ChildChildNode));

                                break;

                            case "dataitems":

                                component.dataItems = ProcessDataItems(ChildChildNode);

                                break;

                            case "description":

                                component.description = new Description(ChildChildNode);

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

                            DataItem dataItem = new DataItem(ChildNode);

                            foreach (XmlNode ChildChildNode in ChildNode.ChildNodes)
                            {
                                if (ChildChildNode.NodeType == XmlNodeType.Element)
                                {
                                    switch (ChildChildNode.Name.ToLower())
                                    {
                                        case "source":

                                            dataItem.source = new DataItem.Source(ChildChildNode);
                                            break;

                                        case "constraints":

                                            dataItem.constraints = ProcessConstraints(ChildChildNode);

                                            break;
                                    }
                                }
                            }

                            // Add to corresponding List in DataItemCollection object
                            switch (dataItem.category.ToLower())
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

            return Result;
        }

        static DataItem.Constraints ProcessConstraints(XmlNode ConstraintsNode)
        {
            DataItem.Constraints Result = new DataItem.Constraints();

            foreach (XmlNode ChildNode in ConstraintsNode.ChildNodes)
            {
                if (ChildNode.NodeType == XmlNodeType.Element)
                {
                    switch (ChildNode.Name.ToLower())
                    {
                        case "value": Result.value = ChildNode.InnerText; break;
                        case "minimum": Result.minimum = ChildNode.InnerText; break;
                        case "maximum": Result.maximum = ChildNode.InnerText; break;

                        case "filter": Result.filter = new DataItem.Constraints.Filter(ChildNode); break;
                    }
                }
            }

            return Result;
        }

        public static DataItemCollection GetDataItemsFromDevice(Device device)
        {
            DataItemCollection DIC = new DataItemCollection();

            foreach (DataItem condition_DI in device.dataItems.Conditions) DIC.Conditions.Add(condition_DI);
            foreach (DataItem event_DI in device.dataItems.Events) DIC.Events.Add(event_DI);
            foreach (DataItem sample_DI in device.dataItems.Samples) DIC.Samples.Add(sample_DI);

            foreach (Component component in device.components)
            {
                DataItemCollection CDIC = GetDataItemsFromComponent(component);

                foreach (DataItem condition_DI in CDIC.Conditions) DIC.Conditions.Add(condition_DI);
                foreach (DataItem event_DI in CDIC.Events) DIC.Events.Add(event_DI);
                foreach (DataItem sample_DI in CDIC.Samples) DIC.Samples.Add(sample_DI);
            }

            return DIC;
        }

        static DataItemCollection GetDataItemsFromComponent(Component component)
        {
            DataItemCollection DIC = new DataItemCollection();

            foreach (DataItem condition_DI in component.dataItems.Conditions) DIC.Conditions.Add(condition_DI);
            foreach (DataItem event_DI in component.dataItems.Events) DIC.Events.Add(event_DI);
            foreach (DataItem sample_DI in component.dataItems.Samples) DIC.Samples.Add(sample_DI);

            foreach (Component childcomponent in component.components)
            {
                DataItemCollection CDIC = GetDataItemsFromComponent(childcomponent);

                foreach (DataItem condition_DI in CDIC.Conditions) DIC.Conditions.Add(condition_DI);
                foreach (DataItem event_DI in CDIC.Events) DIC.Events.Add(event_DI);
                foreach (DataItem sample_DI in CDIC.Samples) DIC.Samples.Add(sample_DI);
            }

            return DIC;
        }
    }
}
