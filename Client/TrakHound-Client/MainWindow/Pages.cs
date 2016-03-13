using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;

using TH_Global;
using TH_UserManagement.Management;
using TH_DeviceManager;

using TrakHound_Client.Controls;

namespace TrakHound_Client
{
    public partial class MainWindow
    {

        ObservableCollection<TabPage> _tabPages;
        public ObservableCollection<TabPage> TabPages
        {
            get
            {
                if (_tabPages == null) _tabPages = new ObservableCollection<TabPage>();
                return _tabPages;
            }
            set
            {
                _tabPages = value;
            }
        }

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


        public void AddPageAsTab(object page, string title, ImageSource image)
        {
            //// Check to see if Page already exists
            //TH_TabItem TI = Pages_TABCONTROL.Items.Cast<TH_TabItem>().ToList().Find(x => x.Title.ToString().ToLower() == title.ToLower());

            //if (TI == null)
            //{
            //    TI = new TH_TabItem();
            //    TI.Content = CreatePage(page); ;
            //    TI.Title = title;
            //    TI.Closed += TI_Closed;

            //    TH_TabHeader_Top header = new TH_TabHeader_Top();
            //    header.Text = title;
            //    header.Image = image;
            //    header.TabParent = TI;
            //    header.Clicked += header_Clicked;
            //    header.CloseClicked += header_CloseClicked;
            //    TI.TH_Header = header;

            //    int zlevel = int.MaxValue;

            //    // Move all of the existing tabs to the front so that the new tab is behind it (so it can "slide" in behind it)
            //    for (int x = 0; x <= PageTabHeaders.Count - 1; x++)
            //    {
            //        TH_TabHeader_Top tabHeader = (TH_TabHeader_Top)PageTabHeaders[x];
            //        Panel.SetZIndex(tabHeader, zlevel - x);
            //    }

            //    PageTabHeaders.Add(header);

            //    Panel.SetZIndex(header, -1);

            //    TI.Template = (ControlTemplate)TryFindResource("TabItemControlTemplate");

            //    Pages_TABCONTROL.Items.Add(TI);
            //    Pages_TABCONTROL.SelectedItem = TI;
            //}
            //else
            //{
            //    Pages_TABCONTROL.SelectedItem = TI;
            //}
        }

        public void AddTab(IPage page, string name = null, ImageSource image = null)
        {
            string txt = page.Title;
            ImageSource img = page.Image;

            if (name != null) txt = name;
            if (image != null) img = image;

            var header = new TabHeader();
            header.Text = txt;
            header.Image = img;
            header.Page = new TabPage(page);

            header.Clicked += TabHeader_Clicked;
            header.CloseClicked += TabHeader_CloseClicked;
            header.Closed += TabHeader_Closed;

            TabHeaders.Add(header);

            header.Open();

            SelectTab(header);
        }

        private void TabHeader_Closed(object sender, EventArgs e)
        {
            var tab = (TabHeader)sender;
            if (TabHeaders.Contains(tab)) TabHeaders.Remove(tab);
        }

        #region "Select"

        public void SelectTab(string name)
        {
            var tab = FindTab(name);
            if (tab != null) SelectTab(tab);
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


        const double TAB_PAGE_OPEN_ANIMATION_TIME = 300;
        const double TAB_PAGE_CLOSE_ANIMATION_TIME = 300;

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

        public TabHeader FindTab(string name)
        {
            int index = TabHeaders.ToList().FindIndex(x => x.Text == name);
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


        public int FindTabIndex(string name)
        {
            int index = TabHeaders.ToList().FindIndex(x => x.Text == name);
            return index;
        }

        public int FindTabIndex(TabHeader header)
        {
            int index = TabHeaders.ToList().FindIndex(x => x.Text == header.Text);
            return index;
        }

        #endregion

        #region "Close"

        public void CloseTab(string name)
        {
            var tab = FindTab(name);
            if (tab != null)
            {
                bool cancel = CheckCancel(tab);
                if (!cancel)
                {
                    tab.Close();

                    // If current tab then switch to tab to the left
                    if (CurrentPage == tab.Page)
                    {
                        int tabIndex = FindTabIndex(name);
                        if (tabIndex >= 0)
                        {
                            if (tabIndex > 0) SelectTab(tabIndex - 1);
                            else if (TabHeaders.Count > 1) SelectTab(tabIndex + 1);
                            else AnimateTabPageClose();
                        }
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

        void TabHeader_Clicked(TabHeader header) { SelectTab(header.Text); }

        void TabHeader_CloseClicked(TabHeader header) { CloseTab(header.Text); }

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

        #region "Zoom"

        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set
            {
                SetValue(ZoomLevelProperty, value);

                //if (Pages_TABCONTROL.SelectedIndex >= 0)
                //{
                //    TH_TabItem tab = (TH_TabItem)Pages_TABCONTROL.Items[Pages_TABCONTROL.SelectedIndex];

                //    Controls.Page page = (Controls.Page)tab.Content;
                //    page.ZoomLevel = value;

                //    ZoomLevelDisplay = value.ToString("P0");

                //    if (ZoomLevelChanged != null) ZoomLevelChanged(value);
                //}

            }
        }

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(double), typeof(MainWindow), new PropertyMetadata(1D));


        public string ZoomLevelDisplay
        {
            get { return (string)GetValue(ZoomLevelDisplayProperty); }
            set
            {
                SetValue(ZoomLevelDisplayProperty, value);
            }
        }

        public static readonly DependencyProperty ZoomLevelDisplayProperty =
            DependencyProperty.Register("ZoomLevelDisplay", typeof(string), typeof(MainWindow), new PropertyMetadata("100%"));

        public delegate void ZoomLevelChanged_Handler(double zoomlevel);
        public event ZoomLevelChanged_Handler ZoomLevelChanged;

        #endregion

        void Pages_Initialize()
        {
            //About_Initialize();
            DeviceManager_Initialize();
            //AccountManager_Initialize();
            //Options_Initialize();
            //Plugins_Initialize();
        }

        #region "About"

        public PageManager aboutManager;

        void About_Initialize()
        {
            if (aboutManager == null)
            {
                aboutManager = new PageManager();
                aboutManager.TabTitle = "About";
                aboutManager.TabImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/About_01.png"));
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

        //public DeviceManager devicemanager;
        public DeviceManagerList devicemanager;

        void DeviceManager_Initialize()
        {
            devicemanager = new DeviceManagerList();
            devicemanager.DeviceListUpdated += Devicemanager_DeviceListUpdated;
            devicemanager.DeviceUpdated += Devicemanager_DeviceUpdated;
            devicemanager.AddDeviceSelected += Devicemanager_AddDeviceSelected;
            devicemanager.ShareDeviceSelected += Devicemanager_ShareDeviceSelected;
            devicemanager.CopyDeviceSelected += Devicemanager_CopyDeviceSelected;
            devicemanager.DeviceEditSelected += Devicemanager_DeviceEditSelected;
            devicemanager.DeviceEditTableSelected += Devicemanager_DeviceEditTableSelected;
            devicemanager.DeviceManagerListSelected += Devicemanager_DeviceManagerListSelected;
        }

        private void Devicemanager_DeviceManagerListSelected()
        {
            DeviceManager_Open();
        }

        private void Devicemanager_CopyDeviceSelected(TH_Configuration.Configuration config)
        {
            //string title = "Copy Device - ";
            //title += config.Description.Description;
            //if (config.Description.Device_ID != null) title += " (" + config.Description.Device_ID + ")";

            //var index = PageTabHeaders.ToList().FindIndex(x => x.Text == title);
            //if (index >= 0)
            //{
            //    AddPageAsTab(null, title, null);
            //}
            //else
            //{
            //    var page = new TH_DeviceManager.Pages.CopyDevice.Page();

            //    AddPageAsTab(page, title, new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Copy_01.png")));
            //}
        }

        private void Devicemanager_ShareDeviceSelected(TH_Configuration.Configuration config)
        {
            //string title = "Share Device - ";
            //title += config.Description.Description;
            //if (config.Description.Device_ID != null) title += " (" + config.Description.Device_ID + ")";

            //var index = PageTabHeaders.ToList().FindIndex(x => x.Text == title);
            //if (index >= 0)
            //{
            //    AddPageAsTab(null, title, null);
            //}
            //else
            //{
            //    var page = new TH_DeviceManager.ShareDevice.Page();
            //    page.ParentManager = devicemanager;
            //    page.LoadConfiguration(config);

            //    AddPageAsTab(page, title, new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Share_01.png")));
            //}
        }

        private void Devicemanager_AddDeviceSelected()
        {
            var tab = FindTab("Add Device");
            if (tab == null)
            {
                var page = new TH_DeviceManager.AddDevice.Page();
                page.ParentManager = devicemanager;
                page.ShowAutoDetect();

                AddTab(page, "Add Device");
            }
            else
            {
                SelectTab(tab);
            }
        }

        private void Page_DeviceAdded(TH_Configuration.Configuration config)
        {
            if (devicemanager != null)
            {
                devicemanager.AddDevice(config);
            }
        }

        private void Devicemanager_DeviceEditSelected(TH_Configuration.Configuration config)
        {
            string title = "Edit Device - " + config.Description.Description;
            if (config.Description.Device_ID != null) title += " (" + config.Description.Device_ID + ")";

            var tab = FindTab(title);
            if (tab == null)
            {
                var page = new DeviceManagerPage(config, DeviceManagerType.Client);
                page.ParentManager = devicemanager;

                AddTab(page, title);
            }
            else
            {
                SelectTab(tab);
            }
        }

        private void Devicemanager_DeviceEditTableSelected(TH_Configuration.Configuration config)
        {
            //string title = "Table - " + config.Description.Description;
            //if (config.Description.Device_ID != null) title += " (" + config.Description.Device_ID + ")";

            //var index = PageTabHeaders.ToList().FindIndex(x => x.Text == title);
            //if (index >= 0)
            //{
            //    AddPageAsTab(null, title, null);
            //}
            //else
            //{
            //    var page = new DeviceManagerTable(config);

            //    AddPageAsTab(page, title, new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Root.png")));
            //}
        }

        public void DeviceManager_Open()
        { 
            AddTab(devicemanager);
        }

        #endregion

        #region "Account Manager"

        public PageManager accountManager;

        TH_UserManagement.MyAccountPage accountpage;

        void AccountManager_Initialize()
        {
            if (accountManager == null)
            {
                accountpage = new TH_UserManagement.MyAccountPage();
                accountpage.LoadUserConfiguration(CurrentUser);
                accountpage.UserChanged += accountpage_UserChanged;

                accountManager = new PageManager();
                accountManager.TabTitle = "Account Management";
                accountManager.TabImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/blank_profile_01_sm.png"));
                accountManager.AddPage(accountpage);
            }
        }

        void accountpage_UserChanged(UserConfiguration userConfig)
        {
            if (LoginMenu != null) LoginMenu.LoadUserConfiguration(userConfig);
        }

        public void AccountManager_Open()
        {
            AccountManager_Initialize();

            AddTab(accountManager);
        }

        #endregion

        #region "Options"

        PageManager optionsManager;

        void Options_Initialize()
        {
            //optionsManager = new PageManager();
            //optionsManager.AddPage(new Pages.Options.Updates.Page());
        }

        public void Options_Open()
        {
            //AddPageAsTab(optionsManager, "Options", new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/options_gear_30px.png")));
        }

        #endregion

        #region "Plugins"

        PageManager pluginsManager;

        Pages.Plugins.Installed.Page pluginsPage;

        void Plugins_Initialize()
        {
            if (pluginsManager == null)
            {
                pluginsManager = new PageManager();
                pluginsManager.TabTitle = "Plugins";
                pluginsManager.TabImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Rocket_02.png"));
                pluginsPage = new Pages.Plugins.Installed.Page();
                pluginsManager.AddPage(pluginsPage);

                if (PluginConfigurations != null) Plugins_AddItems(PluginConfigurations);
            }
        }

        private void Plugins_AddItems(List<TH_Plugins_Client.PluginConfiguration> configs)
        {
            pluginsPage.ClearInstalledItems();

            configs.Sort((a, b) => a.Name.CompareTo(b.Name));

            foreach (var config in configs)
            {
                pluginsPage.AddPlugin(config);
            }
        }

        public void Plugins_Open()
        {
            Plugins_Initialize();

            AddTab(pluginsManager);
        }

        #endregion

    }
}
