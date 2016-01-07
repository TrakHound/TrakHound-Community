using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
//using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.ObjectModel;
using System.IO;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Client;
using TH_Styles;
using TH_UserManagement.Management;
using TH_WPF;

using TH_History.Controls;
using TH_DataCenter.Controls.Graphs.Bar.Category;

namespace TH_History
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class History : UserControl, Plugin
    {
        public History()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "History"; } }

        public string Description { get { return "View Device Data History"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DataCenter;component/Resources/History_01.png")); } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DataCenter;component/Resources/TrakHound_Logo_10_200px.png")); } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\License\" + "License.txt"); } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return null; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return null; } }
        public string DefaultParentCategory { get { return null; } }

        public bool AcceptsPlugins { get { return true; } }

        public bool OpenOnStartUp { get { return false; } }

        public bool ShowInAppMenu { get { return true; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<Plugin> Plugins { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Closing() { }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {
            //if (de_d.id.ToLower() == "deviceselected")
            //{
            //    int index = (int)de_d.data;

            //    Configuration config = Devices[index].configuration;


            //    Controls.HistoryContainer container = new HistoryContainer();

            //    container.Info_GRID.Children.Add(new ShiftComparison(config));
                
            //    // Create Calendar
            //    Controls.Calendar.Calendar calendar = new Controls.Calendar.Calendar();
            //    calendar.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            //    container.Calendar_GRID.Children.Add(calendar);

            //    HistoryItems.Add(container);



            ////    DataTable dt = Global.Table_Get(config.SQL, "shifts");

            ////    GraphInfo info = new GraphInfo();
            ////    info.Name = "Production Seconds";
            ////    info.Table = dt;
            ////    info.DataColumn = "Production_Status__Full_Production";
            ////    info.CategoryColumn = "Segment";

            ////    HistoryItem item = new HistoryItem(info);
            ////    HistoryItems.Add(item);
            //}
        }

        public event DataEvent_Handler DataEvent;

        public event PluginTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Device Properties"

        private List<Configuration> devices;
        public List<Configuration> Devices
        {
            get
            {
                return devices;
            }
            set
            {
                devices = value;

                foreach (Configuration device in devices)
                {
                    AddDevice(device);
                }

            }
        }

        #endregion

        #region "Options"

        public TH_Global.Page Options { get; set; }

        #endregion

        #region "User"

        public UserConfiguration CurrentUser { get; set; }

        //UserConfiguration currentuser = null;
        //public UserConfiguration CurrentUser
        //{
        //    get { return currentuser; }
        //    set
        //    {
        //        currentuser = value;
        //    }
        //}

        public Database_Settings UserDatabaseSettings { get; set; }

        #endregion

        public object RootParent { get; set; }

        #endregion

        #region "History"

        private ObservableCollection<ListButton> devicelist;
        public ObservableCollection<ListButton> DeviceList
        {
            get
            {
                if (devicelist == null)
                    devicelist = new ObservableCollection<ListButton>();

                return devicelist;
            }

            set
            {
                devicelist = value;
            }
        }

        void AddDevice(Configuration config)
        {

            ListButton bt = new ListButton();
            bt.Text = config.Description.Description;
            DeviceList.Add(bt);


            GetOEE_Day(config);
            //GetOEE_Week(config);

            //graph1.Plot1.InvalidatePlot(true);
        }

        List<Data> CreateDummyData()
        {
            var result = new List<Data>();

            var rnd = new Random();

            for (var x = 0; x <= 100; x++)
            {
                var data = new Data();
                int val = rnd.Next(100);
                data.Value = val;
                //data.Category = x;
                result.Add(data);
            }

            return result;
        }

        void GetOEE_Day(Configuration config)
        {
            DataEvent_Data result = new DataEvent_Data();

            // Get OEE Table
            //string shiftQuery = DateTime.Now.Subtract(TimeSpan.FromDays(1)).ToString("yyyyMMdd_");
            string query = DateTime.Now.ToString("yyyyMMdd_");
            DataTable oee_dt = Table.Get(config.Databases_Client, TableNames.OEE, "WHERE SHIFT_ID LIKE '" + query + "%'");

            // Get Shifts Table
            DataTable shifts_dt = Table.Get(config.Databases_Client, TableNames.Shifts, "WHERE ID LIKE '" + query + "%'");

            shiftDisplay.LoadData(oee_dt, shifts_dt);


            //if (dt != null)
            //{
                


            //    //ShiftInfo_Day(dt);


            //    //var dataList = new List<Data>();

            //    //List<double> oees = new List<double>();

            //    //foreach (DataRow row in dt.Rows)
            //    //{
            //    //    string shiftId = row["SHIFT_ID"].ToString();

            //    //    string oee_str = row["OEE"].ToString();
            //    //    double oee = 0;
            //    //    double.TryParse(oee_str, out oee);

            //    //    oees.Add(oee);

            //    //    //var data = new Data(); 
            //    //    //data.Category = shiftId;
            //    //    //data.Value = oee;
            //    //    //dataList.Add(data);
            //    //}

            //    //ShiftInfo_Day_OEE(oees);

            //    //var color = ((SolidColorBrush)TryFindResource("Accent_Normal")).Color;
            //    //var color = Colors.Green;

            //    //graph1.viewModel.LoadData(dataList, color);
            //}
        }

        //void GetOEE_Week(Configuration config)
        //{
        //    DataEvent_Data result = new DataEvent_Data();

        //    string where = "";

        //    // Get Past Week
        //    int days = 7;
        //    for (var x = 0; x <= days - 1; x++)
        //    {
        //        string shiftQuery = "Shift_Id LIKE '" + DateTime.Now.Subtract(TimeSpan.FromDays(x)).ToString("yyyyMMdd_") + "%'";
        //        if (x < days - 1) shiftQuery += " OR ";
        //        where += shiftQuery;
        //    }

        //    DataTable dt = Table.Get(config.Databases_Client, TableNames.OEE, "WHERE " + where);
        //    if (dt != null)
        //    {
        //        List<double> oees = new List<double>();

        //        foreach (DataRow row in dt.Rows)
        //        {
        //            string shiftId = row["SHIFT_ID"].ToString();

        //            string oee_str = row["OEE"].ToString();
        //            double oee = 0;
        //            double.TryParse(oee_str, out oee);

        //            oees.Add(oee);
        //        }

        //        ShiftInfo_Week_OEE(oees);
        //    }
        //}

        //void ShiftInfo_Day(DataTable dt)
        //{
        //    List<double> total_oees = new List<double>();

        //    foreach (DataRow row in dt.Rows)
        //    {
        //        string shiftId = row["SHIFT_ID"].ToString();

        //        string oee_str = row["OEE"].ToString();
        //        double oee = 0;
        //        double.TryParse(oee_str, out oee);

        //        total_oees.Add(oee);
        //    }

        //    var total_data = new ShiftInfo.Data();
        //    total_data.OEEAverage = Math.Round(total_oees.Average(), 2);

        //    //Day_ShiftInfo.SetData(total_data);



        //    //// Average
        //    //rowData = new VariableTable.RowData();
        //    //rowData.Variable = "Average OEE";
        //    //rowData.Value = Math.Round(oees.Average(), 2).ToString();
        //    //Day_ShiftInfo.LoadData(rowData);

        //    //// Median
        //    //rowData = new VariableTable.RowData();
        //    //rowData.Variable = "Median OEE";
        //    //rowData.Value = Math.Round(Math_Functions.GetMedian(oees.ToArray()), 2).ToString();
        //    //Day_ShiftInfo.LoadData(rowData);

        //    //// Standard Deviation
        //    //rowData = new VariableTable.RowData();
        //    //rowData.Variable = "Standard Deviation OEE";
        //    //rowData.Value = Math.Round(Math_Functions.StdDev(oees.ToArray()), 2).ToString();
        //    //Day_ShiftInfo.LoadData(rowData);

        //}

        //void ShiftInfo_Week_OEE(List<double> oees)
        //{
        //    Controls.VariableTable.RowData rowData;

        //    // Average
        //    rowData = new VariableTable.RowData();
        //    rowData.Variable = "Average OEE";
        //    rowData.Value = Math.Round(oees.Average(), 2).ToString();
        //    Week_ShiftInfo.LoadData(rowData);

        //    // Median
        //    rowData = new VariableTable.RowData();
        //    rowData.Variable = "Median OEE";
        //    rowData.Value = Math.Round(Math_Functions.GetMedian(oees.ToArray()), 2).ToString();
        //    Week_ShiftInfo.LoadData(rowData);

        //    // Standard Deviation
        //    rowData = new VariableTable.RowData();
        //    rowData.Variable = "Standard Deviation OEE";
        //    rowData.Value = Math.Round(Math_Functions.StdDev(oees.ToArray()), 2).ToString();
        //    Week_ShiftInfo.LoadData(rowData);

        //}

        #endregion

    }
}
