// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound_UI;

namespace TrakHound_Device_Manager.AddDevice
{
    /// <summary>
    /// Page that contains the GUI for adding a new device. 
    /// Acts as host to IPage classes in the TrakHound_Device_Manager.AddDevice.Pages namespace
    /// </summary>
    public partial class Page : UserControl, IPage
    {
        public Page()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        #region "IPage"

        public string Title { get { return "Add Device"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Device-Manager;component/Resources/Add_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { PageClosed?.Invoke(); }
        public bool Closing()
        {
            if (autoDetectPage != null) autoDetectPage.Closing();
            if (manualPage != null) manualPage.Closing();
            if (loadFromFilePage != null) loadFromFilePage.Closing();

            return true;
        }

        public event SendData_Handler SendData;

        public void GetSentData(EventData data) { }

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

        private Pages.AutoDetect autoDetectPage;
        private Pages.Manual manualPage;
        private Pages.LoadFromFile loadFromFilePage;


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
            DeviceListSelected?.Invoke();
        }


        public bool AutoDetectSelected
        {
            get { return (bool)GetValue(AutoDetectSelectedProperty); }
            set { SetValue(AutoDetectSelectedProperty, value); }
        }

        public static readonly DependencyProperty AutoDetectSelectedProperty =
            DependencyProperty.Register("AutoDetectSelected", typeof(bool), typeof(Page), new PropertyMetadata(true));

        public bool ManualSelected
        {
            get { return (bool)GetValue(ManualSelectedProperty); }
            set { SetValue(ManualSelectedProperty, value); }
        }

        public static readonly DependencyProperty ManualSelectedProperty =
            DependencyProperty.Register("ManualSelected", typeof(bool), typeof(Page), new PropertyMetadata(false));

        public bool LoadFromFileSelected
        {
            get { return (bool)GetValue(LoadFromFileSelectedProperty); }
            set { SetValue(LoadFromFileSelectedProperty, value); }
        }

        public static readonly DependencyProperty LoadFromFileSelectedProperty =
            DependencyProperty.Register("LoadFromFileSelected", typeof(bool), typeof(Page), new PropertyMetadata(false));


        public void ShowAutoDetect()
        {
            if (manualPage != null) manualPage.Closing();
            if (loadFromFilePage != null) loadFromFilePage.Closing();

            if (autoDetectPage == null)
            {
                autoDetectPage = new Pages.AutoDetect();
                autoDetectPage.ParentPage = this;
            }

            AutoDetectSelected = true;
            ManualSelected = false;
            LoadFromFileSelected = false;

            CurrentPage = autoDetectPage;
        }

        public void ShowManual()
        {
            if (autoDetectPage != null) autoDetectPage.Closing();
            if (loadFromFilePage != null) loadFromFilePage.Closing();

            if (manualPage == null)
            {
                manualPage = new Pages.Manual();
                manualPage.ParentPage = this;
            }

            AutoDetectSelected = false;
            ManualSelected = true;
            LoadFromFileSelected = false;

            CurrentPage = manualPage;
        }

        public void ShowLoadFromFile()
        {
            if (autoDetectPage != null) autoDetectPage.Closing();
            if (manualPage != null) manualPage.Closing();

            if (loadFromFilePage == null)
            {
                loadFromFilePage = new Pages.LoadFromFile();
                loadFromFilePage.ParentPage = this;
            }

            AutoDetectSelected = false;
            ManualSelected = false;
            LoadFromFileSelected = true;

            CurrentPage = loadFromFilePage;
        }

        public void ShowCreateNew()
        {


        }


        #region "Navigation (Side Panel) Buttons"

        private void AutoDetect_Clicked(TrakHound_UI.ListButton bt) { ShowAutoDetect(); }

        private void Manual_Clicked(TrakHound_UI.ListButton bt) { ShowManual(); }

        private void LoadFromFile_Clicked(TrakHound_UI.ListButton bt) { ShowLoadFromFile(); }

        private void CreateNew_Clicked(TrakHound_UI.ListButton bt) { ShowCreateNew(); }

        #endregion

    }
}
