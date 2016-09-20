// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;

using TrakHound.API;
using TrakHound.Configurations;

namespace TrakHound_Dashboard.Pages.Dashboard.ProductionStatusTimes.Controls
{
    /// <summary>
    /// Interaction logic for Row.xaml
    /// </summary>
    public partial class Row : UserControl
    {
        public Row(DeviceDescription device)
        {
            InitializeComponent();
            root.DataContext = this;

            Device = device;
        }

        #region "Dependency Properties"

        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(Row), new PropertyMetadata(false));


        public DeviceDescription Device
        {
            get { return (DeviceDescription)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceDescription), typeof(Row), new PropertyMetadata(null));

        public string DeviceStatus
        {
            get { return (string)GetValue(DeviceStatusProperty); }
            set { SetValue(DeviceStatusProperty, value); }
        }

        public static readonly DependencyProperty DeviceStatusProperty =
            DependencyProperty.Register("DeviceStatus", typeof(string), typeof(Row), new PropertyMetadata(null));

        


        public double TotalSeconds
        {
            get { return (double)GetValue(TotalSecondsProperty); }
            set { SetValue(TotalSecondsProperty, value); }
        }

        public static readonly DependencyProperty TotalSecondsProperty =
            DependencyProperty.Register("TotalSeconds", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double ProductionSeconds
        {
            get { return (double)GetValue(ProductionSecondsProperty); }
            set { SetValue(ProductionSecondsProperty, value); }
        }

        public static readonly DependencyProperty ProductionSecondsProperty =
            DependencyProperty.Register("ProductionSeconds", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double AlarmSeconds
        {
            get { return (double)GetValue(AlarmSecondsProperty); }
            set { SetValue(AlarmSecondsProperty, value); }
        }

        public static readonly DependencyProperty AlarmSecondsProperty =
            DependencyProperty.Register("AlarmSeconds", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double SetupSeconds
        {
            get { return (double)GetValue(SetupSecondsProperty); }
            set { SetValue(SetupSecondsProperty, value); }
        }

        public static readonly DependencyProperty SetupSecondsProperty =
            DependencyProperty.Register("SetupSeconds", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double TeardownSeconds
        {
            get { return (double)GetValue(TeardownSecondsProperty); }
            set { SetValue(TeardownSecondsProperty, value); }
        }

        public static readonly DependencyProperty TeardownSecondsProperty =
            DependencyProperty.Register("TeardownSeconds", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double MaintenanceSeconds
        {
            get { return (double)GetValue(MaintenanceSecondsProperty); }
            set { SetValue(MaintenanceSecondsProperty, value); }
        }

        public static readonly DependencyProperty MaintenanceSecondsProperty =
            DependencyProperty.Register("MaintenanceSeconds", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double ProcessDevelopmentSeconds
        {
            get { return (double)GetValue(ProcessDevelopmentSecondsProperty); }
            set { SetValue(ProcessDevelopmentSecondsProperty, value); }
        }

        public static readonly DependencyProperty ProcessDevelopmentSecondsProperty =
            DependencyProperty.Register("ProcessDevelopmentSeconds", typeof(double), typeof(Row), new PropertyMetadata(0d));

        #endregion

        public DateTime CurrentTime { get; set; }

        public void UpdateData(Data.StatusInfo info)
        {
            if (info != null)
            {
                Connected = info.Connected == 1;
                DeviceStatus = info.DeviceStatus;
            }
        }

        public void UpdateData(Data.TimersInfo info)
        {
            if (info != null)
            {
                TotalSeconds = info.Total;

                ProductionSeconds = info.Production;
                SetupSeconds = info.Setup;
                TeardownSeconds = info.Teardown;
                MaintenanceSeconds = info.Maintenance;
                ProcessDevelopmentSeconds = info.ProcessDevelopment;
            }
        }

    }
}
