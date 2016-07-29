// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

using TrakHound.Configurations;
using TrakHound.Tools;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;
using TrakHound_UI;
using TrakHound_UI.Functions;

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

        public double TotalSeconds
        {
            get { return (double)GetValue(TotalSecondsProperty); }
            set { SetValue(TotalSecondsProperty, value); }
        }

        public static readonly DependencyProperty TotalSecondsProperty =
            DependencyProperty.Register("TotalSeconds", typeof(double), typeof(Plugin), new PropertyMetadata(1d));



        public double ActiveSeconds
        {
            get { return (double)GetValue(ActiveSecondsProperty); }
            set { SetValue(ActiveSecondsProperty, value); }
        }

        public static readonly DependencyProperty ActiveSecondsProperty =
            DependencyProperty.Register("ActiveSeconds", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        public double IdleSeconds
        {
            get { return (double)GetValue(IdleSecondsProperty); }
            set { SetValue(IdleSecondsProperty, value); }
        }

        public static readonly DependencyProperty IdleSecondsProperty =
            DependencyProperty.Register("IdleSeconds", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        public double AlertSeconds
        {
            get { return (double)GetValue(AlertSecondsProperty); }
            set { SetValue(AlertSecondsProperty, value); }
        }

        public static readonly DependencyProperty AlertSecondsProperty =
            DependencyProperty.Register("AlertSeconds", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        #endregion

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;


        void Update(EventData data)
        {
            if (data != null && data.Data01 != null && data.Data01.GetType() == typeof(DeviceConfiguration))
            {
                // Shifts Table Data
                if (data.Id.ToLower() == "statusdata_shiftdata")
                {
                    // Production Status Times
                    this.Dispatcher.BeginInvoke(new Action<object>(UpdateProductionStatusTimes_ShiftData), Priority_Context, new object[] { data.Data02 });
                }

                // Snapshot Table Data
                if (data.Id.ToLower() == "statusdata_snapshots")
                {
                    // Production Status Times
                    this.Dispatcher.BeginInvoke(new Action<object>(UpdateProductionStatusTimes_SnapshotData), Priority_Context, new object[] { data.Data02 });
                }
            }

        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Plugin), new PropertyMetadata(null));



        ObservableCollection<TimeProgress> times;
        public ObservableCollection<TimeProgress> Times
        {
            get
            {
                if (times == null) times = new ObservableCollection<TimeProgress>();
                return times;
            }
            set
            {
                times = value;
            }
        }


        // Get list of Production Status variables and create TimeProgress controls for each 
        void UpdateProductionStatusTimes_GenEventValues(object geneventvalues)
        {
            DataTable dt = geneventvalues as DataTable;
            if (dt != null)
            {
                DataView dv = dt.AsDataView();
                dv.RowFilter = "EVENT = 'device_status'";
                DataTable temp_dt = dv.ToTable();

                if (temp_dt != null)
                {
                    foreach (DataRow row in temp_dt.Rows)
                    {
                        string val = row["VALUE"].ToString();

                        // Get the Numval 
                        int numval = -1;
                        int.TryParse(row["NUMVAL"].ToString(), out numval);

                        if (!Times.ToList().Exists(x => x.Text == val))
                        {
                            var timeProgress = new TimeProgress();

                            // Set Text
                            timeProgress.Text = val;
                            timeProgress.Index = numval;

                            // Initialize values
                            timeProgress.Percentage = "0%";
                            timeProgress.Value = 0;
                            timeProgress.Maximum = 1;

                            Times.Add(timeProgress);

                            Times.SortReverse();
                        } 
                    }

                    temp_dt.Dispose();
                }
            }
        }

        // Get the Times for each Production Status variable from the 'Shifts' table
        void UpdateProductionStatusTimes_ShiftData(object shiftData)
        {
            DataTable dt = shiftData as DataTable;
            if (dt != null)
            {
                // Get Total Time for this shift (all segments)
                TotalSeconds = GetTime("TOTALTIME", dt);

                ActiveSeconds = GetTime("device_status__active", dt);
                IdleSeconds = GetTime("device_status__idle", dt);
                AlertSeconds = GetTime("device_status__alert", dt);
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


        // Highlight the Current Production Status
        void UpdateProductionStatusTimes_SnapshotData(object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                string currentValue = DataTable_Functions.GetTableValue(dt, "name", "Production Status", "value");

                foreach (var time in Times)
                {
                    if (time.Text != null && currentValue != null && 
                        time.Text.ToLower() == currentValue.ToLower()) time.IsSelected = true;
                    else time.IsSelected = false;
                }
            }
        }

    }
}
