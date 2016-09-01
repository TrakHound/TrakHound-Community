// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;

using TrakHound.API;
using TrakHound.Configurations;

namespace TrakHound_Dashboard.Pages.Dashboard.OeeStatus.Controls
{
    /// <summary>
    /// Interaction logic for Row.xaml
    /// </summary>
    public partial class Row : UserControl
    {
        public Row(DeviceConfiguration config)
        {
            InitializeComponent();
            root.DataContext = this;

            Configuration = config;
        }

        #region "Dependency Properties"

        public DeviceConfiguration Configuration
        {
            get { return (DeviceConfiguration)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(DeviceConfiguration), typeof(Row), new PropertyMetadata(null));

        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(Row), new PropertyMetadata(false));



        public string DeviceStatus
        {
            get { return (string)GetValue(DeviceStatusProperty); }
            set { SetValue(DeviceStatusProperty, value); }
        }

        public static readonly DependencyProperty DeviceStatusProperty =
            DependencyProperty.Register("DeviceStatus", typeof(string), typeof(Row), new PropertyMetadata(null));
        

        public double Oee
        {
            get { return (double)GetValue(OeeProperty); }
            set
            {
                SetValue(OeeProperty, value);

                if (value > 0.75) OeeStatus = 2;
                else if (value > 0.5) OeeStatus = 1;
                else OeeStatus = 0;
            }
        }

        public static readonly DependencyProperty OeeProperty =
            DependencyProperty.Register("Oee", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public int OeeStatus
        {
            get { return (int)GetValue(OeeStatusProperty); }
            set { SetValue(OeeStatusProperty, value); }
        }

        public static readonly DependencyProperty OeeStatusProperty =
            DependencyProperty.Register("OeeStatus", typeof(int), typeof(Row), new PropertyMetadata(0));



        public double Availability
        {
            get { return (double)GetValue(AvailabilityProperty); }
            set
            {
                SetValue(AvailabilityProperty, value);

                if (value > 0.75) AvailabilityStatus = 2;
                else if (value > 0.5) AvailabilityStatus = 1;
                else AvailabilityStatus = 0;
            }
        }

        public static readonly DependencyProperty AvailabilityProperty =
            DependencyProperty.Register("Availability", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public int AvailabilityStatus
        {
            get { return (int)GetValue(AvailabilityStatusProperty); }
            set { SetValue(AvailabilityStatusProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityStatusProperty =
            DependencyProperty.Register("AvailabilityStatus", typeof(int), typeof(Row), new PropertyMetadata(0));



        public double Performance
        {
            get { return (double)GetValue(PerformanceProperty); }
            set
            {
                SetValue(PerformanceProperty, value);

                if (value > 0.75) PerformanceStatus = 2;
                else if (value > 0.5) PerformanceStatus = 1;
                else PerformanceStatus = 0;
            }
        }

        public static readonly DependencyProperty PerformanceProperty =
            DependencyProperty.Register("Performance", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public int PerformanceStatus
        {
            get { return (int)GetValue(PerformanceStatusProperty); }
            set { SetValue(PerformanceStatusProperty, value); }
        }

        public static readonly DependencyProperty PerformanceStatusProperty =
            DependencyProperty.Register("PerformanceStatus", typeof(int), typeof(Row), new PropertyMetadata(0));



        public double Quality
        {
            get { return (double)GetValue(QualityProperty); }
            set
            {
                SetValue(QualityProperty, value);

                if (value > 0.75) QualityStatus = 2;
                else if (value > 0.5) QualityStatus = 1;
                else QualityStatus = 0;
            }
        }

        public static readonly DependencyProperty QualityProperty =
            DependencyProperty.Register("Quality", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public int QualityStatus
        {
            get { return (int)GetValue(QualityStatusProperty); }
            set { SetValue(QualityStatusProperty, value); }
        }

        public static readonly DependencyProperty QualityStatusProperty =
            DependencyProperty.Register("QualityStatus", typeof(int), typeof(Row), new PropertyMetadata(0));


        #endregion

        public DateTime CurrentTime { get; set; }

        public void UpdateData(Data.OeeInfo info)
        {
            if (info != null)
            {
                Oee = info.Oee;
                Availability = info.Availability;
                Performance = info.Performance;
                Quality = info.Quality;
            }
        }

        public void UpdateData(Data.StatusInfo info)
        {
            if (info != null)
            {
                Connected = info.Connected == 1;
                DeviceStatus = info.DeviceStatus;
            }
        }

    }
}
