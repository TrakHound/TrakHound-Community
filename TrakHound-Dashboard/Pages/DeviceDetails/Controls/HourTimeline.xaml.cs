// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TrakHound_Dashboard.Pages.DeviceDetails.Controls
{
    /// <summary>
    /// Interaction logic for Row.xaml
    /// </summary>
    public partial class HourTimeline : UserControl
    {
        public HourTimeline()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        #region "Dependency Properties"

        public List<HourData> HourDatas
        {
            get { return (List<HourData>)GetValue(HourDatasProperty); }
            set { SetValue(HourDatasProperty, value); }
        }

        public static readonly DependencyProperty HourDatasProperty =
            DependencyProperty.Register("HourDatas", typeof(List<HourData>), typeof(HourTimeline), new PropertyMetadata(null));


        public string ValueFormat
        {
            get { return (string)GetValue(ValueFormatProperty); }
            set { SetValue(ValueFormatProperty, value); }
        }

        public static readonly DependencyProperty ValueFormatProperty =
            DependencyProperty.Register("ValueFormat", typeof(string), typeof(HourTimeline), new PropertyMetadata("P0"));

        #endregion

        public DateTime CurrentTime { get; set; }
        
    }
}
