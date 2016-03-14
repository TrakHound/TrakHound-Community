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
    public partial class DeviceList
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

       


        //ObservableCollection<DeviceInfo> _devices;
        //public ObservableCollection<DeviceInfo> Devices
        //{
        //    get
        //    {
        //        if (_devices == null)
        //            _devices = new ObservableCollection<DeviceInfo>();
        //        return _devices;
        //    }

        //    set
        //    {
        //        _devices = value;
        //    }
        //}

        //ObservableCollection<DeviceInfo> _sharedDevices;
        //public ObservableCollection<DeviceInfo> SharedDevices
        //{
        //    get
        //    {
        //        if (_sharedDevices == null)
        //            _sharedDevices = new ObservableCollection<DeviceInfo>();
        //        return _sharedDevices;
        //    }

        //    set
        //    {
        //        _sharedDevices = value;
        //    }
        //}

        public List<Configuration> configurations;

        Thread loaddevices_THREAD;

        public void LoadDevices()
        {
            DevicesLoading = true;
            Devices.Clear();
            SharedDevices.Clear();

            // Remove all devices
            if (configurations != null)
            {
                foreach (var config in configurations)
                {
                    RemoveDevice(config);
                }
            }

            if (loaddevices_THREAD != null) loaddevices_THREAD.Abort();

            loaddevices_THREAD = new Thread(new ThreadStart(LoadDevices_Worker));
            loaddevices_THREAD.Start();

            if (LoadingDevices != null) LoadingDevices();
        }

        void LoadDevices_Worker()
        {
            List<Configuration> added = null;
            List<Configuration> shared = null;

            if (currentuser != null)
            {
                // Get Added Configurations
                added = LoadDevices_Added();

                shared = LoadDevices_Shared();

                this.Dispatcher.BeginInvoke(new Action<List<Configuration>, List<Configuration>>(LoadDevices_GUI), PRIORITY_BACKGROUND, new object[] { added, shared });
            }
            // If not logged in Read from File in 'C:\TrakHound\'
            else
            {
                added = Configuration.ReadAll(FileLocations.Devices).ToList();

                added = added.OrderBy(x => x.Index).ToList();

                // Reset order to be in intervals of 1000 in order to leave room in between for changes in index
                // This index model allows for devices to change index without having to update every device each time.
                for (var x = 0; x <= added.Count - 1; x++)
                {
                    added[x].Index = 1000 + (1000 * x);
                }

                foreach (var device in added) SaveFileConfiguration(device);

                this.Dispatcher.BeginInvoke(new Action<List<Configuration>, List<Configuration>>(LoadDevices_GUI), PRIORITY_BACKGROUND, new object[] { added, shared });
            }

            configurations = added.ToList();

            this.Dispatcher.BeginInvoke(new Action(LoadDevices_Finished), PRIORITY_BACKGROUND, new object[] { });
        }

        List<Configuration> LoadDevices_Added()
        {
            var result = Configurations.GetConfigurationsListForUser(currentuser);
            if (result != null)
            {
                result = result.OrderBy(x => x.Index).ToList();

                // Reset order to be in intervals of 1000 in order to leave room in between for changes in index
                // This index model allows for devices to change index without having to update every device each time.
                for (var x = 0; x <= result.Count - 1; x++)
                {
                    result[x].Index = 1000 + (1000 * x);
                }

                var indexItems = new List<Tuple<string, int>>();

                foreach (var config in result) indexItems.Add(new Tuple<string, int>(config.TableName, config.Index));

                Configurations.UpdateIndexes(indexItems);
            }

            return result;
        }

        List<Configuration> LoadDevices_Shared()
        {
            var result = Shared.GetOwnedSharedConfigurations(currentuser);
            if (result != null)
            {
                result = result.OrderBy(x => x.Description.Manufacturer).
                    ThenBy(x => x.Description.Controller).
                    ThenBy(x => x.Description.Model).ToList();
            }

            return result;
        }

        void LoadDevices_GUI(List<Configuration> added, List<Configuration> shared)
        {
            Devices.Clear();

            if (added != null)
            {
                foreach (var config in added)
                {
                    AddDevice(config);
                }
            }

            if (shared != null)
            {
                foreach (var config in shared)
                {
                    AddSharedDevice(config);
                }
            }
        }

        public void AddDevice(Configuration config)
        {
            var info = new DeviceInfo(config);
            Devices.Add(info);

            Devices.Sort();

            // Raise DeviceUpdated Event
            var args = new DeviceUpdateArgs();
            args.Event = DeviceUpdateEvent.Added;
            UpdateDevice(config, args);
        }

        public void AddSharedDevice(Configuration config)
        {
            var info = new DeviceInfo(config);
            SharedDevices.Add(info);

            SharedDevices.Sort();
        }

        public void RemoveDevice(Configuration config)
        {
            int index = Devices.ToList().FindIndex(x => x.UniqueId == config.UniqueId);
            if (index >= 0) Devices.RemoveAt(index);

            // Raise DeviceUpdated Event
            var args = new DeviceUpdateArgs();
            args.Event = DeviceUpdateEvent.Removed;
            UpdateDevice(config, args);
        }

        void LoadDevices_Finished()
        {
            DevicesLoading = false;

            //UpdateDeviceList();
        }

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
