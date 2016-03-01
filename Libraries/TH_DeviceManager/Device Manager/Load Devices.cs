using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using TH_Configuration;
using TH_Global;
using TH_UserManagement.Management;

namespace TH_DeviceManager
{
    public partial class DeviceManager
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

        List<Configuration> configurations;

        
        Thread loaddevices_THREAD;

        public void LoadDevices()
        {
            CurrentPage = null;
            PageListShown = false;
            DevicesLoading = true;
            DeviceListShown = false;
            AddedDeviceList.Clear();
            SharedDeviceList.Clear();

            if (loaddevices_THREAD != null) loaddevices_THREAD.Abort();

            loaddevices_THREAD = new Thread(new ThreadStart(LoadDevices_Worker));
            loaddevices_THREAD.Start();

            if (LoadingDevices != null) LoadingDevices();
        }

        void LoadDevices_Worker()
        {
            var added = new List<Configuration>();
            var shared = new List<Configuration>();

            if (currentuser != null)
            {
                // Get Added Configurations
                added = Configurations.GetConfigurationsListForUser(currentuser);

                // Get shared configurations that are owned by the user
                shared = Shared.GetOwnedSharedConfigurations(currentuser);
            }
            // If not logged in Read from File in 'C:\TrakHound\'
            else
            {
                added = Configuration.ReadAll(FileLocations.Devices).ToList();
            }

            this.Dispatcher.BeginInvoke(new Action<List<Configuration>, List<Configuration>>(LoadDevices_GUI), background, new object[] { added, shared });
        }

        void LoadDevices_GUI(List<Configuration> added, List<Configuration> shared)
        {
            configurations = added;

            AddedDevices.Clear();
            SharedDevices.Clear();

            // Add the 'added' configurations to the list
            if (added != null)
            {
                var orderedAddedConfigs = added.OrderBy(x => x.Description.Manufacturer).ThenBy(x => x.Description.Description).ThenBy(x => x.Description.Device_ID);

                // Create DevicesList based on Configurations
                foreach (Configuration config in orderedAddedConfigs)
                {
                    AddedDevices.Add(config);
                    this.Dispatcher.BeginInvoke(new Action<Configuration>(AddDeviceButton), background, new object[] { config });
                }
            }

            // Add the owned configurations to the 'shared' list
            if (shared != null)
            {
                var orderedSharedConfigs = shared.OrderBy(x => x.Description.Manufacturer).ThenBy(x => x.Description.Description).ThenBy(x => x.Description.Device_ID);

                // Create DevicesList based on Configurations
                foreach (Configuration config in orderedSharedConfigs)
                {
                    SharedDevices.Add(config);
                    this.Dispatcher.BeginInvoke(new Action<Configuration>(AddSharedDeviceButton), background, new object[] { config });
                }
            }

            this.Dispatcher.BeginInvoke(new Action(LoadDevices_Finished), background, null);
        }

        void LoadDevices_Finished()
        {
            ShowAdded_RADIO.IsChecked = true;
            ShowAddedDevices();

            // Show Shared device list option if SharedDeviceList is not empty
            if (SharedDeviceList.Count > 0) DeviceListOptionsShown = true;
            else DeviceListOptionsShown = false;

            DeviceListShown = true;
            DevicesLoading = false;

            UpdateDeviceList();
        }

        void UpdateDeviceList()
        {
            // Raise DevicesLoaded event to update devices for rest of TrakHound 
            // (Client is the only one that uses this for now)
            if (DeviceListUpdated != null) DeviceListUpdated(AddedDevices);
        }

        void UpdateDevice(Configuration config, DeviceUpdateArgs args)
        {
            if (DeviceUpdated != null) DeviceUpdated(config, args);
        }

    }
}
