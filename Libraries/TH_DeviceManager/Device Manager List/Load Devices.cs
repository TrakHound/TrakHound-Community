using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Data;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement.Management;

namespace TH_DeviceManager
{
    public partial class DeviceManagerList
    {
        public delegate void DevicesStatus_Handler();
        public event DevicesStatus_Handler LoadingDevices;

        public delegate void DevicesLoaded_Handler(List<Configuration> configs);
        public event DevicesLoaded_Handler DeviceListUpdated;

        public enum DeviceUpdateEvent
        {
            Added,
            Changed,
            Removed
        }
        public class DeviceUpdateArgs
        {
            public DeviceUpdateEvent Event { get; set; }
        }
        public delegate void DeviceUpdated_Handler(Configuration config, DeviceUpdateArgs args);
        public event DeviceUpdated_Handler DeviceUpdated;

        public bool DevicesLoading
        {
            get { return (bool)GetValue(DevicesLoadingProperty); }
            set { SetValue(DevicesLoadingProperty, value); }
        }

        public static readonly DependencyProperty DevicesLoadingProperty =
            DependencyProperty.Register("DevicesLoading", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


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

        public List<Configuration> configurations;

        Thread loaddevices_THREAD;

        public void LoadDevices()
        {
            DevicesLoading = true;

            if (loaddevices_THREAD != null) loaddevices_THREAD.Abort();

            loaddevices_THREAD = new Thread(new ThreadStart(LoadDevices_Worker));
            loaddevices_THREAD.Start();

            if (LoadingDevices != null) LoadingDevices();
        }

        void LoadDevices_Worker()
        {
            List<Configuration> devices = null;

            if (currentuser != null)
            {
                // Get Added Configurations
                devices = Configurations.GetConfigurationsListForUser(currentuser);
                this.Dispatcher.BeginInvoke(new Action<List<Configuration>>(LoadDevices_GUI), PRIORITY_BACKGROUND, new object[] { devices });
            }
            // If not logged in Read from File in 'C:\TrakHound\'
            else
            {
                devices = Configuration.ReadAll(FileLocations.Devices).ToList();

                devices = devices.OrderBy(x => x.Index).ToList();

                for (var x = 0; x <= devices.Count - 1; x++)
                {
                    //if (x == 0) aboveIndex = 1000;
                    //else aboveIndex = devices[x - 1].Index;

                    //if (x < devices.Count - 1) belowIndex = devices[x + 1].Index;

                    //if (belowIndex < aboveIndex) belowIndex = aboveIndex + 2000;


                    //int index = devices[x].Index;

                    //if (index < aboveIndex || index > belowIndex)
                    //{
                    //    index = aboveIndex + ((belowIndex - aboveIndex) / 2);
                    //}

                    devices[x].Index = 1000 + (1000 * x);
                }

                foreach (var device in devices)
                {
                    Console.WriteLine(device.UniqueId + " :: " + device.Index.ToString());

                    Console.WriteLine(XML_Functions.SetInnerText(device.ConfigurationXML, "/Index", device.Index.ToString()));
                    SaveFileConfiguration(device);
                }

                this.Dispatcher.BeginInvoke(new Action<List<Configuration>>(LoadDevices_GUI), PRIORITY_BACKGROUND, new object[] { devices });
            }

            configurations = devices.ToList();

            this.Dispatcher.BeginInvoke(new Action(LoadDevices_Finished), PRIORITY_BACKGROUND, new object[] { });

            //if (currentuser != null)
            //{
            //    // Get Added Configurations
            //    var devices = Configurations.GetDeviceInfoList(currentuser);
            //    this.Dispatcher.BeginInvoke(new Action<List<DataTable>>(LoadDevices_GUI), PRIORITY_BACKGROUND, new object[] { devices });
            //}
            //// If not logged in Read from File in 'C:\TrakHound\'
            //else
            //{
            //    var devices = Configuration.ReadAll(FileLocations.Devices).ToList();
            //    this.Dispatcher.BeginInvoke(new Action<List<Configuration>>(LoadDevices_GUI), PRIORITY_BACKGROUND, new object[] { devices });
            //}
        }

        //void LoadDevices_GUI(List<DataTable> devices)
        //{
        //    Devices.Clear();

        //    if (devices != null)
        //    {
        //        foreach (var device in devices)
        //        {
        //            var deviceInfo = new DeviceInfo();
        //            deviceInfo.Description = DataTable_Functions.GetTableValue(device, "address", "/Description/Description", "value");
        //            deviceInfo.Manufacturer = DataTable_Functions.GetTableValue(device, "address", "/Description/Manufacturer", "value");
        //            deviceInfo.Model = DataTable_Functions.GetTableValue(device, "address", "/Description/Model", "value");
        //            deviceInfo.Serial = DataTable_Functions.GetTableValue(device, "address", "/Description/Serial", "value");
        //            deviceInfo.Id = DataTable_Functions.GetTableValue(device, "address", "/Description/Device_Id", "value");

        //            Devices.Add(deviceInfo);
        //        }
        //    }
        //}

        void LoadDevices_GUI(List<Configuration> devices)
        {
            Devices.Clear();

            if (devices != null)
            {
                foreach (var device in devices)
                {
                    AddDevice(device);
                }
            }
        }

        public void AddDevice(Configuration config)
        {
            var info = new DeviceInfo(config);
            Devices.Add(info);

            Devices.Sort();
        }

        public void RemoveDevice(Configuration config)
        {
            int index = Devices.ToList().FindIndex(x => x.UniqueId == config.UniqueId);
            if (index >= 0) Devices.RemoveAt(index);
        }

        void LoadDevices_Finished()
        {
            UpdateDeviceList();
        }

        //void LoadDevices_Worker()
        //{
        //    var added = new List<Configuration>();
        //    var shared = new List<Configuration>();

        //    if (currentuser != null)
        //    {
        //        // Get Added Configurations
        //        added = Configurations.GetConfigurationsListForUser(currentuser);

        //        // Get shared configurations that are owned by the user
        //        shared = Shared.GetOwnedSharedConfigurations(currentuser);
        //    }
        //    // If not logged in Read from File in 'C:\TrakHound\'
        //    else
        //    {
        //        added = Configuration.ReadAll(FileLocations.Devices).ToList();
        //    }

        //    this.Dispatcher.BeginInvoke(new Action<List<Configuration>, List<Configuration>>(LoadDevices_GUI), PRIORITY_BACKGROUND, new object[] { added, shared });
        //}

        //void LoadDevices_GUI(List<Configuration> added, List<Configuration> shared)
        //{
        //    configurations = added;

        //    // Add the 'added' configurations to the list
        //    if (added != null)
        //    {
        //        var orderedAddedConfigs = added.OrderBy(x => x.Description.Manufacturer).ThenBy(x => x.Description.Description).ThenBy(x => x.Description.Device_ID);

        //        // Create DevicesList based on Configurations
        //        foreach (Configuration config in orderedAddedConfigs)
        //        {
        //            AddedDevices.Add(config);
        //            this.Dispatcher.BeginInvoke(new Action<Configuration>(AddDeviceButton), PRIORITY_BACKGROUND, new object[] { config });
        //        }
        //    }

        //    // Add the owned configurations to the 'shared' list
        //    if (shared != null)
        //    {
        //        var orderedSharedConfigs = shared.OrderBy(x => x.Description.Manufacturer).ThenBy(x => x.Description.Description).ThenBy(x => x.Description.Device_ID);

        //        // Create DevicesList based on Configurations
        //        foreach (Configuration config in orderedSharedConfigs)
        //        {
        //            SharedDevices.Add(config);
        //            this.Dispatcher.BeginInvoke(new Action<Configuration>(AddSharedDeviceButton), PRIORITY_BACKGROUND, new object[] { config });
        //        }
        //    }

        //    this.Dispatcher.BeginInvoke(new Action(LoadDevices_Finished), PRIORITY_BACKGROUND, null);
        //}

        //void LoadDevices_Finished()
        //{
        //    ShowAdded_RADIO.IsChecked = true;
        //    ShowAddedDevices();

        //    // Show Shared device list option if SharedDeviceList is not empty
        //    if (SharedDeviceList.Count > 0) DeviceListOptionsShown = true;
        //    else DeviceListOptionsShown = false;

        //    DeviceListShown = true;
        //    DevicesLoading = false;

        //    UpdateDeviceList();
        //}

        void UpdateDeviceList()
        {
            // Raise DevicesLoaded event to update devices for rest of TrakHound 
            // (Client is the only one that uses this for now)
            if (DeviceListUpdated != null) DeviceListUpdated(configurations);
        }

        void UpdateDevice(Configuration config, DeviceUpdateArgs args)
        {
            if (DeviceUpdated != null) DeviceUpdated(config, args);
        }

    }
}
