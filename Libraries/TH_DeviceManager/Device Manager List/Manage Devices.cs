using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Data;
using System.Xml;
using System.IO;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Server;
using TH_UserManagement.Management;
using TH_WPF;

using TH_DeviceManager.Controls;

namespace TH_DeviceManager
{
    public partial class DeviceManagerList
    {
        
        #region "Remove Devices"

        class RemoveDevices_Info
        {
            public List<DeviceInfo> DeviceInfos { get; set; }
            public bool Success { get; set; }
        }

        void RemoveDevices(List<DeviceInfo> infos)
        {
            string msg = null;
            if (infos.Count == 1) msg = "Are you sure you want to permanently remove this device?";
            else msg = "Are you sure you want to permanently remove these " + infos.Count.ToString() + " devices?";

            string title = null;
            if (infos.Count == 1) msg = "Remove Device";
            else msg = "Remove " + infos.Count.ToString() + " Devices";

            bool? result = TH_WPF.MessageBox.Show(msg, title, TH_WPF.MessageBoxButtons.YesNo);
            if (result == true)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(RemoveDevices_Worker), infos);
            }
        }

        void RemoveDevices_Worker(object o)
        {
            var removeInfo = new RemoveDevices_Info();

            if (o != null)
            {
                var infos = (List<DeviceInfo>)o;

                removeInfo.DeviceInfos = infos;

                foreach (var info in infos)
                {
                    if (info.Configuration != null && DeviceManager != null)
                    {
                        DeviceManager.RemoveDevice(info.Configuration);
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action<RemoveDevices_Info>(RemoveDevices_Finshed), PRIORITY_BACKGROUND, new object[] { removeInfo });
        }

        void RemoveDevices_Finshed(RemoveDevices_Info removeInfo)
        {
            if (removeInfo.Success)
            {
                foreach (var info in removeInfo.DeviceInfos)
                {
                    var config = info.Configuration;

                    if (config != null)
                    {
                        // Raise DeviceUpdated Event
                        var args = new DeviceUpdateArgs();
                        args.Event = DeviceUpdateEvent.Removed;
                        UpdateDevice(config, args);
                    }

                    var index = Devices.ToList().FindIndex(x => x.UniqueId == info.UniqueId);
                    if (index >= 0) Devices.RemoveAt(index);
                }
            }
            else
            {
                TH_WPF.MessageBox.Show("An error occured while attempting to Remove Device. Please try again.", "Remove Device Error", MessageBoxButtons.Ok);
                LoadDevices();
            }
        }

        #endregion

        #region "Enable Device"

        class EnableDevice_Info
        {
            public DataGridCellCheckBox Sender { get; set; }
            public bool Success { get; set; }
            public ManagementType Type { get; set; }
            public object DataObject { get; set; }
        }

        void EnableDevice(DataGridCellCheckBox chk, ManagementType type)
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

        void EnableDevice_Worker(object o)
        {
            if (o != null)
            {
                EnableDevice_Info info = (EnableDevice_Info)o;

                var config = ((DeviceInfo)info.DataObject).Configuration;

                if (DeviceManager != null) DeviceManager.EnableDevice(config, info.Type);

                // If changes were successful, then update DeviceManager's Congifuration
                if (info.Success)
                {
                    if (info.Type == ManagementType.Client) config.ClientEnabled = true;
                    else if (info.Type == ManagementType.Server) config.ServerEnabled = true;
                }

                this.Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(EnableDevice_Finished), PRIORITY_BACKGROUND, new object[] { info });
            }
        }

        void EnableDevice_Finished(EnableDevice_Info info)
        {
            if (info.Sender != null)
            {
                if (info.Success)
                {
                    var config = ((DeviceInfo)info.DataObject).Configuration;

                    // Raise DeviceUpdated Event
                    var args = new DeviceUpdateArgs();
                    args.Event = DeviceUpdateEvent.Added;
                    UpdateDevice(config, args);
                }
                // If not successful then set Checkbox back to previous state
                else info.Sender.IsChecked = false;
            }
        }

        #endregion

        #region "Disable Device"

        void DisableDevice(DataGridCellCheckBox chk, ManagementType type)
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

        void DisableDevice_Worker(object o)
        {
            if (o != null)
            {
                EnableDevice_Info info = (EnableDevice_Info)o;

                var config = ((DeviceInfo)info.DataObject).Configuration;

                if (DeviceManager != null) DeviceManager.DisableDevice(config, info.Type);

                // If changes were successful, then update DeviceManager's Congifuration
                if (info.Success)
                {
                    if (info.Type == ManagementType.Client) config.ClientEnabled = false;
                    else if (info.Type == ManagementType.Server) config.ServerEnabled = false;
                }

                this.Dispatcher.BeginInvoke(new Action<EnableDevice_Info>(DisableDevice_Finished), PRIORITY_BACKGROUND, new object[] { info });
            }
        }

        void DisableDevice_Finished(EnableDevice_Info info)
        {
            if (info.Sender != null)
            {
                if (info.Success)
                {
                    var config = ((DeviceInfo)info.DataObject).Configuration;

                    // Raise DeviceUpdated Event
                    var args = new DeviceUpdateArgs();
                    args.Event = DeviceUpdateEvent.Removed;
                    UpdateDevice(config, args);
                }
                // If not successful then set Checkbox back to previous state
                else info.Sender.IsChecked = true;
            }
        }

        #endregion

        #region "Device Indexes"

        class DeviceIndex_Info
        {
            public bool Success { get; set; }
            public Configuration Configuration { get; set; }
            public int NewIndex { get; set; }
        }

        void ChangeDeviceIndex(int newIndex, Configuration config)
        {
            var info = new DeviceIndex_Info();
            info.Configuration = config;
            info.NewIndex = newIndex;

            ThreadPool.QueueUserWorkItem(new WaitCallback(ChangeDeviceIndex_Worker), info);
        }

        void ChangeDeviceIndex_Worker(object o)
        {
            if (o != null)
            {
                var info = (DeviceIndex_Info)o;

                var config = info.Configuration;

                if (DeviceManager != null) DeviceManager.ChangeDeviceIndex(config, info.NewIndex);
            }
        }

        public static void SelectRowByIndexes(DataGrid dataGrid, params int[] rowIndexes)
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

        public static DataGridCell GetCell(DataGrid dataGrid, DataGridRow rowContainer, int column)
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

        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
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

    }

}
