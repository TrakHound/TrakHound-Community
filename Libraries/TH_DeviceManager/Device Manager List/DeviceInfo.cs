using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TH_Configuration;

namespace TH_DeviceManager
{
    /// <summary>
    /// Basic Device Information used to display in Device Manager Device Table
    /// </summary>
    public class DeviceInfo
    {
        public string UniqueId { get; set; }
        public string TableName { get; set; }

        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string Id { get; set; }

        public bool ClientEnabled { get; set; }
        public bool ServerEnabled { get; set; }

        public Configuration Configuration { get; set; }
    }
}
