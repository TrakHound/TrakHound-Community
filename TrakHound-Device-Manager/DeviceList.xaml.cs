// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Logging;
using TrakHound.Tools;
using TrakHound_Device_Manager.Controls;
using TrakHound_UI;

namespace TrakHound_Device_Manager
{
    /// <summary>
    /// Main GUI page for DeviceManager which lists Devices and has a toolbar to perform related functions
    /// </summary>
    public partial class DeviceList : UserControl, IPage
    {
        public DeviceList()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "IPage"

        public string Title { get { return "Device Manager"; } }

        private BitmapImage _image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Device-Manager;component/Resources/Root.png"));
        public ImageSource Image { get { return _image; } }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { PageClosed?.Invoke(); }
        public bool Closing() { return true; }

        public event SendData_Handler SendData;

        public void GetSentData(EventData data)
        {
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateLoggedInChanged), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDevicesLoading), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });

            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceAdded), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceUpdated), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
            Dispatcher.BeginInvoke(new Action<EventData>(UpdateDeviceRemoved), UI_Functions.PRIORITY_DATA_BIND, new object[] { data });
        }

        #endregion

        private UserConfiguration currentUser;

        ObservableCollection<DeviceDescription> _devices;
        /// <summary>
        /// Collection of DeviceDescription objects
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

        /// <summary>
        /// Event to request to open the Edit Page
        /// </summary>
        public event DeviceSelected_Handler EditSelected;

        /// <summary>
        /// Event to request to open the Add Device Page
        /// </summary>
        public event PageSelected_Handler AddDeviceSelected;

        /// <summary>
        /// Event to notify that this page has closed
        /// </summary>
        public event PageSelected_Handler PageClosed;



        #region "Dependency Properties"

        public bool DevicesLoading
        {
            get { return (bool)GetValue(DevicesLoadingProperty); }
            set { SetValue(DevicesLoadingProperty, value); }
        }

        public static readonly DependencyProperty DevicesLoadingProperty =
            DependencyProperty.Register("DevicesLoading", typeof(bool), typeof(DeviceList), new PropertyMetadata(false));


        public bool BackupLoading
        {
            get { return (bool)GetValue(BackupLoadingProperty); }
            set { SetValue(BackupLoadingProperty, value); }
        }

        public static readonly DependencyProperty BackupLoadingProperty =
            DependencyProperty.Register("BackupLoading", typeof(bool), typeof(DeviceList), new PropertyMetadata(false));

        #endregion

        #region "EventData Handlers"

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
                    DevicesLoading = true;
                    ClearDevices();
                }

                if (data.Id == "DEVICES_LOADED")
                {
                    DevicesLoading = false;
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
            ), UI_Functions.PRIORITY_DATA_BIND, new object[] { });
        }

        #endregion


        #region "Remove Devices"

        private class RemoveDevices_Info
        {
            public List<DeviceDescription> Devices { get; set; }
            public bool Success { get; set; }
        }

        private void RemoveDevices(List<DeviceDescription> devices)
        {
            // Set the text for the MessageBox based on how many devices are selected to be removed
            string msg = null;
            if (devices.Count == 1) msg = "Are you sure you want to permanently remove this device?";
            else msg = "Are you sure you want to permanently remove these " + devices.Count.ToString() + " devices?";

            string title = null;
            if (devices.Count == 1) title = "Remove Device?";
            else title = "Remove " + devices.Count.ToString() + " Devices?";

            var result = TrakHound_UI.MessageBox.Show(msg, title, TrakHound_UI.MessageBoxButtons.YesNo);
            if (result == MessageBoxDialogResult.Yes)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(RemoveDevices_Worker), devices);
            }
        }

        private void RemoveDevices_Worker(object o)
        {
            var removeInfo = new RemoveDevices_Info();

            if (o != null)
            {
                var devices = (List<DeviceDescription>)o;

                removeInfo.Devices = devices;

                var uniqueIds = devices.Select(x => x.UniqueId).ToArray();

                if (currentUser != null) removeInfo.Success = TrakHound.API.Devices.Remove(currentUser, uniqueIds);
                else removeInfo.Success = RemoveLocalDevices(devices);
            }

            Dispatcher.BeginInvoke(new Action<RemoveDevices_Info>(RemoveDevices_Finshed), UI_Functions.PRIORITY_BACKGROUND, new object[] { removeInfo });
        }

        private bool RemoveLocalDevices(List<DeviceDescription> devices)
        {
            bool result = false;

            foreach (var device in devices)
            {
                string path = FileLocations.Devices + "\\" + device.UniqueId + ".xml";

                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);

                        result = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Remove Local Device :: Exception :: " + path + " :: " + ex.Message);
                        break; 
                    }
                }
            }

            return result; 
        }

        private void RemoveDevices_Finshed(RemoveDevices_Info removeInfo)
        {
            if (removeInfo.Success)
            { 
                foreach (var device in removeInfo.Devices)
                {
                    if (device != null)
                    {
                        // Send message that device has been removed
                        var data = new EventData();
                        data.Id = "DEVICE_REMOVED";
                        data.Data01 = device;
                        SendData?.Invoke(data);
                    }
                }
            }
            else
            {
                TrakHound_UI.MessageBox.Show("An error occured while attempting to Remove Device. Please try again.", "Remove Device Error", MessageBoxButtons.Ok);

                // Send request to reload devices
                var data = new EventData();
                data.Id = "LOAD_DEVICES";
                SendData?.Invoke(data);
            }
        }

        #endregion

        #region "Enable Device"

        private class EnableDevice_Info
        {
            public DataGridCellCheckBox Sender { get; set; }
            public bool Success { get; set; }
            public object DataObject { get; set; }
        }

        private void EnableDevice(DataGridCellCheckBox chk)
        {
            var info = new EnableDevice_Info();
            info.Sender = chk;
            info.DataObject = chk.DataObject;

            if (info.DataObject != null)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(EnableDevice_Worker), info);
            }
        }

        private void EnableDevice_Worker(object o)
        {
            if (o != null)
            {
                var info = (EnableDevice_Info)o;

                var device = ((DeviceDescription)info.DataObject);

                bool result = false;

                // Enable Device using DeviceManager
                if (currentUser != null)
                {
                    result = TrakHound.API.Devices.Update(currentUser, device.UniqueId, new TrakHound.API.Devices.DeviceInfo.Row("/Enabled", "True", null));
                    if (result) device.Enabled = true;
                }
                else
                {
                    var config = GetLocalDeviceConfiguration(device.UniqueId);
                    if (config != null)
                    {
                        result = UpdateEnabledXML(config.Xml, true);
                        if (result) result = ResetUpdateId(config);
                        if (result) result = DeviceConfiguration.Save(config);
                    }
                }

                info.Success = result;

                // If changes were successful, then update DeviceManager's Congifuration
                if (info.Success) device.Enabled = true;

                Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(EnableDevice_Finished), UI_Functions.PRIORITY_BACKGROUND, new object[] { info });
            }
        }

        private void EnableDevice_Finished(EnableDevice_Info info)
        {
            if (info.Sender != null)
            {
                if (info.Success)
                {
                    var device = ((DeviceDescription)info.DataObject);

                    // Send message that device has been removed
                    var data = new EventData();
                    data.Id = "DEVICE_ENABLED";
                    data.Data01 = device;
                    SendData?.Invoke(data);
                }
                // If not successful then set Checkbox back to previous state
                else info.Sender.IsChecked = false;
            }
        }

        #endregion

        #region "Disable Device"

        private void DisableDevice(DataGridCellCheckBox chk)
        {
            var info = new EnableDevice_Info();
            info.Sender = chk;
            info.DataObject = chk.DataObject;

            if (info.DataObject != null)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(DisableDevice_Worker), info);
            }
        }

        private void DisableDevice_Worker(object o)
        {
            if (o != null)
            {
                var info = (EnableDevice_Info)o;

                var device = ((DeviceDescription)info.DataObject);

                bool result = false;

                // Disable Device using DeviceManager
                if (currentUser != null)
                {
                    result = TrakHound.API.Devices.Update(currentUser, device.UniqueId, new TrakHound.API.Devices.DeviceInfo.Row("/Enabled", "False", null));
                    if (result) device.Enabled = false;
                }
                else
                {
                    var config = GetLocalDeviceConfiguration(device.UniqueId);
                    if (config != null)
                    {
                        result = UpdateEnabledXML(config.Xml, false);
                        if (result) result = ResetUpdateId(config);
                        if (result) result = DeviceConfiguration.Save(config);
                    }
                }

                info.Success = result;

                // If changes were successful, then update DeviceManager's Congifuration
                if (info.Success) device.Enabled = false;
                
                Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(DisableDevice_Finished), UI_Functions.PRIORITY_BACKGROUND, new object[] { info });
            }
        }

        private void DisableDevice_Finished(EnableDevice_Info info)
        {
            if (info.Sender != null)
            {
                if (info.Success)
                {
                    var device = ((DeviceDescription)info.DataObject);

                    // Send message that device has been removed
                    var data = new EventData();
                    data.Id = "DEVICE_DISABLED";
                    data.Data01 = device;
                    SendData?.Invoke(data);
                }
                // If not successful then set Checkbox back to previous state
                else info.Sender.IsChecked = true;
            }
        }

        #endregion

        #region "Device Indexes"

        private class MoveInfo
        {
            public MoveInfo(int oldListIndex, int newListIndex, int deviceIndex)
            {
                OldListIndex = oldListIndex;
                NewListIndex = newListIndex;
                DeviceIndex = deviceIndex;
            }

            public int OldListIndex { get; set; }
            public int NewListIndex { get; set; }
            public int DeviceIndex { get; set; }
        }

        private void MoveUpClicked() { Move(1); }

        private void MoveDownClicked() { Move(-1); }

        /// <summary>
        /// Move Item Index in DeviceInfo list. 
        /// </summary>
        /// <param name="change">Postive number moves items up. Negative number moves items down</param>
        private void Move(int change)
        {
            var items = Devices_DG.Items;
            var selectedItems = Devices_DG.SelectedItems;

            var infos = new List<MoveInfo>();

            if (selectedItems.Count > 0)
            {
                bool valid = false;

                // Make sure items are valid to be moved
                if (change > 0)
                {
                    // Make sure first item in selecteditems is not the first item in the whole list
                    int firstIndex = items.IndexOf(selectedItems[0]);
                    valid = (firstIndex > 0);
                }
                else if (change < 0)
                {
                    // Make sure last item in selecteditems is not the last item in the whole list
                    int lastIndex = items.IndexOf(selectedItems[selectedItems.Count - 1]);
                    valid = (lastIndex < items.Count - 1);
                }

                if (valid)
                {
                    DeviceDescription refConfig = null;
                    if (change > 0) refConfig = (DeviceDescription)items[items.IndexOf(selectedItems[0]) - 1];
                    else if (change < 0) refConfig = (DeviceDescription)items[items.IndexOf(selectedItems[selectedItems.Count - 1]) + 1];

                    int refIndex = refConfig.Index;

                    for (var x = 0; x <= selectedItems.Count - 1; x++)
                    {
                        int adjChange = 0;
                        if (change > 0) adjChange = change + ((selectedItems.Count - 1) - x);
                        else if (change < 0) adjChange = change - x;

                        var config = (DeviceDescription)selectedItems[x];

                        int listIndex = items.IndexOf(selectedItems[x]);

                        int newIndex = refIndex - adjChange;

                        string a = config.Index.ToString();
                        string a1 = refIndex.ToString();
                        string b = newIndex.ToString();
                        string c = listIndex.ToString();
                        string d = (listIndex - change).ToString();
                        string e = adjChange.ToString();

                        // Change index using DeviceManager
                        //if (DeviceManager != null) DeviceManager.ChangeDeviceIndex(config, newIndex);

                        var info = new MoveInfo(listIndex, listIndex - change, newIndex);
                        infos.Add(info);
                    }

                    // Create new array for selecting rows after sorting
                    int[] selectedIndexes = new int[infos.Count];

                    // Set new indexes
                    for (var x = 0; x <= infos.Count - 1; x++)
                    {
                        var info = infos[x];

                        var config = (DeviceDescription)items[info.OldListIndex];
                        config.Index = info.DeviceIndex;

                        selectedIndexes[x] = info.NewListIndex;
                    }

                    // Sort Devices list using new Configuration.Index
                    Devices.Sort();

                    // Select Rows using new List Indexes
                    SelectRowByIndexes(Devices_DG, selectedIndexes);
                }
            }
        }



        private static void SelectRowByIndexes(DataGrid dataGrid, params int[] rowIndexes)
        {
            if (!dataGrid.SelectionUnit.Equals(DataGridSelectionUnit.FullRow))
                throw new ArgumentException("The SelectionUnit of the DataGrid must be set to FullRow.");

            if (!dataGrid.SelectionMode.Equals(DataGridSelectionMode.Extended))
                throw new ArgumentException("The SelectionMode of the DataGrid must be set to Extended.");

            if (rowIndexes.Length.Equals(0) || rowIndexes.Length > dataGrid.Items.Count)
                throw new ArgumentException("Invalid number of indexes.");

            dataGrid.SelectedItems.Clear();
            foreach (int rowIndex in rowIndexes)
            {
                if (rowIndex < 0 || rowIndex > (dataGrid.Items.Count - 1))
                    throw new ArgumentException(string.Format("{0} is an invalid row index.", rowIndex));

                object item = dataGrid.Items[rowIndex]; //=Product X
                dataGrid.SelectedItems.Add(item);

                DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
                if (row == null)
                {
                    dataGrid.ScrollIntoView(item);
                    row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
                }
                if (row != null)
                {
                    DataGridCell cell = GetCell(dataGrid, row, 0);
                    if (cell != null) cell.Focus();
                }
            }
        }

        private static DataGridCell GetCell(DataGrid dataGrid, DataGridRow rowContainer, int column)
        {
            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
                if (presenter == null)
                {
                    /* if the row has been virtualized away, call its ApplyTemplate() method
                     * to build its visual tree in order for the DataGridCellsPresenter
                     * and the DataGridCells to be created */
                    rowContainer.ApplyTemplate();
                    presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
                }
                if (presenter != null)
                {
                    DataGridCell cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                    if (cell == null)
                    {
                        /* bring the column into view
                         * in case it has been virtualized away */
                        dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);
                        cell = presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
                    }
                    return cell;
                }
            }
            return null;
        }

        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        #endregion

        #region "Device Backup"

        private Thread backupDeviceThread;

        private class BackupDeviceInfo
        {
            public BackupDeviceInfo(UserConfiguration userConfig, string[] uniqueIds)
            {
                UserConfiguration = userConfig;
                UniqueIds = uniqueIds;
            }

            public UserConfiguration UserConfiguration { get; set; }
            public string[] UniqueIds { get; set; }
        }

        private void BackupDevices(string[] uniqueIds)
        {
            BackupLoading = true;

            if (backupDeviceThread != null) backupDeviceThread.Abort();

            if (currentUser != null)
            {
                backupDeviceThread = new Thread(new ParameterizedThreadStart(BackupUserDevices));
                backupDeviceThread.Start(new BackupDeviceInfo(currentUser, uniqueIds));
            }
            else
            {
                backupDeviceThread = new Thread(new ParameterizedThreadStart(BackupLocalDevices));
                backupDeviceThread.Start(uniqueIds);
            }
        }

        private void BackupUserDevices(object o)
        {
            if (o != null)
            {
                var backupDeviceInfo = (BackupDeviceInfo)o;

                // Get DeviceConfigurations using API
                var configs = TrakHound.API.Devices.Get(backupDeviceInfo.UserConfiguration, backupDeviceInfo.UniqueIds);
                if (configs != null)
                {
                    // Create backup directory
                    string backupPath = Path.Combine(FileLocations.Backup, "DeviceBackup-" + DateTime.Now.ToString("yyyy-MM-dd--hh-mm-ss"));
                    if (!Directory.Exists(backupPath)) Directory.CreateDirectory(backupPath);

                    // Save each DeviceConfiguration
                    foreach (var config in configs)
                    {
                        DeviceConfiguration.Save(config, backupPath);
                    }

                    // Open Windows Explorer with the directory the backups were saved to
                    try
                    {
                        System.Diagnostics.Process.Start("explorer", backupPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Device Backup Error :: Error Opening Windows Explorer :: Exception :: " + ex.Message);
                    }
                }
                else
                {

                }
            }

            Dispatcher.BeginInvoke(new Action(() => { BackupLoading = false; }));
        }

        private void BackupLocalDevices(object o)
        {
            if (o != null)
            {
                var uniqueIds = (string[])o;

                // Create backup directory
                string backupPath = Path.Combine(FileLocations.Backup, "DeviceBackup-" + DateTime.Now.ToString("yyyy-MM-dd--hh-mm-ss"));
                if (!Directory.Exists(backupPath)) Directory.CreateDirectory(backupPath);

                foreach (string uniqueId in uniqueIds)
                {
                    try
                    {
                        string filename = Path.ChangeExtension(uniqueId, ".xml");
                        string sourcePath = Path.Combine(FileLocations.Devices, filename);
                        string destPath = Path.Combine(backupPath, filename);

                        // See if file exists
                        if (File.Exists(sourcePath))
                        {
                            // Copy File from Devices to new backup path
                            File.Copy(sourcePath, destPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Local Device Backup Error :: Exception :: " + ex.Message);
                    }
                }

                // Open Windows Explorer with the directory the backups were saved to
                try
                {
                    System.Diagnostics.Process.Start("explorer", backupPath);
                }
                catch (Exception ex)
                {
                    Logger.Log("Device Backup Error :: Error Opening Windows Explorer :: Exception :: " + ex.Message);
                }
            }

            Dispatcher.BeginInvoke(new Action(() => { BackupLoading = false; }));
        }

        #endregion


        #region "Button Actions"

        private void AddClicked()
        {
            AddDeviceSelected?.Invoke();
        }

        private void EditClicked()
        {
            foreach (var item in Devices_DG.SelectedItems)
            {
                var device = (DeviceDescription)item;
                EditSelected?.Invoke(device);
            }
        }

        private void RefreshClicked()
        {
            // Send Message to Reload Devices
            var data = new EventData();
            data.Id = "LOAD_DEVICES";
            SendData?.Invoke(data);
        }

        private void RemoveClicked()
        {
            if (Devices_DG.SelectedItems != null && Devices_DG.SelectedItems.Count > 0)
            {
                var devices = new List<DeviceDescription>();

                foreach (var item in Devices_DG.SelectedItems)
                {
                    var device = item as DeviceDescription;
                    if (device != null) devices.Add(device);
                }

                if (devices.Count > 0) RemoveDevices(devices);
            }
        }

        private void BackupClicked()
        {
            var uniqueIds = new List<string>();

            // Get a list of UniqueIds for each selected item in Devices DataTable
            foreach (var item in Devices_DG.SelectedItems)
            {
                var device = (DeviceDescription)item;
                uniqueIds.Add(device.UniqueId);
            }

            if (uniqueIds.Count > 0)
            {
                BackupDevices(uniqueIds.ToArray());
            }
        }

        #endregion

        #region "Datarow Buttons"

        private void Enabled_Checked(object sender, RoutedEventArgs e)
        {
            EnableDevice((DataGridCellCheckBox)sender);
        }

        private void Enabled_Unchecked(object sender, RoutedEventArgs e)
        {
            DisableDevice((DataGridCellCheckBox)sender);
        }

        private void Edit_Clicked(TrakHound_UI.Button bt)
        {
            if (bt.DataObject != null)
            {
                var config = (DeviceDescription)bt.DataObject;

                EditSelected?.Invoke(config);
            }
        }

        #endregion

        #region "Toolbar Buttons"

        private void Add_Toolbar_Clicked(TrakHound_UI.Button bt) { AddClicked(); }

        private void MoveUp_Toolbar_Clicked(TrakHound_UI.Button bt) { MoveUpClicked(); }

        private void MoveDown_Toolbar_Clicked(TrakHound_UI.Button bt) { MoveDownClicked(); }

        private void Edit_Toolbar_Clicked(TrakHound_UI.Button bt) { EditClicked(); }

        private void Refresh_Toolbar_Clicked(TrakHound_UI.Button bt) { RefreshClicked(); }

        private void Remove_Toolbar_Clicked(TrakHound_UI.Button bt) { RemoveClicked(); }

        private void Backup_Toolbar_Clicked(TrakHound_UI.Button bt) { BackupClicked(); }

        #endregion

        #region "Context Menu Buttons"

        private void Add_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { AddClicked(); }

        private void Edit_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { EditClicked(); }

        private void Remove_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { RemoveClicked(); }

        private void Backup_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { BackupClicked(); }

        #endregion

        private void Grid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { Devices_DG.SelectedItems.Clear(); }

        private DeviceConfiguration GetLocalDeviceConfiguration(string uniqueId)
        {
            string filename = Path.ChangeExtension(uniqueId, ".xml");
            string path = Path.Combine(FileLocations.Devices, filename);

            return DeviceConfiguration.Read(path);
        }

        private bool ResetUpdateId(DeviceConfiguration config)
        {
            bool result = false;

            var updateId = Guid.NewGuid().ToString();

            if (currentUser != null) result = TrakHound.API.Devices.Update(currentUser, config.UniqueId, new TrakHound.API.Devices.DeviceInfo.Row("/UpdateId", updateId, null));
            else result = true;

            if (result)
            {
                config.UpdateId = updateId;
                XML_Functions.SetInnerText(config.Xml, "UpdateId", updateId);
            }

            return result;
        }

        private static bool UpdateEnabledXML(XmlDocument xml, bool enabled)
        {
            bool result = false;

            result = XML_Functions.SetInnerText(xml, "Enabled", enabled.ToString());

            return result;
        }


    }

}
