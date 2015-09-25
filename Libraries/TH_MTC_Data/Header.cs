// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Xml;

namespace TH_MTC_Data
{
    /// <summary>
    /// Contains the Header information in an MTConnect XML document
    /// </summary>
    //public class Header
    //{
    //    public string SenderName { get; set; }
    //    public DateTime CreationTime { get; set; }
    //    public Int64 InstanceID { get; set; }
    //    public string Version { get; set; }
    //    public Int64 BufferSize { get; set; }
    //    public Int64 NextSequence { get; set; }
    //    public Int64 FirstSequence { get; set; }
    //    public Int64 LastSequence { get; set; }

    //    #region "Header Functions"

    //    static public Header H_GetHeaderInfoFromNode(XmlNode HeaderNode)
    //    {
    //        Header HI = new Header();

    //        HI.SenderName = H_AssignNodeAttribute_String(HeaderNode, "sender");
    //        HI.CreationTime = H_AssignNodeAttribute_String(HeaderNode, "creationTime");
    //        HI.InstanceID = H_AssignNodeAttribute_Int64(HeaderNode, "instanceId");
    //        HI.Version = H_AssignNodeAttribute_String(HeaderNode, "version");
    //        HI.BufferSize = H_AssignNodeAttribute_Int64(HeaderNode, "bufferSize");
    //        HI.NextSequence = H_AssignNodeAttribute_Int64(HeaderNode, "nextSequence");
    //        HI.FirstSequence = H_AssignNodeAttribute_Int64(HeaderNode, "firstSequence");
    //        HI.LastSequence = H_AssignNodeAttribute_Int64(HeaderNode, "lastSequence");

    //        return HI;
    //    }

    //    static string H_AssignNodeAttribute_String(XmlNode Node, string AttributeName)
    //    {
    //        string Result = null;

    //        XmlAttribute Attr = Node.Attributes[AttributeName];

    //        if (Attr != null)
    //        {
    //            Result = Attr.Value.ToString();
    //        }

    //        return Result;
    //    }

    //    static Int64 H_AssignNodeAttribute_Int64(XmlNode Node, string AttributeName)
    //    {
    //        Int64 Result = -99999;

    //        XmlAttribute Attr = Node.Attributes[AttributeName];

    //        if (Attr != null)
    //        {
    //            Int64.TryParse(Attr.Value.ToString(), out Result);
    //        }

    //        return Result;
    //    }

    //    #endregion
    //}

    public class Header_Devices
    {
        public Header_Devices() { }

        public Header_Devices(XmlNode HeaderNode)
        {
            XML.AssignProperties(this, HeaderNode);
            creationTime = TH_Global.Functions.ConvertDateTimeToUTC(creationTime);
            //creationTime = new DateTime(creationTime.Ticks, DateTimeKind.Local);
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

    public class Header_Streams
    {
        public Header_Streams() { }

        public Header_Streams(XmlNode HeaderNode)
        {
            XML.AssignProperties(this, HeaderNode);
            creationTime = TH_Global.Functions.ConvertDateTimeToUTC(creationTime);
            //creationTime = new DateTime(creationTime.Ticks, DateTimeKind.Local);
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

    public class Header_Assets
    {
        public Header_Assets() { }

        public Header_Assets(XmlNode HeaderNode)
        {
            XML.AssignProperties(this, HeaderNode);
            creationTime = TH_Global.Functions.ConvertDateTimeToUTC(creationTime);
            //creationTime = new DateTime(creationTime.Ticks, DateTimeKind.Local);
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

    public class Header_Error
    {
        public Header_Error() { }

        public Header_Error(XmlNode HeaderNode)
        {
            XML.AssignProperties(this, HeaderNode);
            creationTime = TH_Global.Functions.ConvertDateTimeToUTC(creationTime);
            //creationTime = new DateTime(creationTime.Ticks, DateTimeKind.Local);
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
