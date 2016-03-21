using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TH_Configuration;

namespace TH_DeviceTable
{
    public class DeviceInfo
    {
        public bool Connected { get; set; }

        public Configuration Configuration { get; set; }

        public Description_Settings Description
        {
            get
            {
                if (Configuration != null) return Configuration.Description;
                return null;
            }
        }

        public double OEE { get; set; }
        public double Availability { get; set; }
        public double Performance { get; set; }

    }
}
