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

using System.Collections.ObjectModel;
using System.IO;
using System.Data;

using TH_Configuration;
using TH_Device_Client;
using TH_Global;
using TH_MySQL;
using TH_PlugIns_Client_Control;
using TH_Styles;

using TH_History.Controls;

namespace TH_History
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl, Control_PlugIn
    {
        public UserControl1()
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

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/datacenter-appinfo.txt"; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return null; } }
        public string DefaultParentCategory { get { return null; } }

        public bool AcceptsPlugIns { get { return true; } }

        public bool OpenOnStartUp { get { return true; } }

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Update(ReturnData rd)
        {
            
        }

        public void Closing() { }

        public void Show() { if (ShowRequested != null) ShowRequested(); }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {
            if (de_d.id.ToLower() == "deviceselected")
            {
                int index = (int)de_d.data;

                Configuration config = Devices[index].configuration;


                Controls.HistoryContainer container = new HistoryContainer();

                container.Info_GRID.Children.Add(new ShiftComparison(config));
                
                // Create Calendar
                Controls.Calendar.Calendar calendar = new Controls.Calendar.Calendar();
                calendar.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                container.Calendar_GRID.Children.Add(calendar);

                HistoryItems.Add(container);



            //    DataTable dt = Global.Table_Get(config.SQL, "shifts");

            //    GraphInfo info = new GraphInfo();
            //    info.Name = "Production Seconds";
            //    info.Table = dt;
            //    info.DataColumn = "Production_Status__Full_Production";
            //    info.CategoryColumn = "Segment";

            //    HistoryItem item = new HistoryItem(info);
            //    HistoryItems.Add(item);
            }
        }

        public event DataEvent_Handler DataEvent;

        public event PlugInTools.ShowRequested_Handler ShowRequested;

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

            }
        }

        #endregion

        #region "Options"

        public OptionsPage Options { get; set; }

        #endregion

        public object RootParent { get; set; }

        #endregion

        #region "History"

        private ObservableCollection<Control> historyitems;
        public ObservableCollection<Control> HistoryItems
        {
            get
            {
                if (historyitems == null)
                    historyitems = new ObservableCollection<Control>();

                return historyitems;
            }

            set
            {
                historyitems = value;
            }
        }

        #endregion

    }
}
