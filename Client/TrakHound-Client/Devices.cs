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
using TH_UserManagement.Management;

namespace TrakHound_Client
{
    public partial class MainWindow
    {

        //public ObservableCollection<Configuration> Devices { get; set; }

        public List<Configuration> Devices { get; set; }

        #region "Load Devices"

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        Thread loaddevices_THREAD;

        void LoadDevices_Initialize()
        {
            Devices = new List<Configuration>();

            //Devices = new ObservableCollection<Configuration>();
            //Devices.CollectionChanged += Devices_CollectionChanged;
        }


        void LoadDevices()
        {
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
                List<Configuration> configurations = Configurations.GetConfigurationsListForUser(currentuser, UserDatabaseSettings);
                if (configurations != null)
                {
                    foreach (Configuration config in configurations)
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

        //void LoadDevices_Worker()
        //{
        //    System.Diagnostics.Stopwatch stpw = new System.Diagnostics.Stopwatch();
        //    stpw.Start();

        //    List<Configuration> configs = new List<Configuration>();

        //    if (currentuser != null)
        //    {
        //        string[] tablenames = Configurations.GetConfigurationsForUser(currentuser, UserDatabaseSettings);

        //        stpw.Stop();
        //        Console.WriteLine("LoadDevices_Worker() : GetConfigurationsForUser : " + stpw.ElapsedMilliseconds.ToString() + "ms");
        //        stpw.Restart();

        //        if (tablenames != null)
        //        {
        //            foreach (string tablename in tablenames)
        //            {
        //                Configurations.UpdateInfo info = Configurations.GetClientUpdateInfo(tablename, UserDatabaseSettings);

        //                stpw.Stop();
        //                Console.WriteLine("LoadDevices_Worker() : GetClientUpdateInfo : " + tablename + " : " + stpw.ElapsedMilliseconds.ToString() + "ms");
        //                stpw.Restart();

        //                if (info != null)
        //                {
        //                    bool enabled = false;
        //                    bool.TryParse(info.Enabled, out enabled);

        //                    if (enabled)
        //                    {
        //                        Configuration config = GetConfiguration(tablename);

        //                        stpw.Stop();
        //                        Console.WriteLine("LoadDevices_Worker() : GetConfiguration : " + tablename + " : " + stpw.ElapsedMilliseconds.ToString() + "ms");
        //                        stpw.Restart();

        //                        if (config != null) configs.Add(config);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    // If not logged in Read from File in 'C:\TrakHound\'
        //    else
        //    {
        //        configs = ReadConfigurationFile();
        //    }

        //    this.Dispatcher.BeginInvoke(new Action<List<Configuration>>(LoadDevices_Finished), priority, new object[] { configs });

        //    //this.Dispatcher.BeginInvoke(new Action(LoadDevices_Finished), priority, new object[] { });

        //    stpw.Stop();
        //    Console.WriteLine("LoadDevices_Worker() : " + stpw.ElapsedMilliseconds.ToString() + "ms");
        //}

        Configuration GetConfiguration(string tablename)
        {
            Configuration result = null;

            DataTable dt = Configurations.GetConfigurationTable(tablename, UserDatabaseSettings);
            if (dt != null)
            {
                XmlDocument xml = Converter.TableToXML(dt);
                Configuration config = Configuration.ReadConfigFile(xml);
                if (config != null)
                {
                    config.TableName = tablename;

                    // Initialize Database Configurations
                    Global.Initialize(config.Databases_Client);

                    result = config;
                }
            }

            return result;
        }

        //void LoadDevices_GUI(List<Configuration> configs)
        //{
        //    Devices = configs;
        //}

        void LoadDevices_Finished(List<Configuration> configs)
        {
            Devices = configs;

            if (Devices.Count == 0 && currentuser != null)
            {
                if (devicemanager != null) devicemanager.AddDevice();
                DeviceManager_Open();
            }

            UpdatePluginDevices(configs);

            DevicesMonitor_Initialize();

            // Send message to plugins that Devices have been loaded
            TH_Plugins_Client.DataEvent_Data de_d = new TH_Plugins_Client.DataEvent_Data();
            de_d.id = "devicesloaded";
            Plugin_DataEvent(de_d);
        }

        //List<Configuration> GetConfigurations()
        //{
        //    List<Configuration> result = new List<Configuration>();

        //    string[] tablenames = Configurations.GetConfigurationsForUser(currentuser, UserDatabaseSettings);

        //    if (tablenames != null)
        //    {
        //        foreach (string tablename in tablenames)
        //        {
        //            DataTable dt = Configurations.GetConfigurationTable(tablename, UserDatabaseSettings);
        //            if (dt != null)
        //            {
        //                XmlDocument xml = Converter.TableToXML(dt);
        //                Configuration config = Configuration.ReadConfigFile(xml);
        //                if (config != null)
        //                {
        //                    config.TableName = tablename;

        //                    if (config.ClientEnabled) result.Add(config);
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

        //void LoadDevices_GUI(List<Configuration> configs)
        //{
        //    Devices.Clear();

        //    if (configs != null)
        //    {
        //        int index = 0;

        //        DatabasePluginReader dpr = new DatabasePluginReader();

        //        // Create DevicesList based on Configurations
        //        foreach (Configuration config in configs)
        //        {
        //            config.Index = index;

        //            //if (config.Remote) { StartMonitor(config); }

        //            if (config.ClientEnabled)
        //            {
        //                Devices.Add(config);

        //                // Initialize Database Configurations
        //                Global.Initialize(config.Databases_Client);
        //            }

        //            index += 1;
        //        }
        //    }

        //    // If a user is logged in but no Devices are found then open up Device Manager and Add Device page
        //    if (CurrentUser != null && Devices.Count == 0)
        //    {
        //        if (devicemanager != null) devicemanager.AddDevice();

        //        DeviceManager_Open();
        //    }

        //    //UpdatePlugInDevices();

        //    DevicesMonitor_Initialize();
        //}


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
                    mData.title = "Device Configuration Error";
                    mData.text = "Could not load device configuration from " + configPath;
                    mData.additionalInfo = "Check to make sure the file exists at "
                        + configPath
                        + " and that the format is correct and restart TrakHound Client."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "For more information please contact us at info@TrakHound.org";
                    if (messageCenter != null) messageCenter.AddError(mData);
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

        #region "New Devices Monitor"

        Thread devicesmonitor_THREAD;
        ManualResetEvent monitorstop = null;

        void DevicesMonitor_Initialize()
        {
            monitorstop = new ManualResetEvent(false);

            if (devicesmonitor_THREAD != null) devicesmonitor_THREAD.Abort();

            devicesmonitor_THREAD = new Thread(new ThreadStart(DevicesMonitor_Start));
            devicesmonitor_THREAD.Start();
        }

        void DevicesMonitor_Start()
        {
            while (!monitorstop.WaitOne(0, true))
            {
                DevicesMonitor_Worker(Devices.ToList());

                Thread.Sleep(5000);
            }
        }

        void DevicesMonitor_Stop()
        {
            if (monitorstop != null) monitorstop.Set();
        }

        void DevicesMonitor_Worker(List<Configuration> devices)
        {
            if (currentuser != null)
            {
                bool changed = false;

                string[] tablenames = Configurations.GetConfigurationsForUser(currentuser, UserDatabaseSettings);
                if (tablenames != null) // Connected to database properly
                {
                    if (tablenames.Length > 0) // Has configurations
                    {
                        foreach (string tablename in tablenames)
                        {
                            if (tablename != null)
                            {
                                Configurations.UpdateInfo info = Configurations.GetClientUpdateInfo(tablename, UserDatabaseSettings);
                                if (info != null)
                                {
                                    bool enabled = false;
                                    bool.TryParse(info.Enabled, out enabled);

                                    int index = devices.FindIndex(x => x.UniqueId == info.UniqueId);
                                    if (index >= 0 && index <= Devices.Count - 1) // Device is already part of list
                                    {
                                        Configuration device = Devices[index];

                                        // Check if Device has changed
                                        if (device.ClientUpdateId != info.UpdateId)
                                        {
                                            changed = true;
                                            break;
                                        }
                                    }
                                    else // Device Added
                                    {
                                        if (enabled)
                                        {
                                            changed = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // Remove any server that was removed from user
                        foreach (Configuration device in devices.ToList())
                        {
                            string ExistingTable = tablenames.ToList().Find(x => x == device.TableName);
                            if (ExistingTable == null)
                            {
                                changed = true;
                                break;
                            }
                        }
                    }
                    else // No configurations found
                    {
                        changed = true;
                    }
                }

                this.Dispatcher.BeginInvoke(new Action<bool>(DevicesMonitor_Finished), priority, new object[] { changed });
            }
        }

        void DevicesMonitor_Finished(bool changed)
        {
            if (changed) LoadDevices();         
        }

        #endregion

        #region "Devices Monitor"

        //System.Timers.Timer devicesMonitor_TIMER;

        //void DevicesMonitor_Initialize()
        //{
        //    //if (devicesMonitor_TIMER != null) devicesMonitor_TIMER.Enabled = false;

        //    //devicesMonitor_TIMER = new System.Timers.Timer();
        //    //devicesMonitor_TIMER.Interval = 5000;
        //    //devicesMonitor_TIMER.Elapsed += devicesMonitor_TIMER_Elapsed;
        //    //devicesMonitor_TIMER.Enabled = true;
        //}

        //void devicesMonitor_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    ThreadPool.QueueUserWorkItem(new WaitCallback(DevicesMonitor_Worker), Devices.ToList());
        //}

        //Thread devicesMonitor_THREAD;

        //void DevicesMonitor_Start()
        //{
        //    if (devicesMonitor_THREAD != null) devicesMonitor_THREAD.Abort();

        //    devicesMonitor_THREAD = new Thread(new ParameterizedThreadStart(DevicesMonitor_Worker));
        //    devicesMonitor_THREAD.Start(Devices.ToList());
        //}

        //void DevicesMonitor_Worker(object o)
        //{
        //    bool changed = false;

        //    if (o != null)
        //    {
        //        List<Configuration> devs = (List<Configuration>)o;

        //        if (currentuser != null)
        //        {
        //            List<Configuration> userConfigs = GetConfigurations();
        //            if (userConfigs != null)
        //            {
        //                foreach (Configuration userConfig in userConfigs)
        //                {
        //                    if (userConfig != null)
        //                    {
        //                        Configuration match = devs.Find(x => x.UniqueId == userConfig.UniqueId);
        //                        if (match != null)
        //                        {
        //                            bool update = userConfig.ClientUpdateId == match.ClientUpdateId;
        //                            if (!update)
        //                            {
        //                                // Configuration has been updated / changed
        //                                changed = true;
        //                                break;
        //                            }
        //                        }
        //                        else if (userConfig.ClientEnabled)
        //                        {
        //                            // Configuration has been added or removed
        //                            changed = true;
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //            else if (devs.Count > 0) changed = true;
        //        }
        //    }

        //    this.Dispatcher.BeginInvoke(new Action<bool>(DevicesMonitor_Finished), priority, new object[] { changed });
        //}

        //void DevicesMonitor_Finished(bool changed)
        //{
        //    if (changed)
        //    {
        //        if (devicesMonitor_TIMER != null) devicesMonitor_TIMER.Enabled = false;

        //        LoadDevices();
        //    }
        //}

        #endregion


    }
}
