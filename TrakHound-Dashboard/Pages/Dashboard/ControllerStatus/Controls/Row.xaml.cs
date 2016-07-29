using System;
using System.Windows;
using System.Windows.Controls;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.Dashboard.ControllerStatus.Controls
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


        public DeviceConfiguration Configuration
        {
            get { return (DeviceConfiguration)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(DeviceConfiguration), typeof(Row), new PropertyMetadata(null));


        public string DeviceStatus
        {
            get { return (string)GetValue(DeviceStatusProperty); }
            set { SetValue(DeviceStatusProperty, value); }
        }

        public static readonly DependencyProperty DeviceStatusProperty =
            DependencyProperty.Register("DeviceStatus", typeof(string), typeof(Row), new PropertyMetadata(null));


        public string Availability
        {
            get { return (string)GetValue(AvailabilityProperty); }
            set { SetValue(AvailabilityProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityProperty =
            DependencyProperty.Register("Availability", typeof(string), typeof(Row), new PropertyMetadata("UNAVAILABLE"));


        public string EmergencyStop
        {
            get { return (string)GetValue(EmergencyStopProperty); }
            set { SetValue(EmergencyStopProperty, value); }
        }

        public static readonly DependencyProperty EmergencyStopProperty =
            DependencyProperty.Register("EmergencyStop", typeof(string), typeof(Row), new PropertyMetadata("UNAVAILABLE"));


        public string ExecutionMode
        {
            get { return (string)GetValue(ExecutionModeProperty); }
            set { SetValue(ExecutionModeProperty, value); }
        }

        public static readonly DependencyProperty ExecutionModeProperty =
            DependencyProperty.Register("ExecutionMode", typeof(string), typeof(Row), new PropertyMetadata("UNAVAILABLE"));


        public string ControllerMode
        {
            get { return (string)GetValue(ControllerModeProperty); }
            set { SetValue(ControllerModeProperty, value); }
        }

        public static readonly DependencyProperty ControllerModeProperty =
            DependencyProperty.Register("ControllerMode", typeof(string), typeof(Row), new PropertyMetadata("UNAVAILABLE"));


        public string SystemStatus
        {
            get { return (string)GetValue(SystemStatusProperty); }
            set { SetValue(SystemStatusProperty, value); }
        }

        public static readonly DependencyProperty SystemStatusProperty =
            DependencyProperty.Register("SystemStatus", typeof(string), typeof(Row), new PropertyMetadata("UNAVAILABLE"));


        public string SystemMessage
        {
            get { return (string)GetValue(SystemMessageProperty); }
            set { SetValue(SystemMessageProperty, value); }
        }

        public static readonly DependencyProperty SystemMessageProperty =
            DependencyProperty.Register("SystemMessage", typeof(string), typeof(Row), new PropertyMetadata("UNAVAILABLE"));


        public string Program
        {
            get { return (string)GetValue(ProgramProperty); }
            set { SetValue(ProgramProperty, value); }
        }

        public static readonly DependencyProperty ProgramProperty =
            DependencyProperty.Register("Program", typeof(string), typeof(Row), new PropertyMetadata("UNAVAILABLE"));


        public string Block
        {
            get { return (string)GetValue(BlockProperty); }
            set { SetValue(BlockProperty, value); }
        }

        public static readonly DependencyProperty BlockProperty =
            DependencyProperty.Register("Block", typeof(string), typeof(Row), new PropertyMetadata("UNAVAILABLE"));


        public string Line
        {
            get { return (string)GetValue(LineProperty); }
            set { SetValue(LineProperty, value); }
        }

        public static readonly DependencyProperty LineProperty =
            DependencyProperty.Register("Line", typeof(string), typeof(Row), new PropertyMetadata("UNAVAILABLE"));

        #endregion

        public DateTime CurrentTime { get; set; }

        public void UpdateData(EventData data)
        {
            UpdateDatabaseConnection(data);
            UpdateAvailability(data);
            UpdateSnapshots(data);
            UpdateVariables(data);
            UpdateStatus(data);
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
       
        private void UpdateStatus(EventData data)
        {
            // Status Table Data
            if (data.Id.ToLower() == "statusdata_status" && data.Data02 != null)
            {
                Availability = DataTable_Functions.GetTableValue(data.Data02, "type", "AVAILABILITY", "value1");
                EmergencyStop = DataTable_Functions.GetTableValue(data.Data02, "type", "EMERGENCY_STOP", "value1");
                ExecutionMode = DataTable_Functions.GetTableValue(data.Data02, "type", "EXECUTION", "value1");
                ControllerMode = DataTable_Functions.GetTableValue(data.Data02, "type", "CONTROLLER_MODE", "value1");
                SystemStatus = DataTable_Functions.GetTableValue(data.Data02, "type", "SYSTEM", "value2");
                SystemMessage = DataTable_Functions.GetTableValue(data.Data02, "type", "SYSTEM", "value1");
                Program = DataTable_Functions.GetTableValue(data.Data02, "type", "PROGRAM", "value1");
                Block = DataTable_Functions.GetTableValue(data.Data02, "type", "BLOCK", "value1");
                Line = DataTable_Functions.GetTableValue(data.Data02, "type", "LINE", "value1");
            }
        }

    }
}
