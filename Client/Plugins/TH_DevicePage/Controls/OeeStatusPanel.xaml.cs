// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;

using TH_Configuration;
using TH_Global.Functions;
using TH_Plugins;

namespace TH_DevicePage.Controls
{
    /// <summary>
    /// Interaction logic for OeeStatusPanel.xaml
    /// </summary>
    public partial class OeeStatusPanel : UserControl
    {
        public OeeStatusPanel()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public void Update(EventData data)
        {
            oee_timeline.Update(data);
            availability_timeline.Update(data);
            performance_timeline.Update(data);
        }
    }
}
