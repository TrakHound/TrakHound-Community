// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Windows.Controls;

using TH_Plugins.Client;

namespace TH_DeviceCompare
{
    /// <summary>
    /// Interaction logic for DeviceCompare.xaml
    /// </summary>
    public partial class DeviceCompare : UserControl
    {
        public DeviceCompare()
        {
            InitializeComponent();
            DataContext = this;

            SubCategories = new List<PluginConfigurationCategory>();
            PluginConfigurationCategory components = new PluginConfigurationCategory();
            components.Name = "Components";
            SubCategories.Add(components);
        }


        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;
    }
}
