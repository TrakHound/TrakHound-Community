// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Xml;

using TH_Global.Functions;

namespace TH_MTC_Data
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
            creationTime = DateTime_Functions.ConvertDateTimeToUTC(creationTime);
        }

        // Required
        public Int64 assetBufferSize { get; set; }
        public Int64 assetCount { get; set; }
        public Int64 bufferSize { get; set; }
        public DateTime creationTime { get; set; }
        public Int64 instanceId { get; set; }
        public string sender { get; set; }
        public string version { get; set; }

        // Optional
        public string testIndicator { get; set; }
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
            creationTime = DateTime_Functions.ConvertDateTimeToUTC(creationTime);
        }

        // Required
        public Int64 bufferSize { get; set; }
        public DateTime creationTime { get; set; }
        public Int64 instanceId { get; set; }
        public string sender { get; set; }
        public string version { get; set; }

        public Int64 firstSequence { get; set; }
        public Int64 lastSequence { get; set; }
        public Int64 nextSequence { get; set; }

        // Optional
        public string testIndicator { get; set; }
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
            creationTime = DateTime_Functions.ConvertDateTimeToUTC(creationTime);
        }

        // Required
        public Int64 assetBufferSize { get; set; }
        public Int64 assetCount { get; set; }
        public DateTime creationTime { get; set; }
        public Int64 instanceId { get; set; }
        public string sender { get; set; }
        public string version { get; set; }

        // Optional
        public string testIndicator { get; set; }
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
            creationTime = DateTime_Functions.ConvertDateTimeToUTC(creationTime);
        }

        // Required
        public Int64 assetBufferSize { get; set; }
        public Int64 assetCount { get; set; }
        public Int64 bufferSize { get; set; }
        public DateTime creationTime { get; set; }
        public Int64 instanceId { get; set; }
        public string sender { get; set; }
        public string version { get; set; }

        // Optional
        public string testIndicator { get; set; }
        public Int64 nextSequence { get; set; }
        public Int64 lastSequence { get; set; }
        public Int64 firstSequence { get; set; }
    }

}
