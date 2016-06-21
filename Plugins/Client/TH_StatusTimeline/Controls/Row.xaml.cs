using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Data;

using TH_Global.Functions;
using TH_Plugins;
using UI_Tools.Timeline;

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



        public TH_Configuration.Configuration Configuration
        {
            get { return (TH_Configuration.Configuration)GetValue(ConfigurationProperty); }
            set { SetValue(ConfigurationProperty, value); }
        }

        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register("Configuration", typeof(TH_Configuration.Configuration), typeof(Row), new PropertyMetadata(null));



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







        public HourData[] HourDatas
        {
            get { return (HourData[])GetValue(HourDatasProperty); }
            set { SetValue(HourDatasProperty, value); }
        }

        public static readonly DependencyProperty HourDatasProperty =
            DependencyProperty.Register("HourDatas", typeof(HourData[]), typeof(Row), new PropertyMetadata(null));


        #endregion

        private string shiftDate;
        private DateTime currentTime;
        private DateTime previousTimestamp;

        public void LoadData(EventData data)
        {
            LoadSnapshots(data);
            LoadVariables(data);
            LoadStatusData(data);
        }

        private void LoadVariables(EventData data)
        {
            if (data.Id.ToLower() == "statusdata_variables")
            {
                if (data.Data02 != null)
                {
                    currentTime = DataTable_Functions.GetDateTimeTableValue(data.Data02, "variable", "shift_currenttime", "value");

                    shiftDate = DataTable_Functions.GetTableValue(data.Data02, "variable", "shift_date", "value");
                }
            }
        }

        private void LoadSnapshots(EventData data)
        {
            if (data.Id.ToLower() == "statusdata_snapshots")
            {
                if (data.Data02 != null)
                {
                    Alert = DataTable_Functions.GetBooleanTableValue(data.Data02, "name", "Alert", "value");

                    Idle = DataTable_Functions.GetBooleanTableValue(data.Data02, "name", "Idle", "value");

                    Production = DataTable_Functions.GetBooleanTableValue(data.Data02, "name", "Production", "value");
                }
            }
        }

        private DateTime lastUIUpdate = DateTime.MinValue;

        public TimeSpan UIDelay = TimeSpan.FromSeconds(30);

        private TimeSpan MIN_DURATION = TimeSpan.FromSeconds(60);

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

        private void LoadStatusData(EventData data)
        {
            if (data.Id.ToLower() == "statusdata_productionstatus" && data.Data02 != null)
            {
                if ((DateTime.Now - lastUIUpdate) > UIDelay)
                {
                    lastUIUpdate = DateTime.Now;

                    var dt = data.Data02 as DataTable;
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
        }

        //private void LoadStatusData(EventData data)
        //{
        //    if (data.Id.ToLower() == "statusdata_productionstatus" && data.Data02 != null)
        //    {
        //        if ((DateTime.Now - lastUIUpdate) > UIDelay)
        //        {
        //            lastUIUpdate = DateTime.Now;

        //            var dt = data.Data02 as DataTable;
        //            if (dt != null)
        //            {
        //                DateTime previousTimestamp = DateTime.MinValue;
        //                string previousValue = null;

        //                DateTime previousUsedTimestamp = DateTime.MinValue;
        //                string previousUsedValue = null;

        //                var infos = new List<StatusInfo>();

        //                var dv = dt.AsDataView();
        //                dv.Sort = "TIMESTAMP ASC";
        //                var temp_dt = dv.ToTable();

        //                for (var x = 0; x < temp_dt.Rows.Count; x++)
        //                {
        //                    DataRow row = temp_dt.Rows[x];

        //                    DateTime timestamp = DataTable_Functions.GetDateTimeFromRow("timestamp", row).ToLocalTime();
        //                    string value = DataTable_Functions.GetRowValue("value", row);

        //                    if (previousTimestamp > DateTime.MinValue && timestamp > previousTimestamp)
        //                    {
        //                        if ((timestamp - previousTimestamp) >= MIN_DURATION && value != previousUsedValue)
        //                        {
        //                            var info = new StatusInfo();
        //                            info.Start = previousUsedTimestamp;
        //                            info.End = timestamp;
        //                            info.Value = previousUsedValue;

        //                            if (info.Duration >= MIN_DURATION)
        //                            {
        //                                infos.Add(info);

        //                                previousUsedTimestamp = info.End;
        //                                previousUsedValue = info.Value;
        //                            }
        //                        }
        //                    }

        //                    // Capture on first iteration
        //                    if (x == 0)
        //                    {
        //                        previousUsedTimestamp = timestamp;
        //                        previousUsedValue = value;
        //                    }

        //                    previousTimestamp = timestamp;
        //                    previousValue = value;
        //                }

        //                //DateTime previousTimestamp1;
        //                //DateTime previousTimestamp2;
        //                //string previousValue1;
        //                //string previousValue2;

        //                var events = new List<TimelineEvent>();

        //                foreach (var info in infos)
        //                {
        //                    var e = CreateEvent(info.Start, info.End, info.Value);
        //                    events.Add(e);
        //                }

        //                timeline.ResetEvents(events);
        //            }
        //        }
        //    }
        //}

        //private void LoadStatusData(EventData data)
        //{
        //    if (data.Id.ToLower() == "statusdata_productionstatus" && data.Data02 != null)
        //    {
        //        var dt = data.Data02 as DataTable;
        //        if (dt != null)
        //        {
        //            DateTime previousTimestamp = DateTime.MinValue;
        //            string previousValue = null;

        //            var events = new List<TimelineEvent>();

        //            for (var x = 0; x < dt.Rows.Count; x++)
        //            {
        //                DataRow row = dt.Rows[x];

        //                string ts = DataTable_Functions.GetRowValue("timestamp", row);
        //                string value = DataTable_Functions.GetRowValue("value", row);

        //                if (ts != null)
        //                {
        //                    DateTime timestamp = DateTime.MinValue;
        //                    if (DateTime.TryParse(ts, out timestamp))
        //                    {
        //                        timestamp = timestamp.ToLocalTime();

        //                        if (previousTimestamp > DateTime.MinValue && timestamp > previousTimestamp)
        //                        {
        //                            var e = CreateEvent(previousTimestamp, timestamp, previousValue);
        //                            events.Add(e);
        //                        }

        //                        previousTimestamp = timestamp;
        //                        previousValue = value;
        //                    }
        //                }
        //            }

        //            if (currentTime > previousTimestamp && currentTime > DateTime.MinValue && previousTimestamp > DateTime.MinValue)
        //            {
        //                var current = CreateEvent(previousTimestamp, currentTime, previousValue);
        //                events.Add(current);

        //                DateTime n = currentTime;

        //                timeline.MaxDateTime = new DateTime(n.Year, n.Month, n.Day, 23, 59, 59);
        //                timeline.MinDateTime = new DateTime(n.Year, n.Month, n.Day, 0, 0, 0);

        //                timeline.CurrentDateTime = n;
        //            }

        //            timeline.ResetEvents(events);
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
            Color production = UI_Tools.Functions.Colors.GetColorFromResource(this, "StatusGreen");

            if (alert == null) alert = Colors.Red;
            if (alert == null) idle = Colors.Yellow;
            if (alert == null) production = Colors.Green;

            if (value == "Alert") e.EventBrush = new SolidColorBrush(alert);
            else if (value == "Idle") e.EventBrush = new SolidColorBrush(idle);
            else if (value == "Production") e.EventBrush = new SolidColorBrush(production);

            e.Title = value;
            e.Description = TimeSpan_Functions.ToFormattedString(duration);
            e.RowOverride = 0;
            e.IsDuration = true;

            return e;
        }
    }
}
