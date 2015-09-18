using System;
using System.Data;
using System.Xml;

namespace TH_MTC_Data.Components
{
    // <summary>
    // Object class to return all data associated with Probe command results
    // </summary>
    public class ReturnData
    {
         //Dataset with DataTables containing Probe values
        //public DataSet DS;

         //Device object with heirarchy of values and xml structure
        public Device device;

         //Raw XML document
        //public XmlDocument xmlDocument;

         //Header Information
        public Header_Devices header;
    }
}
