// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

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

            dataItems = new DataItemCollection();
        }

        public DeviceStream(XmlNode DeviceStreamNode)
        {
            XML.AssignProperties(this, DeviceStreamNode);
            ComponentStreams = new List<ComponentStream>();

            dataItems = new DataItemCollection();
        }

        // Required
        public string name { get; set; }
        public string uuid { get; set; }

        public List<ComponentStream> ComponentStreams;

        public DataItemCollection dataItems;

        public void Dispose()
        {
            ComponentStreams.Clear();
            ComponentStreams = null;

            dataItems.Dispose();
            dataItems = null;
        }
    }

    public class ComponentStream : IDisposable
    {
        public ComponentStream()
        {
            dataItems = new DataItemCollection();
        }

        public ComponentStream(XmlNode ComponentStreamNode)
        {
            XML.AssignProperties(this, ComponentStreamNode);
            fullAddress = XML.GetFullAddress(ComponentStreamNode);

            dataItems = new DataItemCollection();
        }

        // Required
        public string componentId { get; set; }
        public string component { get; set; }

        // Optional
        public string name { get; set; }
        public string nativeName { get; set; }
        public string uuid { get; set; }

        public DataItemCollection dataItems;

        public string fullAddress { get; set; }

        public void Dispose()
        {
            dataItems.Dispose();
            dataItems = null;
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

        public List<Condition> Conditions;
        public List<Event> Events;
        public List<Sample> Samples;

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

    public class Condition
    {
        public Condition() { }

        public Condition(XmlNode ConditionNode)
        {
            XML.AssignProperties(this, ConditionNode);
            fullAddress = XML.GetFullAddress(ConditionNode);
            value = ConditionNode.Name;
            CDATA = ConditionNode.InnerText;
        }

        public string value { get; set; }

        // Required
        public Int64 sequence { get; set; }
        public DateTime timestamp { get; set; }
        public string dataItemId { get; set; }
        public string type { get; set; }

        // Optional
        public string name { get; set; }
        public string nativeCode { get; set; }
        public string nativeSeverity { get; set; }
        public string qualifier { get; set; }
        public string statistic { get; set; }
        public string subType { get; set; }

        public string CDATA { get; set; }

        public string fullAddress { get; set; }
    }

    public class Event
    {
        public Event() { }

        public Event(XmlNode EventNode)
        {
            XML.AssignProperties(this, EventNode);
            fullAddress = XML.GetFullAddress(EventNode);
            Type = EventNode.Name;
            CDATA = EventNode.InnerText;
        }

        public string Type { get; set; }

        // Required
        public Int64 sequence { get; set; }
        public DateTime timestamp { get; set; }
        public string dataItemId { get; set; }

        // Optional
        public string subType { get; set; }
        public string name { get; set; }

        public string CDATA { get; set; }

        public string fullAddress { get; set; }
    }

    public class Sample
    {
        public Sample() { }

        public Sample(XmlNode SampleNode)
        {
            XML.AssignProperties(this, SampleNode);
            fullAddress = XML.GetFullAddress(SampleNode);
            Type = SampleNode.Name;
            CDATA = SampleNode.InnerText;
        }

        public string Type { get; set; }

        // Required
        public Int64 sequence { get; set; }
        public DateTime timestamp { get; set; }
        public string dataItemId { get; set; }

        // Optional
        public string subType { get; set; }
        public string name { get; set; }
        public string sampleRate { get; set; }
        public string statistic { get; set; }
        public string duration { get; set; }
        public string sampleCount { get; set; }

        public string CDATA { get; set; }

        public string fullAddress { get; set; }
    }
}
