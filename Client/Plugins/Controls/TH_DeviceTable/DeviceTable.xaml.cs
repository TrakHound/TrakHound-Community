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

using System.IO;
using System.Data;
using System.Collections.ObjectModel;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Client;
using TH_UserManagement.Management;

//using TH_WPF;

using TH_DeviceTable.Components;

namespace TH_DeviceTable
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DeviceTable : UserControl, Plugin
    {
        public DeviceTable()
        {
            InitializeComponent();
            DataContext = this;

            CreateColumnHeaders();

            DeviceDisplays = new List<DeviceDisplay>();
            RowHeaders.Clear();
            Rows.Clear();
        }

        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Device Table"; } }

        public string Description { get { return "Compare Devices using a Table Format"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceTable;component/Resources/List_01.png")); } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return null; } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return null; } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return null; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return "Dashboard"; } }
        public string DefaultParentCategory { get { return "Pages"; } }

        public bool AcceptsPlugins { get { return true; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ShowInAppMenu { get { return true; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<Plugin> Plugins { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Closing() { }

        public void Show()
        {
            if (ShowRequested != null)
            {
                PluginShowInfo info = new PluginShowInfo();
                info.Page = this;
                ShowRequested(info);
            }
        }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {
            Update(de_d);
        }

        public event DataEvent_Handler DataEvent;

        public event PluginTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Device Properties"

        List<Configuration> devices;
        public List<Configuration> Devices
        {
            get { return devices; }
            set
            {
                devices = value;

                if (devices != null)
                {
                    DeviceDisplays = new List<DeviceDisplay>();
                    RowHeaders.Clear();
                    Rows.Clear();

                    foreach (Configuration device in devices)
                    {
                        CreateDeviceDisplay(device);
                    }
                }
            }
        }

        #endregion

        #region "Options"

        public TH_Global.Page Options { get; set; }

        #endregion

        #region "User"

        UserConfiguration currentuser = null;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;
            }
        }

        public Database_Settings UserDatabaseSettings { get; set; }

        #endregion

        public object RootParent { get; set; }

        #endregion

        #region "Device Table"

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;

        void Update(DataEvent_Data de_d)
        {
            if (de_d != null)
            {
                Configuration config = de_d.data01 as Configuration;
                if (config != null)
                {
                    DeviceDisplay dd = DeviceDisplays.Find(x => x.configuration.UniqueId == config.UniqueId);
                    if (dd != null)
                    {
                        // Connection
                        if (de_d.id.ToLower() == "statusdata_connection")
                        {
                            bool connected;
                            bool.TryParse(de_d.data02.ToString(), out connected);

                            if (dd.Connected != connected)
                            {
                                dd.Connected = connected;
                                dd.ComparisonGroup.row.Loading = false;
                            }
                        }

                        // Snapshot Table Data
                        if (de_d.id.ToLower() == "statusdata_snapshots")
                        {

                            // Production
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateStatus_Production), Priority_Context, new object[] { dd, de_d.data02 });

                            // Idle
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateStatus_Idle), Priority_Context, new object[] { dd, de_d.data02 });

                            // Alert
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateStatus_Alert), Priority_Context, new object[] { dd, de_d.data02 });


                            // Production Status
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatus), Priority_Context, new object[] { dd, de_d.data02 });


                            //// Shift Info
                            //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateShiftInfo_Snapshots), Priority_Context, new object[] { dd, de_d.data02 });

                            //// OEE Timeline / Histogram
                            //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Timeline_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });

                            //// Production Status Times
                            //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });

                            // Current Program
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProgram_Current), Priority_Context, new object[] { dd, de_d.data02 });

                            // Previous Program
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProgram_Previous), Priority_Context, new object[] { dd, de_d.data02 });


                            // Production Status Timeline
                            //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });
                        }

                        //// Shifts Table Data
                        //if (de_d.id.ToLower() == "statusdata_shiftdata")
                        //{
                        //    // Shift Info
                        //    //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateShiftInfo_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });

                        //    // OEE Timeline / Histogram
                        //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Timeline_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });

                        //    // Production Status Times
                        //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });
                        //}

                        //// GenEvent Values
                        //if (de_d.id.ToLower() == "statusdata_geneventvalues")
                        //{
                        //    // Production Status Times
                        //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_GenEventValues), Priority_Context, new object[] { dd, de_d.data02 });

                        //    // Production Status Timeline
                        //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_GenEventValues), Priority_Context, new object[] { dd, de_d.data02 });
                        //}

                        // OEE Table Data
                        if (de_d.id.ToLower() == "statusdata_oee")
                        {
                            // OEE Average
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Avg), Priority_Context, new object[] { dd, de_d.data02 });

                            // Current Segment OEE
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Segment), Priority_Context, new object[] { dd, de_d.data02 });

                            // OEE Timeline / Histogram
                            //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Timeline_OEEData), Priority_Context, new object[] { dd, de_d.data02 });
                        }

                        //// Production Status (Generated Event) Table Data
                        //if (de_d.id.ToLower() == "statusdata_productionstatus")
                        //{
                        //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_ProductionStatusData), Priority_Context, new object[] { dd, de_d.data02 });
                        //}

                    }
                }
            }
        }

        #region "Data"

        #region "Status"

        void UpdateStatus_Production(DeviceDisplay dd, object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                if (dd.ComparisonGroup != null)
                {
                    string value = DataTable_Functions.GetTableValue(dt, "name", "Production", "value");

                    bool val = true;
                    if (value != null) bool.TryParse(value, out val);

                    Header header = dd.ComparisonGroup.header;
                    if (header != null) header.Production = val;

                    Row row = dd.ComparisonGroup.row;
                    if (row != null) row.Production = val;
                }
            }
        }

        void UpdateStatus_Idle(DeviceDisplay dd, object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                if (dd.ComparisonGroup != null)
                {
                    string value = DataTable_Functions.GetTableValue(dt, "name", "Idle", "value");

                    bool val = true;
                    if (value != null) bool.TryParse(value, out val);

                    Header header = dd.ComparisonGroup.header;
                    if (header != null) header.Idle = val;

                    Row row = dd.ComparisonGroup.row;
                    if (row != null) row.Idle = val;
                }
            }
        }

        void UpdateStatus_Alert(DeviceDisplay dd, object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                if (dd.ComparisonGroup != null)
                {
                    string value = DataTable_Functions.GetTableValue(dt, "name", "Alert", "value");

                    bool val = true;
                    if (value != null) bool.TryParse(value, out val);

                    Header header = dd.ComparisonGroup.header;
                    if (header != null) header.Alert = val;

                    Row row = dd.ComparisonGroup.row;
                    if (row != null) row.Alert = val;
                }
            }
        }

        #endregion

        void UpdateProductionStatus(DeviceDisplay dd, object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "name", "Production Status", "value");
                string time = DataTable_Functions.GetTableValue(dt, "name", "Production Status", "timestamp");
                string currenttime = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Time UTC", "timestamp");

                int cellIndex = dd.ComparisonGroup.row.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatus");
                if (cellIndex >= 0)
                {
                    Controls.TextDisplay txt;

                    object ddData = dd.ComparisonGroup.row.Cells[cellIndex].Data;

                    if (ddData == null)
                    {
                        txt = new Controls.TextDisplay();
                        txt.Width = 300;
                        dd.ComparisonGroup.row.Cells[cellIndex].Data = txt;
                        dd.ComparisonGroup.row.Cells[cellIndex].Width = 300;
                    }
                    else txt = (Controls.TextDisplay)ddData;

                    // Calculate how long the device has been in this status
                    if (time != null && currenttime != null)
                    {
                        TimeSpan duration = GetStatusTimeSpan(time, currenttime);

                        txt.Text = value + " for " + duration.ToString();
                    }
                    else txt.Text = value;
                }
            }
        }

        TimeSpan GetStatusTimeSpan(string begin_str, string end_str)
        {
            var begin = DateTime.MinValue;
            DateTime.TryParse(begin_str, out begin);

            var end = DateTime.MinValue;
            DateTime.TryParse(end_str, out end);

            if (begin > DateTime.MinValue && end > DateTime.MinValue)
            {
                return end - begin;
            }

            return TimeSpan.Zero;
        }

        #region "Shift Break"

        void UpdateBreak(DeviceDisplay dd, object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                Header header = dd.ComparisonGroup.header;
                if (header != null)
                {
                    string value = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Type", "value");
                    if (value != null)
                    {
                        if (value.ToLower() == "break") header.Break = true;
                        else header.Break = false;
                    }
                    else header.Break = false;
                }
            }
        }

        #endregion

        #region "Shift Info"

        //void UpdateShiftInfo_Snapshots(DeviceDisplay dd, object snapshotData)
        //{
        //    DataTable dt = snapshotData as DataTable;
        //    if (dt != null)
        //    {
        //        int cellIndex = -1;
        //        cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "shiftinfo");
        //        if (cellIndex >= 0)
        //        {
        //            object data = dd.ComparisonGroup.column.Cells[cellIndex].Data;
        //            Controls.ShiftDisplay sd;
        //            if (data != null)
        //            {
        //                sd = (Controls.ShiftDisplay)data;
        //            }
        //            else
        //            {
        //                sd = new Controls.ShiftDisplay();
        //                dd.ComparisonGroup.column.Cells[cellIndex].Data = sd;
        //            }

        //            // Update Shift Name
        //            string prevShiftName = sd.Shift_Name;
        //            bool shiftChanged = false;
        //            sd.Shift_Name = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Name", "value");
        //            if (prevShiftName != sd.Shift_Name) shiftChanged = true;

        //            // Update Shift Data
        //            string date = "";
        //            string shiftdate = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Date", "value");
        //            if (shiftdate != null)
        //            {
        //                DateTime timestamp = DateTime.MinValue;
        //                DateTime.TryParse(shiftdate, out timestamp);
        //                if (timestamp > DateTime.MinValue)
        //                {
        //                    sd.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(timestamp.Month);
        //                    sd.Day = timestamp.Day.ToString();
        //                }

        //                date = shiftdate + " ";
        //            }

        //            // Get Shift Progress Bar Maximum -------------------------------------------------
        //            string shiftBegin = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Begin", "value");
        //            DateTime begin = DateTime.MinValue;
        //            DateTime.TryParse(shiftBegin, out begin);

        //            string shiftEnd = DataTable_Functions.GetTableValue(dt, "name", "Current Shift End", "value");
        //            DateTime end = DateTime.MinValue;
        //            DateTime.TryParse(shiftEnd, out end);

        //            if (end < begin) end = end.AddDays(1);

        //            sd.Shift_Times = "(" + begin.ToShortTimeString() + " - " + end.ToShortTimeString() + ")";

        //            Int64 duration = Convert.ToInt64((end - begin).TotalSeconds);
        //            if (duration <= int.MaxValue && duration >= int.MinValue)
        //            {
        //                sd.Bar_Maximum = Convert.ToInt32(duration);
        //            }
        //            // --------------------------------------------------------------------------------


        //            // Get Shift Progress Bar Value ---------------------------------------------------
        //            string shiftProgress = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Time", "value");
        //            DateTime progress = DateTime.MinValue;
        //            DateTime.TryParse(shiftProgress, out progress);

        //            if (progress > DateTime.MinValue)
        //            {
        //                duration = Convert.ToInt64((end - progress).TotalSeconds);
        //                if (duration <= int.MaxValue && duration >= int.MinValue)
        //                {
        //                    sd.Bar_Value = Convert.ToInt32(duration);
        //                }
        //            }
        //            // --------------------------------------------------------------------------------


        //            string CurrentShiftId = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Id", "value");

        //            foreach (Controls.ShiftSegmentIndicator indicator in sd.SegmentIndicators)
        //            {
        //                SegmentInfo segment = indicator.Segment;

        //                if (CurrentShiftId == segment.id)
        //                {
        //                    indicator.CurrentShift = true;

        //                    string sCurrentTime = shiftProgress;

        //                    DateTime currentTime = DateTime.MinValue;
        //                    if (DateTime.TryParse(sCurrentTime, out currentTime))
        //                    {
        //                        double segmentProgress = (currentTime - segment.segmentStart).TotalSeconds;
        //                        indicator.BarValue = Math.Max(0, Convert.ToInt32(segmentProgress));
        //                    }
        //                }
        //                else
        //                {
        //                    double segmentDuration = (segment.segmentEnd - segment.segmentStart).TotalSeconds;

        //                    indicator.BarValue = Math.Max(0, Convert.ToInt32(segmentDuration));
        //                    indicator.CurrentShift = false;
        //                }
        //            }
        //        }
        //    }
        //}

        //void UpdateShiftInfo_ShiftData(DeviceDisplay dd, object shiftData)
        //{
        //    int cellIndex = -1;
        //    cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "shiftinfo");
        //    if (cellIndex >= 0)
        //    {
        //        object data = dd.ComparisonGroup.column.Cells[cellIndex].Data;
        //        Controls.ShiftDisplay sd;
        //        if (data != null)
        //        {
        //            sd = (Controls.ShiftDisplay)data;
        //        }
        //        else
        //        {
        //            sd = new Controls.ShiftDisplay();
        //            dd.ComparisonGroup.column.Cells[cellIndex].Data = sd;
        //        }

        //        // Get Shift Segment Indicators ---------------------------------------------------

        //        List<SegmentInfo> shiftSegments = GetShiftSegments(shiftData);

        //        double maxDuration = -1;

        //        foreach (SegmentInfo segment in shiftSegments)
        //        {
        //            double d = (segment.segmentEnd - segment.segmentStart).TotalSeconds;
        //            if (d > maxDuration) maxDuration = d;
        //        }

        //        foreach (SegmentInfo segment in shiftSegments)
        //        {
        //            this.Dispatcher.BeginInvoke(new Action<SegmentInfo, Controls.ShiftDisplay, double>(UpdateShiftSegment), Priority_Context, new object[] { segment, sd, maxDuration });
        //        }
        //    }
        //}

        //void UpdateShiftSegment(SegmentInfo segment, Controls.ShiftDisplay sd, double maxDuration)
        //{
        //    Controls.ShiftSegmentIndicator indicator;

        //    int indicatorIndex = sd.SegmentIndicators.ToList().FindIndex(x => x.id == segment.segmentId);
        //    if (indicatorIndex >= 0) indicator = sd.SegmentIndicators[indicatorIndex];
        //    else
        //    {
        //        indicator = new Controls.ShiftSegmentIndicator();
        //        sd.SegmentIndicators.Add(indicator);
        //    }

        //    indicator.Segment = segment;

        //    indicator.id = segment.segmentId;

        //    DateTime segmentEnd = segment.segmentEnd;
        //    if (segment.segmentEnd < segment.segmentStart) segmentEnd = segmentEnd.AddDays(1);

        //    double segmentDuration = (segmentEnd - segment.segmentStart).TotalSeconds;
        //    indicator.BarMaximum = Math.Max(0, Convert.ToInt32(segmentDuration));

        //    indicator.ProgressWidth = (segmentDuration * 55) / maxDuration;

        //    indicator.SegmentTimes = segment.segmentStart.ToShortTimeString() + " - " + segmentEnd.ToShortTimeString();

        //    indicator.SegmentDuration = (segmentEnd - segment.segmentStart).ToString();

        //    indicator.SegmentId = (segment.segmentId + 1).ToString("00");

        //    if (segment.segmentType.ToLower() == "break") indicator.BreakType = true;

        //    indicator.SegmentType = segment.segmentType;
        //}

        //List<SegmentInfo> GetShiftSegments(object shiftData)
        //{
        //    List<SegmentInfo> Result = new List<SegmentInfo>();

        //    if (shiftData != null)
        //    {
        //        DataTable dt = shiftData as DataTable;
        //        if (dt != null)
        //        {
        //            foreach (DataRow row in dt.Rows)
        //            {
        //                string id = null;
        //                int segmentId = -1;
        //                string type = null;

        //                object start_str = row["Start"];
        //                DateTime start = DateTime.MinValue;

        //                object end_str = row["End"];
        //                DateTime end = DateTime.MinValue;

        //                if (row["id"] != null) id = row["id"].ToString();
        //                if (row["segmentid"] != null) int.TryParse(row["segmentid"].ToString(), out segmentId);
        //                if (row["type"] != null) type = row["type"].ToString();

        //                if (start_str != null) DateTime.TryParse(start_str.ToString(), out start);
        //                if (end_str != null) DateTime.TryParse(end_str.ToString(), out end);


        //                if (segmentId >= 0 && start > DateTime.MinValue && end > DateTime.MinValue)
        //                {
        //                    SegmentInfo info = new SegmentInfo();
        //                    info.id = id;
        //                    info.segmentId = segmentId;
        //                    info.segmentType = type;
        //                    info.segmentStart = start;
        //                    info.segmentEnd = end;
        //                    Result.Add(info);
        //                }
        //            }
        //        }
        //    }

        //    return Result;

        //}

        #endregion

        #region "Production Status Times"

        //// Get list of Production Status variables and create TimeDisplay controls for each 
        //void UpdateProductionStatusTimes_GenEventValues(DeviceDisplay dd, object geneventvalues)
        //{
        //    StackPanel stack;

        //    int cellIndex = -1;
        //    cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimes");

        //    if (cellIndex >= 0)
        //    {
        //        object data = dd.ComparisonGroup.column.Cells[cellIndex].Data;
        //        if (data == null)
        //        {
        //            stack = new StackPanel();
        //            stack.Background = new SolidColorBrush(Colors.Transparent);
        //            dd.ComparisonGroup.column.Cells[cellIndex].Data = stack;

        //            DataTable dt = geneventvalues as DataTable;
        //            if (dt != null)
        //            {
        //                DataView dv = dt.AsDataView();
        //                dv.RowFilter = "EVENT = 'production_status'";
        //                DataTable temp_dt = dv.ToTable(false, "VALUE");

        //                List<Color> colors = TH_Styles.IndicatorColors.GetIndicatorColors(temp_dt.Rows.Count);
        //                int colorIndex = 0;

        //                foreach (DataRow row in temp_dt.Rows)
        //                {
        //                    Controls.TimeDisplay td = new Controls.TimeDisplay();

        //                    // Set Text
        //                    td.Text = row[0].ToString();

        //                    // Set Bar Color
        //                    Color color = colors[colorIndex];
        //                    td.BarBrush = new SolidColorBrush(Color.FromArgb(221, color.R, color.G, color.B));
        //                    colorIndex += 1;

        //                    // Initialize values
        //                    td.Percentage = "0%";
        //                    td.BarValue = 0;
        //                    td.BarMaximum = 1;

        //                    stack.Children.Add(td);
        //                }
        //            }
        //        }
        //    }
        //}

        //class ProductionStatus_Variable
        //{
        //    public string name { get; set; }
        //    public int seconds { get; set; }
        //}

        //// Get the Times for each Production Status variable from the 'Shifts' table
        //void UpdateProductionStatusTimes_ShiftData(DeviceDisplay dd, object shiftData)
        //{
        //    StackPanel stack;

        //    int cellIndex = -1;
        //    cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimes");

        //    if (cellIndex >= 0)
        //    {
        //        object data = dd.ComparisonGroup.column.Cells[cellIndex].Data;
        //        if (data != null)
        //        {
        //            if (data.GetType() == typeof(StackPanel))
        //            {
        //                stack = (StackPanel)data;

        //                DataTable dt = shiftData as DataTable;
        //                if (dt != null)
        //                {
        //                    int totalSeconds = 0;
        //                    List<ProductionStatus_Variable> variables = new List<ProductionStatus_Variable>();

        //                    // Get List of variables from 'Shifts' table and collect the total number of seconds
        //                    foreach (DataColumn column in dt.Columns)
        //                    {
        //                        if (column.ColumnName.Contains("PRODUCTION_STATUS") || column.ColumnName.Contains("Production_Status") || column.ColumnName.Contains("production_status"))
        //                        {
        //                            ProductionStatus_Variable variable = new ProductionStatus_Variable();

        //                            // Get Variable name from Column Name
        //                            if (column.ColumnName.Contains("__"))
        //                            {
        //                                int i = column.ColumnName.IndexOf("__") + 2;
        //                                if (i < column.ColumnName.Length)
        //                                {
        //                                    string name = column.ColumnName.Substring(i);
        //                                    name = name.Replace('_', ' ');

        //                                    variable.name = name;
        //                                }
        //                            }

        //                            // Get Total Seconds for Variable
        //                            DataView dv = dt.AsDataView();
        //                            DataTable temp_dt = dv.ToTable(false, column.ColumnName);

        //                            foreach (DataRow row in temp_dt.Rows)
        //                            {
        //                                string value = row[0].ToString();

        //                                int seconds = 0;
        //                                int.TryParse(value, out seconds);
        //                                variable.seconds += seconds;
        //                                totalSeconds += seconds;
        //                            }

        //                            variables.Add(variable);
        //                        }
        //                    }

        //                    // Loop through variables and update TimeDisplay controls
        //                    foreach (ProductionStatus_Variable var in variables)
        //                    {
        //                        foreach (Controls.TimeDisplay td in stack.Children.OfType<Controls.TimeDisplay>())
        //                        {
        //                            if (td.Text != null)
        //                            {
        //                                if (td.Text.ToLower() == var.name.ToLower())
        //                                {
        //                                    this.Dispatcher.BeginInvoke(new Action<Controls.TimeDisplay, int, int>(UpdateProductionStatusTimes_ShiftData_GUI), Priority_Context, new object[] { td, var.seconds, totalSeconds });
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //// Assign the TimeDisplay control changes
        //void UpdateProductionStatusTimes_ShiftData_GUI(Controls.TimeDisplay td, int barvalue, int barmaximum)
        //{
        //    td.Time = TimeSpan.FromSeconds(barvalue).ToString();

        //    double percentage = (double)barvalue / barmaximum;
        //    td.Percentage = percentage.ToString("P1");

        //    td.BarValue = barvalue;
        //    td.BarMaximum = barmaximum;
        //}

        // Highlight the Current Production Status
        //void UpdateProductionStatusTimes_SnapshotData(DeviceDisplay dd, object snapshotData)
        //{
        //    StackPanel stack;

        //    DataTable dt = snapshotData as DataTable;
        //    if (dt != null)
        //    {
        //        int cellIndex = -1;
        //        cellIndex = dd.ComparisonGroup.row.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimes");

        //        if (cellIndex >= 0)
        //        {
        //            object data = dd.ComparisonGroup.row.Cells[cellIndex].Data;
        //            if (data != null)
        //            {
        //                if (data.GetType() == typeof(StackPanel)) stack = (StackPanel)data;
        //                else { stack = new StackPanel(); dd.ComparisonGroup.row.Cells[cellIndex].Data = stack; }

        //                string currentStatus = DataTable_Functions.GetTableValue(dt, "name", "Production Status", "value");

        //                foreach (Controls.TimeDisplay td in stack.Children.OfType<Controls.TimeDisplay>())
        //                {
        //                    if (td.Text == currentStatus) td.IsSelected = true;
        //                    else td.IsSelected = false;
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion

        #region "OEE Values"

        void Update_OEE_Avg(DeviceDisplay dd, object oeedata)
        {

            DataTable dt = oeedata as DataTable;
            if (dt != null)
            {
                List<double> oees = new List<double>();

                foreach (DataRow row in dt.Rows)
                {
                    string oee_str = null;

                    if (row.Table.Columns.Contains("oee")) if (row["oee"] != null) oee_str = row["oee"].ToString();

                    if (oee_str != null)
                    {
                        double oee = -1;
                        double.TryParse(oee_str, out oee);
                        if (oee >= 0) oees.Add(oee);
                    }
                }

                if (oees.Count > 0)
                {
                    double average = oees.Average();

                    int cellIndex = dd.ComparisonGroup.row.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeeaverage");
                    if (cellIndex >= 0)
                    {
                        Controls.NumberDisplay oeeAverage;

                        object ddData = dd.ComparisonGroup.row.Cells[cellIndex].Data;

                        if (ddData == null)
                        {
                            oeeAverage = new Controls.NumberDisplay();
                            dd.ComparisonGroup.row.Cells[cellIndex].Data = oeeAverage;
                        }
                        else oeeAverage = (Controls.NumberDisplay)ddData;

                        oeeAverage.Value_Format = "P1";
                        oeeAverage.Value = average;
                    }
                }
            }
        }

        void Update_OEE_Segment(DeviceDisplay dd, object oeedata)
        {
            DataTable dt = oeedata as DataTable;
            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                {
                    int index = dt.Rows.Count - 1;

                    if (dt.Columns.Contains("oee")) if (dt.Rows[index]["oee"] != null)
                        {
                            string oee_str = dt.Rows[index]["oee"].ToString();

                            double oee = -1;
                            double.TryParse(oee_str, out oee);
                            if (oee >= 0)
                            {
                                int cellIndex = dd.ComparisonGroup.row.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeesegment");
                                if (cellIndex >= 0)
                                {
                                    Controls.NumberDisplay oeeSegment;

                                    object ddData = dd.ComparisonGroup.row.Cells[cellIndex].Data;

                                    if (ddData == null)
                                    {
                                        oeeSegment = new Controls.NumberDisplay();
                                        dd.ComparisonGroup.row.Cells[cellIndex].Data = oeeSegment;
                                    }
                                    else oeeSegment = (Controls.NumberDisplay)ddData;

                                    oeeSegment.Value_Format = "P1";
                                    oeeSegment.Value = oee;
                                }
                            }
                        }
                }
            }
        }

        #endregion

        #region "OEE Timeline/Histogram"

        //class OEE_TimelineInfo
        //{
        //    public string id { get; set; }
        //    public double value { get; set; }
        //    public string segmentTimes { get; set; }
        //    public string valueText { get; set; }
        //}

        //void Update_OEE_Timeline_OEEData(DeviceDisplay dd, object oeedata)
        //{
        //    List<OEE_TimelineInfo> infos = new List<OEE_TimelineInfo>();

        //    DataTable dt = oeedata as DataTable;
        //    if (dt != null)
        //    {
        //        List<double> oees = new List<double>();

        //        foreach (DataRow row in dt.Rows)
        //        {
        //            string oee_str = DataTable_Functions.GetRowValue("oee", row);

        //            if (oee_str != null)
        //            {
        //                // Get OEE Value
        //                double oee = -1;
        //                double.TryParse(oee_str, out oee);
        //                if (oee >= 0)
        //                {
        //                    // Get Segment Time Info
        //                    string oee_shiftId = DataTable_Functions.GetRowValue("shift_id", row);

        //                    if (oee_shiftId != null)
        //                    {
        //                        OEE_TimelineInfo info = new OEE_TimelineInfo();
        //                        info.id = oee_shiftId;

        //                        info.value = oee;
        //                        info.valueText = oee.ToString("P2");

        //                        infos.Add(info);
        //                    }
        //                }
        //            }
        //        }


        //        int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeetimeline");
        //        if (cellIndex >= 0)
        //        {
        //            Controls.HistogramDisplay oeeTimeline;

        //            object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

        //            if (ddData == null)
        //            {
        //                oeeTimeline = new Controls.HistogramDisplay();
        //                dd.ComparisonGroup.column.Cells[cellIndex].Data = oeeTimeline;
        //            }
        //            else oeeTimeline = (Controls.HistogramDisplay)ddData;


        //            foreach (OEE_TimelineInfo info in infos)
        //            {
        //                Controls.DataBar db;

        //                int dbIndex = oeeTimeline.DataBars.ToList().FindIndex(x => x.Id == info.id);
        //                if (dbIndex < 0)
        //                {
        //                    db = new Controls.DataBar();
        //                    db.Id = info.id;
        //                    db.SegmentTimes = info.segmentTimes;
        //                    oeeTimeline.DataBars.Add(db);
        //                }
        //                else db = oeeTimeline.DataBars[dbIndex];

        //                db.Value = info.value * 100;
        //                db.ValueText = info.valueText;
        //            }
        //        }

        //    }
        //}

        //void Update_OEE_Timeline_ShiftData(DeviceDisplay dd, object shiftData)
        //{
        //    int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeetimeline");
        //    if (cellIndex >= 0)
        //    {
        //        Controls.HistogramDisplay oeeTimeline;

        //        object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

        //        if (ddData != null)
        //        {
        //            oeeTimeline = (Controls.HistogramDisplay)ddData;

        //            // Get Segment Times (for ToolTip)
        //            foreach (Controls.DataBar db in oeeTimeline.DataBars)
        //            {
        //                string segmentTimes = GetSegmentName(db.Id, shiftData);
        //                if (segmentTimes != null)
        //                {
        //                    if (db.SegmentTimes != segmentTimes) db.SegmentTimes = segmentTimes;
        //                }
        //            }
        //        }
        //    }
        //}

        //void Update_OEE_Timeline_SnapshotData(DeviceDisplay dd, object snapshotData)
        //{
        //    List<OEE_TimelineInfo> infos = new List<OEE_TimelineInfo>();

        //    DataTable dt = snapshotData as DataTable;
        //    if (dt != null)
        //    {
        //        int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeetimeline");
        //        if (cellIndex >= 0)
        //        {
        //            Controls.HistogramDisplay oeeTimeline;

        //            object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

        //            if (ddData != null)
        //            {
        //                oeeTimeline = (Controls.HistogramDisplay)ddData;

        //                // Get Shift Name to check if still in the same shift as last update
        //                string prev_shiftName = oeeTimeline.shiftName;
        //                oeeTimeline.shiftName = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Name", "value");
        //                if (prev_shiftName != oeeTimeline.shiftName) oeeTimeline.DataBars.Clear();

        //                // Get Current Segment
        //                foreach (Controls.DataBar db in oeeTimeline.DataBars)
        //                {
        //                    string currentShiftId = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Id", "value");
        //                    if (currentShiftId == db.Id) db.CurrentSegment = true;
        //                    else db.CurrentSegment = false;
        //                }
        //            }
        //        }
        //    }
        //}

        //string GetSegmentName(string shiftId, object shiftData)
        //{
        //    string Result = null;

        //    DataTable dt = shiftData as DataTable;
        //    if (dt != null)
        //    {
        //        DataView dv = dt.AsDataView();
        //        dv.RowFilter = "id='" + shiftId + "'";
        //        DataTable temp_dt = dv.ToTable();

        //        // Should be max of one row
        //        if (temp_dt.Rows.Count > 0)
        //        {
        //            DataRow row = temp_dt.Rows[0];

        //            //Get Segment start Time
        //            string start = DataTable_Functions.GetRowValue("start", row);

        //            // Get Segment end Time
        //            string end = DataTable_Functions.GetRowValue("end", row);

        //            // Create Segment Times string
        //            if (start != null && end != null)
        //            {
        //                DateTime timestamp = DateTime.MinValue;
        //                DateTime.TryParse(start, out timestamp);
        //                if (timestamp > DateTime.MinValue) start = timestamp.ToShortTimeString();

        //                timestamp = DateTime.MinValue;
        //                DateTime.TryParse(end, out timestamp);
        //                if (timestamp > DateTime.MinValue) end = timestamp.ToShortTimeString();

        //                Result = start + " - " + end;
        //            }
        //        }
        //    }

        //    return Result;
        //}

        #endregion

        #region "Programs"

        void UpdateProgram_Current(DeviceDisplay dd, object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "name", "Program", "value");

                int cellIndex = dd.ComparisonGroup.row.Cells.ToList().FindIndex(x => x.Link.ToLower() == "currentprogram");
                if (cellIndex >= 0)
                {
                    Controls.TextDisplay txt;

                    object ddData = dd.ComparisonGroup.row.Cells[cellIndex].Data;

                    if (ddData == null)
                    {
                        txt = new Controls.TextDisplay();
                        txt.Width = 300;
                        dd.ComparisonGroup.row.Cells[cellIndex].Data = txt;
                        dd.ComparisonGroup.row.Cells[cellIndex].Width = 300;
                    }
                    else txt = (Controls.TextDisplay)ddData;

                    txt.Text = value;
                }
            }
        }

        void UpdateProgram_Previous(DeviceDisplay dd, object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "name", "Program", "previous_value");

                int cellIndex = dd.ComparisonGroup.row.Cells.ToList().FindIndex(x => x.Link.ToLower() == "previousprogram");
                if (cellIndex >= 0)
                {
                    Controls.TextDisplay txt;

                    object ddData = dd.ComparisonGroup.row.Cells[cellIndex].Data;

                    if (ddData == null)
                    {
                        txt = new Controls.TextDisplay();
                        dd.ComparisonGroup.row.Cells[cellIndex].Data = txt;
                    }
                    else txt = (Controls.TextDisplay)ddData;

                    txt.Text = value;
                }
            }
        }

        #endregion

        #region "Production Status Timeline"

        //// Get list of Production Status variables
        //void UpdateProductionStatusTimeline_GenEventValues(DeviceDisplay dd, object geneventvalues)
        //{
        //    int cellIndex = -1;
        //    cellIndex = dd.ComparisonGroup.row.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimeline");

        //    if (cellIndex >= 0)
        //    {
        //        object data = dd.ComparisonGroup.row.Cells[cellIndex].Data;
        //        if (data == null)
        //        {
        //            TimeLine timeline = new TimeLine();
        //            dd.ComparisonGroup.column.Cells[cellIndex].Data = timeline;

        //            DataTable dt = geneventvalues as DataTable;
        //            if (dt != null)
        //            {
        //                DataView dv = dt.AsDataView();
        //                dv.RowFilter = "EVENT = 'production_status'";
        //                DataTable temp_dt = dv.ToTable(false, "NUMVAL");

        //                List<Color> colors = TH_Styles.IndicatorColors.GetIndicatorColors(temp_dt.Rows.Count);

        //                timeline.SeriesCount = temp_dt.Rows.Count;

        //                List<Tuple<Color, string>> colorList = new List<Tuple<Color, string>>();

        //                int colorIndex = 0;

        //                foreach (DataRow row in temp_dt.Rows)
        //                {
        //                    string val = row[0].ToString();

        //                    Tuple<Color, string> colorItem = new Tuple<Color, string>(colors[colorIndex], val);

        //                    colorList.Add(colorItem);

        //                    colorIndex += 1;
        //                }

        //                timeline.Colors = colorList;
        //            }
        //        }
        //    }
        //}

        //void UpdateProductionStatusTimeline_ProductionStatusData(DeviceDisplay dd, object productionStatusData)
        //{
        //    DataTable dt = productionStatusData as DataTable;
        //    if (dt != null)
        //    {
        //        int cellIndex = dd.ComparisonGroup.row.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimeline");
        //        if (cellIndex >= 0)
        //        {
        //            // Get / Create TimeLineDisplay
        //            TH_WPF.TimeLine.TimeLine timeline;

        //            object ddData = dd.ComparisonGroup.row.Cells[cellIndex].Data;
        //            if (ddData != null)
        //            {
        //                timeline = (TH_WPF.TimeLine.TimeLine)ddData;


        //                List<Tuple<DateTime, string, string>> data_forShift = new List<Tuple<DateTime, string, string>>();

        //                foreach (DataRow row in dt.Rows)
        //                {
        //                    string val = DataTable_Functions.GetRowValue("Timestamp", row);
        //                    if (val != null)
        //                    {
        //                        DateTime timestamp = DateTime.MinValue;
        //                        DateTime.TryParse(val, out timestamp);

        //                        if (timestamp > DateTime.MinValue)
        //                        {
        //                            if (timestamp > timeline.previousTimestamp)
        //                            {
        //                                string value = DataTable_Functions.GetRowValue("Value", row);

        //                                val = DataTable_Functions.GetRowValue("Numval", row);
        //                                if (val != null)
        //                                {
        //                                    int numval;
        //                                    if (int.TryParse(val, out numval))
        //                                    {
        //                                        if (!timeline.Series.Contains(numval))
        //                                        {
        //                                            timeline.Series.Add(numval);
        //                                            timeline.SeriesCount = timeline.Series.Count;
        //                                        }
        //                                    }

        //                                    data_forShift.Add(new Tuple<DateTime, string, string>(timestamp, val, value));
        //                                }

        //                                timeline.previousTimestamp = timestamp;
        //                            }
        //                        }
        //                    }
        //                }

        //                timeline.AddData(data_forShift);

        //            }

        //            //object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;
        //            //if (ddData == null)
        //            //{
        //            //    timeline = new TH_WPF.TimeLine.TimeLine();
        //            //    dd.ComparisonGroup.column.Cells[cellIndex].Data = timeline;
        //            //}
        //            //else timeline = (TH_WPF.TimeLine.TimeLine)ddData;



        //            //List<Tuple<DateTime, string, string>> data_forShift = new List<Tuple<DateTime, string, string>>();

        //            //foreach (DataRow row in dt.Rows)
        //            //{
        //            //    string val = DataTable_Functions.GetRowValue("Timestamp", row);
        //            //    if (val != null)
        //            //    {
        //            //        DateTime timestamp = DateTime.MinValue;
        //            //        DateTime.TryParse(val, out timestamp);

        //            //        if (timestamp > DateTime.MinValue)
        //            //        {
        //            //            if (timestamp > timeline.previousTimestamp)
        //            //            {
        //            //                string value = DataTable_Functions.GetRowValue("Value", row);

        //            //                val = DataTable_Functions.GetRowValue("Numval", row);
        //            //                if (val != null)
        //            //                {
        //            //                    int numval;
        //            //                    if (int.TryParse(val, out numval))
        //            //                    {
        //            //                        if (!timeline.Series.Contains(numval))
        //            //                        {
        //            //                            timeline.Series.Add(numval);
        //            //                            timeline.SeriesCount = timeline.Series.Count;
        //            //                        }
        //            //                    }

        //            //                    data_forShift.Add(new Tuple<DateTime, string, string>(timestamp, val, value));
        //            //                }

        //            //                timeline.previousTimestamp = timestamp;
        //            //            }
        //            //        }
        //            //    }
        //            //}

        //            //timeline.AddData(data_forShift);

        //        }
        //    }
        //}

        //// Set Shift time from 'Snapshots' table
        //void UpdateProductionStatusTimeline_SnapshotData(DeviceDisplay dd, object snapshotData)
        //{
        //    DataTable dt = snapshotData as DataTable;
        //    if (dt != null)
        //    {
        //        int cellIndex = dd.ComparisonGroup.row.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimeline");
        //        if (cellIndex >= 0)
        //        {
        //            // Local
        //            string begin = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Begin", "value");
        //            DateTime shift_begin = DateTime.MinValue;
        //            DateTime.TryParse(begin, out shift_begin);

        //            string end = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Time", "value");
        //            DateTime shift_end = DateTime.MinValue;
        //            DateTime.TryParse(end, out shift_end);

        //            //string end = DataTable_Functions.GetTableValue(dt, "name", "Current Shift End", "value");
        //            //DateTime shift_end = DateTime.MinValue;
        //            //DateTime.TryParse(end, out shift_end);

        //            // Get / Create TimeLineDisplay
        //            TH_WPF.TimeLine.TimeLine timeline;

        //            object ddData = dd.ComparisonGroup.row.Cells[cellIndex].Data;
        //            if (ddData != null)
        //            {
        //                timeline = (TH_WPF.TimeLine.TimeLine)ddData;

        //                timeline.StartTime = shift_begin;
        //                timeline.EndTime = shift_end;

        //                //// Get Shift Name to check if still in the same shift as last update
        //                //string prev_shiftName = timeline.Title;
        //                //timeline.Title = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Name", "value");
        //                //if (prev_shiftName != timeline.Title)
        //                //{
        //                //    timeline.StartTime = shift_begin;
        //                //    timeline.EndTime = shift_end;
        //                //}
        //            }

        //            //if (ddData == null)
        //            //{
        //            //    timeline = new TH_WPF.TimeLine.TimeLine();
        //            //    dd.ComparisonGroup.column.Cells[cellIndex].Data = timeline;
        //            //}
        //            //else timeline = (TH_WPF.TimeLine.TimeLine)ddData;




        //            //if (timeline.prev_SeriesCount != timeline.Series.Count)
        //            //{
        //            //    List<Color> rawColors = TH_Styles.IndicatorColors.GetIndicatorColors(timeline.Series.Count);

        //            //    List<Tuple<Color, string>> colors = new List<Tuple<Color, string>>();
        //            //    var levels = timeline.Series.OrderByDescending(i => i).ToList();
        //            //    //var levels = timeline.Series.OrderBy(i => i);
        //            //    for (int x = 0; x <= levels.Count - 1; x++)
        //            //    {
        //            //        colors.Add(new Tuple<Color, string>(rawColors[x], levels[x].ToString()));
        //            //    }

        //            //    timeline.Colors = colors;
        //            //}

        //            //timeline.prev_SeriesCount = timeline.Series.Count;


        //            //// Get Shift Name to check if still in the same shift as last update
        //            //string prev_shiftName = timeline.Title;
        //            //timeline.Title = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Name", "value");
        //            //if (prev_shiftName != timeline.Title)
        //            //{
        //            //    timeline.StartTime = shift_begin;
        //            //    timeline.EndTime = shift_end;
        //            //}
        //        }
        //    }
        //}

        #endregion

        #endregion

        #region "Components"

        #region "Device Displays"

        List<DeviceDisplay> DeviceDisplays { get; set; }

        void CreateDeviceDisplay(Configuration config)
        {
            DeviceDisplay dd = new DeviceDisplay(config);

            int index = DeviceDisplays.Count;

            Header header = CreateRowHeader(config);
            Row row = CreateRow();

            header.Index = index;
            row.Index = index;

            header.row = row;
            row.header = header;

            dd.ComparisonGroup.header = header;
            dd.ComparisonGroup.row = row;

            RowHeaders.Add(header);
            Rows.Add(row);

            dd.ConnectedChanged += dd_ConnectedChanged;
            dd.ConnectionStatusChanged += dd_ConnectionStatusChanged;

            DeviceDisplays.Add(dd);
        }

        //void CreateDeviceDisplay(Configuration config)
        //{
        //    DeviceDisplay dd = new DeviceDisplay(config);

        //    int index = DeviceDisplays.Count;

        //    Row row = CreateRow();

        //    dd.row = row;

        //    //ColumnHeaders.Add(header);
        //    //Columns.Add(column);
        //    Rows.Add(row);

        //    dd.ConnectedChanged += dd_ConnectedChanged;
        //    dd.ConnectionStatusChanged += dd_ConnectionStatusChanged;

        //    DeviceDisplays.Add(dd);
        //}

        void dd_ConnectedChanged(DeviceDisplay dd)
        {
            if (dd.ComparisonGroup.header != null) dd.ComparisonGroup.header.Connected = dd.Connected;
            if (dd.ComparisonGroup.row != null) dd.ComparisonGroup.row.Connected = dd.Connected;
        }

        void dd_ConnectionStatusChanged(DeviceDisplay dd)
        {
            if (dd.ComparisonGroup.row != null) dd.ComparisonGroup.row.ConnectionStatus = dd.ConnectionStatus;
        }


        #endregion

        #region "Row Headers"

        ObservableCollection<Header> rowheaders;
        public ObservableCollection<Header> RowHeaders
        {
            get
            {
                if (rowheaders == null) rowheaders = new ObservableCollection<Header>();
                return rowheaders;
            }
            set
            {
                rowheaders = value;
            }
        }

        Header CreateRowHeader(Configuration config)
        {
            Header Result = new Header();

            Result.userDatabaseSettings = UserDatabaseSettings;

            Result.Device_Description = config.Description.Description;
            Result.Device_Manufacturer = config.Description.Manufacturer;
            Result.Device_Model = config.Description.Model;
            Result.Device_Serial = config.Description.Serial;
            Result.Device_ID = config.Description.Device_ID;

            // Load Device Logo
            if (config.FileLocations.Manufacturer_Logo_Path != null)
            {
                Result.LoadManufacturerLogo(config.FileLocations.Manufacturer_Logo_Path);
            }

            // Load Device Image
            if (config.FileLocations.Image_Path != null)
            {
                Result.LoadDeviceImage(config.FileLocations.Image_Path);
            }

            Result.Clicked += RowHeader_Clicked;


            if (config.Device_Image != null)
            {
                System.Drawing.Image img = Image_Functions.SetImageSize(config.Device_Image, 0, 160);
                Result.Device_Image = Image_Functions.SourceFromImage(img);
            }

            if (config.Manufacturer_Logo != null)
            {
                System.Drawing.Image img = Image_Functions.SetImageSize(config.Manufacturer_Logo, 0, 50);
                Result.Device_Logo = Image_Functions.SourceFromImage(img);
            }

            return Result;
        }

        void RowHeader_Clicked(int Index)
        {
            //DeviceSelected(Index);
        }

        //public bool ColumnHeaderMinimized
        //{
        //    get { return (bool)GetValue(ColumnHeaderMinimizedProperty); }
        //    set { SetValue(ColumnHeaderMinimizedProperty, value); }
        //}

        //public static readonly DependencyProperty ColumnHeaderMinimizedProperty =
        //    DependencyProperty.Register("ColumnHeaderMinimized", typeof(bool), typeof(DeviceCompare), new PropertyMetadata(false));

        #endregion

        #region "Rows"

        ObservableCollection<Row> rows;
        public ObservableCollection<Row> Rows
        {
            get
            {
                if (rows == null) rows = new ObservableCollection<Row>();
                return rows;
            }
            set
            {
                rows = value;
            }
        }

        Row CreateRow()
        {
            Row Result = new Row();

            Result.Clicked += Result_Clicked;

            int index = 0;

            foreach (DeviceDisplay.DataItem item in new DeviceDisplay().DataItems)
            {
                Cell cell = new Cell();
                cell.SizeChanged += cell_SizeChanged;
                cell.Link = item.id;
                cell.Data = item.data;
                cell.Index = index;
                cell.Clicked += cell_Clicked;
                index += 1;

                Result.Cells.Add(cell);
            }

            return Result;
        }

        void cell_Clicked(int Index)
        {
            //RowHeaders_UnselectAll();
            //Rows_UnSelectAll();
        }

        void cell_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender.GetType() == typeof(Cell))
            {
                Cell cell = (Cell)sender;
                SetCellHeights(cell);
            }
        }

        void SetCellHeights(Cell callingCell)
        {
            double height = callingCell.DesiredSize.Height;
            int index = callingCell.Index;

            foreach (Row row in Rows)
            {
                Cell cell = row.Cells[index];

                if (cell.DesiredSize.Height < callingCell.DesiredSize.Height)
                {
                    cell.MinHeight = height;
                }
            }

            //RowHeaders[index].MinHeight = height;
        }

        void Result_Clicked(int Index)
        {
            //DeviceSelected(Index);
        }

        #endregion

        #region "Column Headers"

        ObservableCollection<Column_Header> columnheaders;
        public ObservableCollection<Column_Header> ColumnHeaders
        {
            get
            {
                if (columnheaders == null) columnheaders = new ObservableCollection<Column_Header>();
                return columnheaders;
            }
            set
            {
                columnheaders = value;
            }
        }

        void CreateColumnHeaders()
        {
            int i = 0;

            foreach (DeviceDisplay.DataItem item in new DeviceDisplay().DataItems)
            {
                Column_Header ch = new Column_Header();
                ch.Text = item.header;
                ch.Width = Math.Max(200, item.columnWidth);
                ch.Index = i;
                ch.Clicked += ch_Clicked;
                ColumnHeaders.Add(ch);

                i += 1;
            }
        }

        void ch_Clicked(int index)
        {
            foreach (Column_Header header in ColumnHeaders)
            {
                if (header.Index != index) header.IsSelected = false;
                else header.IsSelected = true;
            }

            foreach (Row row in Rows)
            {
                for (int x = 0; x <= row.Cells.Count - 1; x++)
                {
                    if (x != index) row.Cells[x].IsSelected = false;
                }

                row.Cells[index].IsSelected = true;
            }
        }

        void RowHeaders_UnselectAll()
        {
            foreach (Column_Header header in ColumnHeaders)
            {
                header.IsSelected = false;
            }
        }

        void Rows_UnSelectAll()
        {
            foreach (Row row in Rows)
            {
                foreach (Cell cell in row.Cells) cell.IsSelected = false;
            }
        }

        void rh_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Column_Header))
            {
                Column_Header ch = (Column_Header)sender;

                int index = ColumnHeaders.IndexOf(ch);

                foreach (Row row in Rows)
                {
                    for (int x = 0; x <= row.Cells.Count - 1; x++)
                    {
                        if (x != index) row.Cells[x].IsSelected = false;
                    }

                    row.Cells[index].IsSelected = true;
                }
            }
        }

        #endregion

        #endregion


        //public DataView TableData
        //{
        //    get { return (DataView)GetValue(TableDataProperty); }
        //    set { SetValue(TableDataProperty, value); }
        //}

        //public static readonly DependencyProperty TableDataProperty =
        //    DependencyProperty.Register("TableData", typeof(DataView), typeof(DeviceTable), new PropertyMetadata(null));


        #endregion

    }
}
