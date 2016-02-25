using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;
using System.Threading;
using System.Data;
using System.Xml;
using System.IO;

using TH_Configuration;
using TH_Database;
using TH_DeviceManager;
using TH_UserManagement.Management;

namespace TrakHound_Client
{
    public partial class MainWindow
    {

        public List<Configuration> Devices { get; set; }

        private void Devicemanager_DeviceListUpdated(List<Configuration> configs)
        {
            this.Dispatcher.BeginInvoke(new Action<List<Configuration>>(UpdateDeviceList), priority, new object[] { configs });
        }

        private void Devicemanager_DeviceUpdated(Configuration config, DeviceManager.DeviceUpdateArgs args)
        {
            switch (args.Event)
            {
                case DeviceManager.DeviceUpdateEvent.Added:
                    AddDevice(config);
                    break;

                case DeviceManager.DeviceUpdateEvent.Changed:
                    UpdateDevice(config);
                    break;

                case DeviceManager.DeviceUpdateEvent.Removed:
                    RemoveDevice(config);
                    break;
            }
        }

        private void Devicemanager_LoadingDevices()
        {
            // Send message to plugins that Devices are being loaded
            TH_Plugins_Client.DataEvent_Data de_d = new TH_Plugins_Client.DataEvent_Data();
            de_d.id = "LoadingDevices";
            Plugin_DataEvent(de_d);
        }


        /// <summary>
        /// Method that loads devices from DeviceManager's DeviceLoaded event
        /// </summary>
        /// <param name="configs"></param>
        private void UpdateDeviceList(List<Configuration> configs)
        {
            var enabledConfigs = new List<Configuration>();

            var orderedConfigs = configs.OrderBy(x => x.Description.Manufacturer).ThenBy(x => x.Description.Description).ThenBy(x => x.Description.Device_ID);

            foreach (Configuration config in orderedConfigs)
            {
                if (config.ClientEnabled)
                {
                    Global.Initialize(config.Databases_Client);
                    enabledConfigs.Add(config);
                }
            }

            LoadDevices_Finished(enabledConfigs);
        }

        /// <summary>
        /// Device Manager Added a device so add this device to Devices
        /// </summary>
        /// <param name="config"></param>
        private void AddDevice(Configuration config)
        {
            Console.WriteLine("AddDevice() :: " + config.Description.Description);

            if (Devices.Exists(x => x.UniqueId == config.UniqueId)) Devices.Add(config);
        }

        /// <summary>
        /// Device Manager Updated a device so add this device to Devices
        /// </summary>
        /// <param name="config"></param>
        private void UpdateDevice(Configuration config)
        {
            Console.WriteLine("UpdateDevice() :: " + config.Description.Description);

            int index = Devices.FindIndex(x => x.UniqueId == config.UniqueId);
            if (index >= 0)
            {
                Devices.RemoveAt(index);
                Devices.Insert(index, config);
            }
        }

        /// <summary>
        /// Device Manager Removed a device so add this device to Devices
        /// </summary>
        /// <param name="config"></param>
        private void RemoveDevice(Configuration config)
        {
            Console.WriteLine("RemoveDevice() :: " + config.Description.Description);

            var match = Devices.Find(x => x.UniqueId == config.UniqueId);
            if (match != null) Devices.Remove(match);
        }







        #region "Load Devices"

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        Thread loaddevices_THREAD;

        void LoadDevices_Initialize()
        {
            Devices = new List<Configuration>();
        }

        

        void LoadDevices()
        {
            DevicesMonitor_Close(true);
            Devices.Clear();

            // Send message to plugins that Devices are being loaded
            TH_Plugins_Client.DataEvent_Data de_d = new TH_Plugins_Client.DataEvent_Data();
            de_d.id = "LoadingDevices";
            Plugin_DataEvent(de_d);

            if (loaddevices_THREAD != null) loaddevices_THREAD.Abort();

            loaddevices_THREAD = new Thread(new ThreadStart(LoadDevices_Worker));
            loaddevices_THREAD.Start();
        }

        void LoadDevices_Worker()
        {
            List<Configuration> configs = new List<Configuration>();

            if (currentuser != null)
            {
                var stpw = new System.Diagnostics.Stopwatch();
                stpw.Start();

                List<Configuration> configurations = Configurations.GetConfigurationsListForUser(currentuser);

                stpw.Stop();
                Console.WriteLine("LoadDevices_Worker() :: " + stpw.ElapsedMilliseconds + "ms");

                if (configurations != null)
                {
                    var orderedConfigs = configurations.OrderBy(x => x.Description.Manufacturer).ThenBy(x => x.Description.Description).ThenBy(x => x.Description.Device_ID);

                    foreach (Configuration config in orderedConfigs)
                    {
                        if (config.ClientEnabled)
                        {
                            Global.Initialize(config.Databases_Client);
                            configs.Add(config);
                        }
                    }
                }


            }
            // If not logged in Read from File in 'C:\TrakHound\'
            else
            {
                configs = ReadConfigurationFile();
            }

            this.Dispatcher.BeginInvoke(new Action<List<Configuration>>(LoadDevices_Finished), priority, new object[] { configs });
        }

        bool addDeviceOpened = false;

        void LoadDevices_Finished(List<Configuration> configs)
        {
            Devices = configs;

            if (!addDeviceOpened && configs.Count == 0 && currentuser != null)
            {
                addDeviceOpened = true;
                //if (devicemanager != null) devicemanager.AddDevice();
                DeviceManager_Open();
            }
            else if (configs.Count > 0)
            {
                addDeviceOpened = false;
            }

            Plugins_UpdateDeviceList(configs);

            //DevicesMonitor_Initialize();

            // Send message to plugins that Devices have been loaded
            TH_Plugins_Client.DataEvent_Data de_d = new TH_Plugins_Client.DataEvent_Data();
            de_d.id = "devicesloaded";
            Plugin_DataEvent(de_d);
        }

        #region "Offline Configurations"

        List<Configuration> ReadConfigurationFile()
        {
            List<Configuration> result = new List<Configuration>();

            //UpdateExceptionsThrown = new List<string>();

            string configPath;

            string localPath = AppDomain.CurrentDomain.BaseDirectory + @"\" + "Configuration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "Configuration.Xml";

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            if (System.IO.File.Exists(configPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(configPath);

                foreach (XmlNode Node in doc.DocumentElement.ChildNodes)
                {
                    if (Node.NodeType == XmlNodeType.Element)
                    {
                        switch (Node.Name.ToLower())
                        {
                            case "devices":
                                foreach (XmlNode ChildNode in Node.ChildNodes)
                                {
                                    if (ChildNode.NodeType == XmlNodeType.Element)
                                    {
                                        switch (ChildNode.Name.ToLower())
                                        {
                                            case "device":

                                                Configuration config = GetSettingsFromNode(ChildNode);
                                                if (config != null) result.Add(config);

                                                break;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            return result;
        }

        private Configuration GetSettingsFromNode(XmlNode Node)
        {

            Configuration Result = null;

            string configPath = null;

            foreach (XmlNode ChildNode in Node.ChildNodes)
            {
                switch (ChildNode.Name.ToLower())
                {
                    case "configuration_path": configPath = ChildNode.InnerText; break;
                }
            }

            if (configPath != null)
            {
                configPath = GetConfigurationPath(configPath);

                Result = Configuration.ReadConfigFile(configPath);

                if (Result == null)
                {
                    Controls.Message_Center.Message_Data mData = new Controls.Message_Center.Message_Data();
                    mData.Title = "Device Configuration Error";
                    mData.Text = "Could not load device configuration from " + configPath;
                    mData.AdditionalInfo = "Check to make sure the file exists at "
                        + configPath
                        + " and that the format is correct and restart TrakHound Client."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "For more information please contact us at info@TrakHound.org";
                    if (messageCenter != null) messageCenter.AddMessage(mData);
                }
            }

            return Result;

        }

        static string GetConfigurationPath(string path)
        {
            // If not full path, try System Dir ('C:\TrakHound\') and then local App Dir
            if (!System.IO.Path.IsPathRooted(path))
            {
                // Remove initial Backslash if contained in "configuration_path"
                if (path[0] == '\\' && path.Length > 1) path.Substring(1);

                string original = path;

                // Check System Path
                path = TH_Global.FileLocations.TrakHound + "\\Configuration Files\\" + original;
                if (File.Exists(path)) return path;

                // Check local app Path
                path = AppDomain.CurrentDomain.BaseDirectory + "Configuration Files\\" + original;
                if (File.Exists(path)) return path;

                // if no files exist return null
                return null;
            }
            else return path;
        }

        #endregion

        #endregion

        #region "Devices Monitor"

        Thread devicesmonitor_THREAD;
        ManualResetEvent stop = null;

        void DevicesMonitor_Initialize()
        {
            stop = new ManualResetEvent(false);

            if (devicesmonitor_THREAD != null) devicesmonitor_THREAD.Abort();

            devicesmonitor_THREAD = new Thread(new ThreadStart(DevicesMonitor_Start));
            devicesmonitor_THREAD.Start();
        }

        void DevicesMonitor_Start()
        {
            while (!stop.WaitOne(0, true))

            {
                bool changed = DevicesMonitor_Worker(Devices.ToList());

                if (changed)
                {
                    this.Dispatcher.BeginInvoke(new Action<bool>(DevicesMonitor_Finished), priority, new object[] { changed });
                    break;
                }
                else
                {
                    if (!stop.WaitOne(0, true)) Thread.Sleep(5000);
                }
            }
            devicesmonitor_THREAD = null;
        }

        void DevicesMonitor_Stop()
        {
            if (stop != null) stop.Set();
        }

        void DevicesMonitor_Close()
        {
            DevicesMonitor_Stop();
            //if (devicesmonitor_THREAD != null) devicesmonitor_THREAD.Abort();
        }

        void DevicesMonitor_Close(bool wait)
        {
            DevicesMonitor_Stop();
            //if (devicesmonitor_THREAD != null)
            //{
            //    if (wait) devicesmonitor_THREAD.Join(5000);
            //    devicesmonitor_THREAD.Abort();
            //}
        }

        bool DevicesMonitor_Worker(List<Configuration> devices)
        {
            bool changed = false;

            if (currentuser != null)
            {
                var configs = Configurations.GetConfigurationsListForUser(currentuser);
                if (configs != null)
                {
                    foreach (var config in configs)
                    {
                        int index = devices.FindIndex(x => x.UniqueId == config.UniqueId);
                        if (index >= 0 && index <= Devices.Count - 1) // Device is already part of list
                        {
                            Configuration device = Devices[index];

                            // Check if Device has changed
                            if (device.ClientUpdateId != config.ClientUpdateId)
                            {
                                changed = true;
                                break;
                            }
                        }
                        else // Device Added
                        {
                            if (config.ClientEnabled)
                            {
                                changed = true;
                                break;
                            }
                        }
                    }
                }
            }

            return changed;
        }

        void DevicesMonitor_Finished(bool changed)
        {
            if (changed) LoadDevices();
        }

        #endregion

    }
}
