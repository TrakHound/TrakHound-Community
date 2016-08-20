// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

using TrakHound.API;
using TrakHound.Tools;
using TrakHound.Plugins;

namespace TH_StatusTimes.ProductionStatus.Controls
{
    /// <summary>
    /// Interaction logic for Row.xaml
    /// </summary>
    public partial class Row : UserControl
    {
        public Row()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        #region "Dependency Properties"

        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(Row), new PropertyMetadata(false));


        public TrakHound.Configurations.DeviceConfiguration Configuration
        {
            get { return (TrakHound.Configurations.DeviceConfiguration)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(TrakHound.Configurations.DeviceConfiguration), typeof(Row), new PropertyMetadata(null));

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
            Connected = info.Connected == 1;
            DeviceStatus = info.DeviceStatus;
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

        //public void UpdateData(EventData data)
        //{
        //    UpdateDatabaseConnection(data);
        //    UpdateAvailability(data);
        //    UpdateSnapshots(data);
        //    UpdateVariables(data);
        //    UpdateShiftData(data);
        //}

        //private void UpdateDatabaseConnection(EventData data)
        //{
        //    if (data.Id.ToLower() == "statusdata_connection")
        //    {
        //        if (data.Data02.GetType() == typeof(bool))
        //        {
        //            Connected = (bool)data.Data02;
        //        }
        //    }
        //}

        //private void UpdateAvailability(EventData data)
        //{
        //    if (data.Id.ToLower() == "statusdata_availability")
        //    {
        //        if (data.Data02.GetType() == typeof(bool))
        //        {
        //            Available = (bool)data.Data02;
        //        }
        //    }
        //}

        //private void UpdateVariables(EventData data)
        //{
        //    if (data.Id.ToLower() == "statusdata_variables")
        //    {
        //        if (data.Data02 != null)
        //        {
        //            var currentTime = DataTable_Functions.GetTableValue(data.Data02, "variable", "shift_currenttime", "value");
        //            if (currentTime != null)
        //            {
        //                DateTime time = DateTime.MinValue;
        //                if (DateTime.TryParse(currentTime, out time))
        //                {
        //                    CurrentTime = time;
        //                }
        //            }
        //        }
        //    }
        //}

        //private void UpdateSnapshots(EventData data)
        //{
        //    if (data.Id.ToLower() == "statusdata_snapshots")
        //    {
        //        if (data.Data02 != null)
        //        {
        //            DeviceStatus = DataTable_Functions.GetTableValue(data.Data02, "name", "Device Status", "value");
        //        }
        //    }
        //}

        //private void UpdateShiftData(EventData data)
        //{
        //    if (data.Id.ToLower() == "statusdata_shiftdata")
        //    {
        //        var dt = data.Data02 as DataTable;
        //        if (dt != null)
        //        {
        //            double total = 0;
        //            double production = 0;
        //            double alarm = 0;
        //            double setup = 0;
        //            double teardown = 0;
        //            double maintenance = 0;
        //            double processDevelopment = 0;

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                total += DataTable_Functions.GetDoubleFromRow("totaltime", row);
        //                production += DataTable_Functions.GetDoubleFromRow("production_status__production", row);
        //                alarm += DataTable_Functions.GetDoubleFromRow("production_status__alarm", row);
        //                setup += DataTable_Functions.GetDoubleFromRow("production_status__setup", row);
        //                teardown += DataTable_Functions.GetDoubleFromRow("production_status__teardown", row);
        //                maintenance += DataTable_Functions.GetDoubleFromRow("production_status__maintenance", row);
        //                processDevelopment += DataTable_Functions.GetDoubleFromRow("production_status__process_development", row);
        //            }

        //            TotalSeconds = total;
        //            ProductionSeconds = production;
        //            AlarmSeconds = alarm;
        //            SetupSeconds = setup;
        //            TeardownSeconds = teardown;
        //            MaintenanceSeconds = maintenance;
        //            ProcessDevelopmentSeconds = processDevelopment;
        //        }
        //    }
        //}

    }
}
