// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using System.Xml;

namespace TH_MTConnect.Components
{

    public class Device : IDisposable
    {
        public Device()
        {
            Components = new List<Component>();
            DataItems = new DataItemCollection();
        }

        public Device(XmlNode DeviceNode)
        {
            XML.AssignProperties(this, DeviceNode);
            Components = new List<Component>();
            DataItems = new DataItemCollection();
        }

        // Required
        public string Id { get; set; }
        public string Uuid { get; set; }
        public string Name { get; set; }

        // Optional
        public string NativeName { get; set; }
        public string SampleInterval { get; set; }
        public string SampleRate { get; set; }
        public string Iso841Class { get; set; }

        public Description Description { get; set; }

        public List<Component> Components { get; set; }

        public DataItemCollection DataItems { get; set; }

        public void Dispose()
        {
            Components.Clear();
            Components = null;

            DataItems.Dispose();
            DataItems = null;
        }
    }

    public class Component : IDisposable
    {
        public Component() 
        { 
            Components = new List<Component>();
            DataItems = new DataItemCollection(); 
        }

        public Component(XmlNode ComponentNode)
        {
            XML.AssignProperties(this, ComponentNode);
            FullAddress = XML.GetFullAddress(ComponentNode);
            Components = new List<Component>();
            DataItems = new DataItemCollection();
        }

        // Required
        public string Id { get; set; }

        // Optional
        public string Uuid { get; set; }
        public string Name { get; set; }
        public string NativeName { get; set; }
        public string SampleInterval { get; set; }
        public string SampleRate { get; set; }

        // Sub-Elements
        public Description Description { get; set; }

        public string Configuration { get; set; }

        public List<Component> Components;

        public DataItemCollection DataItems { get; set; }

        public string FullAddress { get; set; }

        public void Dispose()
        {
            Components.Clear();
            Components = null;

            DataItems.Dispose();
            DataItems = null;
        }
    }

    public class DataItemCollection : IDisposable
    {
        public DataItemCollection()
        {
            Conditions = new List<DataItem>();
            Events = new List<DataItem>();
            Samples = new List<DataItem>();
        }

        public List<DataItem> Conditions;
        public List<DataItem> Events;
        public List<DataItem> Samples;

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

    public class Description
    {
        public Description(XmlNode DescriptionNode)
        {
            XML.AssignProperties(this, DescriptionNode);
            CDATA = DescriptionNode.InnerText;
        }

        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public string Station { get; set; }

        public string CDATA { get; set; }
    }

    public class DataItem
    {
        public DataItem() { }

        public DataItem(XmlNode DataItemNode)
        {
            XML.AssignProperties(this, DataItemNode);
            FullAddress = XML.GetFullAddress(DataItemNode);
        }


        // Required
        public string Category { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }

        // optional
        public string Name { get; set; }
        public string Subtype { get; set; }
        public string Statistic { get; set; }
        public string Units { get; set; }
        public string NativeUnits { get; set; }
        public string NativeScale { get; set; }
        public string CoordinateSystem { get; set; }
        public string SampleRate { get; set; }
        public string Representation { get; set; }
        public string SignificantDigits { get; set; }


        public string FullAddress { get; set; }

        public Source Source;

        public Constraints Constraints; 
    }

    public class Source
    {
        public Source() { }

        public Source(XmlNode SourceNode)
        {
            XML.AssignProperties(this, SourceNode);
            CDATA = SourceNode.InnerText;
        }

        public string ComponentID { get; set; }
        public string DataItemID { get; set; }

        public string CDATA { get; set; }
    }

    public class Constraints
    {
        public Constraints() { }

        public Constraints(XmlNode ConstraintsNode)
        {
            XML.AssignProperties(this, ConstraintsNode);
        }

        public string Value { get; set; }
        public string Minimum { get; set; }
        public string Maximum { get; set; }

        public Filter Filter;
    }

    public class Filter
    {
        public Filter() { }

        public Filter(XmlNode FilterNode)
        {
            XML.AssignProperties(this, FilterNode);
        }

        public string Type { get; set; }
    }
}
