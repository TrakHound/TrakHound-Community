// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TH_Configuration;
using TH_DeviceManager.Controls;
using TH_Global;
using TH_Global.Functions;
using TH_WPF;

namespace TH_DeviceManager
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

        public void Closed() { if (PageClosed != null) PageClosed(); }
        public bool Closing() { return true; }

        #endregion

        private BitmapImage image = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Root.png"));


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
                    AddSharedDevices(_deviceManager.SharedDevices);

                    _deviceManager.DeviceListUpdated += _deviceManager_DeviceListUpdated;
                    _deviceManager.SharedDeviceListUpdated += _deviceManager_SharedDeviceListUpdated;
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
        /// Event to request to open the Share Device Page
        /// </summary>
        public event DeviceSelected_Handler ShareDeviceSelected;

        /// <summary>
        /// Event to request to open the Copy Device Page
        /// </summary>
        public event DeviceSelected_Handler CopyDeviceSelected;

        /// <summary>
        /// Event to request to open the Device List Page
        /// </summary>
        //public event PageSelected_Handler DeviceListSelected;

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

        private void _deviceManager_DeviceListUpdated(List<Configuration> configs) { AddDevices(configs); }

        private void _deviceManager_SharedDeviceListUpdated(List<Configuration> configs) { AddSharedDevices(configs); }

        private void _deviceManager_DeviceUpdated(Configuration config, DeviceManager.DeviceUpdateArgs args)
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

        private void OpenEditTable(Configuration config)
        {
            if (EditTableSelected != null) EditTableSelected(config);
        }


        #region "Device Lists"

        ObservableCollection<Configuration> _devices;
        /// <summary>
        /// Collection of TH_Configuration.Configuration objects that represent the active devices
        /// </summary>
        public ObservableCollection<Configuration> Devices
        {
            get
            {
                if (_devices == null)
                    _devices = new ObservableCollection<Configuration>();
                return _devices;
            }

            set
            {
                _devices = value;
            }
        }

        ObservableCollection<Configuration> _sharedDevices;
        /// <summary>
        /// Collection of TH_Configuration.Configuration objects that represent the shared devices
        /// that are owned by the current user
        /// </summary>
        public ObservableCollection<Configuration> SharedDevices
        {
            get
            {
                if (_sharedDevices == null)
                    _sharedDevices = new ObservableCollection<Configuration>();
                return _sharedDevices;
            }

            set
            {
                _sharedDevices = value;
            }
        }

        private void AddDeviceToList(Configuration config, int index = -1)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (index < 0) Devices.Add(config);
                else Devices.Insert(index, config);

                Devices.Sort();
            }
            ), PRIORITY_BACKGROUND, new object[] { });
        }

        private void ReplaceDeviceInList(Configuration config)
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
            ), PRIORITY_BACKGROUND, new object[] { });
        }

        private void RemoveDeviceFromList(Configuration config)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var index = Devices.ToList().FindIndex(x => x.UniqueId == config.UniqueId);
                if (index >= 0) Devices.RemoveAt(index);
            }
            ));
        }

        private void ClearDevices()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Devices.Clear();
            }
            ), PRIORITY_BACKGROUND, new object[] { });
        }


        private void AddSharedDeviceToList(Configuration config)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                SharedDevices.Add(config);
            }
            ), PRIORITY_BACKGROUND, new object[] { });
        }

        private void ClearSharedDevices()
        {
            Dispatcher.BeginInvoke(new Action(() => 
            {
                SharedDevices.Clear();
            }
            ));
        }

        #endregion

        #region "Add Devices"

        private void AddDevices(List<Configuration> configs)
        {
            ClearDevices();

            if (configs != null)
            {
                foreach (var config in configs) AddDeviceToList(config);
            }
        }

        private void AddSharedDevices(List<Configuration> configs)
        {
            ClearSharedDevices();

            if (configs != null)
            {
                foreach (var config in configs)
                {
                    AddSharedDeviceToList(config);
                }
            }
        }

        #endregion

        #region "Remove Devices"

        private class RemoveDevices_Info
        {
            public List<Configuration> Devices { get; set; }
            public bool Success { get; set; }
        }

        private void RemoveDevices(List<Configuration> configs)
        {
            // Set the text for the MessageBox based on how many devices are selected to be removed
            string msg = null;
            if (configs.Count == 1) msg = "Are you sure you want to permanently remove this device?";
            else msg = "Are you sure you want to permanently remove these " + configs.Count.ToString() + " devices?";

            string title = null;
            if (configs.Count == 1) msg = "Remove Device";
            else msg = "Remove " + configs.Count.ToString() + " Devices";

            var result = TH_WPF.MessageBox.Show(msg, title, TH_WPF.MessageBoxButtons.YesNo);
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
                var configs = (List<Configuration>)o;

                removeInfo.Devices = configs;

                foreach (var config in configs)
                {
                    if (config != null && DeviceManager != null)
                    {
                        // Remove device using DeviceManager
                        removeInfo.Success = DeviceManager.RemoveDevice(config);
                        if (!removeInfo.Success) break;
                    }
                }
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
                TH_WPF.MessageBox.Show("An error occured while attempting to Remove Device. Please try again.", "Remove Device Error", MessageBoxButtons.Ok);
                DeviceManager.LoadDevices();
            }
        }

        #endregion

        #region "Enable Device"

        private class EnableDevice_Info
        {
            public DataGridCellCheckBox Sender { get; set; }
            public bool Success { get; set; }
            public ManagementType Type { get; set; }
            public object DataObject { get; set; }
        }

        private void EnableDevice(DataGridCellCheckBox chk, ManagementType type)
        {
            EnableDevice_Info info = new EnableDevice_Info();
            info.Sender = chk;
            info.Type = type;
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
                EnableDevice_Info info = (EnableDevice_Info)o;

                var config = ((Configuration)info.DataObject);

                // Enable Device using DeviceManager
                if (DeviceManager != null) info.Success = DeviceManager.EnableDevice(config, info.Type);

                // If changes were successful, then update DeviceManager's Congifuration
                if (info.Success)
                {
                    if (info.Type == ManagementType.Client) config.ClientEnabled = true;
                    else if (info.Type == ManagementType.Server) config.ServerEnabled = true;
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
                    var config = ((Configuration)info.DataObject);

                    //Raise DeviceUpdated Event
                    var args = new DeviceManager.DeviceUpdateArgs();
                    args.Sender = this;
                    args.Event = DeviceManager.DeviceUpdateEvent.Changed;
                    DeviceManager.UpdateDevice(config, args);
                }
                // If not successful then set Checkbox back to previous state
                else info.Sender.IsChecked = false;
            }
        }

        #endregion

        #region "Disable Device"

        private void DisableDevice(DataGridCellCheckBox chk, ManagementType type)
        {
            EnableDevice_Info info = new EnableDevice_Info();
            info.Sender = chk;
            info.Type = type;
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
                EnableDevice_Info info = (EnableDevice_Info)o;

                var config = ((Configuration)info.DataObject);

                // Disable Device using DeviceManager
                if (DeviceManager != null) info.Success = DeviceManager.DisableDevice(config, info.Type);

                // If changes were successful, then update DeviceManager's Congifuration
                if (info.Success)
                {
                    if (info.Type == ManagementType.Client) config.ClientEnabled = false;
                    else if (info.Type == ManagementType.Server) config.ServerEnabled = false;
                }

                Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(DisableDevice_Finished), PRIORITY_BACKGROUND, new object[] { info });
            }
        }

        private void DisableDevice_Finished(EnableDevice_Info info)
        {
            if (info.Sender != null)
            {
                if (info.Success)
                {
                    var config = ((Configuration)info.DataObject);

                    // Raise DeviceUpdated Event
                    var args = new DeviceManager.DeviceUpdateArgs();
                    args.Sender = this;
                    args.Event = DeviceManager.DeviceUpdateEvent.Changed;
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
                    Configuration refConfig = null;
                    if (change > 0) refConfig = (Configuration)items[items.IndexOf(selectedItems[0]) - 1];
                    else if (change < 0) refConfig = (Configuration)items[items.IndexOf(selectedItems[selectedItems.Count - 1]) + 1];

                    int refIndex = refConfig.Index;

                    for (var x = 0; x <= selectedItems.Count - 1; x++)
                    {
                        int adjChange = 0;
                        if (change > 0) adjChange = change + ((selectedItems.Count - 1) - x);
                        else if (change < 0) adjChange = change - x;

                        var config = (Configuration)selectedItems[x];

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

                        var config = (Configuration)items[info.OldListIndex];
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
                    if (cell != null)
                        cell.Focus();
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
            if (AddDeviceSelected != null) AddDeviceSelected();
        }

        private void EditClicked()
        {
            foreach (var device in Devices_DG.SelectedItems)
            {
                var config = (Configuration)device;

                if (EditSelected != null) EditSelected(config);
            }
        }

        private void EditTableClicked()
        {
            foreach (var device in Devices_DG.SelectedItems)
            {
                var config = (Configuration)device;

                if (EditTableSelected != null) EditTableSelected(config);
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
                var config = (Configuration)Devices_DG.SelectedItem;

                if (config != null)
                {
                    if (CopyDeviceSelected != null) CopyDeviceSelected(config);
                }
            }
        }

        private void RemoveClicked()
        {
            if (Devices_DG.SelectedItems != null && Devices_DG.SelectedItems.Count > 0)
            {
                var configs = new List<Configuration>();

                foreach (var item in Devices_DG.SelectedItems)
                {
                    var config = item as Configuration;
                    if (config != null) configs.Add(config);
                }

                if (configs.Count > 0) RemoveDevices(configs);
            }
        }

        private void ShareClicked()
        {
            if (Devices_DG.SelectedItem != null)
            {
                var config = (Configuration)Devices_DG.SelectedItem;

                if (config != null)
                {
                    if (ShareDeviceSelected != null) ShareDeviceSelected(config);
                }
            }
        }

        /// <summary>
        /// Save a copy of this file to the Local Devices Directory (ex. C:\TrakHound\Devices\)
        /// </summary>
        private void SaveClicked()
        {
            foreach (var device in Devices_DG.SelectedItems)
            {
                var config = (Configuration)device;

                Configuration.Save(config);
            }

            try
            {
                System.Diagnostics.Process.Start("explorer", FileLocations.Devices);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SaveClicked() :: Exception :: " + ex.Message);
            }
        }

        #endregion

        #region "Datarow Buttons"

        private void ClientEnabled_Checked(object sender, RoutedEventArgs e)
        {
            EnableDevice((DataGridCellCheckBox)sender, ManagementType.Client);
        }

        private void ClientEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            DisableDevice((DataGridCellCheckBox)sender, ManagementType.Client);
        }

        private void ServerEnabled_Checked(object sender, RoutedEventArgs e)
        {
            EnableDevice((DataGridCellCheckBox)sender, ManagementType.Server);
        }

        private void ServerEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            DisableDevice((DataGridCellCheckBox)sender, ManagementType.Server);
        }

        private void Edit_Clicked(TH_WPF.Button bt)
        {
            if (bt.DataObject != null)
            {
                var config = (Configuration)bt.DataObject;

                if (EditSelected != null) EditSelected(config);
            }
        }

        #endregion

        #region "Toolbar Buttons"

        private void Add_Toolbar_Clicked(TH_WPF.Button bt) { AddClicked(); }

        private void MoveUp_Toolbar_Clicked(TH_WPF.Button bt) { MoveUpClicked(); }

        private void MoveDown_Toolbar_Clicked(TH_WPF.Button bt) { MoveDownClicked(); }

        private void Edit_Toolbar_Clicked(TH_WPF.Button bt) { EditClicked(); }

        private void EditTable_Toolbar_Clicked(TH_WPF.Button bt) { EditTableClicked(); }

        private void Refresh_Toolbar_Clicked(TH_WPF.Button bt) { RefreshClicked(); }

        private void Copy_Toolbar_Clicked(TH_WPF.Button bt) { CopyClicked(); }

        private void Remove_Toolbar_Clicked(TH_WPF.Button bt) { RemoveClicked(); }

        private void Share_Toolbar_Clicked(TH_WPF.Button bt) { ShareClicked(); }

        #endregion

        #region "Context Menu Buttons"

        private void Add_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { AddClicked(); }

        private void Edit_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { EditClicked(); }

        private void EditTable_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { EditTableClicked(); }

        private void Copy_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { CopyClicked(); }

        private void Remove_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { RemoveClicked(); }

        private void Share_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { ShareClicked(); }

        private void Save_DataGridRowContextMenu_Click(object sender, RoutedEventArgs e) { SaveClicked(); }

        #endregion

        private void Grid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { Devices_DG.SelectedItems.Clear(); }

        private void Devices_DG_SelectionChanged(object sender, SelectionChangedEventArgs e) { SharedDevices_DG.UnselectAll(); }

        private void SharedDevices_DG_SelectionChanged(object sender, SelectionChangedEventArgs e) { Devices_DG.UnselectAll(); }

    }

}
