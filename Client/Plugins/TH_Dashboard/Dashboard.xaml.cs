// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using System.Collections.ObjectModel;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using TH_Configuration;
using TH_Global;
using TH_Plugins_Client;
using TH_WPF;

namespace TH_Dashboard
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class Dashboard : UserControl, IClientPlugin
    {

        #region "Plugin"

        #region "Descriptive"

        public string Title { get { return "Dashboard"; } }

        public string Description { get { return "Contains and organizes pages for displaying Device data in various ways. Acts as the Home page for other Device Monitoring Plugins."; } }

        //public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_Dashboard;component/Images/Dashboard.png")); } }
        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_Dashboard;component/Resources/Dashboard_01.png")); } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return new BitmapImage(new Uri("pack://application:,,,/TH_Dashboard;component/Resources/TrakHound_Logo_10_200px.png")); } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\License\" + "License.txt"); } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return null; } }
        public string DefaultParentCategory { get { return null; } }

        public bool AcceptsPlugins { get { return true; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ShowInAppMenu { get { return true; } }

        public List<PluginConfigurationCategory> SubCategories { get; set; }

        public List<IClientPlugin> Plugins { get; set; }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/dashboard-appinfo.json"; } }

        #endregion

        #region "Methods"

        const System.Windows.Threading.DispatcherPriority Priority = System.Windows.Threading.DispatcherPriority.Background;

        public void Initialize()
        {
            EnabledPlugins = new List<PluginConfiguration>();

            foreach (PluginConfigurationCategory category in SubCategories)
            {
                foreach (PluginConfiguration config in category.PluginConfigurations)
                {
                    config.EnabledChanged += config_EnabledChanged;

                    if (config.Enabled) Plugins_Load(config);
                }
            }
        }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        public void Show() 
        {
            if (ShowRequested != null)
            {
                PluginShowInfo info = new PluginShowInfo();
                info.Page = this;
                ShowRequested(info);
            }
        }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {
            this.Dispatcher.BeginInvoke(new Action<DataEvent_Data>(UpdateLoggedInChanged), Priority, new object[] { de_d });

            this.Dispatcher.BeginInvoke(new Action<DataEvent_Data>(UpdateDevicesLoading), Priority, new object[] { de_d });

            if (Plugins != null)
            {
                foreach (IClientPlugin plugin in Plugins)
                {
                    this.Dispatcher.BeginInvoke(new Action<DataEvent_Data>(plugin.Update_DataEvent), Priority, new object[] { de_d });
                }
            }        
        }

        public event DataEvent_Handler DataEvent;

        public event PluginTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Device Properties"

        private ObservableCollection<Configuration> _devices;
        public ObservableCollection<Configuration> Devices
        {
            get
            {
                if (_devices == null)
                {
                    _devices = new ObservableCollection<Configuration>();
                }
                return _devices;
            }
            set
            {
                _devices = value;
            }
        }

        #endregion

        #region "Options"

        public TH_Global.IPage Options { get; set; }

        #endregion

        #endregion

        #region "Dashboard"

        public Dashboard()
        {
            InitializeComponent();
            DataContext = this;

            SubCategories = new List<PluginConfigurationCategory>();
            PluginConfigurationCategory pages = new PluginConfigurationCategory();
            pages.Name = "Pages";
            SubCategories.Add(pages);
        }

        ObservableCollection<ListButton> pages;
        public ObservableCollection<ListButton> Pages
        {
            get
            {
                if (pages == null)
                    pages = new ObservableCollection<ListButton>();
                return pages;
            }

            set
            {
                pages = value;
            }
        }


        public object PageContent
        {
            get { return (object)GetValue(PageContentProperty); }
            set { SetValue(PageContentProperty, value); }
        }

        public static readonly DependencyProperty PageContentProperty =
            DependencyProperty.Register("PageContent", typeof(object), typeof(Dashboard), new PropertyMetadata(null));


        public bool LoggedIn
        {
            get { return (bool)GetValue(LoggedInProperty); }
            set { SetValue(LoggedInProperty, value); }
        }

        public static readonly DependencyProperty LoggedInProperty =
            DependencyProperty.Register("LoggedIn", typeof(bool), typeof(Dashboard), new PropertyMetadata(false));


        public bool LoadingDevices
        {
            get { return (bool)GetValue(LoadingDevicesProperty); }
            set { SetValue(LoadingDevicesProperty, value); }
        }

        public static readonly DependencyProperty LoadingDevicesProperty =
            DependencyProperty.Register("LoadingDevices", typeof(bool), typeof(Dashboard), new PropertyMetadata(false));


        void UpdateLoggedInChanged(DataEvent_Data de_d)
        {
            if (de_d != null)
            {
                if (de_d.id.ToLower() == "userloggedin")
                {
                    LoggedIn = true;
                }

                if (de_d.id.ToLower() == "userloggedout")
                {
                    LoggedIn = false;
                }
            }
        }

        void UpdateDevicesLoading(DataEvent_Data de_d)
        {
            if (de_d != null)
            {
                if (de_d.id.ToLower() == "loadingdevices")
                {
                    LoadingDevices = true;
                }

                if (de_d.id.ToLower() == "devicesloaded")
                {
                    LoadingDevices = false;
                }
            }
        }

        #region "Child PlugIns"

        PluginConfiguration currentPage;

        List<PluginConfiguration> EnabledPlugins;

        public void Plugins_Load(PluginConfiguration config)
        {
            if (Plugins != null)
            {
                if (!EnabledPlugins.Contains(config))
                {
                    IClientPlugin plugin = Plugins.Find(x => x.Title.ToUpper() == config.Name.ToUpper());
                    if (plugin != null)
                    {
                        try
                        {
                            plugin.SubCategories = config.SubCategories;
                            plugin.DataEvent += Plugin_DataEvent;

                            plugin.Initialize();
                        }

                        catch { }

                        ListButton lb = new ListButton();
                        lb.ToolTip = config.Name;
                        lb.Image = plugin.Image;
                        lb.Selected += lb_Selected;
                        lb.DataObject = plugin;
                        Pages.Add(lb);

                        EnabledPlugins.Add(config);
                    }
                }
            }
        }

        void AddSubPlugins(IClientPlugin plugin)
        {
            plugin.Plugins = new List<IClientPlugin>();

            if (plugin.SubCategories != null)
            {
                foreach (PluginConfigurationCategory subcategory in plugin.SubCategories)
                {
                    foreach (PluginConfiguration subConfig in subcategory.PluginConfigurations)
                    {
                        IClientPlugin cplugin = Plugins.Find(x => x.Title.ToUpper() == subConfig.Name.ToUpper());
                        if (cplugin != null)
                        {
                            plugin.Plugins.Add(cplugin);
                        }
                    }
                }
            }
        }

        void Plugin_DataEvent(DataEvent_Data de_d)
        {
            if (DataEvent != null) DataEvent(de_d);
        }

        public void Plugins_Unload(PluginConfiguration config)
        {
            if (config != null)
            {
                if (!config.Enabled)
                {
                    ListButton lb = Pages.ToList().Find(x => GetPluginName(x.Text) == GetPluginName(config.Name));
                    if (lb != null)
                    {
                        Pages.Remove(lb);
                    }

                    if (config == currentPage) PageContent = null;

                    if (EnabledPlugins.Contains(config)) EnabledPlugins.Remove(config);
                }
            }
        }

        static string GetPluginName(string s)
        {
            if (s != null) return s.ToUpper();
            return s;
        }

        private void lb_Selected(ListButton LB)
        {
            foreach (ListButton oLB in Pages)
            {
                if (oLB == LB) oLB.IsSelected = true;
                else oLB.IsSelected = false;
            }

            foreach (PluginConfigurationCategory category in SubCategories)
            {
                PluginConfiguration config = category.PluginConfigurations.Find(x => GetPluginName(x.Name) == GetPluginName(LB.Text));
                if (config != null)
                {
                    currentPage = config;
                    break;
                }

            }

            UserControl childPlugIn = LB.DataObject as UserControl;

            PageContent = childPlugIn;
        }

        void config_EnabledChanged(PluginConfiguration config)
        {
            if (config.Enabled) Plugins_Load(config);
            else Plugins_Unload(config);
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (PageContent == null)
            {
                if (Pages.Count > 0)
                {
                    if (Pages[0].GetType() == typeof(ListButton))
                    {
                        ListButton lb = (ListButton)Pages[0];

                        foreach (ListButton oLB in Pages)
                        {
                            if (oLB == lb) oLB.IsSelected = true;
                            else oLB.IsSelected = false;
                        }

                        IClientPlugin plugin = lb.DataObject as IClientPlugin;
                        if (plugin != null)
                        {
                            foreach (PluginConfigurationCategory category in SubCategories)
                            {
                                PluginConfiguration config = category.PluginConfigurations.Find(x => x.Name.ToUpper() == plugin.Title.ToUpper());
                                if (config != null)
                                {
                                    currentPage = config;
                                    break;
                                }
                            }

                            UserControl childPlugIn = lb.DataObject as UserControl;

                            PageContent = childPlugIn;
                        }
                    }
                }
            }
        }

        #endregion
      
    }
}
