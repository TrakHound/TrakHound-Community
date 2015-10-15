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

using TH_Configuration;
using TH_Device_Client;
using TH_Global;
using TH_PlugIns_Client_Control;
using TH_WPF;

namespace TH_DeviceTable
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DeviceTable : UserControl, Control_PlugIn
    {
        public DeviceTable()
        {
            InitializeComponent();
            DataContext = this;
        }


        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Device Table"; } }

        public string Description { get { return "Compare Devices using a Table Format"; } }

        public ImageSource Image { get { return null; } }


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

        public bool AcceptsPlugIns { get { return true; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ShowInAppMenu { get { return true; } }

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Update(ReturnData rd)
        {
            //this.Dispatcher.BeginInvoke(new Action<ReturnData>(Update_GUI), Priority_Background, new object[] { rd });
        }

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

                DataTable dt = new DataTable();

                dt.Columns.Add("Device");
                dt.Columns.Add("Average OEE");
                dt.Columns.Add("Segment OEE");

                foreach (Device_Client device in lDevices)
                {
                    DataRow row = dt.NewRow();
                    row["Device"] = device.configuration.Description.Description;
                    row["Average OEE"] = "50.0%";
                    row["Segment OEE"] = "63.3%";
                    dt.Rows.Add(row);
                }

                TableData = dt.AsDataView();


                //DeviceDisplays = new List<DeviceDisplay>();

                //foreach (Device_Client device in Devices) CreateDeviceDisplay(device);
            }
        }

        #endregion

        #region "Options"

        public OptionsPage Options { get; set; }

        #endregion

        public object RootParent { get; set; }

        #endregion

        #region "Device Table"

        public DataView TableData
        {
            get { return (DataView)GetValue(TableDataProperty); }
            set { SetValue(TableDataProperty, value); }
        }

        public static readonly DependencyProperty TableDataProperty =
            DependencyProperty.Register("TableData", typeof(DataView), typeof(DeviceTable), new PropertyMetadata(null));


        #endregion



    }
}
