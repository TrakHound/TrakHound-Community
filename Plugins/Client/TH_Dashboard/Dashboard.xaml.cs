// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TH_Plugins;
using TH_Plugins.Client;
using TH_WPF;

namespace TH_Dashboard
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class Dashboard : UserControl
    {

        public Dashboard()
        {
            InitializeComponent();
            DataContext = this;

            SubCategories = new List<PluginConfigurationCategory>();
            PluginConfigurationCategory pages = new PluginConfigurationCategory();
            pages.Name = "Pages";
            SubCategories.Add(pages);

            IsExpanded = Properties.Settings.Default.IsExpanded;
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


        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(Dashboard), new PropertyMetadata(true));



        void UpdateLoggedInChanged(EventData data)
        {
            if (data != null)
            {
                if (data.Id.ToLower() == "userloggedin")
                {
                    LoggedIn = true;
                }

                if (data.Id.ToLower() == "userloggedout")
                {
                    LoggedIn = false;
                }
            }
        }

        void UpdateDevicesLoading(EventData data)
        {
            if (data != null)
            {
                if (data.Id.ToLower() == "loadingdevices")
                {
                    LoadingDevices = true;
                }

                if (data.Id.ToLower() == "devicesloaded")
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
                            plugin.SendData += Plugin_SendData;
                            
                            plugin.Initialize();
                        }

                        catch { }

                        var bt = new ListButton();
                        bt.Image = plugin.Image;
                        bt.Text = plugin.Title;
                        bt.Selected += lb_Selected;
                        bt.DataObject = plugin;
                        Pages.Add(bt);

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

        void Plugin_SendData(EventData data)
        {
            if (SendData != null) SendData(data);
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

        private void lb_Selected(ListButton lb)
        {
            StopSlideshow();

            SelectPage(lb);
        }

        void config_EnabledChanged(PluginConfiguration config)
        {
            if (config.Enabled) Plugins_Load(config);
            else Plugins_Unload(config);
        }

        #endregion

        #region "Slideshow"

        public bool SlideshowRunning
        {
            get { return (bool)GetValue(SlideshowRunningProperty); }
            set { SetValue(SlideshowRunningProperty, value); }
        }

        public static readonly DependencyProperty SlideshowRunningProperty =
            DependencyProperty.Register("SlideshowRunning", typeof(bool), typeof(Dashboard), new PropertyMetadata(false));

        private void StartSlideshow_Clicked(TH_WPF.Button bt)
        {
            StartSlideshow();
        }

        private void StopSlideshow_Clicked(TH_WPF.Button bt)
        {
            StopSlideshow();
        }

        private System.Timers.Timer slideshowTimer;

        private void StartSlideshow()
        {
            SlideshowRunning = true;

            slideshowTimer = new System.Timers.Timer();
            slideshowTimer.Interval = 20000;
            slideshowTimer.Elapsed += SlideshowTimer_Elapsed;
            slideshowTimer.Enabled = true;
        }

        private void SlideshowTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => {

                SelectNextPage();

            }), TH_Global.Functions.UI_Functions.PRIORITY_BACKGROUND, new object[] { });
        }

        private void StopSlideshow()
        {
            SlideshowRunning = false;

            if (slideshowTimer != null) slideshowTimer.Enabled = false;
            slideshowTimer = null;
        }

        #endregion

        private void SelectPage(ListButton lb)
        {
            foreach (var olb in Pages)
            {
                if (olb == lb) olb.IsSelected = true;
                else olb.IsSelected = false;
            }

            foreach (var category in SubCategories)
            {
                var config = category.PluginConfigurations.Find(x => GetPluginName(x.Name) == GetPluginName(lb.Text));
                if (config != null)
                {
                    currentPage = config;
                    break;
                }
            }

            var childPlugIn = lb.DataObject as UserControl;
            PageContent = childPlugIn;

            int index = Pages.ToList().FindIndex(x => x == lb);
            if (index >= 0)
            {
                Properties.Settings.Default.SelectedPage = index;
                Properties.Settings.Default.Save();
            }
        }


        private void SelectNextPage()
        {
            if (currentPage != null && Pages.Count > 0)
            {
                int currentIndex = Pages.ToList().FindIndex(x => x.Text == currentPage.Name);
                if (currentIndex >= 0)
                {
                    int next = 0;

                    if (currentIndex < Pages.Count - 1) next = currentIndex + 1;

                    SelectPage(Pages[next]);
                }
            }
        }

        private void SelectPreviousPage()
        {
            if (currentPage != null && Pages.Count > 0)
            {
                int currentIndex = Pages.ToList().FindIndex(x => x.Text == currentPage.Name);
                if (currentIndex >= 0)
                {
                    int next = Pages.Count - 1;

                    if (currentIndex > 0) next = currentIndex - 1;

                    SelectPage(Pages[next]);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (PageContent == null)
            {
                int selectedPage = Properties.Settings.Default.SelectedPage;

                if (Pages.Count > selectedPage)
                {
                    ListButton lb = Pages[selectedPage];

                    foreach (var oLB in Pages)
                    {
                        if (oLB == lb) oLB.IsSelected = true;
                        else oLB.IsSelected = false;
                    }

                    var plugin = lb.DataObject as IClientPlugin;
                    if (plugin != null)
                    {
                        foreach (var category in SubCategories)
                        {
                            var config = category.PluginConfigurations.Find(x => x.Name.ToUpper() == plugin.Title.ToUpper());
                            if (config != null)
                            {
                                currentPage = config;
                                break;
                            }
                        }

                        var childPlugIn = lb.DataObject as UserControl;
                        PageContent = childPlugIn;
                    }
                }
            }
        }

        private void Expand_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsExpanded = !IsExpanded;

            Properties.Settings.Default.IsExpanded = IsExpanded;
            Properties.Settings.Default.Save();
        }

    }
}
