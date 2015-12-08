using System;
using System.Data;
using System.Xml;
using System.Collections.Generic;

namespace TH_MTC_Data.Components
{
    // <summary>
    // Object class to return all data associated with Probe command results
    // </summary>
    public class ReturnData : IDisposable
    {
         //Device object with heirarchy of values and xml structure
        public List<Device> devices;

         //Header Information
        public Header_Devices header;

        public void Dispose()
        {
            devices.Clear();
            devices = null;
        }
    }
}
