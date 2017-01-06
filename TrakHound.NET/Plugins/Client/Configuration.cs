// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace TrakHound.Plugins.Client
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class PluginConfiguration
    {
        public PluginConfiguration() { SubCategories = new List<PluginConfigurationCategory>(); }

        public string Name { get; set; }

        public string Description { get; set; }

        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (EnabledChanged != null) EnabledChanged(this);
            }
        }

        public string Parent { get; set; }
        public string Category { get; set; }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public delegate void EnabledChanged_Handler(PluginConfiguration config);
        public event EnabledChanged_Handler EnabledChanged;

        public PluginConfiguration Copy()
        {
            var result = new PluginConfiguration();

            result.Name = Name;
            result.Description = Description;
            result.Enabled = Enabled;
            result.Parent = Parent;
            result.SubCategories = SubCategories.ToList();

            return result;
        }
    }

    public class PluginConfigurationCategory
    {
        public PluginConfigurationCategory() { PluginConfigurations = new List<PluginConfiguration>(); }

        public string Name { get; set; }

        public List<PluginConfiguration> PluginConfigurations { get; set; }
    }

}
