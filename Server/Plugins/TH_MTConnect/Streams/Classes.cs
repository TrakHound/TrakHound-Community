// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using System.Xml;

namespace TH_MTConnect.Streams
{
    public class DeviceStream : IDisposable
    {
        public DeviceStream()
        {
            ComponentStreams = new List<ComponentStream>();

            DataItems = new DataItemCollection();
        }

        public DeviceStream(XmlNode DeviceStreamNode)
        {
            XML.AssignProperties(this, DeviceStreamNode);
            ComponentStreams = new List<ComponentStream>();

            DataItems = new DataItemCollection();
        }

        // Required
        public string Name { get; set; }
        public string Uuid { get; set; }

        public List<ComponentStream> ComponentStreams { get; set; }

        public DataItemCollection DataItems { get; set; }

        public void Dispose()
        {
            ComponentStreams.Clear();
            ComponentStreams = null;

            DataItems.Dispose();
            DataItems = null;
        }
    }

    public class ComponentStream : IDisposable
    {
        public ComponentStream()
        {
            DataItems = new DataItemCollection();
        }

        public ComponentStream(XmlNode ComponentStreamNode)
        {
            XML.AssignProperties(this, ComponentStreamNode);
            FullAddress = Tools.GetFullAddress(ComponentStreamNode);

            DataItems = new DataItemCollection();
        }

        // Required
        public string ComponentId { get; set; }
        public string Component { get; set; }

        // Optional
        public string Name { get; set; }
        public string NativeName { get; set; }
        public string Uuid { get; set; }

        public DataItemCollection DataItems { get; set; }

        public string FullAddress { get; set; }

        public void Dispose()
        {
            DataItems.Dispose();
            DataItems = null;
        }
    }

    // Data Items

    public class DataItemCollection : IDisposable
    {
        public DataItemCollection()
        {
            Conditions = new List<Condition>();
            Events = new List<Event>();
            Samples = new List<Sample>();
        }

        public List<Condition> Conditions { get; set; }
        public List<Event> Events { get; set; }
        public List<Sample> Samples { get; set; }

        public void Dispose()
        {
            Conditions.Clear();
            Events.Clear();
            Samples.Clear();

            Conditions = null;
            Events = null;
            Samples = null;
        }
    }

    public class DataItem
    {
        public string Value { get; set; }

        // Required
        public Int64 Sequence { get; set; }
        public DateTime Timestamp { get; set; }
        public string DataItemId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

        public string SubType { get; set; }

        public string CDATA { get; set; }

        public string FullAddress { get; set; }
    }

    public class Condition : DataItem
    {
        public Condition() { }

        public Condition(XmlNode ConditionNode)
        {
            XML.AssignProperties(this, ConditionNode);
            FullAddress = Tools.GetFullAddress(ConditionNode);
            Value = ConditionNode.Name;
            CDATA = ConditionNode.InnerText;
        }

        // Optional
        public string NativeCode { get; set; }
        public string NativeSeverity { get; set; }
        public string Qualifier { get; set; }
        public string Statistic { get; set; }
    }

    public class Event : DataItem
    {
        public Event() { }

        public Event(XmlNode EventNode)
        {
            XML.AssignProperties(this, EventNode);
            FullAddress = Tools.GetFullAddress(EventNode);
            Type = EventNode.Name;
            CDATA = EventNode.InnerText;
        }
    }

    public class Sample : DataItem
    {
        public Sample() { }

        public Sample(XmlNode SampleNode)
        {
            XML.AssignProperties(this, SampleNode);
            FullAddress = Tools.GetFullAddress(SampleNode);
            Type = SampleNode.Name;
            CDATA = SampleNode.InnerText;
        }

        // Optional
        public string SampleRate { get; set; }
        public string Statistic { get; set; }
        public string Duration { get; set; }
        public string SampleCount { get; set; }
    }
}
