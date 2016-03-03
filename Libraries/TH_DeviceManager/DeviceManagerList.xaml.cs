// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using System.Windows.Markup;
using System.Windows.Data;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement.Management;

using TH_DeviceManager.Controls;

namespace TH_DeviceManager
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DeviceManagerList : UserControl
    {
        public DeviceManagerList()
        {
            InitializeComponent();
            DataContext = this;
        }

        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;
                LoadDevices();
            }
        }

        public delegate void DeviceSelected_Handler(Configuration config);
        public event DeviceSelected_Handler DeviceEditSelected;
        public event DeviceSelected_Handler DeviceEditTableSelected;
        public event DeviceSelected_Handler ShareDeviceSelected;
        public event DeviceSelected_Handler CopyDeviceSelected;

        public delegate void PageSelected_Handler();
        public event PageSelected_Handler AddDeviceSelected;



        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;

        private void AddClicked()
        {
            if (AddDeviceSelected != null) AddDeviceSelected();
        }

        class MoveInfo
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

        private void MoveUpClicked()
        {
            Move(1);
        }

        /// <summary>
        /// Move Item Index in DeviceInfo list. 
        /// </summary>
        /// <param name="change">Postive number moves items up. Negative number moves items down</param>
        private void Move(int change)
        {
            var items = Device_DG.Items;
            var selectedItems = Device_DG.SelectedItems;

            foreach (var selectedItem in selectedItems) Console.WriteLine(selectedItems.IndexOf(selectedItem));

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
                    DeviceInfo refInfo = null;
                    if (change > 0) refInfo = (DeviceInfo)items[items.IndexOf(selectedItems[0]) - 1];
                    else if (change < 0) refInfo = (DeviceInfo)items[items.IndexOf(selectedItems[selectedItems.Count - 1]) + 1];

                    int refIndex = refInfo.Index;

                    for (var x = 0; x <= selectedItems.Count - 1; x++)
                    {
                        int adjChange = 0;
                        if (change > 0) adjChange = change + ((selectedItems.Count - 1) - x);
                        //else if (change < 0) adjChange = change - ((selectedItems.Count - 1) - x);
                        else if (change < 0) adjChange = change - x;

                        var deviceInfo = (DeviceInfo)selectedItems[x];

                        int listIndex = items.IndexOf(selectedItems[x]);

                        // Check if still valid (maybe not needed if checked above but just to be safe)
                        //valid = false;
                        //if (change > 0) valid = listIndex > 0;
                        //else if (change < 0) valid = listIndex < items.Count - 1;

                        //if (valid)
                        //{


                        // Set new index
                        //int newIndex = refIndex - change;
                        int newIndex = refIndex - adjChange;

                        string a = deviceInfo.Index.ToString();
                        string a1 = refIndex.ToString();
                        string b = newIndex.ToString();
                        string c = listIndex.ToString();
                        string d = (listIndex - change).ToString();
                        string e = adjChange.ToString();

                        if (change > 0) Console.WriteLine("Move Up :: " + a + " to " + b + " above " + a1 + " :: " + c + " to " + d + " :: " + e);
                            else if (change < 0) Console.WriteLine("Move Down :: " + a + " to " + b + " below " + a1 + " :: " + c + " to " + d + " :: " + e);

                        ChangeDeviceIndex(newIndex, deviceInfo.Configuration);

                            var info = new MoveInfo(listIndex, listIndex - change, newIndex);
                            infos.Add(info);
                        //}
                    }

                    // Create new array for selecting rows after sorting
                    int[] selectedIndexes = new int[infos.Count];

                    // Set new indexes
                    for (var x = 0; x <= infos.Count - 1; x++)
                    {
                        var info = infos[x];

                        var deviceInfo = (DeviceInfo)items[info.OldListIndex];
                        deviceInfo.Configuration.Index = info.DeviceIndex;
                        deviceInfo.Index = info.DeviceIndex;

                        selectedIndexes[x] = info.NewListIndex;
                    }

                    // Sort Devices list using new Configuration.Index
                    Devices.Sort();

                    // Select Rows using new List Indexes
                    SelectRowByIndexes(Device_DG, selectedIndexes);
                }
            }
        }

        private void MoveDownClicked()
        {
            Move(-1);

            //var items = Device_DG.Items;
            //var selectedItems = Device_DG.SelectedItems;

            //int[,] newIndexes = new int[selectedItems.Count, 2];

            //if (selectedItems.Count > 0)
            //{
            //    int lastIndex = items.IndexOf(selectedItems[selectedItems.Count - 1]);
            //    if (lastIndex < items.Count - 1)
            //    {
            //        for (var x = 0; x <= selectedItems.Count - 1; x++)
            //        {
            //            var info = (DeviceInfo)selectedItems[x];

            //            int index = info.Index;

            //            int listIndex = items.IndexOf(selectedItems[x]);

            //            if (listIndex < items.Count - 1)
            //            {
            //                var belowInfo = (DeviceInfo)items[listIndex + 1];
            //                int belowIndex = belowInfo.Index;

            //                //int newIndex = belowIndex + (Math.Abs(index - belowIndex) / 2);
            //                int newIndex = belowIndex + 1;

            //                Console.WriteLine("Move Down :: " + index.ToString() + " to " + newIndex.ToString());

            //                ChangeDeviceIndex(newIndex, info.Configuration);

            //                newIndexes[x, 0] = listIndex;
            //                newIndexes[x, 1] = listIndex + 1;
            //            }
            //        }

            //        int[] selectedIndexes = new int[newIndexes.GetLength(0)];

            //        for (var i = 0; i <= newIndexes.GetLength(0) - 1; i++)
            //        {
            //            int oldIndex = newIndexes[i, 0];
            //            int newIndex = newIndexes[i, 1];

            //            var info = (DeviceInfo)items[oldIndex];

            //            info.Configuration.Index = newIndex;
            //            info.Index = newIndex;

            //            selectedIndexes[i] = newIndex;
            //        }

            //        Devices.Sort();

            //        SelectRowByIndexes(Device_DG, selectedIndexes);
            //    }
            //}

            ////var items = Device_DG.Items;
            ////var selectedItems = Device_DG.SelectedItems;

            ////int[,] newIndexes = new int[selectedItems.Count, 2];

            ////if (selectedItems.Count > 0)
            ////{
            ////    int lastIndex = items.IndexOf(selectedItems[selectedItems.Count - 1]);
            ////    if (lastIndex < items.Count - 1)
            ////    {
            ////        for (var x = 0; x <= selectedItems.Count - 1; x++)
            ////        {
            ////            var info = (DeviceInfo)selectedItems[x];

            ////            int index = info.Index;

            ////            int listIndex = items.IndexOf(selectedItems[x]);

            ////            if (listIndex < items.Count - 1)
            ////            {
            ////                var belowInfo = (DeviceInfo)items[listIndex + 1];
            ////                int belowIndex = belowInfo.Index;

            ////                //int newIndex = belowIndex + (Math.Abs(index - belowIndex) / 2);
            ////                int newIndex = belowIndex + 1;

            ////                Console.WriteLine("Move Down :: " + index.ToString() + " to " + newIndex.ToString());

            ////                ChangeDeviceIndex(newIndex, info.Configuration);

            ////                newIndexes[x, 0] = listIndex;
            ////                newIndexes[x, 1] = listIndex + 1;
            ////            }
            ////        }

            ////        int[] selectedIndexes = new int[newIndexes.GetLength(0)];

            ////        for (var i = 0; i <= newIndexes.GetLength(0) - 1; i++)
            ////        {
            ////            var info = (DeviceInfo)items[newIndexes[i,0]];

            ////            info.Configuration.Index = newIndexes[i,1];
            ////            info.Index = newIndexes[i, 1];

            ////            selectedIndexes[i] = newIndexes[i, 1];
            ////        }

            ////        Devices.Sort();

            ////        SelectRowByIndexes(Device_DG, selectedIndexes);
            ////    }
            ////}
        }

        private void EditClicked()
        {
            foreach (var device in Device_DG.SelectedItems)
            {
                var info = (DeviceInfo)device;

                if (DeviceEditSelected != null) DeviceEditSelected(info.Configuration);
            }
        }

        private void EditTableClicked()
        {
            foreach (var device in Device_DG.SelectedItems)
            {
                var info = (DeviceInfo)device;

                if (DeviceEditTableSelected != null) DeviceEditTableSelected(info.Configuration);
            }
        }

        private void RefreshClicked()
        {
            LoadDevices();
        }

        private void CopyClicked()
        {
            if (Device_DG.SelectedItem != null)
            {
                var info = (DeviceInfo)Device_DG.SelectedItem;

                if (info.Configuration != null)
                {
                    if (CopyDeviceSelected != null) CopyDeviceSelected(info.Configuration);
                }
            }
        }

        private void RemoveClicked()
        {
            if (Device_DG.SelectedItems != null && Device_DG.SelectedItems.Count > 0)
            {
                var infos = new List<DeviceInfo>();

                foreach (var item in Device_DG.SelectedItems)
                {
                    var info = item as DeviceInfo;
                    if (info != null) infos.Add(info);
                }

                if (infos.Count > 0) RemoveDevices(infos);
            }
        }

        private void ShareClicked()
        {
            if (Device_DG.SelectedItem != null)
            {
                var info = (DeviceInfo)Device_DG.SelectedItem;

                if (info.Configuration != null)
                {
                    if (ShareDeviceSelected != null) ShareDeviceSelected(info.Configuration);
                }
            }
        }

        private void SaveClicked()
        {

        }


        #region "Datarow Buttons"

        private void ClientEnabled_Checked(object sender, RoutedEventArgs e)
        {
            EnableDevice((DataGridCellCheckBox)sender, DeviceManagerType.Client);
        }

        private void ClientEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            DisableDevice((DataGridCellCheckBox)sender, DeviceManagerType.Client);
        }

        private void ServerEnabled_Checked(object sender, RoutedEventArgs e)
        {
            EnableDevice((DataGridCellCheckBox)sender, DeviceManagerType.Server);
        }

        private void ServerEnabled_Unchecked(object sender, RoutedEventArgs e)
        {
            DisableDevice((DataGridCellCheckBox)sender, DeviceManagerType.Server);
        }

        private void Edit_Clicked(TH_WPF.Button bt)
        {
            if (bt.DataObject != null)
            {
                var info = (DeviceInfo)bt.DataObject;

                if (DeviceEditSelected != null) DeviceEditSelected(info.Configuration);
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

        private void Grid_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Device_DG.SelectedItems.Clear();
        }
    }

}
