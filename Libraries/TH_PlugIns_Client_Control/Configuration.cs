using System;
using System.Collections.Generic;
using System.Windows.Media;

using System.Configuration;

namespace TH_PlugIns_Client
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class PlugInConfiguration
    {
        public PlugInConfiguration() { SubCategories = new List<PlugInConfigurationCategory>(); }

        public string name { get; set; }

        public string description { get; set; }

        private bool lenabled;
        public bool enabled
        {
            get { return lenabled; }
            set
            {
                lenabled = value;
                if (EnabledChanged != null) EnabledChanged(this);
            }
        }

        public string parent { get; set; }
        public string category { get; set; }

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public delegate void EnabledChanged_Handler(PlugInConfiguration config);
        public event EnabledChanged_Handler EnabledChanged;
    }

    public class PlugInConfigurationCategory
    {
        public PlugInConfigurationCategory() { PlugInConfigurations = new List<PlugInConfiguration>(); }

        public string name { get; set; }

        public List<PlugInConfiguration> PlugInConfigurations { get; set; }
    }

}
