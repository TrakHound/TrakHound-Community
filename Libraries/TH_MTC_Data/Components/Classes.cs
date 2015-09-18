// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using System.Xml;

namespace TH_MTC_Data.Components
{

    public class Device
    {
        public Device()
        {
            components = new List<Component>();
            dataItems = new DataItemCollection();
        }

        public Device(XmlNode DeviceNode)
        {
            XML.AssignProperties(this, DeviceNode);
            components = new List<Component>();
            dataItems = new DataItemCollection();
        }

        // Required
        public string id { get; set; }
        public string uuid { get; set; }
        public string name { get; set; }

        // Optional
        public string nativeName { get; set; }
        public string sampleInterval { get; set; }
        public string sampleRate { get; set; }
        public string iso841Class { get; set; }

        public Description description;

        public List<Component> components;

        public DataItemCollection dataItems { get; set; }
    }

    public class Component
    {
        public Component() 
        { 
            components = new List<Component>();
            dataItems = new DataItemCollection(); 
        }

        public Component(XmlNode ComponentNode)
        {
            XML.AssignProperties(this, ComponentNode);
            fullAddress = XML.GetFullAddress(ComponentNode);
            components = new List<Component>();
            dataItems = new DataItemCollection();
        }

        // Required
        public string id { get; set; }

        // Optional
        public string uuid { get; set; }
        public string name { get; set; }
        public string nativeName { get; set; }
        public string sampleInterval { get; set; }
        public string sampleRate { get; set; }

        // Sub-Elements
        public Description description { get; set; }

        public string configuration { get; set; }

        public List<Component> components;

        public DataItemCollection dataItems { get; set; }

        public string fullAddress { get; set; }
    }

    public class DataItemCollection
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
    }

    public class Description
    {
        public Description(XmlNode DescriptionNode)
        {
            XML.AssignProperties(this, DescriptionNode);
            CDATA = DescriptionNode.InnerText;
        }

        public string manufacturer { get; set; }
        public string modle { get; set; }
        public string serialNumber { get; set; }
        public string station { get; set; }

        public string CDATA { get; set; }
    }

    public class DataItem
    {
        public DataItem() { }

        public DataItem(XmlNode DataItemNode)
        {
            XML.AssignProperties(this, DataItemNode);
            fullAddress = XML.GetFullAddress(DataItemNode);
        }


        // Required
        public string category { get; set; }
        public string id { get; set; }
        public string type { get; set; }

        // optional
        public string name { get; set; }
        public string subtype { get; set; }
        public string statistic { get; set; }
        public string units { get; set; }
        public string nativeUnits { get; set; }
        public string nativeScale { get; set; }
        public string coordinateSystem { get; set; }
        public string sampleRate { get; set; }
        public string representation { get; set; }
        public string significantDigits { get; set; }


        public string fullAddress { get; set; }

        public Source source;

        public Constraints constraints;


        public class Source
        {
            public Source() { }

            public Source(XmlNode SourceNode)
            {
                XML.AssignProperties(this, SourceNode);
                CDATA = SourceNode.InnerText;
            }

            public string componentID { get; set; }
            public string dataItemID { get; set; }

            public string CDATA { get; set; }
        }

        public class Constraints
        {
            public Constraints() { }

            public Constraints(XmlNode ConstraintsNode)
            {
                XML.AssignProperties(this, ConstraintsNode);
            }

            public string value { get; set; }
            public string minimum { get; set; }
            public string maximum { get; set; }

            public Filter filter;

            public class Filter
            {

                public Filter() { }

                public Filter(XmlNode FilterNode)
                {
                    XML.AssignProperties(this, FilterNode);
                }

                public string type { get; set; }
            }
        }
    }
}
