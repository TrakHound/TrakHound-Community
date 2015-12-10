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
using System.Threading;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_PlugIns_Client_Control;
using TH_UserManagement.Management;
using TH_WPF.TimeLine;


namespace TH_DevicePage
{
    /// <summary>
    /// Interaction logic for DevicePage.xaml
    /// </summary>
    public partial class DevicePage : UserControl
    {
        public DevicePage()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Plugin ParentPlugin { get; set; }

        #region "Device Page"

        public Configuration Device { get; set; }
        

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;

        public void Load(Configuration device)
        {

            Device_Description = device.Description.Description;
            Device_Manufacturer = device.Description.Manufacturer;
            Device_Model = device.Description.Model;
            Device_Serial = device.Description.Serial;

            LoadManufacturerLogo(device.FileLocations.Manufacturer_Logo_Path);

            LoadOEEComparisonTable(device);

        }

        public void Update(DataEvent_Data de_d)
        {
            if (de_d != null)
            {
                Configuration config = de_d.data01 as Configuration;
                if (config != null)
                {
                    if (Device != null)
                    {
                        if (config.UniqueId == Device.UniqueId)
                        {
                            // Connection
                            //if (de_d.id.ToLower() == "statusdata_connection")
                            //{
                            //    bool connected;
                            //    bool.TryParse(de_d.data02.ToString(), out connected);

                            //    if (Connected != connected)
                            //    {
                            //        Connected = connected;

                            //        Loading = false;
                            //    }
                            //}

                            //// Snapshot Table Data
                            if (de_d.id.ToLower() == "statusdata_snapshots")
                            {

                            //    // Production
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateStatus_Production), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // Idle
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateStatus_Idle), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // Alert
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateStatus_Alert), Priority_Context, new object[] { dd, de_d.data02 });



                            //    // Shift Info
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateShiftInfo_Snapshots), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // OEE Timeline / Histogram
                                this.Dispatcher.BeginInvoke(new Action<object>(Update_OEE_Timeline_SnapshotData), Priority_Context, new object[] { de_d.data02 });

                            //    // Production Status Times
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });

                            //    // Production Status Timeline
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimeline_SnapshotData), Priority_Context, new object[] { dd, de_d.data02 });
                            }

                            // Shifts Table Data
                            if (de_d.id.ToLower() == "statusdata_shiftdata")
                            {
                            //    // Shift Info
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateShiftInfo_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });

                            //  // OEE Timeline / Histogram
                                this.Dispatcher.BeginInvoke(new Action<object>(Update_OEE_Timeline_ShiftData), Priority_Context, new object[] { de_d.data02 });

                            //    // Production Status Times
                            //    this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(UpdateProductionStatusTimes_ShiftData), Priority_Context, new object[] { dd, de_d.data02 });
                            }

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
                                //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Avg), Priority_Context, new object[] { dd, de_d.data02 });

                                // Current Segment OEE
                                //this.Dispatcher.BeginInvoke(new Action<DeviceDisplay, object>(Update_OEE_Segment), Priority_Context, new object[] { dd, de_d.data02 });

                                // OEE Timeline / Histogram
                                this.Dispatcher.BeginInvoke(new Action<object>(Update_OEE_Timeline_OEEData), Priority_Context, new object[] { de_d.data02 });
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
        }

        #region "Properties"

        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(DevicePage), new PropertyMetadata(false));


        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(DevicePage), new PropertyMetadata(false));


        



        public string Device_Description
        {
            get { return (string)GetValue(Device_DescriptionProperty); }
            set { SetValue(Device_DescriptionProperty, value); }
        }

        public static readonly DependencyProperty Device_DescriptionProperty =
            DependencyProperty.Register("Device_Description", typeof(string), typeof(DevicePage), new PropertyMetadata(null));


        public string Device_Manufacturer
        {
            get { return (string)GetValue(Device_ManufacturerProperty); }
            set { SetValue(Device_ManufacturerProperty, value); }
        }

        public static readonly DependencyProperty Device_ManufacturerProperty =
            DependencyProperty.Register("Device_Manufacturer", typeof(string), typeof(DevicePage), new PropertyMetadata(null));


        public string Device_Model
        {
            get { return (string)GetValue(Device_ModelProperty); }
            set { SetValue(Device_ModelProperty, value); }
        }

        public static readonly DependencyProperty Device_ModelProperty =
            DependencyProperty.Register("Device_Model", typeof(string), typeof(DevicePage), new PropertyMetadata(null));



        public string Device_Serial
        {
            get { return (string)GetValue(Device_SerialProperty); }
            set { SetValue(Device_SerialProperty, value); }
        }

        public static readonly DependencyProperty Device_SerialProperty =
            DependencyProperty.Register("Device_Serial", typeof(string), typeof(DevicePage), new PropertyMetadata(null));

        
        

        #endregion

        #region "Device Logo"

        public ImageSource DeviceLogo
        {
            get { return (ImageSource)GetValue(DeviceLogoProperty); }
            set { SetValue(DeviceLogoProperty, value); }
        }

        public static readonly DependencyProperty DeviceLogoProperty =
            DependencyProperty.Register("DeviceLogo", typeof(ImageSource), typeof(DevicePage), new PropertyMetadata(null));

        

        public bool ManufacturerLogoLoading
        {
            get { return (bool)GetValue(ManufacturerLogoLoadingProperty); }
            set { SetValue(ManufacturerLogoLoadingProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerLogoLoadingProperty =
            DependencyProperty.Register("ManufacturerLogoLoading", typeof(bool), typeof(DevicePage), new PropertyMetadata(false));


        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;


        Thread LoadManufacturerLogo_THREAD;

        public void LoadManufacturerLogo(string filename)
        {
            ManufacturerLogoLoading = true;

            if (LoadManufacturerLogo_THREAD != null) LoadManufacturerLogo_THREAD.Abort();

            LoadManufacturerLogo_THREAD = new Thread(new ParameterizedThreadStart(LoadManufacturerLogo_Worker));
            LoadManufacturerLogo_THREAD.Start(filename);
        }

        void LoadManufacturerLogo_Worker(object o)
        {
            if (o != null)
            {
                string filename = o.ToString();

                System.Drawing.Image img = Images.GetImage(filename, ParentPlugin.UserDatabaseSettings);

                this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadManufacturerLogo_GUI), priority, new object[] { img });
            }
            else this.Dispatcher.BeginInvoke(new Action<System.Drawing.Image>(LoadManufacturerLogo_GUI), priority, new object[] { null });
        }

        void LoadManufacturerLogo_GUI(System.Drawing.Image img)
        {
            if (img != null)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                IntPtr bmpPt = bmp.GetHbitmap();
                BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                bmpSource.Freeze();

                if (bmpSource.PixelWidth > bmpSource.PixelHeight)
                {
                    DeviceLogo = TH_WPF.Image_Functions.SetImageSize(bmpSource, 300);
                }
                else
                {
                    DeviceLogo = TH_WPF.Image_Functions.SetImageSize(bmpSource, 0, 100);
                }

                //ManufacturerLogoSet = true;
            }
            else
            {
                DeviceLogo = null;
                //ManufacturerLogoSet = false;
            }

            ManufacturerLogoLoading = false;
        }

        #endregion

        #region "OEE Comparison"

        void LoadOEEComparisonTable(Configuration config)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Shift Name");
            table.Columns.Add("Average OEE");
            table.Columns.Add("Median OEE");
            table.Columns.Add("Max OEE");
            table.Columns.Add("Min OEE");

            // Get entire table (may need to be sorted using user input)
            DataTable oeeTable = TH_Database.Table.Get(config.Databases_Client, "oee");
            if (oeeTable != null)
            {
                DataView dv = oeeTable.AsDataView();
                string filter = "Shift_Id LIKE '20151012*'";
                //string filter = "Shift_Id LIKE '%00%'";
                dv.RowFilter = filter;
                oeeTable = dv.ToTable();

                double[] valsOEE = new double[oeeTable.Rows.Count];

                if (oeeTable.Rows.Count > 0)
                {
                    for (int x = 0; x <= oeeTable.Rows.Count - 1; x++)
                    {
                        object o = oeeTable.Rows[x]["OEE"];
                        double.TryParse(o.ToString(), out valsOEE[x]);
                    }

                    string shiftName = "Test Shift";
                    double averageOEE = 0;
                    double medianOEE = 0;
                    double maxOEE = 0;
                    double minOEE = 0;

                    averageOEE = valsOEE.Average();
                    medianOEE = TH_Global.Functions.Math_Functions.GetMedian(valsOEE);
                    maxOEE = valsOEE.Max();
                    minOEE = valsOEE.Min();

                    DataRow newRow = table.NewRow();
                    newRow["Shift Name"] = shiftName;
                    newRow["Average OEE"] = averageOEE.ToString("P2");
                    newRow["Median OEE"] = medianOEE.ToString("P2");
                    newRow["Max OEE"] = maxOEE.ToString("P2");
                    newRow["Min OEE"] = minOEE.ToString("P2");

                    table.Rows.Add(newRow);
                }
            }

            if (table != null)
            {
                OEEComparisonDataView = table.AsDataView();
            }
        }


        public DataView OEEComparisonDataView
        {
            get { return (DataView)GetValue(OEEComparisonDataViewProperty); }
            set { SetValue(OEEComparisonDataViewProperty, value); }
        }

        public static readonly DependencyProperty OEEComparisonDataViewProperty =
            DependencyProperty.Register("OEEComparisonDataView", typeof(DataView), typeof(DevicePage), new PropertyMetadata(null));



        #endregion

        #region "OEE Timeline/Histogram"

        class OEE_TimelineInfo
        {
            public string id { get; set; }
            public double value { get; set; }
            public string segmentTimes { get; set; }
            public string valueText { get; set; }
        }

        void Update_OEE_Timeline_OEEData(object oeedata)
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

                foreach (OEE_TimelineInfo info in infos)
                {
                    TH_WPF.Histogram.DataBar db;

                    int dbIndex = OEE_Timeline.DataBars.ToList().FindIndex(x => x.Id == info.id);
                    if (dbIndex < 0)
                    {
                        db = new TH_WPF.Histogram.DataBar();
                        db.Id = info.id;
                        db.SegmentTimes = info.segmentTimes;
                        db.BarWidth = 20;
                        OEE_Timeline.DataBars.Add(db);
                    }
                    else db = OEE_Timeline.DataBars[dbIndex];

                    db.Value = info.value * 100;
                    db.ValueText = info.valueText;
                }

            }
        }

        void Update_OEE_Timeline_ShiftData(object shiftData)
        {
            // Get Segment Times (for ToolTip)
            foreach (TH_WPF.Histogram.DataBar db in OEE_Timeline.DataBars)
            {
                string segmentTimes = GetSegmentName(db.Id, shiftData);
                if (segmentTimes != null)
                {
                    if (db.SegmentTimes != segmentTimes) db.SegmentTimes = segmentTimes;
                }
            }
        }

        void Update_OEE_Timeline_SnapshotData(object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                // Get Shift Name to check if still in the same shift as last update
                string prev_shiftName = OEE_Timeline.shiftName;
                OEE_Timeline.shiftName = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Name", "value");
                if (prev_shiftName != OEE_Timeline.shiftName) OEE_Timeline.DataBars.Clear();

                // Get Current Segment
                foreach (TH_WPF.Histogram.DataBar db in OEE_Timeline.DataBars)
                {
                    string currentShiftId = DataTable_Functions.GetTableValue(dt, "name", "Current Shift Id", "value");
                    if (currentShiftId == db.Id) db.CurrentSegment = true;
                    else db.CurrentSegment = false;
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

        #endregion

    }
}
