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


            GetOEE(config);

            graph1.Plot1.InvalidatePlot(true);
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

        void GetOEE(Configuration config)
        {
            DataEvent_Data result = new DataEvent_Data();

            string where = "";

            // Get Past Week
            int days = 7;
            for (var x = 0; x <= days - 1; x++)
            {
                string shiftQuery = "Shift_Id LIKE '" + DateTime.Now.Subtract(TimeSpan.FromDays(x)).ToString("yyyyMMdd_") + "%'";
                if (x < days - 1) shiftQuery += " OR ";
                where += shiftQuery;
            }


            //string shiftQuery = DateTime.Now.ToString("yyyyMMdd_");
            //DataTable dt = Table.Get(config.Databases_Client, TableNames.OEE, "WHERE Shift_Id LIKE '" + shiftQuery + "%'");

            DataTable dt = Table.Get(config.Databases_Client, TableNames.OEE, "WHERE " + where);
            if (dt != null)
            {
                var dataList = new List<Data>();

                foreach (DataRow row in dt.Rows)
                {
                    string shiftId = row["SHIFT_ID"].ToString();

                    string oee_str = row["OEE"].ToString();
                    double oee = 0;
                    double.TryParse(oee_str, out oee);

                    var data = new Data(); 
                    data.Category = shiftId;
                    data.Value = oee;
                    dataList.Add(data);
                }

                var color = ((SolidColorBrush)TryFindResource("Accent_Normal")).Color;

                graph1.viewModel.LoadData(dataList, color);

            }
        }

        #endregion

    }
}
