using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TH_Global.Functions;
using TH_Plugins;
using UI_Tools.Timeline;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Shifts;

namespace TH_StatusTimeline.Controls
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


        public bool Production
        {
            get { return (bool)GetValue(ProductionProperty); }
            set { SetValue(ProductionProperty, value); }
        }

        public static readonly DependencyProperty ProductionProperty =
            DependencyProperty.Register("Production", typeof(bool), typeof(Row), new PropertyMetadata(false));


        public bool Idle
        {
            get { return (bool)GetValue(IdleProperty); }
            set { SetValue(IdleProperty, value); }
        }

        public static readonly DependencyProperty IdleProperty =
            DependencyProperty.Register("Idle", typeof(bool), typeof(Row), new PropertyMetadata(false));


        public bool Alert
        {
            get { return (bool)GetValue(AlertProperty); }
            set { SetValue(AlertProperty, value); }
        }

        public static readonly DependencyProperty AlertProperty =
            DependencyProperty.Register("Alert", typeof(bool), typeof(Row), new PropertyMetadata(false));

        #endregion

        private string shiftDate;
        private DateTime currentTime;

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
                    currentTime = DataTable_Functions.GetDateTimeTableValue(data.Data02, "variable", "shift_currenttime", "value");

                    shiftDate = DataTable_Functions.GetTableValue(data.Data02, "variable", "shift_date", "value");

                    UpdateDeviceStatusData(data, shiftDate);
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


        public DateTime LastTimelineUpdate = DateTime.MinValue;

        private TimeSpan UIDelay = TimeSpan.FromSeconds(30);
        private TimeSpan MIN_DURATION = TimeSpan.FromSeconds(60);

        private void UpdateDeviceStatusData(EventData data, string shiftDate)
        {
            if (currentTime > DateTime.MinValue && !string.IsNullOrEmpty(shiftDate))
            {
                if ((DateTime.Now - LastTimelineUpdate) > UIDelay)
                {
                    LastTimelineUpdate = DateTime.Now;

                    DateTime d = DateTime.MinValue;
                    if (DateTime.TryParse(shiftDate, out d))
                    {
                        var start = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0).Subtract(TimeSpan.FromDays(1));
                        var end = new DateTime(d.Year, d.Month, d.Day, 23, 59, 59);

                        var deviceConfig = (DeviceConfiguration)data.Data01;

                        UpdateDeviceStatusData(deviceConfig, start, end);
                    }
                }
            }
        }

        private void UpdateDeviceStatusData(DeviceConfiguration config, DateTime start, DateTime end)
        {
            if (end < start) end = end.AddDays(1);

            if (start > DateTime.MinValue && end > DateTime.MinValue)
            {
                string filter = "WHERE TIMESTAMP BETWEEN '" + start.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + end.ToString("yyyy-MM-dd HH:mm:ss") + "'";

                string tableName = TableNames.Gen_Events_TablePrefix + "device_status";

                DataTable dt = Table.Get(config.Databases_Client, Global.GetTableName(tableName, config.DatabaseId), filter);
                if (dt != null)
                {
                    // Get list of all infos
                    var infos = StatusInfo.FromTable(dt);

                    // Filter out infos with duration less than min
                    infos = infos.FindAll(x => x.Duration >= MIN_DURATION);

                    if (infos.Count > 0 && currentTime > DateTime.MinValue && currentTime > infos[infos.Count - 1].End)
                    {
                        var info = new StatusInfo();
                        info.Value = infos[infos.Count - 1].Value;
                        info.Start = infos[infos.Count - 1].End;
                        info.End = currentTime;
                        infos.Add(info);

                        DateTime n = currentTime;
                        timeline.MaxDateTime = new DateTime(n.Year, n.Month, n.Day, 23, 59, 59);
                        timeline.MinDateTime = new DateTime(n.Year, n.Month, n.Day, 0, 0, 0);

                        timeline.CurrentDateTime = n;
                    }


                    // Combine adjacent infos with same values
                    var combinedInfos = new List<StatusInfo>();
                    DateTime previousTimestamp = DateTime.MinValue;
                    for (var x = 0; x < infos.Count; x++)
                    {
                        if (x == 0) previousTimestamp = infos[x].Start;

                        if (x > 0)
                        {
                            if (infos[x].Value != infos[x - 1].Value || x == infos.Count - 1)
                            {
                                var newInfo = new StatusInfo();
                                newInfo.Start = previousTimestamp;

                                if (x == infos.Count - 1) newInfo.End = infos[x].End;
                                else newInfo.End = infos[x].Start;

                                newInfo.Value = infos[x - 1].Value;

                                combinedInfos.Add(newInfo);

                                previousTimestamp = infos[x].Start;
                            }
                        }
                    }

                    var events = new List<TimelineEvent>();

                    foreach (var info in combinedInfos)
                    {
                        var e = CreateEvent(info.Start, info.End, info.Value);
                        events.Add(e);
                    }

                    timeline.ResetEvents(events);
                }
            }
        }

        //static EventData GetProductionStatusData(DeviceConfiguration config, ShiftData shiftData)
        //{
        //    var result = new EventData();

        //    DateTime start = DateTime.MinValue;
        //    DateTime.TryParse(shiftData.shiftStartUTC, out start);

        //    DateTime end = DateTime.MinValue;
        //    DateTime.TryParse(shiftData.shiftEndUTC, out end);

        //    if (end < start) end = end.AddDays(1);

        //    if (start > DateTime.MinValue && end > DateTime.MinValue)
        //    {
        //        string filter = "WHERE TIMESTAMP BETWEEN '" + start.ToString("yyyy-MM-dd HH:mm:ss") + "' AND '" + end.ToString("yyyy-MM-dd HH:mm:ss") + "'";

        //        string tableName = TableNames.Gen_Events_TablePrefix + "production_status";


        //        DataTable dt = Table.Get(config.Databases_Client, Global.GetTableName(tableName, config.DatabaseId), filter);
        //        if (dt != null)
        //        {
        //            var data = new EventData();
        //            data.Id = "StatusData_ProductionStatus";
        //            data.Data01 = config;
        //            data.Data02 = dt;

        //            result = data;
        //        }
        //    }

        //    return result;
        //}

        //private DateTime lastUIUpdate = DateTime.MinValue;

        //public TimeSpan UIDelay = TimeSpan.FromSeconds(30);

        //private TimeSpan MIN_DURATION = TimeSpan.FromSeconds(60);

        private class StatusInfo
        {
            public string Value { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }

            public TimeSpan Duration
            {
                get
                {
                    return End - Start;
                }
            }

            public static List<StatusInfo> FromTable(DataTable dt)
            {
                DateTime previousTimestamp = DateTime.MinValue;
                string previousValue = null;

                var result = new List<StatusInfo>();

                var dv = dt.AsDataView();
                dv.Sort = "TIMESTAMP ASC";
                var temp_dt = dv.ToTable();

                for (var x = 0; x < temp_dt.Rows.Count; x++)
                {
                    DataRow row = temp_dt.Rows[x];

                    DateTime timestamp = DataTable_Functions.GetDateTimeFromRow("timestamp", row).ToLocalTime();
                    string value = DataTable_Functions.GetRowValue("value", row);

                    if (previousTimestamp > DateTime.MinValue && timestamp > previousTimestamp)
                    {
                        var info = new StatusInfo();
                        info.Start = previousTimestamp;
                        info.End = timestamp;
                        info.Value = previousValue;
                        result.Add(info);
                    }

                    previousTimestamp = timestamp;
                    previousValue = value;
                }

                return result;
            }
        }

        //private void UpdateStatusData(EventData data)
        //{
        //    if (data.Id.ToLower() == "statusdata_devicestatus" && data.Data02 != null)
        //    {
        //        if ((DateTime.Now - lastUIUpdate) > UIDelay)
        //        {
        //            lastUIUpdate = DateTime.Now;

        //            var dt = data.Data02 as DataTable;
        //            if (dt != null)
        //            {
        //                // Get list of all infos
        //                var infos = StatusInfo.FromTable(dt);

        //                // Filter out infos with duration less than min
        //                infos = infos.FindAll(x => x.Duration >= MIN_DURATION);

        //                if (infos.Count > 0 && currentTime > DateTime.MinValue && currentTime > infos[infos.Count - 1].End)
        //                {
        //                    var info = new StatusInfo();
        //                    info.Value = infos[infos.Count - 1].Value;
        //                    info.Start = infos[infos.Count - 1].End;
        //                    info.End = currentTime;
        //                    infos.Add(info);

        //                    DateTime n = currentTime;

        //                    timeline.MaxDateTime = new DateTime(n.Year, n.Month, n.Day, 23, 59, 59);
        //                    timeline.MinDateTime = new DateTime(n.Year, n.Month, n.Day, 0, 0, 0);

        //                    timeline.CurrentDateTime = n;
        //                }


        //                // Combine adjacent infos with same values
        //                var combinedInfos = new List<StatusInfo>();
        //                DateTime previousTimestamp = DateTime.MinValue;
        //                for (var x = 0; x < infos.Count; x++)
        //                {
        //                    if (x == 0) previousTimestamp = infos[x].Start;

        //                    if (x > 0)
        //                    {
        //                        if (infos[x].Value != infos[x - 1].Value || x == infos.Count - 1)
        //                        {
        //                            var newInfo = new StatusInfo();
        //                            newInfo.Start = previousTimestamp;

        //                            if (x == infos.Count - 1) newInfo.End = infos[x].End;
        //                            else newInfo.End = infos[x].Start;

        //                            newInfo.Value = infos[x - 1].Value;

        //                            combinedInfos.Add(newInfo);

        //                            previousTimestamp = infos[x].Start;
        //                        }
        //                    }
        //                }

        //                var events = new List<TimelineEvent>();

        //                foreach (var info in combinedInfos)
        //                {
        //                    var e = CreateEvent(info.Start, info.End, info.Value);
        //                    events.Add(e);
        //                }

        //                timeline.ResetEvents(events);
        //            }
        //        }
        //    }
        //}

        //private void UpdateStatusData(EventData data)
        //{
        //    if (data.Id.ToLower() == "statusdata_productionstatus" && data.Data02 != null)
        //    {
        //        if ((DateTime.Now - lastUIUpdate) > UIDelay)
        //        {
        //            lastUIUpdate = DateTime.Now;

        //            var dt = data.Data02 as DataTable;
        //            if (dt != null)
        //            {
        //                // Get list of all infos
        //                var infos = StatusInfo.FromTable(dt);

        //                // Filter out infos with duration less than min
        //                infos = infos.FindAll(x => x.Duration >= MIN_DURATION);

        //                if (infos.Count > 0 && currentTime > DateTime.MinValue && currentTime > infos[infos.Count - 1].End)
        //                {
        //                    var info = new StatusInfo();
        //                    info.Value = infos[infos.Count - 1].Value;
        //                    info.Start = infos[infos.Count - 1].End;
        //                    info.End = currentTime;
        //                    infos.Add(info);

        //                    DateTime n = currentTime;

        //                    timeline.MaxDateTime = new DateTime(n.Year, n.Month, n.Day, 23, 59, 59);
        //                    timeline.MinDateTime = new DateTime(n.Year, n.Month, n.Day, 0, 0, 0);

        //                    timeline.CurrentDateTime = n;
        //                }


        //                // Combine adjacent infos with same values
        //                var combinedInfos = new List<StatusInfo>();
        //                DateTime previousTimestamp = DateTime.MinValue;
        //                for (var x = 0; x < infos.Count; x++)
        //                {
        //                    if (x == 0) previousTimestamp = infos[x].Start;

        //                    if (x > 0)
        //                    {
        //                        if (infos[x].Value != infos[x - 1].Value || x == infos.Count - 1)
        //                        {
        //                            var newInfo = new StatusInfo();
        //                            newInfo.Start = previousTimestamp;

        //                            if (x == infos.Count - 1) newInfo.End = infos[x].End;
        //                            else newInfo.End = infos[x].Start;

        //                            newInfo.Value = infos[x - 1].Value;

        //                            combinedInfos.Add(newInfo);

        //                            previousTimestamp = infos[x].Start;
        //                        }
        //                    }
        //                }

        //                var events = new List<TimelineEvent>();

        //                foreach (var info in combinedInfos)
        //                {
        //                    var e = CreateEvent(info.Start, info.End, info.Value);
        //                    events.Add(e);
        //                }

        //                timeline.ResetEvents(events);
        //            }
        //        }
        //    }
        //}

        private TimelineEvent CreateEvent(DateTime start, DateTime end, string value)
        {
            var e = new TimelineEvent();
            e.StartDate = start;
            e.EndDate = end;

            TimeSpan duration = end - start;

            Color alert = UI_Tools.Functions.Colors.GetColorFromResource(this, "StatusRed");
            Color idle = UI_Tools.Functions.Colors.GetColorFromResource(this, "StatusYellow");
            Color active = UI_Tools.Functions.Colors.GetColorFromResource(this, "StatusGreen");

            if (alert == null) alert = Colors.Red;
            if (alert == null) idle = Colors.Yellow;
            if (alert == null) active = Colors.Green;

            if (value == "Alert") e.EventBrush = new SolidColorBrush(alert);
            else if (value == "Idle") e.EventBrush = new SolidColorBrush(idle);
            else if (value == "Active") e.EventBrush = new SolidColorBrush(active);

            e.Title = value;
            e.Description = TimeSpan_Functions.ToFormattedString(duration);
            e.RowOverride = 0;
            e.IsDuration = true;

            return e;
        }
    }
}
