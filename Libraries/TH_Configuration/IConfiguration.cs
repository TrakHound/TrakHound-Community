using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TH_Configuration
{
    public delegate void DeviceConfigurationPropertyChanged_Handler(string xPath, string value);

    public interface IDeviceConfiguration
    {

        event DeviceConfigurationPropertyChanged_Handler ConfigurationPropertyChanged;

    }
}
