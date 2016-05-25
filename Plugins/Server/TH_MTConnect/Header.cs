// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Xml;

namespace TH_MTConnect
{
    /// <summary>
    /// Contains the Header information in an MTConnect Devices XML document
    /// </summary>
    public class Header_Devices
    {
        public Header_Devices() { }

        public Header_Devices(XmlNode HeaderNode)
        {
            XML.AssignProperties(this, HeaderNode);
            CreationTime = DateTime_Functions.ConvertDateTimeToUTC(CreationTime);
        }

        // Required
        public Int64 AssetBufferSize { get; set; }
        public Int64 AssetCount { get; set; }
        public Int64 BufferSize { get; set; }
        public DateTime CreationTime { get; set; }
        public Int64 InstanceId { get; set; }
        public string Sender { get; set; }
        public string Version { get; set; }

        // Optional
        public string TestIndicator { get; set; }
    }

    /// <summary>
    /// Contains the Header information in an MTConnect Streams XML document
    /// </summary>
    public class Header_Streams
    {
        public Header_Streams() { }

        public Header_Streams(XmlNode HeaderNode)
        {
            XML.AssignProperties(this, HeaderNode);
            CreationTime = DateTime_Functions.ConvertDateTimeToUTC(CreationTime);
        }

        // Required
        public Int64 BufferSize { get; set; }
        public DateTime CreationTime { get; set; }
        public Int64 InstanceId { get; set; }
        public string Sender { get; set; }
        public string Version { get; set; }

        public Int64 FirstSequence { get; set; }
        public Int64 LastSequence { get; set; }
        public Int64 NextSequence { get; set; }

        // Optional
        public string TestIndicator { get; set; }
    }

    /// <summary>
    /// Contains the Header information in an MTConnect Assets XML document
    /// </summary>
    public class Header_Assets
    {
        public Header_Assets() { }

        public Header_Assets(XmlNode HeaderNode)
        {
            XML.AssignProperties(this, HeaderNode);
            CreationTime = DateTime_Functions.ConvertDateTimeToUTC(CreationTime);
        }

        // Required
        public Int64 AssetBufferSize { get; set; }
        public Int64 AssetCount { get; set; }
        public DateTime CreationTime { get; set; }
        public Int64 InstanceId { get; set; }
        public string Sender { get; set; }
        public string Version { get; set; }

        // Optional
        public string TestIndicator { get; set; }
    }

    /// <summary>
    /// Contains the Header information in an MTConnect Error XML document
    /// </summary>
    public class Header_Error
    {
        public Header_Error() { }

        public Header_Error(XmlNode HeaderNode)
        {
            XML.AssignProperties(this, HeaderNode);
            CreationTime = DateTime_Functions.ConvertDateTimeToUTC(CreationTime);
        }

        // Required
        public Int64 AssetBufferSize { get; set; }
        public Int64 AssetCount { get; set; }
        public Int64 BufferSize { get; set; }
        public DateTime CreationTime { get; set; }
        public Int64 InstanceId { get; set; }
        public string Sender { get; set; }
        public string Version { get; set; }

        // Optional
        public string TestIndicator { get; set; }
        public Int64 NextSequence { get; set; }
        public Int64 LastSequence { get; set; }
        public Int64 FirstSequence { get; set; }
    }

}
