using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TrakHound.Tools;
using TrakHound.Plugins;
using TrakHound_UI.Timeline;

using TrakHound.Configurations;
using TrakHound.Databases;

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

            var d = DateTime.Now;
            var start = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
            var end = start.AddDays(1);

            timeline.MinDateTime = start;
            timeline.MaxDateTime = end;

            timeline.CurrentDateTime = start.AddHours(12);
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
                }
            }
        }


        public DateTime LastTimelineUpdate = DateTime.MinValue;

        private TimeSpan UIDelay = TimeSpan.FromSeconds(30);
        private TimeSpan MIN_DURATION = TimeSpan.FromSeconds(60);

        private void UpdateDeviceStatusData(EventData data, string shiftDate)
        {
            var d = DateTime.Now;

            if ((d - LastTimelineUpdate) > UIDelay)
            {
                LastTimelineUpdate = d;

                var start = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0);
                var end = start.AddDays(1);

                timeline.MinDateTime = start;
                timeline.MaxDateTime = end;

                timeline.CurrentDateTime = start.AddHours(12);

                start = start.ToUniversalTime();
                end = end.ToUniversalTime();

                var deviceConfig = (DeviceConfiguration)data.Data01;

                var events = new List<TimelineEvent>();

                events.AddRange(UpdateDeviceStatusData(deviceConfig, start, end));
                events.AddRange(UpdateProductionStatusData(deviceConfig, start, end));

                timeline.ResetEvents(events);
            }
        }


        private List<TimelineEvent> UpdateDeviceStatusData(DeviceConfiguration config, DateTime start, DateTime end)
        {
            if (end < start) end = end.AddDays(1);

            if (start > DateTime.MinValue && end > DateTime.MinValue)
            {
                string filter = "WHERE date(TIMESTAMP) BETWEEN date('" + start.ToString("yyyy-MM-dd HH:mm:ss") + "') AND date('" + end.ToString("yyyy-MM-dd HH:mm:ss") + "')";

                string tableName = TableNames.Gen_Events_TablePrefix + "device_status";

                DataTable dt = Table.Get(config.Databases_Client, Global.GetTableName(tableName, config.DatabaseId), filter);
                if (dt != null)
                {
                    // Get list of all infos
                    var infos = StatusInfo.FromTable(dt, start, end);

                    if (infos.Count == 0 && dt.Rows.Count == 1)
                    {
                        // If only one value in table (the current value)
                        // then create one StatusInfo object using that timestamp and the current timestamp
                        DateTime timestamp = DataTable_Functions.GetDateTimeFromRow("timestamp", dt.Rows[0]);
                        if (timestamp < start) timestamp = start;
                        else if (timestamp > end) timestamp = end;

                        timestamp = timestamp.ToLocalTime();

                        var current = currentTime;
                        if (current > end) current = end.ToLocalTime();

                        string value = DataTable_Functions.GetRowValue("value", dt.Rows[0]);

                        var info = new StatusInfo();
                        info.Value = value;
                        info.Start = timestamp;
                        info.End = current;
                        infos.Add(info);
                    }
                    else if (dt.Rows.Count > 1)
                    {
                        // Filter out infos with duration less than min
                        infos = infos.FindAll(x => x.Duration >= MIN_DURATION);

                        if (infos.Count > 0 && currentTime > DateTime.MinValue && currentTime > infos[infos.Count - 1].End)
                        {
                            DateTime timestamp = DataTable_Functions.GetDateTimeFromRow("timestamp", dt.Rows[dt.Rows.Count - 1]);
                            if (timestamp < start) timestamp = start;
                            else if (timestamp > end) timestamp = end;

                            timestamp = timestamp.ToLocalTime();

                            var current = currentTime;
                            if (current > end) current = end.ToLocalTime();

                            string value = DataTable_Functions.GetRowValue("value", dt.Rows[dt.Rows.Count - 1]);

                            var info = new StatusInfo();
                            info.Value = value;
                            info.Start = timestamp;
                            info.End = current;
                            infos.Add(info);
                        }


                        // Combine adjacent infos with same values
                        var combinedInfos = new List<StatusInfo>();
                        DateTime previousTimestamp = DateTime.MinValue;
                        for (var x = 0; x < infos.Count; x++)
                        {
                            if (x == 0)
                            {
                                previousTimestamp = infos[x].Start;
                            }
                            else if (x > 0)
                            {
                                var newInfo = infos[x];
                                var oldInfo = infos[x - 1];

                                // If new.Value is different from old.Value
                                if (newInfo.Value != oldInfo.Value)
                                {
                                    var combinedInfo = new StatusInfo();
                                    combinedInfo.Start = previousTimestamp;
                                    combinedInfo.End = oldInfo.End;
                                    combinedInfo.Value = oldInfo.Value;
                                    combinedInfos.Add(combinedInfo);

                                    previousTimestamp = newInfo.Start;
                                }

                                if (x == infos.Count - 1)
                                {
                                    var lastInfo = new StatusInfo();
                                    lastInfo.Start = previousTimestamp;
                                    lastInfo.End = newInfo.End;
                                    lastInfo.Value = newInfo.Value;
                                    combinedInfos.Add(lastInfo);
                                }
                            }
                        }

                        infos = combinedInfos;
                    }

                    var events = new List<TimelineEvent>();

                    foreach (var info in infos)
                    {
                        var e = CreateDeviceStatusEvent(info.Start, info.End, info.Value);
                        events.Add(e);
                    }

                    return events;
                }
            }

            return new List<TimelineEvent>();
        }

        private List<TimelineEvent> UpdateProductionStatusData(DeviceConfiguration config, DateTime start, DateTime end)
        {
            if (end < start) end = end.AddDays(1);

            if (start > DateTime.MinValue && end > DateTime.MinValue)
            {
                string filter = "WHERE date(TIMESTAMP) BETWEEN date('" + start.ToString("yyyy-MM-dd HH:mm:ss") + "') AND date('" + end.ToString("yyyy-MM-dd HH:mm:ss") + "')";

                string tableName = TableNames.Gen_Events_TablePrefix + "production_status";

                DataTable dt = Table.Get(config.Databases_Client, Global.GetTableName(tableName, config.DatabaseId), filter);
                if (dt != null)
                {
                    // Get list of all infos
                    var infos = StatusInfo.FromTable(dt, start, end);
                    if (infos.Count == 0 && dt.Rows.Count == 1)
                    {
                        // If only one value in table (the current value)
                        // then create one StatusInfo object using that timestamp and the current timestamp
                        DateTime timestamp = DataTable_Functions.GetDateTimeFromRow("timestamp", dt.Rows[0]);
                        if (timestamp < start) timestamp = start;
                        else if (timestamp > end) timestamp = end;

                        timestamp = timestamp.ToLocalTime();

                        var current = currentTime;
                        if (current > end) current = end.ToLocalTime();

                        string value = DataTable_Functions.GetRowValue("value", dt.Rows[0]);

                        var info = new StatusInfo();
                        info.Value = value;
                        info.Start = timestamp;
                        info.End = current;
                        infos.Add(info);
                    }
                    else if (dt.Rows.Count > 1)
                    {
                        // Filter out infos with duration less than min
                        infos = infos.FindAll(x => x.Duration >= MIN_DURATION);

                        if (infos.Count > 0 && currentTime > DateTime.MinValue && currentTime > infos[infos.Count - 1].End)
                        {
                            DateTime timestamp = DataTable_Functions.GetDateTimeFromRow("timestamp", dt.Rows[dt.Rows.Count - 1]);
                            if (timestamp < start) timestamp = start;
                            else if (timestamp > end) timestamp = end;

                            timestamp = timestamp.ToLocalTime();

                            var current = currentTime;
                            if (current > end) current = end.ToLocalTime();

                            string value = DataTable_Functions.GetRowValue("value", dt.Rows[dt.Rows.Count - 1]);

                            var info = new StatusInfo();
                            info.Value = value;
                            info.Start = timestamp;
                            info.End = current;
                            infos.Add(info);
                        }

                        // Combine adjacent infos with same values
                        var combinedInfos = new List<StatusInfo>();
                        DateTime previousTimestamp = DateTime.MinValue;
                        for (var x = 0; x < infos.Count; x++)
                        {
                            if (x == 0)
                            {
                                previousTimestamp = infos[x].Start;
                            }
                            else if (x > 0)
                            {
                                var newInfo = infos[x];
                                var oldInfo = infos[x - 1];

                                // If new.Value is different from old.Value
                                if (newInfo.Value != oldInfo.Value)
                                {
                                    var combinedInfo = new StatusInfo();
                                    combinedInfo.Start = previousTimestamp;
                                    combinedInfo.End = oldInfo.End;
                                    combinedInfo.Value = oldInfo.Value;
                                    combinedInfos.Add(combinedInfo);

                                    previousTimestamp = newInfo.Start;
                                }

                                if (x == infos.Count - 1)
                                {
                                    var lastInfo = new StatusInfo();
                                    lastInfo.Start = previousTimestamp;
                                    lastInfo.End = newInfo.End;
                                    lastInfo.Value = newInfo.Value;
                                    combinedInfos.Add(lastInfo);
                                }
                            }
                        }

                        infos = combinedInfos;
                    }

                    var events = new List<TimelineEvent>();

                    foreach (var info in infos)
                    {
                        var e = CreateProductionStatusEvent(info.Start, info.End, info.Value);
                        events.Add(e);
                    }

                    return events;
                }
            }

            return new List<TimelineEvent>();
        }


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

            public static List<StatusInfo> FromTable(DataTable dt, DateTime start, DateTime end)
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

                    DateTime timestamp = DataTable_Functions.GetDateTimeFromRow("timestamp", row);
                    if (timestamp < start) timestamp = start;
                    else if (timestamp > end) timestamp = end;

                    timestamp = timestamp.ToLocalTime();

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


        private TimelineEvent CreateDeviceStatusEvent(DateTime start, DateTime end, string value)
        {
            var e = new TimelineEvent();
            e.StartDate = start;
            e.EndDate = end;

            TimeSpan duration = end - start;

            Color alert = TrakHound_UI.Functions.Color_Functions.GetColorFromResource(this, "StatusRed");
            Color idle = TrakHound_UI.Functions.Color_Functions.GetColorFromResource(this, "StatusYellow");
            Color active = TrakHound_UI.Functions.Color_Functions.GetColorFromResource(this, "StatusGreen");

            if (alert == null) alert = Colors.Red;
            if (alert == null) idle = Colors.Yellow;
            if (alert == null) active = Colors.Green;

            if (value == "Alert") e.EventBrush = new SolidColorBrush(alert);
            else if (value == "Idle") e.EventBrush = new SolidColorBrush(idle);
            else if (value == "Active") e.EventBrush = new SolidColorBrush(active);

            e.Title = value;
            e.Description = TimeSpan_Functions.ToFormattedString(duration);
            e.RowOverride = 1;
            e.HeightOverride = 10;
            e.TopOverride = 20;
            e.Tag = "Device Status";
            e.IsDuration = true;

            return e;
        }

        private TimelineEvent CreateProductionStatusEvent(DateTime start, DateTime end, string value)
        {
            var e = new TimelineEvent();
            e.StartDate = start;
            e.EndDate = end;

            TimeSpan duration = end - start;

            Color production = TrakHound_UI.Functions.Color_Functions.GetColorFromResource(this, "StatusGreen");
            Color setup = TrakHound_UI.Functions.Color_Functions.GetColorFromResource(this, "StatusBlue");
            Color teardown = TrakHound_UI.Functions.Color_Functions.GetColorFromResource(this, "StatusOrange");
            Color maintenance = TrakHound_UI.Functions.Color_Functions.GetColorFromResource(this, "StatusTeal");
            Color processDevelopment = TrakHound_UI.Functions.Color_Functions.GetColorFromResource(this, "StatusPurple");

            if (value == "Production") e.EventBrush = new SolidColorBrush(production);
            else if (value == "Setup") e.EventBrush = new SolidColorBrush(setup);
            else if (value == "Teardown") e.EventBrush = new SolidColorBrush(teardown);
            else if (value == "Maintenance") e.EventBrush = new SolidColorBrush(maintenance);
            else if (value == "Process Development") e.EventBrush = new SolidColorBrush(processDevelopment);

            e.Title = value;
            e.Description = TimeSpan_Functions.ToFormattedString(duration);
            e.RowOverride = 0;
            e.HeightOverride = 10;
            e.TopOverride = 0;
            e.Tag = "Production Status";
            e.IsDuration = true;

            return e;
        }
    }
}
