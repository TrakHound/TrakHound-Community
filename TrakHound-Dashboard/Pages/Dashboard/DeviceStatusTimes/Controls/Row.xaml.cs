// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;

using TrakHound.API;
using TrakHound.Configurations;

namespace TrakHound_Dashboard.Pages.Dashboard.DeviceStatusTimes.Controls
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

        public DeviceDescription Device
        {
            get { return (DeviceDescription)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceDescription), typeof(Row), new PropertyMetadata(null));

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


        public int Status
        {
            get { return (int)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(int), typeof(Row), new PropertyMetadata(-1));



        public double ActivePercentage
        {
            get { return (double)GetValue(ActivePercentageProperty); }
            set { SetValue(ActivePercentageProperty, value); }
        }

        public static readonly DependencyProperty ActivePercentageProperty =
            DependencyProperty.Register("ActivePercentage", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double IdlePercentage
        {
            get { return (double)GetValue(IdlePercentageProperty); }
            set { SetValue(IdlePercentageProperty, value); }
        }

        public static readonly DependencyProperty IdlePercentageProperty =
            DependencyProperty.Register("IdlePercentage", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double AlertPercentage
        {
            get { return (double)GetValue(AlertPercentageProperty); }
            set { SetValue(AlertPercentageProperty, value); }
        }

        public static readonly DependencyProperty AlertPercentageProperty =
            DependencyProperty.Register("AlertPercentage", typeof(double), typeof(Row), new PropertyMetadata(0d));




        public TimeSpan ActiveTime
        {
            get { return (TimeSpan)GetValue(ActiveTimeProperty); }
            set { SetValue(ActiveTimeProperty, value); }
        }

        public static readonly DependencyProperty ActiveTimeProperty =
            DependencyProperty.Register("ActiveTime", typeof(TimeSpan), typeof(Row), new PropertyMetadata(TimeSpan.Zero));


        public TimeSpan IdleTime
        {
            get { return (TimeSpan)GetValue(IdleTimeProperty); }
            set { SetValue(IdleTimeProperty, value); }
        }

        public static readonly DependencyProperty IdleTimeProperty =
            DependencyProperty.Register("IdleTime", typeof(TimeSpan), typeof(Row), new PropertyMetadata(TimeSpan.Zero));


        public TimeSpan AlertTime
        {
            get { return (TimeSpan)GetValue(AlertTimeProperty); }
            set { SetValue(AlertTimeProperty, value); }
        }

        public static readonly DependencyProperty AlertTimeProperty =
            DependencyProperty.Register("AlertTime", typeof(TimeSpan), typeof(Row), new PropertyMetadata(TimeSpan.Zero));

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
                double total = info.Total;
                double active = info.Active;
                double idle = info.Idle;
                double alert = info.Alert;

                if (total > 0)
                {
                    ActivePercentage = active / total;
                    IdlePercentage = idle / total;
                    AlertPercentage = alert / total;
                }

                if (active > idle && active > alert) Status = 2;
                else if (idle > alert && idle > active) Status = 1;
                else Status = 0;

                ActiveTime = TimeSpan.FromSeconds(active);
                IdleTime = TimeSpan.FromSeconds(idle);
                AlertTime = TimeSpan.FromSeconds(alert);
            }
        }

        public delegate void Clicked_Handler(Row row);
        public event Clicked_Handler Clicked;

        private void UserControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clicked?.Invoke(this);
        }
    }
}
