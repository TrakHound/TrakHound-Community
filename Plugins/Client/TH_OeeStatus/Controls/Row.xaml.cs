using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

using TH_Global.Functions;
using TH_Plugins;

namespace TH_OeeStatus.Controls
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


        public bool Available
        {
            get { return (bool)GetValue(AvailableProperty); }
            set { SetValue(AvailableProperty, value); }
        }

        public static readonly DependencyProperty AvailableProperty =
            DependencyProperty.Register("Available", typeof(bool), typeof(Row), new PropertyMetadata(false));



        public TH_Global.TrakHound.Configurations.DeviceConfiguration Configuration
        {
            get { return (TH_Global.TrakHound.Configurations.DeviceConfiguration)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(TH_Global.TrakHound.Configurations.DeviceConfiguration), typeof(Row), new PropertyMetadata(null));



        public string DeviceStatus
        {
            get { return (string)GetValue(DeviceStatusProperty); }
            set { SetValue(DeviceStatusProperty, value); }
        }

        public static readonly DependencyProperty DeviceStatusProperty =
            DependencyProperty.Register("DeviceStatus", typeof(string), typeof(Row), new PropertyMetadata(null));



        //public bool Production
        //{
        //    get { return (bool)GetValue(ProductionProperty); }
        //    set { SetValue(ProductionProperty, value); }
        //}

        //public static readonly DependencyProperty ProductionProperty =
        //    DependencyProperty.Register("Production", typeof(bool), typeof(Row), new PropertyMetadata(false));


        //public bool Idle
        //{
        //    get { return (bool)GetValue(IdleProperty); }
        //    set { SetValue(IdleProperty, value); }
        //}

        //public static readonly DependencyProperty IdleProperty =
        //    DependencyProperty.Register("Idle", typeof(bool), typeof(Row), new PropertyMetadata(false));


        //public bool Alert
        //{
        //    get { return (bool)GetValue(AlertProperty); }
        //    set { SetValue(AlertProperty, value); }
        //}

        //public static readonly DependencyProperty AlertProperty =
        //    DependencyProperty.Register("Alert", typeof(bool), typeof(Row), new PropertyMetadata(false));





        public double Oee
        {
            get { return (double)GetValue(OeeProperty); }
            set { SetValue(OeeProperty, value); }
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
            set { SetValue(AvailabilityProperty, value); }
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
            set { SetValue(PerformanceProperty, value); }
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
            set { SetValue(QualityProperty, value); }
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

        public void UpdateData(EventData data)
        {
            UpdateDatabaseConnection(data);
            UpdateAvailability(data);
            UpdateSnapshots(data);
            UpdateVariables(data);
            UpdateOEE(data);
        }

        private void UpdateDatabaseConnection(EventData data)
        {
            if (data.Id.ToLower() == "statusdata_connection")
            {
                if (data.Data02.GetType() == typeof(bool))
                {
                    Connected = (bool)data.Data02;
                }
            }
        }

        private void UpdateAvailability(EventData data)
        {
            if (data.Id.ToLower() == "statusdata_availability")
            {
                if (data.Data02.GetType() == typeof(bool))
                {
                    Available = (bool)data.Data02;
                }
            }
        }

        private void UpdateVariables(EventData data)
        {
            if (data.Id.ToLower() == "statusdata_variables")
            {
                if (data.Data02 != null)
                {
                    var currentTime = DataTable_Functions.GetTableValue(data.Data02, "variable", "shift_currenttime", "value");
                    if (currentTime != null)
                    {
                        DateTime time = DateTime.MinValue;
                        if (DateTime.TryParse(currentTime, out time))
                        {
                            CurrentTime = time;
                        }
                    }
                }
            }
        }

        private void UpdateSnapshots(EventData data)
        {
            if (data.Id.ToLower() == "statusdata_snapshots")
            {
                if (data.Data02 != null)
                {
                    DeviceStatus = DataTable_Functions.GetTableValue(data.Data02, "name", "Device Status", "value");

                    //Alert = DataTable_Functions.GetBooleanTableValue(data.Data02, "name", "Alert", "value");

                    //Idle = DataTable_Functions.GetBooleanTableValue(data.Data02, "name", "Idle", "value");

                    //Production = DataTable_Functions.GetBooleanTableValue(data.Data02, "name", "Production", "value");
                }
            }
        }

        private void UpdateOEE(EventData data)
        {
            if (data.Id.ToLower() == "statusdata_oee_segments")
            {
                var dt = data.Data02 as DataTable;
                if (dt != null)
                {
                    double plannedProductionTime = 0;
                    double operatingTime = 0;
                    double idealOperatingTime = 0;
                    double totalPieces = 0;
                    double goodPieces = 0;

                    foreach (DataRow row in dt.Rows)
                    {
                        plannedProductionTime += DataTable_Functions.GetDoubleFromRow("planned_production_time", row);
                        operatingTime += DataTable_Functions.GetDoubleFromRow("operating_time", row);
                        idealOperatingTime += DataTable_Functions.GetDoubleFromRow("ideal_operating_time", row);

                        totalPieces += DataTable_Functions.GetDoubleFromRow("total_pieces", row);
                        goodPieces += DataTable_Functions.GetDoubleFromRow("good_pieces", row);
                    }

                    // Calculate Availability
                    if (plannedProductionTime > 0)
                    {
                        Availability = Math.Min(1, operatingTime / plannedProductionTime);

                        if (Availability > 0.7) AvailabilityStatus = 2;
                        else if (Availability > 0.4) AvailabilityStatus = 1;
                        else AvailabilityStatus = 0;
                    }

                    // Calculate Performance
                    if (operatingTime > 0)
                    {
                        Performance = Math.Min(1, idealOperatingTime / operatingTime);

                        if (Performance > 0.7) PerformanceStatus = 2;
                        else if (Performance > 0.4) PerformanceStatus = 1;
                        else PerformanceStatus = 0;
                    }

                    // Calculate Quality
                    if (totalPieces > 0)
                    {
                        Quality = Math.Min(1, goodPieces / totalPieces);

                        if (Quality > 0.7) QualityStatus = 2;
                        else if (Quality > 0.4) QualityStatus = 1;
                        else QualityStatus = 0;
                    }
                    else
                    {
                        Quality = 1;
                        QualityStatus = 2;
                    }

                    // Calculate OEE
                    Oee = Availability * Performance * Quality;

                    if (Oee > 0.7) OeeStatus = 2;
                    else if (Oee > 0.4) OeeStatus = 1;
                    else OeeStatus = 0;
                }
            }
        }

    }
}
