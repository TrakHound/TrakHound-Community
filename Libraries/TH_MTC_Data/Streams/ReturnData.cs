using System;
using System.Data;
using System.Xml;

namespace TH_MTC_Data.Streams
{
    /// <summary>
    /// Object class to return all data associated with Current command results
    /// </summary>
    public class ReturnData
    {
        // Dataset with DataTables containing Current values
        //public DataSet DS;

        // Device object with heirarchy of values and xml structure
        public DeviceStream deviceStream;

        // Raw XML document
        //public XmlDocument xmlDocument;

        // Header Information
        public Header_Streams header;
    }
}
