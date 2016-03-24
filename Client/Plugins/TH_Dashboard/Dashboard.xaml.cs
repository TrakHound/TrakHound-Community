// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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


        void UpdateLoggedInChanged(EventData data)
        {
            if (data != null)
            {
                if (data.id.ToLower() == "userloggedin")
                {
                    LoggedIn = true;
                }

                if (data.id.ToLower() == "userloggedout")
                {
                    LoggedIn = false;
                }
            }
        }

        void UpdateDevicesLoading(EventData data)
        {
            if (data != null)
            {
                if (data.id.ToLower() == "loadingdevices")
                {
                    LoadingDevices = true;
                }

                if (data.id.ToLower() == "devicesloaded")
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

    }
}
