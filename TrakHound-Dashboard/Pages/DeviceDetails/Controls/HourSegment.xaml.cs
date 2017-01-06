// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;

namespace TrakHound_Dashboard.Pages.DeviceDetails.Controls
{
    /// <summary>
    /// Interaction logic for HourSegment.xaml
    /// </summary>
    public partial class HourSegment : UserControl
    {
        public HourSegment()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public HourData HourData
        {
            get { return (HourData)GetValue(HourDataProperty); }
            set { SetValue(HourDataProperty, value); }
        }

        public static readonly DependencyProperty HourDataProperty =
            DependencyProperty.Register("HourData", typeof(HourData), typeof(HourSegment), new PropertyMetadata(null));


        public string ValueFormat
        {
            get { return (string)GetValue(ValueFormatProperty); }
            set { SetValue(ValueFormatProperty, value); }
        }

        public static readonly DependencyProperty ValueFormatProperty =
            DependencyProperty.Register("ValueFormat", typeof(string), typeof(HourSegment), new PropertyMetadata("P0"));

    }
}
