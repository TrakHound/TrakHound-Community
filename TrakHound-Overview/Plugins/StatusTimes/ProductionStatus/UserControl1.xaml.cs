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

        #region "Dependency Properties"

        public double TotalSeconds
        {
            get { return (double)GetValue(TotalSecondsProperty); }
            set { SetValue(TotalSecondsProperty, value); }
        }

        public static readonly DependencyProperty TotalSecondsProperty =
            DependencyProperty.Register("TotalSeconds", typeof(double), typeof(Plugin), new PropertyMetadata(1d));


        public double ProductionSeconds
        {
            get { return (double)GetValue(ProductionSecondsProperty); }
            set { SetValue(ProductionSecondsProperty, value); }
        }

        public static readonly DependencyProperty ProductionSecondsProperty =
            DependencyProperty.Register("ProductionSeconds", typeof(double), typeof(Plugin), new PropertyMetadata(0d));


        public double SetupSeconds
        {
            get { return (double)GetValue(SetupSecondsProperty); }
            set { SetValue(SetupSecondsProperty, value); }
        }

        public static readonly DependencyProperty SetupSecondsProperty =
            DependencyProperty.Register("SetupSeconds", typeof(double), typeof(Plugin), new PropertyMetadata(0d));


        public double TeardownSeconds
        {
            get { return (double)GetValue(TeardownSecondsProperty); }
            set { SetValue(TeardownSecondsProperty, value); }
        }

        public static readonly DependencyProperty TeardownSecondsProperty =
            DependencyProperty.Register("TeardownSeconds", typeof(double), typeof(Plugin), new PropertyMetadata(0d));


        public double MaintenanceSeconds
        {
            get { return (double)GetValue(MaintenanceSecondsProperty); }
            set { SetValue(MaintenanceSecondsProperty, value); }
        }

        public static readonly DependencyProperty MaintenanceSecondsProperty =
            DependencyProperty.Register("MaintenanceSeconds", typeof(double), typeof(Plugin), new PropertyMetadata(0d));


        public double ProcessDevelopmentSeconds
        {
            get { return (double)GetValue(ProcessDevelopmentSecondsProperty); }
            set { SetValue(ProcessDevelopmentSecondsProperty, value); }
        }

        public static readonly DependencyProperty ProcessDevelopmentSecondsProperty =
            DependencyProperty.Register("ProcessDevelopmentSeconds", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        #endregion

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;


        void Update(EventData data)
        {
            if (data != null && data.Data01 != null && data.Data01.GetType() == typeof(DeviceConfiguration))
            {
                //// GenEvent Values
                //if (data.Id.ToLower() == "statusdata_geneventvalues")
                //{
                //    // Production Status Times
                //    this.Dispatcher.BeginInvoke(new Action<object>(UpdateProductionStatusTimes_GenEventValues), Priority_Context, new object[] { data.Data02 });
                //}

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

                        if (!Times.ToList().Exists(x => x.Text == val))
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

                    //// Set Bar Colors
                    //foreach (var time in Times)
                    //{
                    //    if (time.Index == Times.Count - 1)
                    //    {
                    //        time.BarBrush = Brush_Functions.GetSolidBrushFromResource(this, "StatusGreen");
                    //        time.BarBackgroundBrush = Brush_Functions.GetSolidBrushFromResource(this, "StatusGreen_Hover");
                    //    }
                    //    else if (time.Index == 0)
                    //    {
                    //        time.BarBrush = Brush_Functions.GetSolidBrushFromResource(this, "StatusRed");
                    //        time.BarBackgroundBrush = Brush_Functions.GetSolidBrushFromResource(this, "StatusRed_Hover");
                    //    }
                    //    else
                    //    {
                    //        time.BarBrush = Brush_Functions.GetSolidBrushFromResource(this, "StatusYellow");
                    //        time.BarBackgroundBrush = Brush_Functions.GetSolidBrushFromResource(this, "StatusYellow_Hover");
                    //    }
                    //}

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

                ProductionSeconds = GetTime("production_status__production", dt);
                SetupSeconds = GetTime("production_status__setup", dt);
                TeardownSeconds = GetTime("production_status__teardown", dt);
                MaintenanceSeconds = GetTime("production_status__maintenance", dt);
                ProcessDevelopmentSeconds = GetTime("production_status__process_development", dt);

            }
        }

        //// Get the Times for each Production Status variable from the 'Shifts' table
        //void UpdateProductionStatusTimes_ShiftData(object shiftData)
        //{
        //    DataTable dt = shiftData as DataTable;
        //    if (dt != null)
        //    {
        //        // Get Total Time for this shift (all segments)
        //        double totalSeconds = GetTime("TOTALTIME", dt);

        //        // Get List of variables from 'Shifts' table and collect the total number of seconds
        //        foreach (DataColumn column in dt.Columns)
        //        {
        //            if (column.ColumnName.Contains("PRODUCTION_STATUS") || column.ColumnName.Contains("Production_Status") || column.ColumnName.Contains("production_status"))
        //            {
        //                double seconds = GetTime(column.ColumnName, dt);

        //                // Get Value name from Column Name
        //                string valueName = null;
        //                if (column.ColumnName.Contains("__"))
        //                {
        //                    int i = column.ColumnName.IndexOf("__") + 2;
        //                    if (i < column.ColumnName.Length)
        //                    {
        //                        string name = column.ColumnName.Substring(i);
        //                        name = name.Replace('_', ' ');

        //                        valueName = name;
        //                    }
        //                }

        //                if (valueName != null)
        //                {
        //                    int index = Times.ToList().FindIndex(x => x.Text.ToLower() == valueName.ToLower());
        //                    if (index >= 0)
        //                    {
        //                        var timeDisplay = Times[index];
        //                        timeDisplay.Maximum = totalSeconds;
        //                        timeDisplay.Value = seconds;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

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
