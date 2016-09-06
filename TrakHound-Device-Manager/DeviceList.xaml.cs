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

using TrakHound;
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

        public ImageSource Image { get { return image; } }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { PageClosed?.Invoke(); }
        public bool Closing() { return true; }

        #endregion

        private BitmapImage image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Device-Manager;component/Resources/Root.png"));


        private DeviceManager _deviceManager;
        /// <summary>
        /// Parent DeviceManager object
        /// </summary>
        public DeviceManager DeviceManager
        {
            get { return _deviceManager; }
            set
            {
                _deviceManager = value;

                if (_deviceManager != null)
                {
                    AddDevices(_deviceManager.Devices);

                    _deviceManager.DeviceListUpdated += _deviceManager_DeviceListUpdated;
                    _deviceManager.DeviceUpdated += _deviceManager_DeviceUpdated;
                    _deviceManager.LoadingDevices += _deviceManager_LoadingDevices;
                    _deviceManager.DevicesLoaded += _deviceManager_DevicesLoaded;
                }
            }
        }

        #region "Events"

        /// <summary>
        /// Event to request to open the Edit Page
        /// </summary>
        public event DeviceSelected_Handler EditSelected;

        /// <summary>
        /// Event to request to open the Edit Table Page
        /// </summary>
        public event DeviceSelected_Handler EditTableSelected;

        /// <summary>
        /// Event to request to open the Copy Device Page
        /// </summary>
        public event DeviceSelected_Handler CopyDeviceSelected;

        /// <summary>
        /// Event to request to open the Add Device Page
        /// </summary>
        public event PageSelected_Handler AddDeviceSelected;

        /// <summary>
        /// Event to notify that this page has closed
        /// </summary>
        public event PageSelected_Handler PageClosed;

        #endregion
        
        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;


        #region "Device Manager Event Handlers"

        private void _deviceManager_DeviceListUpdated(List<DeviceConfiguration> configs) { AddDevices(configs); }

        private void _deviceManager_DeviceUpdated(DeviceConfiguration config, DeviceManager.DeviceUpdateArgs args)
        {
            if (args.Sender != this)
            {
                switch (args.Event)
                {
                    case DeviceManager.DeviceUpdateEvent.Added: AddDeviceToList(config); break;

                    case DeviceManager.DeviceUpdateEvent.Changed: ReplaceDeviceInList(config); break;

                    case DeviceManager.DeviceUpdateEvent.Removed: RemoveDeviceFromList(config); break;
                }
            }
        }

        private void _deviceManager_LoadingDevices()
        {
            Dispatcher.BeginInvoke(new Action(() => { DevicesLoading = true; }));
        }

        private void _deviceManager_DevicesLoaded()
        {
            Dispatcher.BeginInvoke(new Action(() => { DevicesLoading = false; }));
        }

        #endregion

        #region "Dependency Properties"

        public bool DevicesLoading
        {
            get { return (bool)GetValue(DevicesLoadingProperty); }
            set { SetValue(DevicesLoadingProperty, value); }
        }

        public static readonly DependencyProperty DevicesLoadingProperty =
            DependencyProperty.Register("DevicesLoading", typeof(bool), typeof(DeviceList), new PropertyMetadata(false));

        #endregion

        private void OpenEditTable(DeviceConfiguration config)
        {
            EditTableSelected?.Invoke(config);
        }


        #region "Device Lists"

        ObservableCollection<DeviceConfiguration> _devices;
        /// <summary>
        /// Collection of TrakHound.Configurations.Configuration objects that represent the active devices
        /// </summary>
        public ObservableCollection<DeviceConfiguration> Devices
        {
            get
            {
                if (_devices == null)
                    _devices = new ObservableCollection<DeviceConfiguration>();
                return _devices;
            }

            set
            {
                _devices = value;
            }
        }

        private void AddDeviceToList(DeviceConfiguration config, int index = -1)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (index < 0) Devices.Add(config);
                else Devices.Insert(index, config);

                Devices.Sort();
            }
            ), UI_Functions.PRIORITY_DATA_BIND, new object[] { });
        }

        private void ReplaceDeviceInList(DeviceConfiguration config)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                int index = Devices.ToList().FindIndex(x => x.UniqueId == config.UniqueId);
                if (index >= 0)
                {
                    Devices.RemoveAt(index);
                    AddDeviceToList(config, index);
                }
            }
            ), UI_Functions.PRIORITY_DATA_BIND, new object[] { });
        }

        private void RemoveDeviceFromList(DeviceConfiguration config)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var index = Devices.ToList().FindIndex(x => x.UniqueId == config.UniqueId);
                if (index >= 0) Devices.RemoveAt(index);
            }
            ), UI_Functions.PRIORITY_DATA_BIND, new object[] { });
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

        #region "Add Devices"

        private void AddDevices(List<DeviceConfiguration> configs)
        {
            ClearDevices();

            if (configs != null)
            {
                foreach (var config in configs) AddDeviceToList(config);
            }
        }

        #endregion

        #region "Remove Devices"

        private class RemoveDevices_Info
        {
            public List<DeviceConfiguration> Devices { get; set; }
            public bool Success { get; set; }
        }

        private void RemoveDevices(List<DeviceConfiguration> configs)
        {
            // Set the text for the MessageBox based on how many devices are selected to be removed
            string msg = null;
            if (configs.Count == 1) msg = "Are you sure you want to permanently remove this device?";
            else msg = "Are you sure you want to permanently remove these " + configs.Count.ToString() + " devices?";

            string title = null;
            if (configs.Count == 1) msg = "Remove Device";
            else msg = "Remove " + configs.Count.ToString() + " Devices";

            var result = TrakHound_UI.MessageBox.Show(msg, title, TrakHound_UI.MessageBoxButtons.YesNo);
            if (result == MessageBoxDialogResult.Yes)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(RemoveDevices_Worker), configs);
            }
        }

        private void RemoveDevices_Worker(object o)
        {
            var removeInfo = new RemoveDevices_Info();

            if (o != null)
            {
                var configs = (List<DeviceConfiguration>)o;

                removeInfo.Devices = configs;

                removeInfo.Success = DeviceManager.RemoveDevices(configs);
            }

            Dispatcher.BeginInvoke(new Action<RemoveDevices_Info>(RemoveDevices_Finshed), PRIORITY_BACKGROUND, new object[] { removeInfo });
        }

        private void RemoveDevices_Finshed(RemoveDevices_Info removeInfo)
        {
            if (removeInfo.Success)
            { 
                foreach (var config in removeInfo.Devices)
                {
                    if (config != null)
                    {
                        // Raise DeviceUpdated Event
                        var args = new DeviceManager.DeviceUpdateArgs();
                        args.Event = DeviceManager.DeviceUpdateEvent.Removed;
                        DeviceManager.UpdateDevice(config, args);
                    }
                }
            }
            else
            {
                TrakHound_UI.MessageBox.Show("An error occured while attempting to Remove Device. Please try again.", "Remove Device Error", MessageBoxButtons.Ok);
                DeviceManager.LoadDevices();
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

                var config = ((DeviceConfiguration)info.DataObject);

                // Enable Device using DeviceManager
                if (DeviceManager != null) info.Success = DeviceManager.EnableDevice(config);

                // If changes were successful, then update DeviceManager's Congifuration
                if (info.Success)
                {
                    config.Enabled = true;
                }

                Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(EnableDevice_Finished), PRIORITY_BACKGROUND, new object[] { info });
            }
        }

        private void EnableDevice_Finished(EnableDevice_Info info)
        {
            if (info.Sender != null)
            {
                if (info.Success)
                {
                    var config = ((DeviceConfiguration)info.DataObject);

                    //Raise DeviceUpdated Event
                    var args = new DeviceManager.DeviceUpdateArgs();
                    args.Sender = this;
                    args.Event = DeviceManager.DeviceUpdateEvent.Added;
                    DeviceManager.UpdateDevice(config, args);
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

                var config = ((DeviceConfiguration)info.DataObject);

                // Disable Device using DeviceManager
                if (DeviceManager != null) info.Success = DeviceManager.DisableDevice(config);

                // If changes were successful, then update DeviceManager's Congifuration
                if (info.Success) config.Enabled = false;
                
                Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(DisableDevice_Finished), PRIORITY_BACKGROUND, new object[] { info });
            }
        }

        private void DisableDevice_Finished(EnableDevice_Info info)
        {
            if (info.Sender != null)
            {
                if (info.Success)
                {
                    var config = ((DeviceConfiguration)info.DataObject);

                    // Raise DeviceUpdated Event
                    var args = new DeviceManager.DeviceUpdateArgs();
                    args.Sender = this;
                    args.Event = DeviceManager.DeviceUpdateEvent.Removed;
                    DeviceManager.UpdateDevice(config, args);
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
                    DeviceConfiguration refConfig = null;
                    if (change > 0) refConfig = (DeviceConfiguration)items[items.IndexOf(selectedItems[0]) - 1];
                    else if (change < 0) refConfig = (DeviceConfiguration)items[items.IndexOf(selectedItems[selectedItems.Count - 1]) + 1];

                    int refIndex = refConfig.Index;

                    for (var x = 0; x <= selectedItems.Count - 1; x++)
                    {
                        int adjChange = 0;
                        if (change > 0) adjChange = change + ((selectedItems.Count - 1) - x);
                        else if (change < 0) adjChange = change - x;

                        var config = (DeviceConfiguration)selectedItems[x];

                        int listIndex = items.IndexOf(selectedItems[x]);

                        int newIndex = refIndex - adjChange;

                        string a = config.Index.ToString();
                        string a1 = refIndex.ToString();
                        string b = newIndex.ToString();
                        string c = listIndex.ToString();
                        string d = (listIndex - change).ToString();
                        string e = adjChange.ToString();

                        // Change index using DeviceManager
                        if (DeviceManager != null) DeviceManager.ChangeDeviceIndex(config, newIndex);

                        var info = new MoveInfo(listIndex, listIndex - change, newIndex);
                        infos.Add(info);
                    }

                    // Create new array for selecting rows after sorting
                    int[] selectedIndexes = new int[infos.Count];

                    // Set new indexes
                    for (var x = 0; x <= infos.Count - 1; x++)
                    {
                        var info = infos[x];

                        var config = (DeviceConfiguration)items[info.OldListIndex];
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


        #region "Button Actions"

        private void AddClicked()
        {
            AddDeviceSelected?.Invoke();
        }

        private void EditClicked()
        {
            foreach (var device in Devices_DG.SelectedItems)
            {
                var config = (DeviceConfiguration)device;

                EditSelected?.Invoke(config);
            }
        }

        private void EditTableClicked()
        {
            foreach (var device in Devices_DG.SelectedItems)
            {
                var config = (DeviceConfiguration)device;

                EditTableSelected?.Invoke(config);
            }
        }

        private void RefreshClicked()
        {
            if (DeviceManager != null) DeviceManager.LoadDevices();
        }

        private void CopyClicked()
        {
            if (Devices_DG.SelectedItem != null)
            {
                var config = (DeviceConfiguration)Devices_DG.SelectedItem;

                if (config != null)
                {
                    CopyDeviceSelected?.Invoke(config);
                }
            }
        }

        private void RemoveClicked()
        {
            if (Devices_DG.SelectedItems != null && Devices_DG.SelectedItems.Count > 0)
            {
                var configs = new List<DeviceConfiguration>();

                foreach (var item in Devices_DG.SelectedItems)
                {
                    var config = item as DeviceConfiguration;
                    if (config != null) configs.Add(config);
                }

                if (configs.Count > 0) RemoveDevices(configs);
            }
        }

        private void BackupClicked()
        {
            string backupPath = Path.Combine(FileLocations.Backup, "DeviceBackup-" + DateTime.Now.ToString("yyyy-MM-dd--hh-mm-ss"));
            if (!Directory.Exists(backupPath)) Directory.CreateDirectory(backupPath);

            foreach (var device in Devices_DG.SelectedItems)
            {
                var config = (DeviceConfiguration)device;

                DeviceConfiguration.Save(config, backupPath);
            }

            try
            {
                System.Diagnostics.Process.Start("explorer", backupPath);
            }
            catch (Exception ex)
            {
                Logger.Log("SaveClicked() :: Exception :: " + ex.Message);
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
                var config = (DeviceConfiguration)bt.DataObject;

                EditSelected?.Invoke(config);
            }
        }

        #endregion

        #region "Toolbar Buttons"

        private void Add_Toolbar_Clicked(TrakHound_UI.Button bt) { AddClicked(); }

        private void MoveUp_Toolbar_Clicked(TrakHound_UI.Button bt) { MoveUpClicked(); }

        private void MoveDown_Toolbar_Clicked(TrakHound_UI.Button bt) { MoveDownClicked(); }

        private void Edit_Toolbar_Clicked(TrakHound_UI.Button bt) { EditClicked(); }

        private void EditTable_Toolbar_Clicked(TrakHound_UI.Button bt) { EditTableClicked(); }

        private void Refresh_Toolbar_Clicked(TrakHound_UI.Button bt) { RefreshClicked(); }

        private void Copy_Toolbar_Clicked(TrakHound_UI.Button bt) { CopyClicked(); }

        private void Remove_Toolbar_Clicked(TrakHound_UI.Button bt) { RemoveClicked(); }

        private void Backup_Toolbar_Clicked(TrakHound_UI.Button bt) { BackupClicked(); }

        #endregion

        #region "Context Menu Buttons"

        private void Add_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { AddClicked(); }

        private void Edit_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { EditClicked(); }

        private void EditTable_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { EditTableClicked(); }

        private void Copy_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { CopyClicked(); }

        private void Remove_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { RemoveClicked(); }

        private void Backup_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { BackupClicked(); }

        #endregion

        private void Grid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { Devices_DG.SelectedItems.Clear(); }

    }

}
