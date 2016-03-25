// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;

namespace TH_DevicePage.Controls
{
    /// <summary>
    /// Interaction logic for OeeTimelineToolTip.xaml
    /// </summary>
    public partial class OeeToolTip : UserControl
    {
        public OeeToolTip()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Times
        {
            get { return (string)GetValue(TimesProperty); }
            set { SetValue(TimesProperty, value); }
        }

        public static readonly DependencyProperty TimesProperty =
            DependencyProperty.Register("Times", typeof(string), typeof(OeeToolTip), new PropertyMetadata(null));


        public string Oee
        {
            get { return (string)GetValue(OeeProperty); }
            set { SetValue(OeeProperty, value); }
        }

        public static readonly DependencyProperty OeeProperty =
            DependencyProperty.Register("Oee", typeof(string), typeof(OeeToolTip), new PropertyMetadata(null));


        public string Availability
        {
            get { return (string)GetValue(AvailabilityProperty); }
            set { SetValue(AvailabilityProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityProperty =
            DependencyProperty.Register("Availability", typeof(string), typeof(OeeToolTip), new PropertyMetadata(null));


        public string Performance
        {
            get { return (string)GetValue(PerformanceProperty); }
            set { SetValue(PerformanceProperty, value); }
        }

        public static readonly DependencyProperty PerformanceProperty =
            DependencyProperty.Register("Performance", typeof(string), typeof(OeeToolTip), new PropertyMetadata(null));

    }
}
