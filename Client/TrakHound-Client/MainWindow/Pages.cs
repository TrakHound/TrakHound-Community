using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using TH_UserManagement.Management;
using TH_DeviceManager;

using TrakHound_Client.Controls;

namespace TrakHound_Client
{
    public partial class MainWindow
    {

        ObservableCollection<TH_TabHeader_Top> pagetabheaders;
        public ObservableCollection<TH_TabHeader_Top> PageTabHeaders
        {
            get
            {
                if (pagetabheaders == null) pagetabheaders = new ObservableCollection<TH_TabHeader_Top>();
                return pagetabheaders;
            }
            set
            {
                pagetabheaders = value;
            }
        }

        public void AddPageAsTab(object page, string title, ImageSource image)
        {
            // Check to see if Page already exists
            TH_TabItem TI = Pages_TABCONTROL.Items.Cast<TH_TabItem>().ToList().Find(x => x.Title.ToString().ToLower() == title.ToLower());

            if (TI == null)
            {
                TI = new TH_TabItem();
                TI.Content = CreatePage(page); ;
                TI.Title = title;
                TI.Closed += TI_Closed;

                TH_TabHeader_Top header = new TH_TabHeader_Top();
                header.Text = title;
                header.Image = image;
                header.TabParent = TI;
                header.Clicked += header_Clicked;
                header.CloseClicked += header_CloseClicked;
                TI.TH_Header = header;

                int zlevel = int.MaxValue;

                // Move all of the existing tabs to the front so that the new tab is behind it (so it can "slide" in behind it)
                for (int x = 0; x <= PageTabHeaders.Count - 1; x++)
                {
                    TH_TabHeader_Top tabHeader = (TH_TabHeader_Top)PageTabHeaders[x];
                    Panel.SetZIndex(tabHeader, zlevel - x);
                }

                PageTabHeaders.Add(header);

                Panel.SetZIndex(header, -1);

                TI.Template = (ControlTemplate)TryFindResource("TabItemControlTemplate");

                Pages_TABCONTROL.Items.Add(TI);
                Pages_TABCONTROL.SelectedItem = TI;
            }
            else
            {
                Pages_TABCONTROL.SelectedItem = TI;
            }
        }

        void TI_Closed(TH_TabItem tab)
        {
            List<TH_TabHeader_Top> headers = new List<TH_TabHeader_Top>();
            headers.AddRange(PageTabHeaders);

            List<TH_TabItem> tabs = Pages_TABCONTROL.Items.OfType<TH_TabItem>().ToList();

            foreach (TH_TabHeader_Top header in headers)
            {
                if (tabs.Find(x => x.Title.ToLower() == header.Text.ToLower()) == null)
                    PageTabHeaders.Remove(header);
            }
        }

        public Controls.Page CreatePage(object control)
        {
            Controls.Page Result = new Controls.Page();

            Result.PageContent = control;

            return Result;
        }

        public void ClosePage(string pageName)
        {
            TH_TabItem ti = Pages_TABCONTROL.Items.Cast<TH_TabItem>().ToList().Find(x => x.Title.ToString().ToLower() == pageName.ToLower());
            if (ti != null)
            {
                ti.Close();

                int index = 0;

                if (Pages_TABCONTROL.SelectedIndex < Pages_TABCONTROL.Items.Count - 1)
                    index = Math.Min(Pages_TABCONTROL.Items.Count, Pages_TABCONTROL.SelectedIndex + 1);
                else
                    index = Math.Max(0, Pages_TABCONTROL.SelectedIndex - 1);

                Pages_TABCONTROL.SelectedItem = Pages_TABCONTROL.Items[index];
            }
        }

        void header_Clicked(TH_TabHeader_Top header)
        {
            if (header.TabParent != null) Pages_TABCONTROL.SelectedItem = header.TabParent;
        }

        void header_CloseClicked(TH_TabHeader_Top header)
        {
            int index = 0;

            if (header.IsSelected)
            {
                if (Pages_TABCONTROL.SelectedIndex < Pages_TABCONTROL.Items.Count - 1)
                    index = Math.Min(Pages_TABCONTROL.Items.Count, Pages_TABCONTROL.SelectedIndex + 1);
                else
                    index = Math.Max(0, Pages_TABCONTROL.SelectedIndex - 1);

                Pages_TABCONTROL.SelectedItem = Pages_TABCONTROL.Items[index];
            }

            if (header.TabParent != null)
            {
                header.TabParent.Close();
            }
        }

        private void Pages_TABCONTROL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType() == typeof(TabControl))
            {
                TabControl tc = (TabControl)sender;

                for (int x = 0; x <= PageTabHeaders.Count - 1; x++)
                {
                    if (x != tc.SelectedIndex)
                    {
                        PageTabHeaders[x].IsSelected = false;
                    }
                    else
                    {
                        PageTabHeaders[x].IsSelected = true;
                    }

                    ZoomLevel = 1;
                }
            }
        }

        void ChangePage_Forward()
        {
            if (Pages_TABCONTROL.Items.Count > 0)
            {
                int index = Pages_TABCONTROL.SelectedIndex;
                int max = Pages_TABCONTROL.Items.Count - 1;

                if (index < max)
                {
                    Pages_TABCONTROL.SelectedItem = Pages_TABCONTROL.Items[index + 1];
                }
                else
                {
                    Pages_TABCONTROL.SelectedItem = Pages_TABCONTROL.Items[0];
                }
            }
        }

        void ChangePage_Backward()
        {
            if (Pages_TABCONTROL.Items.Count > 0)
            {
                int index = Pages_TABCONTROL.SelectedIndex;
                int max = Pages_TABCONTROL.Items.Count - 1;

                if (index > 0)
                {
                    Pages_TABCONTROL.SelectedItem = Pages_TABCONTROL.Items[index - 1];
                }
                else
                {
                    Pages_TABCONTROL.SelectedItem = Pages_TABCONTROL.Items[max];
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

                if (Pages_TABCONTROL.SelectedIndex >= 0)
                {
                    TH_TabItem tab = (TH_TabItem)Pages_TABCONTROL.Items[Pages_TABCONTROL.SelectedIndex];

                    Controls.Page page = (Controls.Page)tab.Content;
                    page.ZoomLevel = value;

                    ZoomLevelDisplay = value.ToString("P0");

                    if (ZoomLevelChanged != null) ZoomLevelChanged(value);
                }

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
            About_Initialize();
            DeviceManager_Initialize();
            AccountManager_Initialize();
            Options_Initialize();
            Plugins_Initialize();
        }

        #region "About"

        public PageManager aboutManager;

        void About_Initialize()
        {
            aboutManager = new PageManager();
            aboutManager.AddPage(new Pages.About.Information.Page());
            aboutManager.AddPage(new Pages.About.License.Page());
        }

        public void About_Open()
        {
            AddPageAsTab(aboutManager, "About", new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/About_01.png")));
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

            //devicemanager = new DeviceManager(DeviceManagerType.Client);
            //devicemanager.DeviceListUpdated += Devicemanager_DeviceListUpdated;
            //devicemanager.DeviceUpdated += Devicemanager_DeviceUpdated;
            //devicemanager.LoadingDevices += Devicemanager_LoadingDevices;
        }

        private void Devicemanager_CopyDeviceSelected(TH_Configuration.Configuration config)
        {
            string title = "Copy Device - ";
            title += config.Description.Description;
            if (config.Description.Device_ID != null) title += " (" + config.Description.Device_ID + ")";

            var index = PageTabHeaders.ToList().FindIndex(x => x.Text == title);
            if (index >= 0)
            {
                AddPageAsTab(null, title, null);
            }
            else
            {
                var page = new TH_DeviceManager.Pages.CopyDevice.Page();

                AddPageAsTab(page, title, new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Copy_01.png")));
            }
        }

        private void Devicemanager_ShareDeviceSelected(TH_Configuration.Configuration config)
        {
            string title = "Share Device - ";
            title += config.Description.Description;
            if (config.Description.Device_ID != null) title += " (" + config.Description.Device_ID + ")";

            var index = PageTabHeaders.ToList().FindIndex(x => x.Text == title);
            if (index >= 0)
            {
                AddPageAsTab(null, title, null);
            }
            else
            {
                var page = new TH_DeviceManager.Pages.AddShare.Page();

                AddPageAsTab(page, title, new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Share_01.png")));
            }
        }

        private void Devicemanager_AddDeviceSelected()
        {
            string title = "Add Device";

            var index = PageTabHeaders.ToList().FindIndex(x => x.Text == title);
            if (index >= 0)
            {
                AddPageAsTab(null, title, null);
            }
            else
            {
                //var page = new TH_DeviceManager.Pages.AddDevice.Page();
                //page.DeviceAdded += Page_DeviceAdded;
                //page.currentuser = currentuser;

                var page = new TH_DeviceManager.AddDevice.Page();
                page.ParentManager = devicemanager;

                AddPageAsTab(page, title, new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Add_01.png")));
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
            string title = config.Description.Description;
            if (config.Description.Device_ID != null) title += " (" + config.Description.Device_ID + ")";

            var index = PageTabHeaders.ToList().FindIndex(x => x.Text == title);
            if (index >= 0)
            {
                AddPageAsTab(null, title, null);
            }
            else
            {
                var page = new DeviceManagerPage(config, DeviceManagerType.Client);

                AddPageAsTab(page, title, new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Root.png")));
            }
        }

        private void Devicemanager_DeviceEditTableSelected(TH_Configuration.Configuration config)
        {
            string title = "Table - " + config.Description.Description;
            if (config.Description.Device_ID != null) title += " (" + config.Description.Device_ID + ")";

            var index = PageTabHeaders.ToList().FindIndex(x => x.Text == title);
            if (index >= 0)
            {
                AddPageAsTab(null, title, null);
            }
            else
            {
                var page = new DeviceManagerTable(config);

                AddPageAsTab(page, title, new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Root.png")));
            }
        }

        public void DeviceManager_Open()
        {
            AddPageAsTab(devicemanager, "Device Manager", new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Root.png")));
        }

        #endregion

        #region "Account Manager"

        public PageManager accountManager;

        TH_UserManagement.MyAccountPage accountpage;

        void AccountManager_Initialize()
        {
            accountManager = new PageManager();

            accountpage = new TH_UserManagement.MyAccountPage();
            accountpage.UserChanged += accountpage_UserChanged;
            accountManager.AddPage(accountpage);
        }

        void accountpage_UserChanged(UserConfiguration userConfig)
        {
            if (LoginMenu != null) LoginMenu.LoadUserConfiguration(userConfig);
        }

        public void AccountManager_Open()
        {
            AddPageAsTab(accountManager, "Acount Manager", new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/blank_profile_01_sm.png")));
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
            pluginsManager = new PageManager();

            pluginsPage = new Pages.Plugins.Installed.Page();
            pluginsManager.AddPage(pluginsPage);
        }

        public void Plugins_Open()
        {
            AddPageAsTab(pluginsManager, "Plugins", new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Rocket_02.png")));
        }

        #endregion

    }
}
