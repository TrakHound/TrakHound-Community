// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Windows.Controls;

using TrakHound.API.Users;
using TrakHound.Plugins.Client;

namespace TrakHound_Overview
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    public partial class Overview : UserControl
    {
        public Overview()
        {
            InitializeComponent();
            DataContext = this;

            SubCategories = new List<PluginConfigurationCategory>();
            PluginConfigurationCategory components = new PluginConfigurationCategory();
            components.Name = "Components";
            SubCategories.Add(components);
        }

        public static UserConfiguration CurrentUser { get; set; }
    }
}
