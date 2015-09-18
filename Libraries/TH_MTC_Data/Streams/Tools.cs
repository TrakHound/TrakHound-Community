// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using System.Xml;

namespace TH_MTC_Data.Streams
{
    public static class Tools
    {

        public static DeviceStream GetDeviceStreamFromXML(XmlNode DeviceNode)
        {
            DeviceStream Result = null;

            if (DeviceNode != null)
            {
                DeviceStream deviceStream = new DeviceStream(DeviceNode);

                foreach (XmlNode ChildNode in DeviceNode.ChildNodes)
                {
                    if (ChildNode.NodeType == XmlNodeType.Element)
                    {
                        switch (ChildNode.Name.ToLower())
                        {
                            case "componentstream":

                                deviceStream.ComponentStreams.Add(ProcessComponentStream(ChildNode));

                                break;

                            case "events":

                                Result.dataItems.Events.AddRange(ProcessEvents(ChildNode));

                                break;

                            case "samples":

                                Result.dataItems.Samples.AddRange(ProcessSamples(ChildNode));

                                break;
                        }
                    }
                }

                Result = deviceStream;
            }

            return Result;
        }

        public static ComponentStream ProcessComponentStream(XmlNode ComponentStreamNode)
        {
            ComponentStream Result = new ComponentStream(ComponentStreamNode);

            foreach (XmlNode DataItemNode in ComponentStreamNode.ChildNodes)
            {
                if (DataItemNode.NodeType == XmlNodeType.Element)
                {
                    switch (DataItemNode.Name.ToLower())
                    {
                        case "condition":

                            Result.dataItems.Conditions.AddRange(ProcessConditions(DataItemNode));

                            break;

                        case "events":

                            Result.dataItems.Events.AddRange(ProcessEvents(DataItemNode));

                            break;

                        case "samples":

                            Result.dataItems.Samples.AddRange(ProcessSamples(DataItemNode));

                            break;
                    }
                }
            }

            return Result;
        }

        public static List<Condition> ProcessConditions(XmlNode ConditionNode)
        {
            List<Condition> Result = new List<Condition>();

            foreach (XmlNode ChildNode in ConditionNode.ChildNodes)
            {
                Condition condition = new Condition(ChildNode);
                Result.Add(condition);
            }

            return Result;
        }

        public static List<Event> ProcessEvents(XmlNode EventsNode)
        {
            List<Event> Result = new List<Event>();

            foreach (XmlNode EventNode in EventsNode.ChildNodes)
            {
                Event event_DI = new Event(EventNode);
                Result.Add(event_DI);
            }

            return Result;
        }

        public static List<Sample> ProcessSamples(XmlNode SamplesNode)
        {
            List<Sample> Result = new List<Sample>();

            foreach (XmlNode SampleNode in SamplesNode.ChildNodes)
            {
                Sample sample_DI = new Sample(SampleNode);
                Result.Add(sample_DI);
            }

            return Result;
        }

        public static DataItemCollection GetDataItemsFromDeviceStream(DeviceStream deviceStream)
        {
            DataItemCollection Result = new DataItemCollection();

            foreach (ComponentStream componentStream in deviceStream.ComponentStreams)
            {
                foreach (Condition condition_DI in componentStream.dataItems.Conditions) Result.Conditions.Add(condition_DI);
                foreach (Event event_DI in componentStream.dataItems.Events) Result.Events.Add(event_DI);
                foreach (Sample sample_DI in componentStream.dataItems.Samples) Result.Samples.Add(sample_DI);
            }

            foreach (Condition condition_DI in deviceStream.dataItems.Conditions) Result.Conditions.Add(condition_DI);
            foreach (Event event_DI in deviceStream.dataItems.Events) Result.Events.Add(event_DI);
            foreach (Sample sample_DI in deviceStream.dataItems.Samples) Result.Samples.Add(sample_DI);

            return Result;
        }
    }
}
