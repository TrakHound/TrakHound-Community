using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Data;
using System.Xml;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Server;
using TH_UserManagement.Management;
using TH_WPF;

using TH_DeviceManager.Controls;

namespace TH_DeviceManager
{
    public partial class DeviceManager
    {

        Configuration selecteddevice;
        public Configuration SelectedDevice
        {
            get { return selecteddevice; }
            set
            {
                selecteddevice = value;
            }
        }

        DeviceButton selecteddevicebutton;
        public DeviceButton SelectedDeviceButton
        {
            get { return selecteddevicebutton; }
            set
            {
                selecteddevicebutton = value;
            }
        }

        DataTable configurationtable;
        public DataTable ConfigurationTable
        {
            get { return configurationtable; }
            set
            {
                configurationtable = value;
            }
        }

        List<Configuration> _addedDevices;
        List<Configuration> AddedDevices
        {
            get
            {
                if (_addedDevices == null) _addedDevices = new List<Configuration>();
                return _addedDevices;
            }
            set
            {
                _addedDevices = value;
            }
        }

        List<Configuration> _sharedDevices;
        List<Configuration> SharedDevices
        {
            get
            {
                if (_sharedDevices == null) _sharedDevices = new List<Configuration>();
                return _sharedDevices;
            }
            set
            {
                _sharedDevices = value;
            }
        }

        #region "Remove Device"

        class RemoveDevice_Info
        {
            public DeviceButton bt { get; set; }
            public bool success { get; set; }
        }

        void RemoveDevice(DeviceButton bt)
        {
            bool? result = TH_WPF.MessageBox.Show("Are you sure you want to permanently remove this device?", "Remove Device", TH_WPF.MessageBoxButtons.YesNo);
            if (result == true)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(RemoveDevice_Worker), bt);
            }
        }

        void RemoveDevice_Worker(object o)
        {
            RemoveDevice_Info info = new RemoveDevice_Info();

            if (o != null)
            {
                DeviceButton bt = (DeviceButton)o;

                info.bt = bt;

                if (bt.Config != null)
                {
                    if (bt.Config.TableName != null)
                    {
                        info.success = Configurations.RemoveConfigurationTable(bt.Config.TableName);
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action<RemoveDevice_Info>(RemoveDevice_Finshed), priority, new object[] { info });
        }

        void RemoveDevice_Finshed(RemoveDevice_Info info)
        {
            if (info.success)
            {
                if (info.bt != null)
                {
                    DeviceButton bt = info.bt;

                    if (bt.Config != null)
                    {
                        if (SelectedDevice != null && SelectedDevice.UniqueId == bt.Config.UniqueId)
                        {
                            SelectedDevice = null;
                            CurrentPage = null;
                            PageListShown = false;
                        }

                        // Raise DeviceUpdated Event
                        var args = new DeviceUpdateArgs();
                        args.Event = DeviceUpdateEvent.Removed;
                        UpdateDevice(bt.Config, args);
                    }

                    if (bt.Parent != null)
                    {
                        if (bt.Parent.GetType() == typeof(ListButton))
                        {
                            ListButton lb = (ListButton)bt.Parent;

                            if (DeviceList.Contains(lb)) DeviceList.Remove(lb);
                        }
                    }
                }
            }
            else
            {
                TH_WPF.MessageBox.Show("An error occured while attempting to Remove Device. Please try again.", "Remove Device Error", MessageBoxButtons.Ok);
                LoadDevices();
            }
        }

        #endregion

        #region "Add Device"

        private void AddDevice_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (TH_WPF.ListButton lb in DeviceList.OfType<TH_WPF.ListButton>()) lb.IsSelected = false;

            SelectedDevice = null;
            selectedPageIndex = 0;

            AddDevice();
        }

        Pages.AddDevice.Page addPage;

        void AddDevice_Initialize()
        {
            addPage = new Pages.AddDevice.Page();
            addPage.deviceManager = this;
            addPage.DeviceAdded += page_DeviceAdded;
            addPage.currentuser = CurrentUser;
            addPage.LoadCatalog();
        }

        public void AddDevice()
        {
            PageListShown = false;
            ToolbarShown = false;

            if (CurrentPage != null)
            {
                if (CurrentPage.GetType() != typeof(Pages.AddDevice.Page))
                {
                    CurrentPage = addPage;
                }
            }
            else CurrentPage = addPage;
        }

        void page_DeviceAdded(Configuration config)
        {
            AddDeviceButton(config);
            ShowAddedDevices();

            // Raise DeviceUpdated Event
            var args = new DeviceUpdateArgs();
            args.Event = DeviceUpdateEvent.Added;
            UpdateDevice(config, args);
        }

        #endregion

        #region "Copy Device"

        Pages.CopyDevice.Page copyPage;

        void CopyDevice_Initialize()
        {
            copyPage = new Pages.CopyDevice.Page();
            //copyPage.deviceManager = this;
            //copyPage.DeviceAdded += page_DeviceAdded;
            copyPage.currentuser = CurrentUser;
            //copyPage.LoadCatalog();
        }

        public void CopyDevice(Configuration config)
        {
            PageListShown = false;
            ToolbarShown = false;

            copyPage.LoadConfiguration(config);

            if (CurrentPage != null)
            {
                if (CurrentPage.GetType() != typeof(Pages.CopyDevice.Page))
                {
                    CurrentPage = copyPage;
                }
            }
            else CurrentPage = copyPage;
        }

        //Thread CopyDevice_THREAD;

        //void CopyDevice(Configuration config)
        //{
        //    bool? result = TH_WPF.MessageBox.Show("Create a copy of this device?", "Copy Device", TH_WPF.MessageBoxButtons.YesNo);
        //    if (result == true)
        //    {
        //        if (CopyDevice_THREAD != null) CopyDevice_THREAD.Abort();

        //        CopyDevice_THREAD = new Thread(new ParameterizedThreadStart(CopyDevice_Worker));
        //        CopyDevice_THREAD.Start(config);
        //    }
        //}

        //void CopyDevice_Worker(object o)
        //{
        //    bool success = false;

        //    if (o != null)
        //    {
        //        Configuration config = (Configuration)o;

        //        if (currentuser != null)
        //        {
        //            success = Configurations.AddConfigurationToUser(currentuser, config, userDatabaseSettings);
        //        }
        //        else
        //        {
        //            success = false;
        //        }

        //        this.Dispatcher.BeginInvoke(new Action<bool, Configuration>(CopyDevice_GUI), priority, new object[] { success, config });
        //    }
        //}

        //void CopyDevice_GUI(bool success, Configuration config)
        //{
        //    if (success) AddDeviceButton(config);
        //    else
        //    {
        //        TH_WPF.MessageBox.Show("Error during Device Copy. Please try again", "Device Copy Error", MessageBoxButtons.Ok);
        //    }
        //}

        #endregion

        private bool ResetUpdateId(Configuration config)
        {
            bool result = false;

            if (ManagerType == DeviceManagerType.Client)
            {
                var updateId = String_Functions.RandomString(20);

                if (currentuser != null) result = Configurations.UpdateConfigurationTable("/ClientUpdateId", updateId, config.TableName);
                else result = true;

                if (result)
                {
                    config.ClientUpdateId = updateId;
                    XML_Functions.SetInnerText(config.ConfigurationXML, "ClientUpdateId", updateId);
                }
            }
            else if (ManagerType == DeviceManagerType.Server)
            {
                var updateId = String_Functions.RandomString(20);

                if (currentuser != null) result = Configurations.UpdateConfigurationTable("/ServerUpdateId", updateId, config.TableName);
                else result = true;

                if (result)
                {
                    config.ClientUpdateId = updateId;
                    XML_Functions.SetInnerText(config.ConfigurationXML, "ServerUpdateId", updateId);
                }
            }

            return result;
        } 

        private bool UpdateEnabledXML(XmlDocument xml, bool enabled)
        {
            bool result = false;

            if (ManagerType == DeviceManagerType.Client)
            {
                result = XML_Functions.SetInnerText(xml, "ClientEnabled", enabled.ToString());
            }
            else if (ManagerType == DeviceManagerType.Server)
            {
                result = XML_Functions.SetInnerText(xml, "ServerEnabled", enabled.ToString());
            }

            return result;
        }

        #region "Enable Device"

        class EnableDevice_Info
        {
            public DeviceButton bt { get; set; }
            //public string tablename { get; set; }
            public bool success { get; set; }
        }

        void EnableDevice(DeviceButton bt)
        {
            bt.EnableLoading = true;

            EnableDevice_Info info = new EnableDevice_Info();
            info.bt = bt;

            if (info.bt.Config != null)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(EnableDevice_Worker), info);
            }
        }

        void EnableDevice_Worker(object o)
        {
            if (o != null)
            {
                EnableDevice_Info info = (EnableDevice_Info)o;

                // Save Changes
                if (currentuser != null)
                {
                    string tablename = info.bt.Config.TableName;
                    if (tablename != null)
                    {
                        if (ManagerType == DeviceManagerType.Client) info.success = Configurations.UpdateConfigurationTable("/ClientEnabled", "True", tablename);
                        else if (ManagerType == DeviceManagerType.Server) info.success = Configurations.UpdateConfigurationTable("/ServerEnabled", "True", tablename);
                    }

                    if (info.success) info.success = UpdateEnabledXML(info.bt.Config.ConfigurationXML, true);
                    if (info.success) info.success = ResetUpdateId(info.bt.Config);
                }
                else
                {
                    info.success = UpdateEnabledXML(info.bt.Config.ConfigurationXML, true);
                    if (info.success) info.success = ResetUpdateId(info.bt.Config);
                    if (info.success) info.success = SaveFileConfiguration(info.bt.Config);
                }

                // If changes were successful, then update DeviceManager's Congifuration
                if (info.success)
                {
                    if (ManagerType == DeviceManagerType.Client) info.bt.Config.ClientEnabled = true;
                    else if (ManagerType == DeviceManagerType.Server) info.bt.Config.ServerEnabled = true;
                }

                this.Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(EnableDevice_Finished), priority, new object[] { info });
            }
        }

        void EnableDevice_Finished(EnableDevice_Info info)
        {
            if (info.bt != null)
            {
                if (info.success)
                {
                    // Raise DeviceUpdated Event
                    var args = new DeviceUpdateArgs();
                    args.Event = DeviceUpdateEvent.Added;
                    UpdateDevice(info.bt.Config, args);

                    info.bt.DeviceEnabled = true;
                }

                info.bt.EnableLoading = false;
            }
        }

        #endregion

        #region "Disable Device"

        void DisableDevice(DeviceButton bt)
        {
            bt.EnableLoading = true;

            EnableDevice_Info info = new EnableDevice_Info();
            info.bt = bt;

            if (info.bt.Config != null)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(DisableDevice_Worker), info);
            }
        }

        void DisableDevice_Worker(object o)
        {
            if (o != null)
            {
                EnableDevice_Info info = (EnableDevice_Info)o;

                // Save Changes
                if (currentuser != null)
                {
                    string tablename = info.bt.Config.TableName;
                    if (tablename != null)
                    {
                        if (ManagerType == DeviceManagerType.Client) info.success = Configurations.UpdateConfigurationTable("/ClientEnabled", "False", tablename);
                        else if (ManagerType == DeviceManagerType.Server) info.success = Configurations.UpdateConfigurationTable("/ServerEnabled", "False", tablename);
                    }

                    if (info.success) info.success = UpdateEnabledXML(info.bt.Config.ConfigurationXML, false);
                    if (info.success) info.success = ResetUpdateId(info.bt.Config);
                }
                else
                {
                    info.success = UpdateEnabledXML(info.bt.Config.ConfigurationXML, false);
                    if (info.success) info.success = ResetUpdateId(info.bt.Config);
                    if (info.success) info.success = SaveFileConfiguration(info.bt.Config);
                }

                // If changes were successful, then update DeviceManager's Congifuration
                if (info.success)
                {
                    if (ManagerType == DeviceManagerType.Client) info.bt.Config.ClientEnabled = false;
                    else if (ManagerType == DeviceManagerType.Server) info.bt.Config.ServerEnabled = false;
                }

                this.Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(DisableDevice_Finished), priority, new object[] { info });
            }
        }

        void DisableDevice_Finished(EnableDevice_Info info)
        {
            if (info.bt != null)
            {
                if (info.success)
                {
                    // Raise DeviceUpdated Event
                    var args = new DeviceUpdateArgs();
                    args.Event = DeviceUpdateEvent.Removed;
                    UpdateDevice(info.bt.Config, args);

                    info.bt.DeviceEnabled = false;
                }

                info.bt.EnableLoading = false;
            }
        }

        //void DisableDevice_Worker(object o)
        //{
        //    if (o != null)
        //    {
        //        EnableDevice_Info info = (EnableDevice_Info)o;

        //        if (ManagerType == DeviceManagerType.Client) info.success = XML_Functions.SetInnerText(info.bt.Config.ConfigurationXML, "ClientEnabled", "False");
        //        else if (ManagerType == DeviceManagerType.Server) info.success = XML_Functions.SetInnerText(info.bt.Config.ConfigurationXML, "ServerEnabled", "False");

        //        if (info.success && currentuser != null)
        //        {
        //            string tablename = info.bt.Config.TableName;
        //            if (tablename != null)
        //            {
        //                if (ManagerType == DeviceManagerType.Client) info.success = Configurations.UpdateConfigurationTable("/ClientEnabled", "False", tablename);
        //                else if (ManagerType == DeviceManagerType.Server) info.success = Configurations.UpdateConfigurationTable("/ServerEnabled", "False", tablename);
        //            }
        //        }

        //        Reset Update ID
        //        if (info.success) info.success = ResetUpdateId(info.bt.Config);

        //        If using a local file, save it
        //        if (info.success && currentuser == null) info.success = SaveFileConfiguration(info.bt.Config);

        //        this.Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(DisableDevice_Finished), priority, new object[] { info });
        //    }
        //}

        //void DisableDevice_Finished(EnableDevice_Info info)
        //{
        //    if (info.bt != null)
        //    {
        //        if (info.success && info.bt != null)
        //        {
        //            if (ManagerType == DeviceManagerType.Client)
        //            {
        //                info.bt.Config.ClientEnabled = false;
        //                XML_Functions.SetInnerText(info.bt.Config.ConfigurationXML, "ClientEnabled", "false");

        //                Raise DeviceUpdated Event
        //               var args = new DeviceUpdateArgs();
        //                args.Event = DeviceUpdateEvent.Removed;
        //                UpdateDevice(info.bt.Config, args);
        //            }
        //            else if (ManagerType == DeviceManagerType.Server)
        //            {
        //                info.bt.Config.ServerEnabled = false;
        //                XML_Functions.SetInnerText(info.bt.Config.ConfigurationXML, "ServerEnabled", "false");
        //            }
        //            info.bt.DeviceEnabled = false;
        //        }

        //        info.bt.EnableLoading = false;
        //    }
        //}

        #endregion

        #region "Select Device"

        public bool DeviceLoading
        {
            get { return (bool)GetValue(DeviceLoadingProperty); }
            set { SetValue(DeviceLoadingProperty, value); }
        }

        public static readonly DependencyProperty DeviceLoadingProperty =
            DependencyProperty.Register("DeviceLoading", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        Thread selectDevice_THREAD;

        void lb_Device_Selected(TH_WPF.ListButton lb)
        {
            Controls.DeviceButton db = (Controls.DeviceButton)lb.ButtonContent;
            if (db != null)
            {
                if (db.Config != null)
                {
                    if (SelectedDevice != db.Config)
                    {
                        SelectedDevice = db.Config;

                        SelectDevice(db.Config);
                    }
                }
            }

            foreach (TH_WPF.ListButton olb in DeviceList.OfType<TH_WPF.ListButton>()) if (olb != lb) olb.IsSelected = false;
            lb.IsSelected = true;
        }

        void SelectDevice(Configuration config)
        {
            //if (SaveNeeded)
            //{
            //    bool? save = TH_WPF.MessageBox.Show("Do you want to Save changes?", "Save Changed", MessageBoxButtons.YesNo);
            //    if (save == true)
            //    {

            //    }
            //}

            if (config != null)
            {
                DeviceLoading = true;

                if (selectDevice_THREAD != null) selectDevice_THREAD.Abort();

                selectDevice_THREAD = new Thread(new ParameterizedThreadStart(SelectDevice_Worker));
                selectDevice_THREAD.Start(config);
            }
        }

        void SelectDevice_Worker(object o)
        {
            Configuration config = (Configuration)o;

            DataTable dt = TH_Configuration.Converter.XMLToTable(config.ConfigurationXML);
            if (dt != null)
            {
                dt.TableName = config.TableName;

                if (ConfigurationPages != null)
                {
                    foreach (ConfigurationPage page in ConfigurationPages)
                    {
                        this.Dispatcher.BeginInvoke(new Action<DataTable>(page.LoadConfiguration), contextidle, new object[] { dt });
                        //this.Dispatcher.BeginInvoke(new Action<DataTable, ConfigurationPage>(SelectDevice_GUI), background, new object[] { dt, page });
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action<DataTable>(SelectDevice_Finished), background, new object[] { dt });
        }

        //void SelectDevice_GUI(DataTable dt, ConfigurationPage page)
        //{
        //    this.Dispatcher.BeginInvoke(new Action<DataTable>(page.LoadConfiguration), contextidle, new object[] { dt });
        //}

        void SelectDevice_Finished(DataTable dt)
        {
            ConfigurationTable = dt;

            if (PageList.Count > 0)
            {
                if (PageList.Count > selectedPageIndex) Page_Selected((ListButton)PageList[selectedPageIndex]);
                else Page_Selected((ListButton)PageList[0]);
            }

            DeviceLoading = false;
            if (!PageListShown) PageListShown = true;
            SaveNeeded = false;
        }

        #endregion


    }
}
