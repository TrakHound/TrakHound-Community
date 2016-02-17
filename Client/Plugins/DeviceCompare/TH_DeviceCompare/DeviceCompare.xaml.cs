// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using TH_Plugins_Client;

namespace TH_DeviceCompare
{
    /// <summary>
    /// Interaction logic for DeviceCompare.xaml
    /// </summary>
    public partial class DeviceCompare : UserControl, IClientPlugin
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
