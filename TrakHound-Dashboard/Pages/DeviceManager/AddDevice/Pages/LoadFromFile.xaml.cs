// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Pages.DeviceManager.AddDevice.Pages
{
    /// <summary>
    /// Page containing options for loading a Device from a local file
    /// </summary>
    public partial class LoadFromFile : UserControl, IPage
    {
        public LoadFromFile()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "IPage"

        public string Title { get { return "Load Device Configuration From File"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/List_01.png"); } }

        public bool ZoomEnabled { get { return false; } }

        public void SetZoom(double zoomPercentage) { }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        public event SendData_Handler SendData;

        private UserConfiguration currentUser;

        private ObservableCollection<DeviceDescription> _devices;
        /// <summary>
        /// Collection of TrakHound.Configurations.Configuration objects that represent the active devices
        /// </summary>
        public ObservableCollection<DeviceDescription> Devices
        {
            get
            {
                if (_devices == null)
                    _devices = new ObservableCollection<DeviceDescription>();
                return _devices;
            }

            set
            {
                _devices = value;
            }
        }


        public void GetSentData(EventData data)
        {
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateLoggedInChanged), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDevicesLoading), System.Windows.Threading.DispatcherPriority.Normal, new object[] { data });

            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { data });
        }

        void UpdateLoggedInChanged(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "USER_LOGIN")
                {
                    if (data.Data01 != null) currentUser = (UserConfiguration)data.Data01;
                }
                else if (data.Id == "USER_LOGOUT")
                {
                    currentUser = null;
                }
            }
        }

        void UpdateDevicesLoading(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICES_LOADING")
                {
                    ClearDevices();
                }
            }
        }

        void UpdateDeviceAdded(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_ADDED" && data.Data01 != null)
                {
                    Devices.Add((DeviceDescription)data.Data01);
                }
            }
        }

        void UpdateDeviceUpdated(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_UPDATED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;

                    int i = Devices.ToList().FindIndex(x => x.UniqueId == device.UniqueId);
                    if (i >= 0)
                    {
                        Devices.RemoveAt(i);
                        Devices.Insert(i, device);
                    }
                }
            }
        }

        void UpdateDeviceRemoved(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "DEVICE_REMOVED" && data.Data01 != null)
                {
                    var device = (DeviceDescription)data.Data01;

                    int i = Devices.ToList().FindIndex(x => x.UniqueId == device.UniqueId);
                    if (i >= 0)
                    {
                        Devices.RemoveAt(i);
                    }
                }
            }
        }

        private void ClearDevices()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Devices.Clear();
            }
            ), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { });
        }

        #endregion

        /// <summary>
        /// Parent AddDevice.Page object
        /// </summary>
        public Page ParentPage { get; set; }

        #region "Dependency Properties"

        /// <summary>
        /// Number of Devices that were successfully added
        /// </summary>
        public int AddedSuccessfully
        {
            get { return (int)GetValue(AddedSuccessfullyProperty); }
            set { SetValue(AddedSuccessfullyProperty, value); }
        }

        public static readonly DependencyProperty AddedSuccessfullyProperty =
            DependencyProperty.Register("AddedSuccessfully", typeof(int), typeof(LoadFromFile), new PropertyMetadata(0));

        #endregion


        private void Browse_Clicked(TrakHound_UI.Button bt) { LoadDeviceFromFile(); }

        private void DeviceManager_Clicked(TrakHound_UI.Button bt)
        {
            if (ParentPage != null)
            {
                ParentPage.OpenDeviceList();
            }
        }

        /// <summary>
        /// Open a Windows Dialog to select files and then load each file as a Device
        /// </summary>
        private void LoadDeviceFromFile()
        {
            // Browse for Device Configuration path
            string[] paths = OpenConfigurationBrowse();
            if (paths != null)
            {
                AddedSuccessfully = 0;

                foreach (var path in paths)
                {
                    LoadDevice(path);

                    AddedSuccessfully++;
                }
            }
        }

        /// <summary>
        /// Open a Windows Dialog to select files to load as Devices
        /// </summary>
        /// <returns></returns>
        private static string[] OpenConfigurationBrowse()
        {
            string[] result = null;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = FileLocations.TrakHound;
            dlg.Multiselect = true;
            dlg.Title = "Browse for Device Configuration File(s)";
            dlg.Filter = "Device Configuration files (*.xml) | *.xml";

            Nullable<bool> dialogResult = dlg.ShowDialog();
            if (dialogResult == true)
            {
                if (dlg.FileNames != null) result = dlg.FileNames;
            }

            return result;
        }
      
        /// <summary>
        /// Load a Device from a local path
        /// </summary>
        /// <param name="path">Path to the file to load</param>
        /// <returns>Boolean whether load was successful</returns>
        private bool LoadDevice(string path)
        {
            bool result = false;

            // Get Configuration from path
            var config = DeviceConfiguration.Read(path);
            if (config != null)
            {
                if (currentUser != null)
                {
                    result = TrakHound.API.Devices.Update(currentUser, config);
                }
                // If not logged in Read from File in 'C:\TrakHound\'
                else
                {
                    result = DeviceConfiguration.Save(config);
                }

                if (result)
                {
                    // Send message that device was added
                    var data = new EventData(this);
                    data.Id = "DEVICE_ADDED";
                    data.Data01 = new DeviceDescription(config);
                    SendData?.Invoke(data);
                }
            }

            return result;
        }


        #region "Drag and Drop"

        private void Rectangle_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        }

        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                int added = 0;
                AddedSuccessfully = 0;

                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var path in paths)
                {
                    if (LoadDevice(path))
                    {
                        added++;
                        AddedSuccessfully++;
                    }
                }

                if (added < paths.Length) TrakHound_UI.MessageBox.Show("Some devices did not get added correctly. Review the Developer Console for further information.");
            }
        }

        #endregion

    }
}
