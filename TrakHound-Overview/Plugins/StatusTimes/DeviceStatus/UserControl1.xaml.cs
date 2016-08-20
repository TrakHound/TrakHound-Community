// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data;
using System.Windows;

using TrakHound.API;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Tools;

namespace TrakHound_Overview.Plugins.StatusTimes.DeviceStatus
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Plugin
    {
        public Plugin()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        #region "Dependency Properties"

        public string DeviceStatus
        {
            get { return (string)GetValue(DeviceStatusProperty); }
            set { SetValue(DeviceStatusProperty, value); }
        }

        public static readonly DependencyProperty DeviceStatusProperty =
            DependencyProperty.Register("DeviceStatus", typeof(string), typeof(Plugin), new PropertyMetadata(null));


        public double ActivePercentage
        {
            get { return (double)GetValue(ActivePercentageProperty); }
            set { SetValue(ActivePercentageProperty, value); }
        }

        public static readonly DependencyProperty ActivePercentageProperty =
            DependencyProperty.Register("ActivePercentage", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        public TimeSpan ActiveTime
        {
            get { return (TimeSpan)GetValue(ActiveTimeProperty); }
            set { SetValue(ActiveTimeProperty, value); }
        }

        public static readonly DependencyProperty ActiveTimeProperty =
            DependencyProperty.Register("ActiveTime", typeof(TimeSpan), typeof(Plugin), new PropertyMetadata(TimeSpan.Zero));


        public double IdlePercentage
        {
            get { return (double)GetValue(IdlePercentageProperty); }
            set { SetValue(IdlePercentageProperty, value); }
        }

        public static readonly DependencyProperty IdlePercentageProperty =
            DependencyProperty.Register("IdlePercentage", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        public TimeSpan IdleTime
        {
            get { return (TimeSpan)GetValue(IdleTimeProperty); }
            set { SetValue(IdleTimeProperty, value); }
        }

        public static readonly DependencyProperty IdleTimeProperty =
            DependencyProperty.Register("IdleTime", typeof(TimeSpan), typeof(Plugin), new PropertyMetadata(TimeSpan.Zero));


        public double AlertPercentage
        {
            get { return (double)GetValue(AlertPercentageProperty); }
            set { SetValue(AlertPercentageProperty, value); }
        }

        public static readonly DependencyProperty AlertPercentageProperty =
            DependencyProperty.Register("AlertPercentage", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        public TimeSpan AlertTime
        {
            get { return (TimeSpan)GetValue(AlertTimeProperty); }
            set { SetValue(AlertTimeProperty, value); }
        }

        public static readonly DependencyProperty AlertTimeProperty =
            DependencyProperty.Register("AlertTime", typeof(TimeSpan), typeof(Plugin), new PropertyMetadata(TimeSpan.Zero));

        #endregion

        void Update(EventData data)
        {
            if (data != null && data.Data02 != null)
            {
                if (data != null && data.Id == "STATUS_STATUS")
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var info = (Data.StatusInfo)data.Data02;
                        DeviceStatus = info.DeviceStatus;

                    }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
                }

                if (data != null && data.Id == "STATUS_TIMERS")
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var info = (Data.TimersInfo)data.Data02;

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

                        ActiveTime = TimeSpan.FromSeconds(active);
                        IdleTime = TimeSpan.FromSeconds(idle);
                        AlertTime = TimeSpan.FromSeconds(alert);

                    }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
                }




                //// Snapshot Table Data
                //if (data.Id.ToLower() == "statusdata_snapshots")
                //{
                //    this.Dispatcher.BeginInvoke(new Action<object>(UpdateSnapshot), UI_Functions.PRIORITY_BACKGROUND, new object[] { data.Data02 });
                //}

                //// Shifts Table Data
                //if (data.Id.ToLower() == "statusdata_shiftdata")
                //{
                //    // Production Status Times
                //    this.Dispatcher.BeginInvoke(new Action<object>(UpdateShiftData), UI_Functions.PRIORITY_BACKGROUND, new object[] { data.Data02 });
                //}
            }
        }

        void UpdateSnapshot(object data)
        {
            var dt = data as DataTable;
            if (dt != null)
            {
                DeviceStatus = DataTable_Functions.GetTableValue(dt, "name", "Device Status", "value");
            }
        }

        // Get the Times for each Production Status variable from the 'Shifts' table
        void UpdateShiftData(object shiftData)
        {
            DataTable dt = shiftData as DataTable;
            if (dt != null)
            {
                // Get Total Time for this shift (all segments)
                double total = GetTime("TOTALTIME", dt);

                double active = GetTime("device_status__active", dt);
                double idle = GetTime("device_status__idle", dt);
                double alert = GetTime("device_status__alert", dt);

                if (total > 0)
                {
                    ActivePercentage = active / total;
                    IdlePercentage = idle / total;
                    AlertPercentage = alert / total;
                }

                ActiveTime = TimeSpan.FromSeconds(active);
                IdleTime = TimeSpan.FromSeconds(idle);
                AlertTime = TimeSpan.FromSeconds(alert);
            }
        }
        
        static double GetTime(string columnName, DataTable dt)
        {
            double result = 0;

            if (dt != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    double d = DataTable_Functions.GetDoubleFromRow(columnName, row);
                    if (d >= 0) result += d;
                }
            }

            return result;
        }

    }
}
