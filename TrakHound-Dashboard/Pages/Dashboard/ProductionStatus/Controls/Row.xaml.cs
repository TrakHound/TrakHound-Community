// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;

using TrakHound.API;
using TrakHound.Configurations;

namespace TH_ProductionStatus.Controls
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


        public string ProductionStatus
        {
            get { return (string)GetValue(ProductionStatusProperty); }
            set { SetValue(ProductionStatusProperty, value); }
        }

        public static readonly DependencyProperty ProductionStatusProperty =
            DependencyProperty.Register("ProductionStatus", typeof(string), typeof(Row), new PropertyMetadata(null));


        public double DayRunTime
        {
            get { return (double)GetValue(DayRunTimeProperty); }
            set { SetValue(DayRunTimeProperty, value); }
        }

        public static readonly DependencyProperty DayRunTimeProperty =
            DependencyProperty.Register("DayRunTime", typeof(double), typeof(Row), new PropertyMetadata(0d));

        public double TotalRunTime
        {
            get { return (double)GetValue(TotalRunTimeProperty); }
            set { SetValue(TotalRunTimeProperty, value); }
        }

        public static readonly DependencyProperty TotalRunTimeProperty =
            DependencyProperty.Register("TotalRunTime", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double DayOperatingTime
        {
            get { return (double)GetValue(DayOperatingTimeProperty); }
            set { SetValue(DayOperatingTimeProperty, value); }
        }

        public static readonly DependencyProperty DayOperatingTimeProperty =
            DependencyProperty.Register("DayOperatingTime", typeof(double), typeof(Row), new PropertyMetadata(0d));

        public double TotalOperatingTime
        {
            get { return (double)GetValue(TotalOperatingTimeProperty); }
            set { SetValue(TotalOperatingTimeProperty, value); }
        }

        public static readonly DependencyProperty TotalOperatingTimeProperty =
            DependencyProperty.Register("TotalOperatingTime", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double DayCuttingTime
        {
            get { return (double)GetValue(DayCuttingTimeProperty); }
            set { SetValue(DayCuttingTimeProperty, value); }
        }

        public static readonly DependencyProperty DayCuttingTimeProperty =
            DependencyProperty.Register("DayCuttingTime", typeof(double), typeof(Row), new PropertyMetadata(0d));

        public double TotalCuttingTime
        {
            get { return (double)GetValue(TotalCuttingTimeProperty); }
            set { SetValue(TotalCuttingTimeProperty, value); }
        }

        public static readonly DependencyProperty TotalCuttingTimeProperty =
            DependencyProperty.Register("TotalCuttingTime", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double DaySpindleTime
        {
            get { return (double)GetValue(DaySpindleTimeProperty); }
            set { SetValue(DaySpindleTimeProperty, value); }
        }

        public static readonly DependencyProperty DaySpindleTimeProperty =
            DependencyProperty.Register("DaySpindleTime", typeof(double), typeof(Row), new PropertyMetadata(0d));

        public double TotalSpindleTime
        {
            get { return (double)GetValue(TotalSpindleTimeProperty); }
            set { SetValue(TotalSpindleTimeProperty, value); }
        }

        public static readonly DependencyProperty TotalSpindleTimeProperty =
            DependencyProperty.Register("TotalSpindleTime", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public int PartCount
        {
            get { return (int)GetValue(PartCountProperty); }
            set { SetValue(PartCountProperty, value); }
        }

        public static readonly DependencyProperty PartCountProperty =
            DependencyProperty.Register("PartCount", typeof(int), typeof(Row), new PropertyMetadata(null));

        #endregion

        public DateTime CurrentTime { get; set; }

        public void UpdateData(Data.StatusInfo info)
        {
            if (info != null)
            {
                Connected = info.Connected == 1;
                DeviceStatus = info.DeviceStatus;
                ProductionStatus = info.ProductionStatus;
                PartCount = info.PartCount;
            }
        }

        public void UpdateData(Data.TimersInfo info)
        {
            if (info != null)
            {
                DayRunTime = info.DayRun / 3600;
                DayOperatingTime = info.DayOperating / 3600;
                DayCuttingTime = info.DayCutting / 3600;
                DaySpindleTime = info.DaySpindle / 3600;

                TotalRunTime = info.TotalRun / 3600;
                TotalOperatingTime = info.TotalOperating / 3600;
                TotalCuttingTime = info.TotalCutting / 3600;
                TotalSpindleTime = info.TotalSpindle / 3600;
            }
        }
        
    }
}
