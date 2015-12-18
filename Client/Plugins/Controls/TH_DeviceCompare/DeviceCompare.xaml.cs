// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

using System.Threading;
using System.Data;
using System.IO;
using System.Collections.ObjectModel;
using System.Globalization;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_PlugIns_Client_Control;
using TH_UserManagement.Management;
using TH_WPF.TimeLine;

using TH_DeviceCompare.Components;

namespace TH_DeviceCompare
{
    /// <summary>
    /// Interaction logic for DeviceCompare.xaml
    /// </summary>
    public partial class DeviceCompare : UserControl, Control_PlugIn
    {
        public DeviceCompare()
        {
            InitializeComponent();
            DataContext = this;

            CreateRowHeaders();

            DeviceDisplays = new List<DeviceDisplay>();
            ColumnHeaders.Clear();
            Columns.Clear();
        }

        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Device Compare"; } }

        public string Description { get { return "Compare Devices side-by-side"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceCompare;component/Resources/Compare_01.png")); } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceCompare;component/Resources/TrakHound_Logo_10_200px.png")); } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\License\" + "License.txt"); } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/devicecompare-appinfo.json"; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return "Dashboard"; } }
        public string DefaultParentCategory { get { return "Pages"; } }

        public bool AcceptsPlugIns { get { return true; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ShowInAppMenu { get { return true; } }

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        //public void Update(ReturnData rd)
        //{
        //    this.Dispatcher.BeginInvoke(new Action<ReturnData>(Update_GUI), Priority_Background, new object[] { rd });
        //}

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

        public event PlugInTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Device Properties"

        ObservableCollection<Configuration> Devices = new ObservableCollection<Configuration>();

        public void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("DeviceCompare :: Devices :: " + e.Action.ToString());

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                Devices.Clear();
                DeviceDisplays.Clear();
                ColumnHeaders.Clear();
                Columns.Clear();
            }

            if (e.NewItems != null)
            {
                foreach (Configuration newConfig in e.NewItems)
                {
                    if (newConfig != null)
                    {
                        Devices.Add(newConfig);

                        CreateDeviceDisplay(newConfig);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (Configuration oldConfig in e.OldItems)
                {
                    if (oldConfig != null) Devices.Add(oldConfig);
                }
            }
        }

        private int lSelectedDeviceIndex;
        public int SelectedDeviceIndex
        {
            get { return lSelectedDeviceIndex; }

            set
            {
                lSelectedDeviceIndex = value;

                // Unselect other headers and columns
                for (int x = 0; x <= DeviceDisplays.Count - 1; x++) if (x != lSelectedDeviceIndex)
                    {
                        DeviceDisplays[x].ComparisonGroup.header.IsSelected = false;
                        DeviceDisplays[x].ComparisonGroup.column.IsSelected = false;
                    }

                // Select header and column at SelectedDeviceIndex
                DeviceDisplays[lSelectedDeviceIndex].ComparisonGroup.header.IsSelected = true;
                DeviceDisplays[lSelectedDeviceIndex].ComparisonGroup.column.IsSelected = true;

            }
        }

        #endregion

        #region "Options"

        public OptionsPage Options { get; set; }

        #endregion

        #region "User"

        UserConfiguration currentuser = null;
        public UserConfiguration CurrentUser 
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                if (currentuser != null) LoggedIn = true;
                else LoggedIn = false;
            }
        }

        public Database_Settings UserDatabaseSettings { get; set; }

        #endregion

        public object RootParent { get; set; }

        #endregion

        #region "Device Compare"

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

                                dd.ComparisonGroup.column.Loading = false;
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



                            // Shift Info
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateShiftInfo_Snapshots), Priority_Context, new object[] { dd, de_d.data02 });

                            // OEE Timeline / Histogram
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Timeline_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });

                            // Production Status Times
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });

                            // Current Program
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProgram_Current), Priority_Context, new object[] { dd, de_d.data02 });

                            // Previous Program
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProgram_Previous), Priority_Context, new object[] { dd, de_d.data02 });


                            // Production Status Timeline
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });
                        }

                        // Shifts Table Data
                        if (de_d.id.ToLower() == "statusdata_shiftdata")
                        {
                            // Shift Info
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateShiftInfo_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });

                            // OEE Timeline / Histogram
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Timeline_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });

                            // Production Status Times
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });
                        }

                        // GenEvent Values
                        if (de_d.id.ToLower() == "statusdata_geneventvalues")
                        {
                            // Production Status Times
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_GenEventValues), Priority_Context, new object[] { dd, de_d.data02 });

                            // Production Status Timeline
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_GenEventValues), Priority_Context, new object[] { dd, de_d.data02 });
                        }

                        // OEE Table Data
                        if (de_d.id.ToLower() == "statusdata_oee")
                        {
                            // OEE Average
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Avg), Priority_Context, new object[] { dd, de_d.data02 });

                            // Current Segment OEE
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Segment), Priority_Context, new object[] { dd, de_d.data02 });

                            // OEE Timeline / Histogram
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Timeline_OEEData), Priority_Context, new object[] { dd, de_d.data02 });
                        }

                        // Production Status (Generated Event) Table Data
                        if (de_d.id.ToLower() == "statusdata_productionstatus")
                        {
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_ProductionStatusData), Priority_Context, new object[] { dd, de_d.data02 });
                        }

                    }
                }
            }
        }

        void DeviceSelected(int index)
        {
            DataEvent_Data de_d = new DataEvent_Data();
            de_d.id = "DeviceSelected";
            de_d.data01 = Devices[index];
            if (DataEvent != null) DataEvent(de_d);
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
                    Header header = dd.ComparisonGroup.header;
                    if (header != null)
                    {
                        string value = DataTable_Functions.GetTableValue(dt, "name", "Production", "value");

                        bool val = true;
                        if (value != null) bool.TryParse(value, out val);

                        header.Production = val;
                    }
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
                    Header header = dd.ComparisonGroup.header;
                    if (header != null)
                    {
                        string value = DataTable_Functions.GetTableValue(dt, "name", "Idle", "value");

                        bool val = true;
                        if (value != null) bool.TryParse(value, out val);

                        header.Idle = val;
                    }
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
                    Header header = dd.ComparisonGroup.header;
                    if (header != null)
                    {
                        string value = DataTable_Functions.GetTableValue(dt, "name", "Alert", "value");

                        bool val = true;
                        if (value != null) bool.TryParse(value, out val);

                        header.Alert = val;
                    }
                }
            }
        }

        #endregion

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

        void UpdateShiftInfo_Snapshots(DeviceDisplay dd, object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                int cellIndex = -1;
                cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "shiftinfo");
                if (cellIndex >= 0)
                {
                    object data = dd.ComparisonGroup.column.Cells[cellIndex].Data;
                    Controls.ShiftDisplay sd;
                    if (data != null)
                    {
                        sd = (Controls.ShiftDisplay)data;
                    }
                    else
                    {
                        sd = new Controls.ShiftDisplay();
                        dd.ComparisonGroup.column.Cells[cellIndex].Data = sd;
                    }

                    // Update Shift Name
                    string prevShiftName = sd.Shift_Name;
                    bool shiftChanged = false;
                    sd.Shift_Name = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Name", "value");
                    if (prevShiftName != sd.Shift_Name) shiftChanged = true;

                    // Update Shift Data
                    string date = "";
                    string shiftdate = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Date", "value");
                    if (shiftdate != null)
                    {
                        DateTime timestamp = DateTime.MinValue;
                        DateTime.TryParse(shiftdate, out timestamp);
                        if (timestamp > DateTime.MinValue)
                        {
                            sd.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(timestamp.Month);
                            sd.Day = timestamp.Day.ToString();
                        }

                        date = shiftdate + " ";
                    }

                    // Get Shift Progress Bar Maximum -------------------------------------------------
                    string shiftBegin = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Begin", "value");
                    DateTime begin = DateTime.MinValue;
                    DateTime.TryParse(shiftBegin, out begin);

                    string shiftEnd = DataTable_Functions.GetTableValue(dt, "name", "Current Shift End", "value");
                    DateTime end = DateTime.MinValue;
                    DateTime.TryParse(shiftEnd, out end);

                    if (end < begin) end = end.AddDays(1);

                    sd.Shift_Times = "(" + begin.ToShortTimeString() + " - " + end.ToShortTimeString() + ")";

                    Int64 duration = Convert.ToInt64((end - begin).TotalSeconds);
                    if (duration <= int.MaxValue && duration >= int.MinValue)
                    {
                        sd.Bar_Maximum = Convert.ToInt32(duration);
                    }
                    // --------------------------------------------------------------------------------


                    // Get Shift Progress Bar Value ---------------------------------------------------
                    string shiftProgress = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Time", "value");
                    DateTime progress = DateTime.MinValue;
                    DateTime.TryParse(shiftProgress, out progress);

                    if (progress > DateTime.MinValue)
                    {
                        duration = Convert.ToInt64((end - progress).TotalSeconds);
                        if (duration <= int.MaxValue && duration >= int.MinValue)
                        {
                            sd.Bar_Value = Convert.ToInt32(duration);
                        }
                    }
                    // --------------------------------------------------------------------------------


                    string CurrentShiftId = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Id", "value");

                    foreach (Controls.ShiftSegmentIndicator indicator in sd.SegmentIndicators)
                    {
                        SegmentInfo segment = indicator.Segment;

                        if (CurrentShiftId == segment.id)
                        {
                            indicator.CurrentShift = true;

                            string sCurrentTime = shiftProgress;

                            DateTime currentTime = DateTime.MinValue;
                            if (DateTime.TryParse(sCurrentTime, out currentTime))
                            {
                                double segmentProgress = (currentTime - segment.segmentStart).TotalSeconds;
                                indicator.BarValue = Math.Max(0, Convert.ToInt32(segmentProgress));
                            }
                        }
                        else
                        {
                            double segmentDuration = (segment.segmentEnd - segment.segmentStart).TotalSeconds;

                            indicator.BarValue = Math.Max(0, Convert.ToInt32(segmentDuration));
                            indicator.CurrentShift = false;
                        }
                    }
                }
            }
        }

        void UpdateShiftInfo_ShiftData(DeviceDisplay dd, object shiftData)
        {
            int cellIndex = -1;
            cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "shiftinfo");
            if (cellIndex >= 0)
            {
                object data = dd.ComparisonGroup.column.Cells[cellIndex].Data;
                Controls.ShiftDisplay sd;
                if (data != null)
                {
                    sd = (Controls.ShiftDisplay)data;
                }
                else
                {
                    sd = new Controls.ShiftDisplay();
                    dd.ComparisonGroup.column.Cells[cellIndex].Data = sd;
                }

                // Get Shift Segment Indicators ---------------------------------------------------

                List<SegmentInfo> shiftSegments = GetShiftSegments(shiftData);

                double maxDuration = -1;

                foreach (SegmentInfo segment in shiftSegments)
                {
                    double d = (segment.segmentEnd - segment.segmentStart).TotalSeconds;
                    if (d > maxDuration) maxDuration = d;
                }

                foreach (SegmentInfo segment in shiftSegments)
                {
                    Controls.ShiftSegmentIndicator indicator;

                    int indicatorIndex = sd.SegmentIndicators.ToList().FindIndex(x => x.id == segment.segmentId);
                    if (indicatorIndex >= 0) indicator = sd.SegmentIndicators[indicatorIndex];
                    else
                    {
                        indicator = new Controls.ShiftSegmentIndicator();
                        sd.SegmentIndicators.Add(indicator);
                    }

                    indicator.Segment = segment;

                    indicator.id = segment.segmentId;

                    DateTime segmentEnd = segment.segmentEnd;
                    if (segment.segmentEnd < segment.segmentStart) segmentEnd = segmentEnd.AddDays(1);

                    double segmentDuration = (segmentEnd - segment.segmentStart).TotalSeconds;
                    indicator.BarMaximum = Math.Max(0, Convert.ToInt32(segmentDuration));

                    indicator.ProgressWidth = (segmentDuration * 55) / maxDuration;

                    indicator.SegmentTimes = segment.segmentStart.ToShortTimeString() + " - " + segmentEnd.ToShortTimeString();

                    indicator.SegmentDuration = (segmentEnd - segment.segmentStart).ToString();

                    indicator.SegmentId = (segment.segmentId + 1).ToString("00");

                    if (segment.segmentType.ToLower() == "break") indicator.BreakType = true;

                    indicator.SegmentType = segment.segmentType;
                }
            }
        }

        List<SegmentInfo> GetShiftSegments(object shiftData)
        {
            List<SegmentInfo> Result = new List<SegmentInfo>();

            if (shiftData != null)
            {
                DataTable dt = shiftData as DataTable;
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string id = null;
                        int segmentId = -1;
                        string type = null;

                        object start_str = row["Start"];
                        DateTime start = DateTime.MinValue;

                        object end_str = row["End"];
                        DateTime end = DateTime.MinValue;

                        if (row["id"] != null) id = row["id"].ToString();
                        if (row["segmentid"] != null) int.TryParse(row["segmentid"].ToString(), out segmentId);
                        if (row["type"] != null) type = row["type"].ToString();

                        if (start_str != null) DateTime.TryParse(start_str.ToString(), out start);
                        if (end_str != null) DateTime.TryParse(end_str.ToString(), out end);


                        if (segmentId >= 0 && start > DateTime.MinValue && end > DateTime.MinValue)
                        {
                            SegmentInfo info = new SegmentInfo();
                            info.id = id;
                            info.segmentId = segmentId;
                            info.segmentType = type;
                            info.segmentStart = start;
                            info.segmentEnd = end;
                            Result.Add(info);
                        }
                    }
                }
            }

            return Result;

        }

        #endregion

        #region "Production Status Times"

        // Get list of Production Status variables and create TimeDisplay controls for each 
        void UpdateProductionStatusTimes_GenEventValues(DeviceDisplay dd, object geneventvalues)
        {
            StackPanel stack;

            int cellIndex = -1;
            cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimes");

            if (cellIndex >= 0)
            {
                object data = dd.ComparisonGroup.column.Cells[cellIndex].Data;
                if (data == null)
                {
                    stack = new StackPanel();
                    stack.Background = new SolidColorBrush(Colors.Transparent);
                    dd.ComparisonGroup.column.Cells[cellIndex].Data = stack;

                    DataTable dt = geneventvalues as DataTable;
                    if (dt != null)
                    {
                        DataView dv = dt.AsDataView();
                        dv.RowFilter = "EVENT = 'production_status'";
                        DataTable temp_dt = dv.ToTable(false, "VALUE");

                        List<Color> colors = TH_Styles.IndicatorColors.GetIndicatorColors(temp_dt.Rows.Count);
                        int colorIndex = 0;

                        foreach (DataRow row in temp_dt.Rows)
                        {
                            Controls.TimeDisplay td = new Controls.TimeDisplay();

                            // Set Text
                            td.Text = row[0].ToString();

                            // Set Bar Color
                            Color color = colors[colorIndex];
                            td.BarBrush = new SolidColorBrush(Color.FromArgb(221, color.R, color.G, color.B));
                            colorIndex += 1;

                            // Initialize values
                            td.Percentage = "0%";
                            td.BarValue = 0;
                            td.BarMaximum = 1;

                            stack.Children.Add(td);
                        }
                    }
                }
            }
        }

        class ProductionStatus_Variable
        {
            public string name { get; set; }
            public int seconds { get; set; }
        }

        // Get the Times for each Production Status variable from the 'Shifts' table
        void UpdateProductionStatusTimes_ShiftData(DeviceDisplay dd, object shiftData)
        {
            StackPanel stack;

            int cellIndex = -1;
            cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimes");

            if (cellIndex >= 0)
            {
                object data = dd.ComparisonGroup.column.Cells[cellIndex].Data;
                if (data != null)
                {
                    if (data.GetType() == typeof(StackPanel))
                    {
                        stack = (StackPanel)data;

                        DataTable dt = shiftData as DataTable;
                        if (dt != null)
                        {
                            int totalSeconds = 0;
                            List<ProductionStatus_Variable> variables = new List<ProductionStatus_Variable>();

                            // Get List of variables from 'Shifts' table and collect the total number of seconds
                            foreach (DataColumn column in dt.Columns)
                            {
                                if (column.ColumnName.Contains("PRODUCTION_STATUS") || column.ColumnName.Contains("Production_Status") || column.ColumnName.Contains("production_status"))
                                {
                                    ProductionStatus_Variable variable = new ProductionStatus_Variable();

                                    // Get Variable name from Column Name
                                    if (column.ColumnName.Contains("__"))
                                    {
                                        int i = column.ColumnName.IndexOf("__") + 2;
                                        if (i < column.ColumnName.Length)
                                        {
                                            string name = column.ColumnName.Substring(i);
                                            name = name.Replace('_', ' ');

                                            variable.name = name;
                                        }
                                    }

                                    // Get Total Seconds for Variable
                                    DataView dv = dt.AsDataView();
                                    DataTable temp_dt = dv.ToTable(false, column.ColumnName);

                                    foreach (DataRow row in temp_dt.Rows)
                                    {
                                        string value = row[0].ToString();

                                        int seconds = 0;
                                        int.TryParse(value, out seconds);
                                        variable.seconds += seconds;
                                        totalSeconds += seconds;
                                    }

                                    variables.Add(variable);
                                }
                            }

                            // Loop through variables and update TimeDisplay controls
                            foreach (ProductionStatus_Variable var in variables)
                            {
                                foreach (Controls.TimeDisplay td in stack.Children.OfType<Controls.TimeDisplay>())
                                {
                                    if (td.Text != null)
                                    {
                                        if (td.Text.ToLower() == var.name.ToLower())
                                        {
                                            this.Dispatcher.BeginInvoke(new Action<Controls.TimeDisplay, int, int>(UpdateProductionStatusTimes_ShiftData_GUI), Priority_Context, new object[] { td, var.seconds, totalSeconds });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Assign the TimeDisplay control changes
        void UpdateProductionStatusTimes_ShiftData_GUI(Controls.TimeDisplay td, int barvalue, int barmaximum)
        {
            td.Time = TimeSpan.FromSeconds(barvalue).ToString();

            double percentage = (double)barvalue / barmaximum;
            td.Percentage = percentage.ToString("P1");

            td.BarValue = barvalue;
            td.BarMaximum = barmaximum;
        }

        // Highlight the Current Production Status
        void UpdateProductionStatusTimes_SnapshotData(DeviceDisplay dd, object snapshotData)
        {
            StackPanel stack;

            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                int cellIndex = -1;
                cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimes");

                if (cellIndex >= 0)
                {
                    object data = dd.ComparisonGroup.column.Cells[cellIndex].Data;
                    if (data != null)
                    {
                        if (data.GetType() == typeof(StackPanel)) stack = (StackPanel)data;
                        else { stack = new StackPanel(); dd.ComparisonGroup.column.Cells[cellIndex].Data = stack; }

                        string currentStatus = DataTable_Functions.GetTableValue(dt, "name", "Production Status", "value");

                        foreach (Controls.TimeDisplay td in stack.Children.OfType<Controls.TimeDisplay>())
                        {
                            if (td.Text == currentStatus) td.IsSelected = true;
                            else td.IsSelected = false;
                        }
                    }
                }
            }
        }

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

                    int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeeaverage");
                    if (cellIndex >= 0)
                    {
                        Controls.NumberDisplay oeeAverage;

                        object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

                        if (ddData == null)
                        {
                            oeeAverage = new Controls.NumberDisplay();
                            dd.ComparisonGroup.column.Cells[cellIndex].Data = oeeAverage;
                        }
                        else oeeAverage = (Controls.NumberDisplay)ddData;

                        oeeAverage.Value_Format = "P2";
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
                            int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeesegment");
                            if (cellIndex >= 0)
                            {
                                Controls.NumberDisplay oeeSegment;

                                object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

                                if (ddData == null)
                                {
                                    oeeSegment = new Controls.NumberDisplay();
                                    dd.ComparisonGroup.column.Cells[cellIndex].Data = oeeSegment;
                                }
                                else oeeSegment = (Controls.NumberDisplay)ddData;

                                oeeSegment.Value_Format = "P2";
                                oeeSegment.Value = oee;
                            }
                        }
                    }  
                }
            }
        }

        #endregion

        #region "OEE Timeline/Histogram"

        class OEE_TimelineInfo
        {
            public string id { get; set; }
            public double value { get; set; }
            public string segmentTimes { get; set; }
            public string valueText { get; set; }
        }

        void Update_OEE_Timeline_OEEData(DeviceDisplay dd, object oeedata)
        {
            List<OEE_TimelineInfo> infos = new List<OEE_TimelineInfo>();

            DataTable dt = oeedata as DataTable;
            if (dt != null)
            {
                List<double> oees = new List<double>();

                foreach (DataRow row in dt.Rows)
                {
                    string oee_str = DataTable_Functions.GetRowValue("oee", row);

                    if (oee_str != null)
                    {
                        // Get OEE Value
                        double oee = -1;
                        double.TryParse(oee_str, out oee);
                        if (oee >= 0)
                        {
                            // Get Segment Time Info
                            string oee_shiftId = DataTable_Functions.GetRowValue("shift_id", row);

                            if (oee_shiftId != null)
                            {
                                OEE_TimelineInfo info = new OEE_TimelineInfo();
                                info.id = oee_shiftId;

                                info.value = oee;
                                info.valueText = oee.ToString("P2");

                                infos.Add(info);
                            }
                        }
                    }
                }


                int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeetimeline");
                if (cellIndex >= 0)
                {
                    Controls.HistogramDisplay oeeTimeline;

                    object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

                    if (ddData == null)
                    {
                        oeeTimeline = new Controls.HistogramDisplay();
                        dd.ComparisonGroup.column.Cells[cellIndex].Data = oeeTimeline;
                    }
                    else oeeTimeline = (Controls.HistogramDisplay)ddData;


                    foreach (OEE_TimelineInfo info in infos)
                    {
                        Controls.DataBar db;

                        int dbIndex = oeeTimeline.DataBars.ToList().FindIndex(x => x.Id == info.id);
                        if (dbIndex < 0)
                        {
                            db = new Controls.DataBar();
                            db.Id = info.id;
                            db.SegmentTimes = info.segmentTimes;
                            oeeTimeline.DataBars.Add(db);
                        }
                        else db = oeeTimeline.DataBars[dbIndex];

                        db.Value = info.value * 100;
                        db.ValueText = info.valueText;
                    }
                }

            }
        }

        void Update_OEE_Timeline_ShiftData(DeviceDisplay dd, object shiftData)
        {
            int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeetimeline");
            if (cellIndex >= 0)
            {
                Controls.HistogramDisplay oeeTimeline;

                object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

                if (ddData != null)
                {
                    oeeTimeline = (Controls.HistogramDisplay)ddData;

                    // Get Segment Times (for ToolTip)
                    foreach (Controls.DataBar db in oeeTimeline.DataBars)
                    {
                        string segmentTimes = GetSegmentName(db.Id, shiftData);
                        if (segmentTimes != null)
                        {
                            if (db.SegmentTimes != segmentTimes) db.SegmentTimes = segmentTimes;
                        }
                    }
                }
            }
        }

        void Update_OEE_Timeline_SnapshotData(DeviceDisplay dd, object snapshotData)
        {
            List<OEE_TimelineInfo> infos = new List<OEE_TimelineInfo>();

            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "oeetimeline");
                if (cellIndex >= 0)
                {
                    Controls.HistogramDisplay oeeTimeline;

                    object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

                    if (ddData != null)
                    {
                        oeeTimeline = (Controls.HistogramDisplay)ddData;

                        // Get Shift Name to check if still in the same shift as last update
                        string prev_shiftName = oeeTimeline.shiftName;
                        oeeTimeline.shiftName = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Name", "value");
                        if (prev_shiftName != oeeTimeline.shiftName) oeeTimeline.DataBars.Clear();

                        // Get Current Segment
                        foreach (Controls.DataBar db in oeeTimeline.DataBars)
                        {
                            string currentShiftId = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Id", "value");
                            if (currentShiftId == db.Id) db.CurrentSegment = true;
                            else db.CurrentSegment = false;
                        }
                    }
                }
            }
        }

        string GetSegmentName(string shiftId, object shiftData)
        {
            string Result = null;

            DataTable dt = shiftData as DataTable;
            if (dt != null)
            {
                DataView dv = dt.AsDataView();
                dv.RowFilter = "id='" + shiftId + "'";
                DataTable temp_dt = dv.ToTable();

                // Should be max of one row
                if (temp_dt.Rows.Count > 0)
                {
                    DataRow row = temp_dt.Rows[0];

                    //Get Segment start Time
                    string start = DataTable_Functions.GetRowValue("start", row);

                    // Get Segment end Time
                    string end = DataTable_Functions.GetRowValue("end", row);

                    // Create Segment Times string
                    if (start != null && end != null)
                    {
                        DateTime timestamp = DateTime.MinValue;
                        DateTime.TryParse(start, out timestamp);
                        if (timestamp > DateTime.MinValue) start = timestamp.ToShortTimeString();

                        timestamp = DateTime.MinValue;
                        DateTime.TryParse(end, out timestamp);
                        if (timestamp > DateTime.MinValue) end = timestamp.ToShortTimeString();

                        Result = start + " - " + end;
                    }
                }
            }

            return Result;
        }

        #endregion

        #region "Programs"

        void UpdateProgram_Current(DeviceDisplay dd, object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "name", "Program", "value");

                int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "currentprogram");
                if (cellIndex >= 0)
                {
                    Controls.TextDisplay txt;

                    object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

                    if (ddData == null)
                    {
                        txt = new Controls.TextDisplay();
                        dd.ComparisonGroup.column.Cells[cellIndex].Data = txt;
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

                int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "previousprogram");
                if (cellIndex >= 0)
                {
                    Controls.TextDisplay txt;

                    object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;

                    if (ddData == null)
                    {
                        txt = new Controls.TextDisplay();
                        dd.ComparisonGroup.column.Cells[cellIndex].Data = txt;
                    }
                    else txt = (Controls.TextDisplay)ddData;

                    txt.Text = value;
                }
            }
        }

        #endregion

        #region "Production Status Timeline"

        // Get list of Production Status variables
        void UpdateProductionStatusTimeline_GenEventValues(DeviceDisplay dd, object geneventvalues)
        {
            int cellIndex = -1;
            cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimeline");

            if (cellIndex >= 0)
            {
                object data = dd.ComparisonGroup.column.Cells[cellIndex].Data;
                if (data == null)
                {
                    TimeLine timeline = new TimeLine();
                    dd.ComparisonGroup.column.Cells[cellIndex].Data = timeline;

                    DataTable dt = geneventvalues as DataTable;
                    if (dt != null)
                    {
                        DataView dv = dt.AsDataView();
                        dv.RowFilter = "EVENT = 'production_status'";
                        DataTable temp_dt = dv.ToTable(false, "NUMVAL");

                        List<Color> colors = TH_Styles.IndicatorColors.GetIndicatorColors(temp_dt.Rows.Count);

                        timeline.SeriesCount = temp_dt.Rows.Count;

                        List<Tuple<Color, string>> colorList = new List<Tuple<Color, string>>();

                        int colorIndex = 0;

                        foreach (DataRow row in temp_dt.Rows)
                        {
                            string val = row[0].ToString();

                            Tuple<Color, string> colorItem = new Tuple<Color,string>(colors[colorIndex], val);
                            
                            colorList.Add(colorItem);

                            colorIndex += 1;
                        }

                        timeline.Colors = colorList;
                    }
                }
            }
        }

        void UpdateProductionStatusTimeline_ProductionStatusData(DeviceDisplay dd, object productionStatusData)
        {
            DataTable dt = productionStatusData as DataTable;
            if (dt != null)
            {
                int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimeline");
                if (cellIndex >= 0)
                {
                    // Get / Create TimeLineDisplay
                    TH_WPF.TimeLine.TimeLine timeline;

                    object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;
                    if (ddData != null)
                    {
                        timeline = (TH_WPF.TimeLine.TimeLine)ddData;


                        List<Tuple<DateTime, string, string>> data_forShift = new List<Tuple<DateTime, string, string>>();

                        foreach (DataRow row in dt.Rows)
                        {
                            string val = DataTable_Functions.GetRowValue("Timestamp", row);
                            if (val != null)
                            {
                                DateTime timestamp = DateTime.MinValue;
                                DateTime.TryParse(val, out timestamp);

                                if (timestamp > DateTime.MinValue)
                                {
                                    if (timestamp > timeline.previousTimestamp)
                                    {
                                        string value = DataTable_Functions.GetRowValue("Value", row);

                                        val = DataTable_Functions.GetRowValue("Numval", row);
                                        if (val != null)
                                        {
                                            int numval;
                                            if (int.TryParse(val, out numval))
                                            {
                                                if (!timeline.Series.Contains(numval))
                                                {
                                                    timeline.Series.Add(numval);
                                                    timeline.SeriesCount = timeline.Series.Count;
                                                }
                                            }

                                            data_forShift.Add(new Tuple<DateTime, string, string>(timestamp, val, value));
                                        }

                                        timeline.previousTimestamp = timestamp;
                                    }
                                }
                            }
                        }

                        timeline.AddData(data_forShift);

                    }

                    //object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;
                    //if (ddData == null)
                    //{
                    //    timeline = new TH_WPF.TimeLine.TimeLine();
                    //    dd.ComparisonGroup.column.Cells[cellIndex].Data = timeline;
                    //}
                    //else timeline = (TH_WPF.TimeLine.TimeLine)ddData;



                    //List<Tuple<DateTime, string, string>> data_forShift = new List<Tuple<DateTime, string, string>>();

                    //foreach (DataRow row in dt.Rows)
                    //{
                    //    string val = DataTable_Functions.GetRowValue("Timestamp", row);
                    //    if (val != null)
                    //    {
                    //        DateTime timestamp = DateTime.MinValue;
                    //        DateTime.TryParse(val, out timestamp);

                    //        if (timestamp > DateTime.MinValue)
                    //        {
                    //            if (timestamp > timeline.previousTimestamp)
                    //            {
                    //                string value = DataTable_Functions.GetRowValue("Value", row);

                    //                val = DataTable_Functions.GetRowValue("Numval", row);
                    //                if (val != null)
                    //                {
                    //                    int numval;
                    //                    if (int.TryParse(val, out numval))
                    //                    {
                    //                        if (!timeline.Series.Contains(numval))
                    //                        {
                    //                            timeline.Series.Add(numval);
                    //                            timeline.SeriesCount = timeline.Series.Count;
                    //                        }
                    //                    }

                    //                    data_forShift.Add(new Tuple<DateTime, string, string>(timestamp, val, value));
                    //                }

                    //                timeline.previousTimestamp = timestamp;
                    //            }
                    //        }
                    //    }
                    //}

                    //timeline.AddData(data_forShift);

                }
            }
        }

        // Set Shift time from 'Snapshots' table
        void UpdateProductionStatusTimeline_SnapshotData(DeviceDisplay dd, object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                int cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimeline");
                if (cellIndex >= 0)
                {
                    // Local
                    string begin = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Begin", "value");
                    DateTime shift_begin = DateTime.MinValue;
                    DateTime.TryParse(begin, out shift_begin);

                    string end = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Time", "value");
                    DateTime shift_end = DateTime.MinValue;
                    DateTime.TryParse(end, out shift_end);

                    //string end = DataTable_Functions.GetTableValue(dt, "name", "Current Shift End", "value");
                    //DateTime shift_end = DateTime.MinValue;
                    //DateTime.TryParse(end, out shift_end);

                    // Get / Create TimeLineDisplay
                    TH_WPF.TimeLine.TimeLine timeline;

                    object ddData = dd.ComparisonGroup.column.Cells[cellIndex].Data;
                    if (ddData != null)
                    {
                        timeline = (TH_WPF.TimeLine.TimeLine)ddData;

                        timeline.StartTime = shift_begin;
                        timeline.EndTime = shift_end;

                        //// Get Shift Name to check if still in the same shift as last update
                        //string prev_shiftName = timeline.Title;
                        //timeline.Title = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Name", "value");
                        //if (prev_shiftName != timeline.Title)
                        //{
                        //    timeline.StartTime = shift_begin;
                        //    timeline.EndTime = shift_end;
                        //}
                    }

                    //if (ddData == null)
                    //{
                    //    timeline = new TH_WPF.TimeLine.TimeLine();
                    //    dd.ComparisonGroup.column.Cells[cellIndex].Data = timeline;
                    //}
                    //else timeline = (TH_WPF.TimeLine.TimeLine)ddData;

                    


                    //if (timeline.prev_SeriesCount != timeline.Series.Count)
                    //{
                    //    List<Color> rawColors = TH_Styles.IndicatorColors.GetIndicatorColors(timeline.Series.Count);

                    //    List<Tuple<Color, string>> colors = new List<Tuple<Color, string>>();
                    //    var levels = timeline.Series.OrderByDescending(i => i).ToList();
                    //    //var levels = timeline.Series.OrderBy(i => i);
                    //    for (int x = 0; x <= levels.Count - 1; x++)
                    //    {
                    //        colors.Add(new Tuple<Color, string>(rawColors[x], levels[x].ToString()));
                    //    }

                    //    timeline.Colors = colors;
                    //}

                    //timeline.prev_SeriesCount = timeline.Series.Count;


                    //// Get Shift Name to check if still in the same shift as last update
                    //string prev_shiftName = timeline.Title;
                    //timeline.Title = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Name", "value");
                    //if (prev_shiftName != timeline.Title)
                    //{
                    //    timeline.StartTime = shift_begin;
                    //    timeline.EndTime = shift_end;
                    //}
                }
            }
        }

        #endregion

        #endregion

        #region "Components"

        #region "Device Displays"

        List<DeviceDisplay> DeviceDisplays { get; set; }

        void CreateDeviceDisplay(Configuration config)
        {
            DeviceDisplay dd = new DeviceDisplay(config);

            int index = DeviceDisplays.Count;

            Header header = CreateColumnHeader(config);
            Column column = CreateColumn();

            header.Index = index;
            column.Index = index;

            header.column = column;
            column.header = header;

            dd.ComparisonGroup.header = header;
            dd.ComparisonGroup.column = column;

            ColumnHeaders.Add(header);
            Columns.Add(column);

            dd.ConnectedChanged += dd_ConnectedChanged;
            dd.ConnectionStatusChanged += dd_ConnectionStatusChanged;

            DeviceDisplays.Add(dd);
        }

        void dd_ConnectedChanged(DeviceDisplay dd)
        {
            if (dd.ComparisonGroup.header != null) dd.ComparisonGroup.header.Connected = dd.Connected;
            if (dd.ComparisonGroup.column != null) dd.ComparisonGroup.column.Connected = dd.Connected;
        }

        void dd_ConnectionStatusChanged(DeviceDisplay dd)
        {
            if (dd.ComparisonGroup.column != null) dd.ComparisonGroup.column.ConnectionStatus = dd.ConnectionStatus;
        }

        #endregion

        #region "Column Headers"

        ObservableCollection<Header> columnheaders;
        public ObservableCollection<Header> ColumnHeaders
        {
            get
            {
                if (columnheaders == null) columnheaders = new ObservableCollection<Header>();
                return columnheaders;
            }
            set
            {
                columnheaders = value;
            }
        }

        Header CreateColumnHeader(Configuration config)
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

            Result.Clicked += ColumnHeader_Clicked;


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

        void ColumnHeader_Clicked(int Index)
        {
            DeviceSelected(Index);
        }

        public bool ColumnHeaderMinimized
        {
            get { return (bool)GetValue(ColumnHeaderMinimizedProperty); }
            set { SetValue(ColumnHeaderMinimizedProperty, value); }
        }

        public static readonly DependencyProperty ColumnHeaderMinimizedProperty =
            DependencyProperty.Register("ColumnHeaderMinimized", typeof(bool), typeof(DeviceCompare), new PropertyMetadata(false));

        #endregion

        #region "Columns"

        ObservableCollection<Column> columns;
        public ObservableCollection<Column> Columns
        {
            get
            {
                if (columns == null) columns = new ObservableCollection<Column>();
                return columns;
            }
            set
            {
                columns = value;
            }
        }

        Column CreateColumn()
        {
            Column Result = new Column();

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
            RowHeaders_UnselectAll();
            Rows_UnSelectAll();
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

            foreach (Column column in Columns)
            {
                Cell cell = column.Cells[index];

                if (cell.DesiredSize.Height < callingCell.DesiredSize.Height)
                {
                    cell.MinHeight = height;
                }
            }

            RowHeaders[index].MinHeight = height;
        }

        void Result_Clicked(int Index)
        {
            DeviceSelected(Index);
        }

        #endregion

        #region "Row Headers"

        ObservableCollection<Row_Header> rowheaders;
        public ObservableCollection<Row_Header> RowHeaders
        {
            get
            {
                if (rowheaders == null) rowheaders = new ObservableCollection<Row_Header>();
                return rowheaders;
            }
            set
            {
                rowheaders = value;
            }
        }

        void CreateRowHeaders()
        {
            int i = 0;

            foreach (DeviceDisplay.DataItem item in new DeviceDisplay().DataItems)
            {
                Row_Header rh = new Row_Header();
                rh.Text = item.header;
                rh.Index = i;
                rh.Clicked += rh_Clicked;
                RowHeaders.Add(rh);

                i += 1;
            }
        }

        void rh_Clicked(int index)
        {
            foreach (Row_Header header in RowHeaders)
            {
                if (header.Index != index) header.IsSelected = false;
                else header.IsSelected = true;
            }

            foreach (Column column in Columns)
            {
                for (int x = 0; x <= column.Cells.Count - 1; x++)
                {
                    if (x != index) column.Cells[x].IsSelected = false;
                }

                column.Cells[index].IsSelected = true;
            }
        }

        void RowHeaders_UnselectAll()
        {
            foreach (Row_Header header in RowHeaders)
            {
                header.IsSelected = false;
            }
        }

        void Rows_UnSelectAll()
        {
            foreach (Column column in Columns)
            {
                foreach (Cell cell in column.Cells) cell.IsSelected = false;
            }
        }

        void rh_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Row_Header))
            {
                Row_Header rh = (Row_Header)sender;

                int index = RowHeaders.IndexOf(rh);

                foreach (Column column in Columns)
                {
                    for (int x = 0; x <= column.Cells.Count - 1; x++)
                    {
                        if (x != index) column.Cells[x].IsSelected = false;
                    }

                    column.Cells[index].IsSelected = true;
                }
            }
        }

        #endregion

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        #endregion

        bool collapsed = false;
        bool minimized = true;

        private void ColumnHeaderMinimize_Clicked(TH_WPF.Button_02 bt)
        {
            if (!minimized)
            {
                foreach (Header header in ColumnHeaders) header.Minimize();
                minimized = true;              
            }
            else if (!collapsed)
            {
                foreach (Header header in ColumnHeaders) header.Collapse();
                collapsed = true;
            }
        }

        private void ColumnHeaderMaximize_Clicked(TH_WPF.Button_02 bt)
        {
            if (collapsed)
            {
                foreach (Header header in ColumnHeaders) header.Minimize();
                collapsed = false;
            }
            else if (minimized)
            {
                foreach (Header header in ColumnHeaders) header.Expand();
                minimized = false;
            }
        }


        public bool LoggedIn
        {
            get { return (bool)GetValue(LoggedInProperty); }
            set { SetValue(LoggedInProperty, value); }
        }

        public static readonly DependencyProperty LoggedInProperty =
            DependencyProperty.Register("LoggedIn", typeof(bool), typeof(DeviceCompare), new PropertyMetadata(false));

        #endregion

    }

    public class SegmentInfo
    {
        public string id { get; set; }
        public int segmentId { get; set; }
        public string segmentType { get; set; }
        public DateTime segmentStart { get; set; }
        public DateTime segmentEnd { get; set; }
    }

}
