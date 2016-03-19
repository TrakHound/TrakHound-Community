// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TH_Configuration;
using TH_Global;
using TH_UserManagement.Management;

namespace TH_DeviceManager.AddDevice.Pages
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

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/List_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

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


        private void Browse_Clicked(TH_WPF.Button bt) { LoadDeviceFromFile(); }

        private void DeviceManager_Clicked(TH_WPF.Button bt)
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
            Configuration config = Configuration.Read(path);
            if (config != null)
            {
                if (ParentPage.DeviceManager.CurrentUser != null)
                {
                    result = Configurations.AddConfigurationToUser(ParentPage.DeviceManager.CurrentUser, config);
                }
                // If not logged in Read from File in 'C:\TrakHound\'
                else
                {
                    result = Configuration.Save(config);
                }

                if (result) ParentPage.DeviceManager.AddDevice(config);
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

                if (added < paths.Length) TH_WPF.MessageBox.Show("Some devices did not get added correctly. Review the Developer Console for further information.");
            }
        }

        #endregion

    }
}
