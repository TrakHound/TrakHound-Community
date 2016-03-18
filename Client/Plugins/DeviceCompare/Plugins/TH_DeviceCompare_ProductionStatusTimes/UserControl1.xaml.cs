// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;

using TH_Configuration;
using TH_Global.Functions;
using TH_Plugins_Client;
using TH_WPF;

namespace TH_DeviceCompare_ProductionStatusTimes
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

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;


        void Update(DataEvent_Data de_d)
        {
            if (de_d != null && de_d.data01 != null && de_d.data01.GetType() == typeof(Configuration))
            {
                // GenEvent Values
                if (de_d.id.ToLower() == "statusdata_geneventvalues")
                {
                    // Production Status Times
                    this.Dispatcher.BeginInvoke(new Action<object>(UpdateProductionStatusTimes_GenEventValues), Priority_Context, new object[] { de_d.data02 });
                }

                // Shifts Table Data
                if (de_d.id.ToLower() == "statusdata_shiftdata")
                {
                    // Production Status Times
                    this.Dispatcher.BeginInvoke(new Action<object>(UpdateProductionStatusTimes_ShiftData), Priority_Context, new object[] { de_d.data02 });
                }

                // Snapshot Table Data
                if (de_d.id.ToLower() == "statusdata_snapshots")
                {
                    // Production Status Times
                    this.Dispatcher.BeginInvoke(new Action<object>(UpdateProductionStatusTimes_SnapshotData), Priority_Context, new object[] { de_d.data02 });
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
                dv.RowFilter = "EVENT = 'production_status'";
                DataTable temp_dt = dv.ToTable();

                if (temp_dt != null)
                {
                    foreach (DataRow row in temp_dt.Rows)
                    {
                        string val = row["VALUE"].ToString();

                        // Get the Numval 
                        int numval = -1;
                        int.TryParse(row["NUMVAL"].ToString(), out numval);

                        if (Times.ToList().Find(x => x.Text == val) == null)
                        {
                            var timeProgress = new TimeProgress();

                            // Set Text
                            timeProgress.Text = val;
                            timeProgress.Index = numval;

                            // Initialize values
                            timeProgress.Percentage = "0%";
                            timeProgress.BarValue = 0;
                            timeProgress.BarMaximum = 1;

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
                double totalSeconds = GetTime("TOTALTIME", dt);

                // Get List of variables from 'Shifts' table and collect the total number of seconds
                foreach (DataColumn column in dt.Columns)
                {
                    if (column.ColumnName.Contains("PRODUCTION_STATUS") || column.ColumnName.Contains("Production_Status") || column.ColumnName.Contains("production_status"))
                    {
                        double seconds = GetTime(column.ColumnName, dt);

                        // Get Value name from Column Name
                        string valueName = null;
                        if (column.ColumnName.Contains("__"))
                        {
                            int i = column.ColumnName.IndexOf("__") + 2;
                            if (i < column.ColumnName.Length)
                            {
                                string name = column.ColumnName.Substring(i);
                                name = name.Replace('_', ' ');

                                valueName = name;
                            }
                        }

                        if (valueName != null)
                        {
                            int index = Times.ToList().FindIndex(x => x.Text.ToLower() == valueName.ToLower());
                            if (index >= 0)
                            {
                                var timeDisplay = Times[index];
                                timeDisplay.Maximum = totalSeconds;
                                timeDisplay.Value = seconds;
                            }
                        }
                    }
                }
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
                    if (time.Text.ToLower() == currentValue.ToLower()) time.IsSelected = true;
                    else time.IsSelected = false;
                }
            }
        }

    }
}
