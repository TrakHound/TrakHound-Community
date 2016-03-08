using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TH_Configuration
{
    public delegate void ConfigurationPropertyChanged_Handler(string xPath, string value);

    public interface IConfiguration
    {

        event ConfigurationPropertyChanged_Handler ConfigurationPropertyChanged;

    }
}
