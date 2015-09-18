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

using System.IO;
using System.Collections.ObjectModel;
using System.Globalization;

using TH_Configuration;
using TH_Device_Client;
using TH_PlugIns_Client_Control;
using TH_Functions;

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

            CheckHeaderHeight();
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

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/devicecompare-appinfo.txt"; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return "Dashboard"; } }
        public string DefaultParentCategory { get { return "Pages"; } }

        public bool AcceptsPlugIns { get { return true; } }

        public bool OpenOnStartUp { get { return true; } }

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Update(ReturnData rd)
        {
            this.Dispatcher.BeginInvoke(new Action<ReturnData>(Update_GUI), Priority, new object[] { rd });
        }

        public void Closing() { }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {

        }

        public event DataEvent_Handler DataEvent;

        #endregion

        #region "Device Properties"

        private List<Device_Client> lDevices;
        public List<Device_Client> Devices
        {
            get
            {
                return lDevices;
            }
            set
            {
                lDevices = value;

                DeviceDisplays = new List<DeviceDisplay>();

                foreach (Device_Client device in Devices) CreateDeviceDisplay(device);
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

        public object RootParent { get; set; }

        #endregion

        #region "Device Compare"

        const System.Windows.Threading.DispatcherPriority Priority = System.Windows.Threading.DispatcherPriority.Background;

        List<DeviceDisplay> DeviceDisplays { get; set; }

        void CreateDeviceDisplay(Device_Client device)
        {
            DeviceDisplay dd = new DeviceDisplay(device.configuration);

            int index = DeviceDisplays.Count;

            Header header = CreateColumnHeader(device);
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

            DeviceDisplays.Add(dd);
        }

        void dd_ConnectedChanged(DeviceDisplay dd)
        {
            if (dd.ComparisonGroup.header != null) dd.ComparisonGroup.header.Connected = dd.Connected;
            if (dd.ComparisonGroup.column != null) dd.ComparisonGroup.column.Connected = dd.Connected;
        }

        void DeviceSelected(int index)
        {
            DataEvent_Data de_d = new DataEvent_Data();
            de_d.id = "DeviceSelected";
            de_d.data = index;
            if (DataEvent != null) DataEvent(de_d);
        }

        void Update_GUI(ReturnData rd)
        {
            if (Devices != null)
            {
                List<Device_Client> clients = Devices.FindAll(x => (x.configuration.UniqueId == rd.configuration.UniqueId));

                foreach (Device_Client client in clients)
                {
                    int index = Devices.IndexOf(client);

                    if (index >= 0)
                    {
                        DeviceDisplay dd = DeviceDisplays[index];

                        Header hdr = dd.ComparisonGroup.header;

                        hdr.LastUpdatedTimestamp = DateTime.Now.ToLongTimeString();

                        int cellIndex = -1;

                        // Get connection data from TH_DeviceStatus
                        object connectiondata = null;
                        rd.data.TryGetValue("DeviceStatus_Connection", out connectiondata);
                        if (connectiondata != null)
                        {
                            if (connectiondata.GetType() == typeof(bool))
                            {
                                bool connected = (bool)connectiondata;
                                dd.Connected = connected;
                            }

                        }

                        // Get data from Snapshots Table
                        object snapshotdata = null;
                        rd.data.TryGetValue("DeviceStatus_Snapshots", out snapshotdata);

                        if (snapshotdata != null)
                        {
                            // Update Header ----------------------------------------------------------
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateAlert), Priority, new object[] { dd, snapshotdata });
                            // ------------------------------------------------------------------------

                            // Shift Info
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateShiftInfo), Priority, new object[] { dd, snapshotdata });
                        }

                        // Get data from Shifts Table
                        object shiftdata = null;
                        rd.data.TryGetValue("DeviceStatus_Shifts", out shiftdata);

                        if (shiftdata != null)
                        {

                            // Production Status Times
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object, object>(UpdateProductionStatusTimes), Priority, new object[] { dd, shiftdata, snapshotdata });

                        }

                        // Get data from Production Status
                        object productionstatusdata = null;
                        rd.data.TryGetValue("DeviceStatus_ProductionStatus", out productionstatusdata);

                        if (productionstatusdata != null && snapshotdata != null)
                        {
                            if (productionstatusdata.GetType() == typeof(List<Dictionary<string, string>>))
                            {
                                List<Dictionary<string, string>> data = (List<Dictionary<string, string>>)productionstatusdata;

                                List<Tuple<DateTime, string>> data_forShift = new List<Tuple<DateTime, string>>();

                                string begin = GetSnapShotValue("Current Shift Begin", snapshotdata);
                                DateTime shift_begin = DateTime.MinValue;
                                DateTime.TryParse(begin, out shift_begin);

                                string end = GetSnapShotValue("Current Shift End", snapshotdata);
                                DateTime shift_end = DateTime.MinValue;
                                DateTime.TryParse(end, out shift_end);

                                if (shift_end < shift_begin) shift_end = shift_end.AddDays(1);

                                foreach (Dictionary<string, string> row in data)
                                {
                                    string val = null;
                                    row.TryGetValue("TIMESTAMP", out val);

                                    if (val != null)
                                    {
                                        DateTime timestamp = DateTime.MinValue;
                                        DateTime.TryParse(val, out timestamp);

                                        if (timestamp > DateTime.MinValue)
                                        {
                                            if (shift_begin > DateTime.MinValue && shift_end > DateTime.MinValue)
                                            {
                                                if (timestamp >= shift_begin && timestamp <= shift_end)
                                                {
                                                    val = null;
                                                    row.TryGetValue("Value", out val);

                                                    if (val != null)
                                                    {
                                                        data_forShift.Add(new Tuple<DateTime, string>(timestamp, val));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                // Production Status Timeline
                                Controls.TimeLineDisplay timeline = new Controls.TimeLineDisplay(shift_begin, shift_end);

                                List<Tuple<Color, string>> colors = new List<Tuple<Color, string>>();
                                colors.Add(new Tuple<Color, string>(Colors.Red, "Alert"));
                                colors.Add(new Tuple<Color, string>(Colors.Yellow, "Idle"));
                                colors.Add(new Tuple<Color, string>(Colors.Green, "Full Production"));

                                timeline.CreateData(data_forShift, colors);

                                cellIndex = -1;
                                cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimeline");
                                if (cellIndex >= 0) dd.ComparisonGroup.column.Cells[cellIndex].Data = timeline;
                            }
                        }


                        // Get data from OEE Info
                        object oeedata = null;
                        rd.data.TryGetValue("DeviceStatus_OEE", out oeedata);
                        if (oeedata != null)
                        {
                            // Update Average OEE display
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Avg), Priority, new object[] { dd, oeedata });

                            // Update Segment OEE display
                            this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Segment), Priority, new object[] { dd, oeedata });

                            if (shiftdata != null)
                            {
                                // Update OEE Timeline display
                                this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object, object>(Update_OEE_Timeline), Priority, new object[] { dd, oeedata, shiftdata });
                            }
                        }
                    }
                }
            }
        }

        string GetSnapShotValue(string key, object obj)
        {
            string Result = null;

            if (obj.GetType() == typeof(Dictionary<string, Tuple<DateTime, string, string>>))
            {
                Dictionary<string, Tuple<DateTime, string, string>> snapshot = obj as Dictionary<string, Tuple<DateTime, string, string>>;

                Tuple<DateTime, string, string> data;

                if (snapshot.TryGetValue(key, out data))
                {
                    Result = data.Item2;
                }
            }

            return Result;
        }

        string GetSnapShotPreviousValue(string key, object obj)
        {
            string Result = null;

            if (obj.GetType() == typeof(Dictionary<string, Tuple<DateTime, string, string>>))
            {
                Dictionary<string, Tuple<DateTime, string, string>> snapshot = obj as Dictionary<string, Tuple<DateTime, string, string>>;

                Tuple<DateTime, string, string> data;

                if (snapshot.TryGetValue(key, out data))
                {
                    Result = data.Item3;
                }
            }

            return Result;
        }

        #region "Alert"

        void UpdateAlert(DeviceDisplay dd, object snapshotdata)
        {
            int cellIndex = -1;
            cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "shiftinfo");
            if (cellIndex >= 0)
            {
                Header header = dd.ComparisonGroup.header;
                if (header != null)
                {
                    string value = GetSnapShotValue("Alert", snapshotdata);

                    bool alert = true;
                    if (value != null) bool.TryParse(value, out alert);

                    header.Alert = alert;
                }
            }
        }

        #endregion

        #region "Shifts"

        #region "Shift Display"

        void UpdateShiftInfo(DeviceDisplay dd, object snapshotdata)
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

                sd.Shift_Name = GetSnapShotValue("Current Shift Name", snapshotdata);

                string shiftdate = GetSnapShotValue("Current Shift Date", snapshotdata);
                if (shiftdate != null)
                {
                    DateTime dt = DateTime.MinValue;
                    DateTime.TryParse(shiftdate, out dt);
                    if (dt > DateTime.MinValue)
                    {
                        sd.Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dt.Month);
                        sd.Day = dt.Day.ToString();
                    }
                }


                // Get Shift Progress Bar Maximum -------------------------------------------------
                string shiftBegin = GetSnapShotValue("Current Shift Begin", snapshotdata);
                DateTime begin = DateTime.MinValue;
                DateTime.TryParse(shiftBegin, out begin);

                string shiftEnd = GetSnapShotValue("Current Shift End", snapshotdata);
                DateTime end = DateTime.MinValue;
                DateTime.TryParse(shiftEnd, out end);

                if (end < begin) end = end.AddDays(1);

                sd.Shift_Times = "(" + begin.ToShortTimeString() + " - " + end.ToShortTimeString() + ")";

                Int64 duration = Convert.ToInt64((end - begin).TotalSeconds);               
                if (duration <= int.MaxValue && duration >= int.MinValue) sd.Bar_Maximum = Convert.ToInt32(duration);
                // --------------------------------------------------------------------------------

                // Get Shift Progress Bar Value ---------------------------------------------------
                string shiftProgress = GetSnapShotValue("Current Shift Time", snapshotdata);
                DateTime progress = DateTime.MinValue;
                DateTime.TryParse(shiftProgress, out progress);

                if (progress > DateTime.MinValue)
                {
                    duration = Convert.ToInt64((end - progress).TotalSeconds);
                    if (duration <= int.MaxValue && duration >= int.MinValue) sd.Bar_Value = Convert.ToInt16(duration);
                }
                // --------------------------------------------------------------------------------


                //sd.Bar_Maximum = ;
            }
        }

        #endregion

        #region "Production Status"

        class ProductionStatus_Return
        {
            public ProductionStatus_Return()
            {
                variables = new List<ProductionStatus_Variable>();
            }

            public List<ProductionStatus_Variable> variables;

            public int totalSeconds { get; set; }
        }

        class ProductionStatus_Variable
        {
            public string name { get; set; }
            public int seconds { get; set; }
        }

        class ProductionStatus_UpdateData
        {
            public ProductionStatus_Return statusReturn { get; set; }
            public object shiftdata { get; set; }
            public object snapshotdata { get; set; }

            public ProductionStatus_Variable variable { get; set; }
            public StackPanel stack { get; set; }
            public Color color { get; set; }
        }

        void UpdateProductionStatusTimes(DeviceDisplay dd, object shiftdata, object snapshotdata)
        {
            StackPanel stack;

            ProductionStatus_Return statusReturn = GetProductionStatusTimes(shiftdata);

            List<Color> colors = TH_Styles.IndicatorColors.GetIndicatorColors(statusReturn.variables.Count);

            int colorIndex = 0;

            int cellIndex = -1;
            cellIndex = dd.ComparisonGroup.column.Cells.ToList().FindIndex(x => x.Link.ToLower() == "productionstatustimes");

            if (cellIndex >= 0)
            {
                object data = dd.ComparisonGroup.column.Cells[cellIndex].Data;
                if (data != null)
                {
                    if (data.GetType() == typeof(StackPanel)) stack = (StackPanel)data;
                    else { stack = new StackPanel(); dd.ComparisonGroup.column.Cells[cellIndex].Data = stack; }
                }
                else { stack = new StackPanel(); dd.ComparisonGroup.column.Cells[cellIndex].Data = stack; }

                foreach (ProductionStatus_Variable variable in statusReturn.variables)
                {
                    Color color = colors[colorIndex];

                    ProductionStatus_UpdateData updateData = new ProductionStatus_UpdateData();
                    updateData.statusReturn = statusReturn;
                    updateData.shiftdata = shiftdata;
                    updateData.snapshotdata = snapshotdata;

                    updateData.variable = variable;
                    updateData.stack = stack;
                    updateData.color = color;

                    this.Dispatcher.BeginInvoke(new Action<ProductionStatus_UpdateData>(AddProductionStatusTime), Priority, new object[] { updateData });

                    colorIndex += 1;
                }
            }
        }

        void AddProductionStatusTime(ProductionStatus_UpdateData updateData)
        {
            Controls.TimeDisplay td;

            string text = updateData.variable.name.Replace("Production_Status__", "").Replace("_", " ");

            int index = -1;
            index = updateData.stack.Children.OfType<Controls.TimeDisplay>().ToList().FindIndex(x => x.Text.ToLower() == text.ToLower());
            if (index < 0)
            {
                td = new Controls.TimeDisplay();
                td.Text = text;
                td.BarBrush = new SolidColorBrush(Color.FromArgb(221, updateData.color.R, updateData.color.G, updateData.color.B));
                updateData.stack.Children.Add(td);
                index = updateData.stack.Children.OfType<Controls.TimeDisplay>().ToList().FindIndex(x => x.Text.ToLower() == text.ToLower());
            }

            td = (Controls.TimeDisplay)updateData.stack.Children[index];
            td.Text = text;

            //Highlight current status
            if (updateData.snapshotdata != null)
            {
                string currentStatus = GetSnapShotValue("Production Status", updateData.snapshotdata);
                if (currentStatus != null)
                {
                    if (currentStatus.ToLower() == td.Text.ToLower()) td.IsSelected = true;
                    else td.IsSelected = false;
                }
                else td.IsSelected = false;
            }
            else td.IsSelected = false;

            // Set Time (seconds)
            td.Time = TimeSpan.FromSeconds(updateData.variable.seconds).ToString(@"hh\:mm\:ss");

            // Set Percentage (seconds / totalseconds)
            double percentage = (double)updateData.variable.seconds / updateData.statusReturn.totalSeconds;
            td.Percentage = percentage.ToString("P0");

            // Set Bar Values 
            td.BarMaximum = updateData.statusReturn.totalSeconds;
            td.BarValue = updateData.variable.seconds;
        }

        ProductionStatus_Return GetProductionStatusTimes(object data)
        {
            ProductionStatus_Return Result = new ProductionStatus_Return();

            // Get data from returnData.data
            List<Dictionary<string, string>> shiftData = GetShiftData(data);

            if (shiftData != null)
            {
                // Get a List of all rows that match the current shift
                // NO LONGER NEEDED! ONLY RETURNS DATA FOR CURRENT SHIFT NOW || 8-31-15
                //List<Tuple<string, Dictionary<string, string>>> currentShiftData = GetCurrentShiftData(shiftData);

                //if (currentShiftData != null)
                //{
                    int totalSeconds = 0;
                    List<ProductionStatus_Variable> variables = new List<ProductionStatus_Variable>();

                    // Tuple<[SHIFTNAME], Dictionary<[COLUMN, VALUE]>
                    foreach (Dictionary<string, string> row in shiftData)
                    {
                        // KeyValuePair<[COLUMN, VALUE]>
                        foreach (KeyValuePair<string, string> cell in row)
                        {
                            if (cell.Key.ToLower() == "totaltime")
                            {
                                int seconds = 0;
                                int.TryParse(cell.Value, out seconds);
                                totalSeconds += seconds;
                            }
                            else if (cell.Key.Contains("Production_Status"))
                            {
                                ProductionStatus_Variable variable;
                                int index = variables.FindIndex(x => x.name.ToLower() == cell.Key.ToLower());
                                if (index < 0)
                                {
                                    variable = new ProductionStatus_Variable();
                                    variable.name = cell.Key;
                                    variables.Add(variable);
                                    index = variables.FindIndex(x => x.name.ToLower() == cell.Key.ToLower());
                                }

                                variable = variables[index];

                                int seconds = 0;
                                int.TryParse(cell.Value, out seconds);
                                variable.seconds += seconds;
                            }
                        }
                    }

                    Result.totalSeconds = totalSeconds;
                    Result.variables = variables;

                //}
            }

            return Result;

        }

        #endregion

        List<Dictionary<string, string>> GetShiftData(object obj)
        {
            List<Dictionary<string, string>> Result = null;

            if (obj.GetType() == typeof(List<Dictionary<string, string>>))
            {
                Result = obj as List<Dictionary<string, string>>;
            }

            return Result;
        }

        List<Tuple<string, Dictionary<string, string>>> GetCurrentShiftData(List<Tuple<string, Dictionary<string, string>>> obj)
        {
            if (obj.Count > 0)
            {
                string currentShiftName = obj[obj.Count - 1].Item1;

                return obj.FindAll(x => x.Item1.ToLower() == currentShiftName.ToLower());
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region "OEE"

        double prev_oeeAvg = 0;

        void Update_OEE_Avg(DeviceDisplay dd, object oeedata)
        {
            if (oeedata.GetType() == typeof(List<Dictionary<string, string>>))
            {
                List<Dictionary<string, string>> data = (List<Dictionary<string, string>>)oeedata;

                List<double> oees = new List<double>();

                foreach (Dictionary<string, string> row in data)
                {
                    string oeeVal = null;
                    row.TryGetValue("OEE", out oeeVal);
                    if (oeeVal != null)
                    {
                        double oee = -1;
                        double.TryParse(oeeVal, out oee);
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
                            //oeeAverage.Value_Type = "Avg";
                            dd.ComparisonGroup.column.Cells[cellIndex].Data = oeeAverage;
                        }
                        else oeeAverage = (Controls.NumberDisplay)ddData;

                        oeeAverage.Value = average.ToString("P2");

                        if (average > prev_oeeAvg)
                        {
                            oeeAverage.ValueIncreasing = true;
                            oeeAverage.ValueDecreasing = false;
                        }
                        else if (average < prev_oeeAvg)
                        {
                            oeeAverage.ValueIncreasing = false;
                            oeeAverage.ValueDecreasing = true;
                        }
                        else
                        {
                            oeeAverage.ValueIncreasing = false;
                            oeeAverage.ValueDecreasing = false;
                        }

                        prev_oeeAvg = average;
                    }
                }
            }
        }


        double prev_oeeSegment = 0;

        void Update_OEE_Segment(DeviceDisplay dd, object oeedata)
        {
            if (oeedata.GetType() == typeof(List<Dictionary<string, string>>))
            {
                List<Dictionary<string, string>> data = (List<Dictionary<string, string>>)oeedata;

                Dictionary<string, string> lastRow = data[data.Count - 1];

                string oeeVal = null;
                lastRow.TryGetValue("OEE", out oeeVal);
                if (oeeVal != null)
                {
                    double oee = -1;
                    double.TryParse(oeeVal, out oee);
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

                            oeeSegment.Value = oee.ToString("P2");

                            if (oee > prev_oeeSegment)
                            {
                                oeeSegment.ValueIncreasing = true;
                                oeeSegment.ValueDecreasing = false;
                            }
                            else if (oee < prev_oeeSegment)
                            {
                                oeeSegment.ValueIncreasing = false;
                                oeeSegment.ValueDecreasing = true;
                            }
                            else
                            {
                                oeeSegment.ValueIncreasing = false;
                                oeeSegment.ValueDecreasing = false;
                            }

                            prev_oeeSegment = oee;
                        }
                    }
                }
            }
        }


        void Update_OEE_Timeline(DeviceDisplay dd, object oeedata, object shiftdata)
        {
            if (oeedata.GetType() == typeof(List<Dictionary<string, string>>) &&
                shiftdata.GetType() == typeof(List<Dictionary<string, string>>))
            {
                List<Dictionary<string, string>> oee_data = (List<Dictionary<string, string>>)oeedata;
                List<Dictionary<string, string>> shift_data = (List<Dictionary<string, string>>)shiftdata;

                List<KeyValuePair<string, double>> oees = new List<KeyValuePair<string, double>>();

                foreach (Dictionary<string, string> row in oee_data)
                {
                    string oeeVal = null;
                    row.TryGetValue("OEE", out oeeVal);
                    if (oeeVal != null)
                    {
                        // Get OEE Value
                        double oee = -1;
                        double.TryParse(oeeVal, out oee);
                        if (oee >= 0)
                        {
                            // Get Segment Times for Histogram X Axis Labels
                            string oee_shiftId = null;
                            row.TryGetValue("Shift_Id", out oee_shiftId);
                            if (oee_shiftId != null)
                            {
                                string segment = GetSegmentName(oee_shiftId, shift_data);
                                if (segment != null)
                                {
                                    KeyValuePair<string, double> kvp = new KeyValuePair<string, double>(segment, oee);
                                    oees.Add(kvp);
                                }
                            }
                        }   
                    }                  
                }

                if (oees.Count > 0)
                {
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

                        oeeTimeline.CreateData(oees);
                    }
                }
            }
        }

        string GetSegmentName(string shiftId, List<Dictionary<string, string>> shiftData)
        {
            string Result = null;
            foreach (Dictionary<string, string> row in shiftData)
            {
                string id = null;
                row.TryGetValue("Id", out id);
                if (id != null)
                {
                    if (id.ToLower() == shiftId.ToLower())
                    {
                        // Get Segment start Time
                        string start = null;
                        row.TryGetValue("Start", out start);
                        
                        // Get Segment end Time
                        string end = null;
                        row.TryGetValue("End", out end);

                        // Create Segment Times string
                        if (start != null && end != null)
                        {
                            DateTime dt = DateTime.MinValue;
                            DateTime.TryParse(start, out dt);
                            if (dt > DateTime.MinValue) start = dt.ToShortTimeString();

                            dt = DateTime.MinValue;
                            DateTime.TryParse(end, out dt);
                            if (dt > DateTime.MinValue) end = dt.ToShortTimeString();

                            Result = start + " - " + end;  
                        }

                        break;
                    }
                }
            }
            return Result;
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

        Header CreateColumnHeader(Device_Client device)
        {
            Header Result = new Header();

            Result.Device_Description = device.configuration.Description.Description;
            Result.Device_Manufacturer = device.configuration.Description.Manufacturer;
            Result.Device_Model = device.configuration.Description.Model;
            Result.Device_Serial = device.configuration.Description.Serial;
            Result.Device_ID = device.configuration.Description.Machine_ID;

            Result.Clicked += ColumnHeader_Clicked;

            BitmapImage device_image = Image_Functions.GetImageFromFile(device.configuration.FileLocations.Image_Path);
            BitmapImage device_logo = Image_Functions.GetImageFromFile(device.configuration.FileLocations.Manufacturer_Logo_Path);

            Result.Device_Image = Image_Functions.SetImageSize(device_image, 160);
            Result.Device_Logo = Image_Functions.SetImageSize(device_logo, 0, 50);

            return Result;
        }

        void ColumnHeader_Clicked(int Index)
        {
            DeviceSelected(Index);
        }

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
            if (e.HeightChanged)
            {
                CheckHeaderHeight();
            }
        }

        void CheckHeaderHeight()
        {
            if (this.RenderSize.Height < 650) foreach (Header header in ColumnHeaders) header.Minimized = true;
            else foreach (Header header in ColumnHeaders) header.Minimized = false;
        }

        #endregion

    }


    class DeviceDisplay
    {
        public DeviceDisplay()
        {
            Init();
        }
        public DeviceDisplay(Configuration config)
        {
            configuration = config;
            Init();
        }

        void Init()
        {
            DataItems_Init();
            ComparisonGroup = new Comparison_Group();
        }

        private bool connected;
        public bool Connected
        {
            get { return connected; }
            set
            {
                connected = value;
                if (ConnectedChanged != null) ConnectedChanged(this);
            }
        }
        public delegate void ConnectedChanged_Handler(DeviceDisplay dd);
        public event ConnectedChanged_Handler ConnectedChanged;

        public Configuration configuration;

        public class DataItem
        {
            public string id { get; set; }

            public string header { get; set; }

            public object data { get; set; }

            public int rowHeight { get; set; }
        }

        public class Comparison_Group
        {
            public Header header;

            public Column column;
        }

        public List<DataItem> DataItems;

        void DataItems_Init()
        {
            DataItems = new List<DataItem>();

            DataItem dd;

            dd = new DataItem();
            dd.id = "ShiftInfo";
            dd.header = "Shift Info";
            DataItems.Add(dd);

            dd = new DataItem();
            dd.id = "OEEAverage";
            dd.header = "OEE Shift Average";
            DataItems.Add(dd);

            dd = new DataItem();
            dd.id = "OEESegment";
            dd.header = "OEE Shift Segment";
            DataItems.Add(dd);

            dd = new DataItem();
            dd.id = "productionstatustimes";
            dd.header = "Production Status Times";
            DataItems.Add(dd);

            //dd = new DataItem();
            //dd.id = "CNCInfo";
            //dd.header = "CNC Info";
            //DataItems.Add(dd);

            dd = new DataItem();
            dd.id = "ProductionStatusTimeline";
            dd.header = "Production Status Timeline";
            DataItems.Add(dd);

            dd = new DataItem();
            dd.id = "OEETimeLine";
            dd.header = "OEE Timeline";
            DataItems.Add(dd);
        }

        public Comparison_Group ComparisonGroup;

        public ImageSource Logo { get; set; }

        public bool Alert { get; set; }
        public int Index { get; set; }
        public bool IsConnected { get; set; }
    }
}
