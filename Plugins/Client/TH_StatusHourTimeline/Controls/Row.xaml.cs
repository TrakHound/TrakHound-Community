using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

using TH_Global.Functions;
using TH_Plugins;

namespace TH_StatusHourTimeline.Controls
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

            HourDatas = new List<HourData>();
            for (var x = 0; x < 24; x++) HourDatas.Add(new HourData(x, x + 1));
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



        public TH_Configuration.DeviceConfiguration Configuration
        {
            get { return (TH_Configuration.DeviceConfiguration)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(TH_Configuration.DeviceConfiguration), typeof(Row), new PropertyMetadata(null));


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



        public List<HourData> HourDatas
        {
            get { return (List<HourData>)GetValue(HourDatasProperty); }
            set { SetValue(HourDatasProperty, value); }
        }

        public static readonly DependencyProperty HourDatasProperty =
            DependencyProperty.Register("HourDatas", typeof(List<HourData>), typeof(Row), new PropertyMetadata(null));

        

        //public HourData[] HourDatas
        //{
        //    get { return (HourData[])GetValue(HourDatasProperty); }
        //    set { SetValue(HourDatasProperty, value); }
        //}

        //public static readonly DependencyProperty HourDatasProperty =
        //    DependencyProperty.Register("HourDatas", typeof(HourData[]), typeof(Row), new PropertyMetadata(null));

        #endregion

        public DateTime CurrentTime { get; set; }

        public void UpdateData(EventData data)
        {
            UpdateDatabaseConnection(data);
            UpdateAvailability(data);
            UpdateSnapshots(data);
            UpdateVariables(data);
            UpdateShiftData(data);
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

        private void UpdateShiftData(EventData data)
        {
            if (data.Id.ToLower() == "statusdata_shiftdata")
            {
                var dt = data.Data02 as DataTable;
                if (dt != null)
                {
                    var shiftDatas = new List<ShiftData>();

                    // Get ShiftData objects
                    foreach (DataRow row in dt.Rows) shiftDatas.Add(ShiftData.Read(row));

                    foreach (var hourData in HourDatas)
                    {
                        double total = 0;
                        double active = 0;
                        double idle = 0;
                        double alert = 0;

                        var matches = shiftDatas.FindAll(x => x.Start.Hour == hourData.StartHour);
                        if (matches != null)
                        {
                            foreach (var match in matches)
                            {
                                total += match.TotalTime;
                                active += match.ProductionTime;
                                idle += match.IdleTime;
                                alert += match.AlertTime;
                            }
                        }

                        hourData.TotalSeconds = total;
                        hourData.ActiveSeconds = active;
                        hourData.IdleSeconds = idle;
                        hourData.AlertSeconds = alert;
                    }
                }
            }
        }

        private class ShiftData
        {
            public string Id { get; set; }
            public string Date { get; set; }
            public string Shift { get; set; }
            //public int SegmentId { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public DateTime StartUtc { get; set; }
            public DateTime EndUtc { get; set; }
            public string Type { get; set; }
            public double TotalTime { get; set; }
            public double ProductionTime { get; set; }
            public double IdleTime { get; set; }
            public double AlertTime { get; set; }

            public static ShiftData Read(DataRow row)
            {
                var result = new ShiftData();

                result.Id = DataTable_Functions.GetRowValue("id", row);
                result.Date = DataTable_Functions.GetRowValue("date", row);
                result.Shift = DataTable_Functions.GetRowValue("shift", row);

                result.Start = DataTable_Functions.GetDateTimeFromRow("start", row);
                result.End = DataTable_Functions.GetDateTimeFromRow("end", row);

                result.StartUtc = DataTable_Functions.GetDateTimeFromRow("startutc", row);
                result.EndUtc = DataTable_Functions.GetDateTimeFromRow("endutc", row);

                result.Type = DataTable_Functions.GetRowValue("type", row);
                result.TotalTime = DataTable_Functions.GetDoubleFromRow("totaltime", row);
                result.ProductionTime = DataTable_Functions.GetDoubleFromRow("device_status__active", row);
                result.IdleTime = DataTable_Functions.GetDoubleFromRow("device_status__idle", row);
                result.AlertTime = DataTable_Functions.GetDoubleFromRow("device_status__alert", row);

                return result;
            }

        }

    }
}
