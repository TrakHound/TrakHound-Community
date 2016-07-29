using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

using TrakHound.Tools;
using TrakHound.Plugins;

namespace TrakHound_Overview.Pages.Dashboard.OverrideStatus.Controls
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


        //public bool Active
        //{
        //    get { return (bool)GetValue(ActiveProperty); }
        //    set { SetValue(ActiveProperty, value); }
        //}

        //public static readonly DependencyProperty ActiveProperty =
        //    DependencyProperty.Register("Active", typeof(bool), typeof(Row), new PropertyMetadata(false));



        ////public bool Production
        ////{
        ////    get { return (bool)GetValue(ProductionProperty); }
        ////    set { SetValue(ProductionProperty, value); }
        ////}

        ////public static readonly DependencyProperty ProductionProperty =
        ////    DependencyProperty.Register("Production", typeof(bool), typeof(Row), new PropertyMetadata(false));


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



        public double TotalSeconds
        {
            get { return (double)GetValue(TotalSecondsProperty); }
            set { SetValue(TotalSecondsProperty, value); }
        }

        public static readonly DependencyProperty TotalSecondsProperty =
            DependencyProperty.Register("TotalSeconds", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double ActiveSeconds
        {
            get { return (double)GetValue(ActiveSecondsProperty); }
            set { SetValue(ActiveSecondsProperty, value); }
        }

        public static readonly DependencyProperty ActiveSecondsProperty =
            DependencyProperty.Register("ActiveSeconds", typeof(double), typeof(Row), new PropertyMetadata(0d));


        //public double ProductionSeconds
        //{
        //    get { return (double)GetValue(ProductionSecondsProperty); }
        //    set { SetValue(ProductionSecondsProperty, value); }
        //}

        //public static readonly DependencyProperty ProductionSecondsProperty =
        //    DependencyProperty.Register("ProductionSeconds", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double IdleSeconds
        {
            get { return (double)GetValue(IdleSecondsProperty); }
            set { SetValue(IdleSecondsProperty, value); }
        }

        public static readonly DependencyProperty IdleSecondsProperty =
            DependencyProperty.Register("IdleSeconds", typeof(double), typeof(Row), new PropertyMetadata(0d));


        public double AlertSeconds
        {
            get { return (double)GetValue(AlertSecondsProperty); }
            set { SetValue(AlertSecondsProperty, value); }
        }

        public static readonly DependencyProperty AlertSecondsProperty =
            DependencyProperty.Register("AlertSeconds", typeof(double), typeof(Row), new PropertyMetadata(0d));


        #endregion

        public DateTime CurrentTime { get; set; }

        public void UpdateData(EventData data)
        {
            UpdateDatabaseConnection(data);
            UpdateAvailability(data);
            UpdateSnapshots(data);
            UpdateVariables(data);
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

                }
            }
        }

    }
}
