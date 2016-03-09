// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data;
using System.Xml;
using System.Collections.Generic;

namespace TH_MTConnect.Streams
{
    /// <summary>
    /// Object class to return all data associated with Current command results
    /// </summary>
    public class ReturnData : IDisposable
    {
        // Device object with heirarchy of values and xml structure
        public List<DeviceStream> deviceStreams;

        // Header Information
        public Header_Streams header;

        public void Dispose()
        {
            deviceStreams.Clear();
            deviceStreams = null;
        }
    }
}
