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
using System.Data;

using TH_Configuration;

namespace TrakHound_Server_Control_Panel.Pages
{
    /// <summary>
    /// Interaction logic for DescriptionConfiguration.xaml
    /// </summary>
    public partial class DescriptionConfiguration : UserControl
    {
        public DescriptionConfiguration()
        {
            InitializeComponent();
            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;
            //mw.SelectedDeviceChanged -= mw_SelectedDeviceChanged;
            //mw.SelectedDeviceChanged += mw_SelectedDeviceChanged;
        }

        private void TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Loading) SaveNeeded = true;
        }

        public TrakHound_Server_Control_Panel.MainWindow mw;

        DataTable ConfigurationTable;

        #region "Load"

        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(DescriptionConfiguration), new PropertyMetadata(false));

        void LoadConfiguration()
        {
            Loading = true;

            SaveNeeded = false;

            if (mw != null)
            {
                if (mw.ConfigurationTable != null)
                {
                    ConfigurationTable = mw.ConfigurationTable;
                    LoadSettings(ConfigurationTable);
                }
            }

            Loading = false;
        }

        void LoadSettings(DataTable dt)
        {

            string prefix = "/Description/";

            // Load Device Description
            devicedescription_TXT.Text = GetTableValue(prefix + "Description", dt);

            // Load Device Id
            deviceid_TXT.Text = GetTableValue(prefix + "Machine_ID", dt);

            // Load Manufacturer
            manufacturer_TXT.Text = GetTableValue(prefix + "Manufacturer", dt);




            //// Load IP Address
            //ipaddress_TXT.Text = GetTableValue(prefix + "IP_Address", dt);

            //// Load Port
            //port_TXT.Text = GetTableValue(prefix + "Port", dt);

            //// Load Device Name
            //devicename_TXT.Text = GetTableValue(prefix + "Device_Name", dt);

            //// Load Current Heartbeat
            //int currentHeartbeat;
            //if (int.TryParse(GetTableValue(prefix + "Current_Heartbeat", dt), out currentHeartbeat)) CurrentHeartbeat = currentHeartbeat;

            //// Load Sample Heartbeat
            //int sampleHeartbeat;
            //if (int.TryParse(GetTableValue(prefix + "Sample_Heartbeat", dt), out sampleHeartbeat)) SampleHeartbeat = sampleHeartbeat;

            //DeviceList.Clear();
        }

        static string GetTableValue(string address, DataTable dt)
        {
            string result = null;

            DataRow row = dt.Rows.Find(address);
            if (row != null)
            {
                result = row["value"].ToString();
            }

            return result;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfiguration();
        }

        void mw_SelectedDeviceChanged(Configuration config)
        {
            LoadConfiguration();
        }

        private void Restore_Clicked(Controls.Button bt)
        {
            LoadConfiguration();
        }

        #endregion

        #region "Save"

        public bool SaveNeeded
        {
            get { return (bool)GetValue(SaveNeededProperty); }
            set { SetValue(SaveNeededProperty, value); }
        }

        public static readonly DependencyProperty SaveNeededProperty =
            DependencyProperty.Register("SaveNeeded", typeof(bool), typeof(DescriptionConfiguration), new PropertyMetadata(false));

        void Save()
        {
            if (mw != null)
            {
                if (mw.ConfigurationTable != null)
                {
                    //DataTable dt = mw.ConfigurationTable;

                    //string prefix = "/Agent/";

                    //// Save IP Address
                    //UpdateTableValue(ipaddress_TXT.Text, prefix + "IP_Address", dt);

                    //// Save Port
                    //UpdateTableValue(port_TXT.Text, prefix + "Port", dt);

                    //// Save Device Name
                    //UpdateTableValue(devicename_TXT.Text, prefix + "Device_Name", dt);

                    //// Save Current Heartbeat
                    //UpdateTableValue(CurrentHeartbeat.ToString(), prefix + "Current_Heartbeat", dt);

                    //// Save Sample Heartbeat
                    //UpdateTableValue(SampleHeartbeat.ToString(), prefix + "Sample_Heartbeat", dt);

                    //mw.SaveConfiguration();
                    //SaveNeeded = false;
                }
            }
        }

        static void UpdateTableValue(string value, string address, DataTable dt)
        {
            DataRow row = dt.Rows.Find(address);
            if (row != null)
            {
                row["value"] = value;
            }
            else
            {
                row = dt.NewRow();
                row["address"] = address;
                row["value"] = value;
                dt.Rows.Add(row);
            }
        }

        private void Save_Clicked(Controls.Button bt)
        {
            Save();
        }

        #endregion

    }
}
