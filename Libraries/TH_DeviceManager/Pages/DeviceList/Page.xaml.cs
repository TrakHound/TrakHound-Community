using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Threading;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement.Management;

namespace TH_DeviceManager.Pages.DeviceList
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                LoadDevices();
            }
        }

        ObservableCollection<DeviceInfo> _devices;
        public ObservableCollection<DeviceInfo> Devices
        {
            get
            {
                if (_devices == null)
                    _devices = new ObservableCollection<DeviceInfo>();
                return _devices;
            }

            set
            {
                _devices = value;
            }
        }

        /// <summary>
        /// Basic Device Information used to display in Device Manager Device Table
        /// </summary>
        public class DeviceInfo
        {
            public string Description { get; set; }
            public string Manufacturer { get; set; }
            public string Model { get; set; }
            public string Serial { get; set; }
            public string Id { get; set; }

            public bool ClientEnabled { get; set; }
            public bool ServerEnabled { get; set; }
        }

        #region "Load Devices"

        const System.Windows.Threading.DispatcherPriority background = System.Windows.Threading.DispatcherPriority.Background;

        public bool DevicesLoading
        {
            get { return (bool)GetValue(DevicesLoadingProperty); }
            set { SetValue(DevicesLoadingProperty, value); }
        }

        public static readonly DependencyProperty DevicesLoadingProperty =
            DependencyProperty.Register("DevicesLoading", typeof(bool), typeof(Page), new PropertyMetadata(false));


        Thread loaddevices_THREAD;

        public void LoadDevices()
        {
            if (loaddevices_THREAD != null) loaddevices_THREAD.Abort();

            loaddevices_THREAD = new Thread(new ThreadStart(LoadDevices_Worker));
            loaddevices_THREAD.Start();
        }

        void LoadDevices_Worker()
        {
            List<DataTable> devices = null;

            if (currentuser != null)
            {
                // Get Added Configurations
                devices = Configurations.GetDeviceInfoList(currentuser);
            }
            // If not logged in Read from File in 'C:\TrakHound\'
            else
            {
                //Configurations = ReadConfigurationFile();
            }

            this.Dispatcher.BeginInvoke(new Action<List<DataTable>>(LoadDevices_GUI), background, new object[] { devices });
        }

        void LoadDevices_GUI(List<DataTable> devices)
        {
            Devices.Clear();

            if (devices != null)
            {
                foreach (var device in devices)
                {
                    var deviceInfo = new DeviceInfo();
                    deviceInfo.Description = DataTable_Functions.GetTableValue(device, "address", "/Description/Description", "value");
                    deviceInfo.Manufacturer = DataTable_Functions.GetTableValue(device, "address", "/Description/Manufacturer", "value");
                    deviceInfo.Model = DataTable_Functions.GetTableValue(device, "address", "/Description/Model", "value");
                    deviceInfo.Serial = DataTable_Functions.GetTableValue(device, "address", "/Description/Serial", "value");
                    deviceInfo.Id = DataTable_Functions.GetTableValue(device, "address", "/Description/Device_Id", "value");



                    Devices.Add(deviceInfo);
                }
            }
            

            //configurations = added;

            //// Add the 'added' configurations to the list
            //if (added != null)
            //{
            //    var orderedAddedConfigs = added.OrderBy(x => x.Description.Manufacturer).ThenBy(x => x.Description.Description).ThenBy(x => x.Description.Device_ID);

            //    // Create DevicesList based on Configurations
            //    foreach (Configuration config in orderedAddedConfigs)
            //    {
            //        this.Dispatcher.BeginInvoke(new Action<Configuration>(AddDeviceButton), background, new object[] { config });
            //    }
            //}

            //// Add the owned configurations to the 'shared' list
            //if (shared != null)
            //{
            //    var orderedSharedConfigs = shared.OrderBy(x => x.Description.Manufacturer).ThenBy(x => x.Description.Description).ThenBy(x => x.Description.Device_ID);

            //    // Create DevicesList based on Configurations
            //    foreach (Configuration config in orderedSharedConfigs)
            //    {
            //        this.Dispatcher.BeginInvoke(new Action<Configuration>(AddSharedDeviceButton), background, new object[] { config });
            //    }
            //}

            //this.Dispatcher.BeginInvoke(new Action(LoadDevices_Finished), background, null);
        }

        void LoadDevices_Finished()
        {
            //ShowAdded_RADIO.IsChecked = true;
            //ShowAddedDevices();

            //// Show Shared device list option if SharedDeviceList is not empty
            //if (SharedDeviceList.Count > 0) DeviceListOptionsShown = true;
            //else DeviceListOptionsShown = false;

            //DeviceListShown = true;
            //DevicesLoading = false;
        }

        #endregion

    }
}
