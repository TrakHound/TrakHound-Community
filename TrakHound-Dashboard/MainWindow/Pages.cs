// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using TrakHound;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound.Tools;
using TrakHound_Dashboard.Controls;
using TrakHound_Device_Manager;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {
        ObservableCollection<TabHeader> _tabHeaders;
        public ObservableCollection<TabHeader> TabHeaders
        {
            get
            {
                if (_tabHeaders == null) _tabHeaders = new ObservableCollection<TabHeader>();
                return _tabHeaders;
            }
            set
            {
                _tabHeaders = value;
            }
        }

        TabHeader SelectedTab { get; set; }

        public TabPage CurrentPage
        {
            get { return (TabPage)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(TabPage), typeof(MainWindow), new PropertyMetadata(null));


        public void AddTab(IPage page, string name = null, ImageSource image = null, string tag = null)
        {
            string txt = page.Title;

            ImageSource img = null;
            if (page.Image != null) img = new BitmapImage(page.Image);

            if (name != null) txt = name;
            if (image != null) img = image;

            TabHeader header = FindTab(page, txt, tag);
            if (header == null)
            {
                header = new TabHeader();
                header.Text = txt;
                header.Image = img;
                header.Tag = tag;
                header.Page = new TabPage(page);

                header.Clicked += TabHeader_Clicked;
                header.CloseClicked += TabHeader_CloseClicked;
                header.Opened += TabHeader_Opened;
                header.Closed += TabHeader_Closed;
                page.SendData += SendEventData;

                // Send Current User Data
                SendCurrentUser(page);

                // Send Current Device List
                SendCurrentDevices(page);

                TabHeaders.Add(header);

                header.Open(TabHeaders.Count == 1);
            }

            SelectTab(header);
        }

        private void TabHeader_Opened(object sender, EventArgs e)
        {
            SetTabWidths();
        }

        private void TabHeader_Closed(object sender, EventArgs e)
        {
            var tab = (TabHeader)sender;
            if (TabHeaders.Contains(tab)) TabHeaders.Remove(tab);

            tab.Clicked -= TabHeader_Clicked;
            tab.CloseClicked -= TabHeader_CloseClicked;
            tab.Opened -= TabHeader_Opened;
            tab.Closed -= TabHeader_Closed;

            SetTabWidths();
        }

        private void SetTabWidths()
        {
            if (TabHeaders.Count > 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    double windowWidth = TabPanel.ActualWidth;
                    double tabsWidth = GetTabPanelWidth();

                    if (tabsWidth > windowWidth)
                    {
                        var tabs = TabHeaders.ToList().OrderByDescending(x => x.ActualWidth).ToList();
                        double widestTab = tabs[0].ActualWidth;

                        tabs = tabs.FindAll(x => x.ActualWidth >= widestTab);

                        foreach (var tab in tabs)
                        {
                            double increment = (tabsWidth - windowWidth) / tabs.Count;

                            int index = TabHeaders.ToList().FindIndex(x => x.Id == tab.Id);
                            if (index >= 0)
                            {
                                var t = TabHeaders[index];
                                t.MaxWidth = Math.Max(TabHeader.MIN_WIDTH, t.ActualWidth - increment);
                            }

                            tabsWidth = GetTabPanelWidth();
                        }
                    }
                    else if (tabsWidth < windowWidth)
                    {
                        var tabs = TabHeaders.ToList().OrderByDescending(x => x.ActualWidth).ToList();
                        double widestTab = tabs[0].ActualWidth;

                        tabs = tabs.FindAll(x => x.ActualWidth >= widestTab);

                        foreach (var tab in tabs)
                        {
                            double increment = (windowWidth - tabsWidth) / tabs.Count;

                            int index = TabHeaders.ToList().FindIndex(x => x.Id == tab.Id);
                            if (index >= 0)
                            {
                                var t = TabHeaders[index];

                                double width = t.MaxWidth + increment;
                                width = Math.Max(width, TabHeader.MIN_WIDTH);
                                width = Math.Min(width, TabHeader.MAX_WIDTH);

                                t.MaxWidth = width;
                            }

                            tabsWidth = GetTabPanelWidth();
                        }
                    }
                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }
        }

        private double GetTabPanelWidth()
        {
            double tabsWidth = 0;
            foreach (var tab in TabHeaders) tabsWidth += tab.ActualWidth;
            return tabsWidth;
        }

        #region "Select"

        public void SelectTab(string name, string tag = null)
        {
            var tab = FindTab(name, tag);
            if (tab != null) SelectTab(tab);
        }

        public void SelectTab(IPage page, string tag = null)
        {
            SelectTab(page.Title, tag);
        }

        public void SelectTab(TabHeader header)
        {
            // Unselect other tabs
            foreach (var oheader in TabHeaders) if (oheader != header) oheader.IsSelected = false;

            header.IsSelected = true;

            // Fade in if not previously set
            if (CurrentPage == null) AnimateTabPageOpen();

            CurrentPage = header.Page;
            SelectedTab = header;
        }

        public void SelectTab(int index)
        {
            if (index >= 0 && index <= TabHeaders.Count - 1)
            {
                SelectTab(TabHeaders[index]);
            }
        }


        const double TAB_PAGE_OPEN_ANIMATION_TIME = 200;
        const double TAB_PAGE_CLOSE_ANIMATION_TIME = 200;

        private void AnimateTabPageOpen()
        {
            var animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = 1;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(TAB_PAGE_OPEN_ANIMATION_TIME));

            var ease = new CubicEase();
            ease.EasingMode = EasingMode.EaseIn;

            animation.EasingFunction = ease;
            CurrentPage_CONTENT.BeginAnimation(OpacityProperty, animation);
        }

        private void AnimateTabPageClose()
        {
            var animation = new DoubleAnimation();
            animation.From = 1;
            animation.To = 0;
            animation.Completed += AnimateTabPageClose_Completed;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(TAB_PAGE_CLOSE_ANIMATION_TIME));

            var ease = new CubicEase();
            ease.EasingMode = EasingMode.EaseOut;

            animation.EasingFunction = ease;
            CurrentPage_CONTENT.BeginAnimation(OpacityProperty, animation);
        }

        private void AnimateTabPageClose_Completed(object sender, EventArgs e)
        {
            CurrentPage = null;
        }

        #endregion

        #region "Find"

        public TabHeader FindTab(string name, string tag = null)
        {
            int index = TabHeaders.ToList().FindIndex(x => x.Text == name && x.Tag == tag);
            if (index >= 0)
            {
                return TabHeaders[index];
            }
            return null;
        }

        public TabHeader FindTab(IPage page, string name = null, string tag = null)
        {
            string title = page.Title;

            if (name != null) title = name;

            int index = TabHeaders.ToList().FindIndex(x => x.Text == title && x.Tag == tag);
            if (index >= 0)
            {
                return TabHeaders[index];
            }
            return null;
        }

        public TabHeader FindTab(int index)
        {
            if (index >= 0 && index <= TabHeaders.Count - 1)
            {
                return TabHeaders[index];
            }
            return null;
        }


        public int FindTabIndex(string name, string tag = null)
        {
            int index = TabHeaders.ToList().FindIndex(x => x.Text == name && x.Tag == tag);
            return index;
        }

        public int FindTabIndex(TabHeader header)
        {
            int index = TabHeaders.ToList().FindIndex(x => x.Text == header.Text);
            return index;
        }

        #endregion

        #region "Close"

        public void CloseTab(string name, string tag = null)
        {
            var tab = FindTab(name, tag);
            if (tab != null)
            {
                bool cancel = CheckCancel(tab);
                if (!cancel)
                {
                    tab.Close();

                    // If current tab then switch to tab to the left
                    if (CurrentPage == tab.Page)
                    {
                        int tabIndex = FindTabIndex(name, tag);
                        if (tabIndex >= 0)
                        {
                            if (tabIndex > 0) SelectTab(tabIndex - 1);
                            else if (TabHeaders.Count > 1) SelectTab(tabIndex + 1);
                            else AnimateTabPageClose();
                        }
                    }

                    if (tab.Page != null && tab.Page.PageContent != null)
                    {
                        Dispatcher.BeginInvoke(new Action(() => { tab.Page.PageContent.Closed(); }));
                    }
                }
            }
        }

        private bool CheckCancel(TabHeader header)
        {
            bool result = false;

            if (header.Page != null && header.Page.PageContent != null)
            {
                result = !header.Page.PageContent.Closing();
            }

            return result;
        }

        #endregion

        #region "TabHeader Event Handlers"

        void TabHeader_Clicked(TabHeader header) { SelectTab(header.Text, header.Tag); }

        void TabHeader_CloseClicked(TabHeader header) { CloseTab(header.Text, header.Tag); }

        #endregion

        #region "Event Data"

        private void SendEventData(EventData data)
        {
            LoadDevicesRequested(data);
            ShowDeviceManagerRequested(data);
            ShowRequested(data);

            foreach (var tabHeader in TabHeaders)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (tabHeader.Page != null)
                    {
                        if (tabHeader.Page.PageContent != null && tabHeader.Page.PageContent != data.Sender)
                        {
                            tabHeader.Page.PageContent.GetSentData(data);
                        }
                    }
                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
            }
        }

        private void LoadDevicesRequested(EventData data)
        {
            if (data != null && data.Id != null)
            {
                if (data.Id == "LOAD_DEVICES")
                {
                    LoadDevices();
                }
            }
        }

        private void ShowDeviceManagerRequested(EventData data)
        {
            if (data != null && data.Id != null)
            {
                if (data.Id == "SHOW_DEVICE_MANAGER")
                {
                    DeviceManager_DeviceList_Open();
                }
            }
        }

        /// <summary>
        /// Page has sent a message requesting to be shown as a tab
        /// de_d.id = 'show'
        /// de_d.data01 = Configuration
        /// de_d.data02 = Page (IPage)
        /// de_d.data03 = [Optional] Alternate Title
        /// de_d.data04 = [Optional] Tag
        /// </summary>
        /// <param name="de_d"></param>
        private void ShowRequested(EventData data)
        {
            if (data != null)
            {
                if (data.Id == "SHOW")
                {
                    if (data.Data02 != null)
                    {
                        if (typeof(IPage).IsAssignableFrom(data.Data02.GetType()))
                        {
                            var page = (IPage)data.Data02;

                            string title = page.Title;

                            ImageSource img = null;
                            if (page.Image != null) img = new BitmapImage(page.Image);

                            string tag = null;

                            if (data.Data03 != null) title = data.Data03.ToString();
                            if (data.Data04 != null) tag = data.Data04.ToString();

                            AddTab(page, title, img, tag);
                        }
                    }
                }
            }
        }

        #endregion

        void ChangePage_Forward()
        {
            if (TabHeaders.Count > 1)
            {
                if (SelectedTab != null)
                {
                    int index = FindTabIndex(SelectedTab);
                    if (index < TabHeaders.Count - 1) SelectTab(index + 1);
                    else SelectTab(0);
                }
            }
        }

        void ChangePage_Backward()
        {
            if (TabHeaders.Count > 1)
            {
                if (SelectedTab != null)
                {
                    int index = FindTabIndex(SelectedTab);
                    if (index > 0) SelectTab(index - 1);
                    else SelectTab(TabHeaders.Count - 1);
                }
            }
        }

        #region "About"

        public PageManager aboutManager;

        void About_Initialize()
        {
            if (aboutManager == null)
            {
                aboutManager = new PageManager();
                aboutManager.TabTitle = "About";
                aboutManager.TabImage = new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/About_01.png");
                aboutManager.AddPage(new Pages.About.Information.Page());
                aboutManager.AddPage(new Pages.About.License.Page());
            }
        }

        public void About_Open()
        {
            About_Initialize();

            AddTab(aboutManager);
        }

        #endregion

        #region "Device Manager"

        #region "Device List"

        public DeviceList deviceListPage;

        void DeviceManager_DeviceList_Initialize()
        {
            if (deviceListPage == null)
            {
                deviceListPage = new DeviceList();

                deviceListPage.PageClosed += DeviceListPage_PageClosed;
                deviceListPage.AddDeviceSelected += DeviceManager_DeviceList_AddDeviceSelected;
                deviceListPage.EditSelected += DeviceManager_DeviceList_DeviceEditSelected;
            }
        }

        private void DeviceListPage_PageClosed()
        {
            deviceListPage.PageClosed -= DeviceListPage_PageClosed;
            deviceListPage.AddDeviceSelected -= DeviceManager_DeviceList_AddDeviceSelected;
            deviceListPage.EditSelected -= DeviceManager_DeviceList_DeviceEditSelected;

            deviceListPage = null;
        }

        public void DeviceManager_DeviceList_Open()
        {
            DeviceManager_DeviceList_Initialize();

            if (FindTab(deviceListPage) == null) AddTab(deviceListPage);
            else SelectTab(deviceListPage);
        }

        private void DeviceManager_DeviceList_DeviceManagerListSelected() { DeviceManager_DeviceList_Open(); }

        private void DeviceManager_DeviceList_AddDeviceSelected() { DeviceManager_AddDevice_Open(); }

        private void DeviceManager_DeviceList_DeviceEditSelected(DeviceDescription device) { DeviceManager_EditDevice_Open(device); }

        #endregion

        #region "Add Device"

        public TrakHound_Device_Manager.AddDevice.Page addDevicePage;

        public void DeviceManager_AddDevice_Initialize()
        {
            if (addDevicePage == null)
            {
                addDevicePage = new TrakHound_Device_Manager.AddDevice.Page();
                addDevicePage.ShowAutoDetect();

                addDevicePage.DeviceListSelected += DeviceManager_AddDevice_DeviceListSelected;
                addDevicePage.PageClosed += AddDevicePage_PageClosed;
            }
        }

        private void AddDevicePage_PageClosed()
        {
            addDevicePage.DeviceListSelected -= DeviceManager_AddDevice_DeviceListSelected;
            addDevicePage.PageClosed -= AddDevicePage_PageClosed;

            addDevicePage = null;
        }

        public void DeviceManager_AddDevice_Open()
        {
            DeviceManager_AddDevice_Initialize();

            if (FindTab(addDevicePage) == null) AddTab(addDevicePage);
            else SelectTab(addDevicePage);
        }

        private void DeviceManager_AddDevice_DeviceListSelected() { DeviceManager_DeviceList_Open(); }

        private void DeviceManager_AddDevice_EditTableSelected(DeviceConfiguration config) { }

        #endregion

        public void DeviceManager_EditDevice_Open(DeviceDescription device)
        {
            string title = "Edit Device - " + device.Description.Description;
            if (device.Description.DeviceId != null) title += " (" + device.Description.DeviceId + ")";

            string tag = device.UniqueId;

            var tab = FindTab(title, tag);
            if (tab == null)
            {
                var page = new EditPage(CurrentUser, device.UniqueId);

                ImageSource img = null;
                if (page.Image != null) img = new BitmapImage(page.Image);

                page.DeviceListSelected += DeviceManager_EditDevice_DeviceListSelected;

                AddTab(page, title, img, tag);
            }
            else
            {
                SelectTab(tab);
            }
        }

        private void DeviceManager_EditDevice_DeviceListSelected() { DeviceManager_DeviceList_Open(); }

        #endregion

        #region "Account Manager"

        public PageManager accountManager;

        Pages.Account.Page accountpage;

        void AccountManager_Initialize()
        {
            if (accountManager == null)
            {
                accountpage = new Pages.Account.Page();
                accountpage.LoadUserConfiguration(CurrentUser);
                accountpage.UserChanged += accountpage_UserChanged;

                accountManager = new PageManager();
                accountManager.TabTitle = "Account Management";
                accountManager.TabImage = new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/blank_profile_01_sm.png");
                accountManager.AddPage(accountpage);
            }
        }

        void accountpage_UserChanged(UserConfiguration userConfig)
        {
            CurrentUser = userConfig;
        }

        public void AccountManager_Open()
        {
            AccountManager_Initialize();

            AddTab(accountManager);
        }

        #endregion

        #region "Options"

        public PageManager optionsManager;

        void Options_Initialize()
        {
            if (optionsManager == null)
            {
                optionsManager = new PageManager();
                optionsManager.TabTitle = "Options";
                optionsManager.TabImage = new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/options_gear_30px.png");
                optionsManager.AddPage(new Pages.Options.General.Page());
                optionsManager.AddPage(new Pages.Options.API.Page());
                optionsManager.AddPage(new Pages.Options.Logger.Page());
            }
        }

        public void Options_AddPage(IPage page)
        {
            Options_Initialize();
            optionsManager.AddPage(page);
        }

        public void Options_Open()
        {
            Options_Initialize();

            AddTab(optionsManager);
        }

        #endregion

    }
}
