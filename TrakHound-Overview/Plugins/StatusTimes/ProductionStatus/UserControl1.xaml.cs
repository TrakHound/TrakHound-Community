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

        public string ProductionStatus
        {
            get { return (string)GetValue(ProductionStatusProperty); }
            set { SetValue(ProductionStatusProperty, value); }
        }

        public static readonly DependencyProperty ProductionStatusProperty =
            DependencyProperty.Register("ProductionStatus", typeof(string), typeof(Plugin), new PropertyMetadata(null));



        public double ProductionPercentage
        {
            get { return (double)GetValue(ProductionPercentageProperty); }
            set { SetValue(ProductionPercentageProperty, value); }
        }

        public static readonly DependencyProperty ProductionPercentageProperty =
            DependencyProperty.Register("ProductionPercentage", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        public TimeSpan ProductionTime
        {
            get { return (TimeSpan)GetValue(ProductionTimeProperty); }
            set { SetValue(ProductionTimeProperty, value); }
        }

        public static readonly DependencyProperty ProductionTimeProperty =
            DependencyProperty.Register("ProductionTime", typeof(TimeSpan), typeof(Plugin), new PropertyMetadata(TimeSpan.Zero));



        public double SetupPercentage
        {
            get { return (double)GetValue(SetupPercentageProperty); }
            set { SetValue(SetupPercentageProperty, value); }
        }

        public static readonly DependencyProperty SetupPercentageProperty =
            DependencyProperty.Register("SetupPercentage", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        public TimeSpan SetupTime
        {
            get { return (TimeSpan)GetValue(SetupTimeProperty); }
            set { SetValue(SetupTimeProperty, value); }
        }

        public static readonly DependencyProperty SetupTimeProperty =
            DependencyProperty.Register("SetupTime", typeof(TimeSpan), typeof(Plugin), new PropertyMetadata(TimeSpan.Zero));



        public double TeardownPercentage
        {
            get { return (double)GetValue(TeardownPercentageProperty); }
            set { SetValue(TeardownPercentageProperty, value); }
        }

        public static readonly DependencyProperty TeardownPercentageProperty =
            DependencyProperty.Register("TeardownPercentage", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        public TimeSpan TeardownTime
        {
            get { return (TimeSpan)GetValue(TeardownTimeProperty); }
            set { SetValue(TeardownTimeProperty, value); }
        }

        public static readonly DependencyProperty TeardownTimeProperty =
            DependencyProperty.Register("TeardownTime", typeof(TimeSpan), typeof(Plugin), new PropertyMetadata(TimeSpan.Zero));



        public double MaintenancePercentage
        {
            get { return (double)GetValue(MaintenancePercentageProperty); }
            set { SetValue(MaintenancePercentageProperty, value); }
        }

        public static readonly DependencyProperty MaintenancePercentageProperty =
            DependencyProperty.Register("MaintenancePercentage", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        public TimeSpan MaintenanceTime
        {
            get { return (TimeSpan)GetValue(MaintenanceTimeProperty); }
            set { SetValue(MaintenanceTimeProperty, value); }
        }

        public static readonly DependencyProperty MaintenanceTimeProperty =
            DependencyProperty.Register("MaintenanceTime", typeof(TimeSpan), typeof(Plugin), new PropertyMetadata(TimeSpan.Zero));



        public double ProcessDevelopmentPercentage
        {
            get { return (double)GetValue(ProcessDevelopmentPercentageProperty); }
            set { SetValue(ProcessDevelopmentPercentageProperty, value); }
        }

        public static readonly DependencyProperty ProcessDevelopmentPercentageProperty =
            DependencyProperty.Register("ProcessDevelopmentPercentage", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        public TimeSpan ProcessDevelopmentTime
        {
            get { return (TimeSpan)GetValue(ProcessDevelopmentTimeProperty); }
            set { SetValue(ProcessDevelopmentTimeProperty, value); }
        }

        public static readonly DependencyProperty ProcessDevelopmentTimeProperty =
            DependencyProperty.Register("ProcessDevelopmentTime", typeof(TimeSpan), typeof(Plugin), new PropertyMetadata(TimeSpan.Zero));
        
        #endregion

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;


        void Update(EventData data)
        {
            if (data != null && data.Data01 != null)
            {
                if (data != null && data.Id == "STATUS_STATUS")
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var info = (Data.StatusInfo)data.Data02;
                        ProductionStatus = info.ProductionStatus;

                    }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
                }

                if (data != null && data.Id == "STATUS_TIMERS" && data.Data02 != null)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var info = (Data.TimersInfo)data.Data02;

                        double total = info.Total;
                        double production = info.Production;
                        double setup = info.Setup;
                        double teardown = info.Teardown;
                        double maintenance = info.Maintenance;
                        double processDevelopment = info.ProcessDevelopment;

                        if (total > 0)
                        {
                            ProductionPercentage = production / total;
                            SetupPercentage = setup / total;
                            TeardownPercentage = teardown / total;
                            MaintenancePercentage = maintenance / total;
                            ProcessDevelopmentPercentage = processDevelopment / total;
                        }

                        ProductionTime = TimeSpan.FromSeconds(production);
                        SetupTime = TimeSpan.FromSeconds(setup);
                        TeardownTime = TimeSpan.FromSeconds(teardown);
                        MaintenanceTime = TimeSpan.FromSeconds(maintenance);
                        ProcessDevelopmentTime = TimeSpan.FromSeconds(processDevelopment);

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
                //    this.Dispatcher.BeginInvoke(new Action<object>(UpdateShiftData), Priority_Context, new object[] { data.Data02 });
                //}
            }
        }

        void UpdateSnapshot(object data)
        {
            var dt = data as DataTable;
            if (dt != null)
            {
                ProductionStatus = DataTable_Functions.GetTableValue(dt, "name", "Production Status", "value");
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

                double production = GetTime("production_status__production", dt);
                double setup = GetTime("production_status__setup", dt);
                double teardown = GetTime("production_status__teardown", dt);
                double maintenance = GetTime("production_status__maintenance", dt);
                double processDevelopment = GetTime("production_status__process_development", dt);

                if (total > 0)
                {
                    ProductionPercentage = production / total;
                    SetupPercentage = setup / total;
                    TeardownPercentage = teardown / total;
                    MaintenancePercentage = maintenance / total;
                    ProcessDevelopmentPercentage = processDevelopment / total;
                }

                ProductionTime = TimeSpan.FromSeconds(production);
                SetupTime = TimeSpan.FromSeconds(setup);
                TeardownTime = TimeSpan.FromSeconds(teardown);
                MaintenanceTime = TimeSpan.FromSeconds(maintenance);
                ProcessDevelopmentTime = TimeSpan.FromSeconds(processDevelopment);
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
