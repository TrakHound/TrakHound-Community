// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TH_Global;

namespace TH_DeviceManager.AddDevice
{
    /// <summary>
    /// Page that contains the GUI for adding a new device. 
    /// Acts as host to IPage classes in the TH_DeviceManager.AddDevice.Pages namespace
    /// </summary>
    public partial class Page : UserControl, IPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "IPage"

        public string Title { get { return "Add Device"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Add_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { if (PageClosed != null) PageClosed(); }
        public bool Closing() { return true; }

        #endregion

        /// <summary>
        /// Event to request to open the Device List Page
        /// </summary>
        public event PageSelected_Handler DeviceListSelected;

        /// <summary>
        /// Event to request to open the Edit Table Page
        /// </summary>
        public event DeviceSelected_Handler EditTableSelected;

        /// <summary>
        /// Event to notify that this page has closed
        /// </summary>
        public event PageSelected_Handler PageClosed;

        /// <summary>
        /// Parent DeviceManager object
        /// </summary>
        public DeviceManager DeviceManager { get; set; }


        /// <summary>
        /// Object for containing the currently displayed IPage object
        /// </summary>
        public IPage CurrentPage
        {
            get { return (IPage)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(IPage), typeof(Page), new PropertyMetadata(null));


        /// <summary>
        /// Raised the DeviceListSelected event to request to open the Device List Page
        /// </summary>
        public void OpenDeviceList()
        {
            if (DeviceListSelected != null) DeviceListSelected();
        }


        Pages.AutoDetect autoDetectPage;
        Pages.Manual manualPage;
        Pages.LoadFromFile loadFromFilePage;

        public void ShowAutoDetect()
        {
            if (autoDetectPage == null)
            {
                autoDetectPage = new Pages.AutoDetect();
                autoDetectPage.ParentPage = this;
                autoDetectPage.FindDevices();
            }

            CurrentPage = autoDetectPage;
        }

        public void ShowManual()
        {
            if (manualPage == null)
            {
                manualPage = new Pages.Manual();
                manualPage.ParentPage = this;
                manualPage.LoadCatalog();
            }

            CurrentPage = manualPage;
        }

        public void ShowLoadFromFile()
        {
            if (loadFromFilePage == null)
            {
                loadFromFilePage = new Pages.LoadFromFile();
                loadFromFilePage.ParentPage = this;
            }

            CurrentPage = loadFromFilePage;
        }

        public void ShowCreateNew()
        {


        }


        #region "Navigation (Side Panel) Buttons"

        private void AutoDetect_Clicked(TH_WPF.ListButton bt) { ShowAutoDetect(); }

        private void Manual_Clicked(TH_WPF.ListButton bt) { ShowManual(); }

        private void LoadFromFile_Clicked(TH_WPF.ListButton bt) { ShowLoadFromFile(); }

        private void CreateNew_Clicked(TH_WPF.ListButton bt) { ShowCreateNew(); }

        //private void AutoDetect_Clicked(TH_DeviceManager.Controls.PageItem item) { ShowAutoDetect(); }

        //private void Manual_Clicked(TH_DeviceManager.Controls.PageItem item) { ShowManual(); }

        //private void LoadFromFile_Clicked(TH_DeviceManager.Controls.PageItem item) { ShowLoadFromFile(); }

        //private void CreateNew_Clicked(TH_DeviceManager.Controls.PageItem item) { ShowCreateNew(); }

        #endregion

    }
}
