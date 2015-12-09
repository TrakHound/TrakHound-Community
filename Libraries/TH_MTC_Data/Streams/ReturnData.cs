using System;
using System.Data;
using System.Xml;
using System.Collections.Generic;

namespace TH_MTC_Data.Streams
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
