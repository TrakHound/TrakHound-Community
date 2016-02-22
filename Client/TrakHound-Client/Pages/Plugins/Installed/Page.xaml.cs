// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using System.Reflection;
using System.IO;

using TH_Plugins_Client;
using TH_WPF;

namespace TrakHound_Client.Pages.Plugins.Installed
{
    /// <summary>
    /// Interaction logic for Plugins.xaml
    /// </summary>
    public partial class Page : UserControl, TH_Global.Page
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;
        }

        MainWindow mw;

        public string PageName { get { return "Installed"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/CheckMark_01.png")); } }

        ObservableCollection<ListItem> _listItems;
        public ObservableCollection<ListItem> ListItems
        {
            get
            {
                if (_listItems == null) _listItems = new ObservableCollection<ListItem>();
                return _listItems;
            }
            set
            {
                _listItems = value;
            }
        }

        /// <summary>
        /// Create a ListItem control using a IClientPlugin and PluginConfiguration
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private ListItem CreateListItem(IClientPlugin plugin, PluginConfiguration config)
        {
            // Create a new ListItem
            var result = new ListItem();
            result.Plugin = plugin;
            result.PluginConfiguration = config;
            result.Enabled = config.Enabled;

            result.EnabledChanged += li_EnabledChanged;

            AddSubConfigurations(config, result);

            return result;
        }

        /// <summary>
        /// Add a ListItem control representing an Installed Plugin
        /// </summary>
        /// <param name="config"></param>
        public void AddPlugin(PluginConfiguration config)
        {
            if (mw != null)
            {
                var plugin = mw.Plugins.Find(x =>
                    x.Title == config.Name &&
                    x.DefaultParent == config.Parent &&
                    x.DefaultParentCategory == config.Category
                    );
                if (plugin != null)
                {
                    // Create a new ListItem for the plugin
                    var item = CreateListItem(plugin, config);
                    ListItems.Add(item);
                }
            }
        }

        private void AddSubConfigurations(PluginConfiguration config, ListItem item)
        {
            if (config.SubCategories != null)
            {
                foreach (PluginConfigurationCategory category in config.SubCategories)
                {
                    var subcategory = new Subcategory();
                    subcategory.Text = category.Name;

                    AddSubConfigurationListItems(category, subcategory);

                    item.Subcategories.Add(subcategory);
                }
            }
        }

        private void AddSubConfigurationListItems(PluginConfigurationCategory category, Subcategory subcategory)
        {
            foreach (PluginConfiguration config in category.PluginConfigurations)
            {
                var plugin = mw.Plugins.Find(x =>
                    x.Title == config.Name &&
                    x.DefaultParent == config.Parent &&
                    x.DefaultParentCategory == config.Category
                    );
                if (plugin != null)
                {
                    // Create a new ListItem for the plugin
                    var item = CreateListItem(plugin, config);
                    subcategory.ListItems.Add(item);
                }
            }
        }

        void li_EnabledChanged(PluginConfiguration sender, bool enabled)
        {
            if (mw != null)
            {
                var config = mw.PluginConfigurations.Find(x =>
                    x.Name == sender.Name &&
                    x.Parent == sender.Parent &&
                    x.Category == sender.Category
                    );
                if (config != null)
                {
                    if (config.Enabled != enabled)
                    {
                        config.Enabled = enabled;

                        Properties.Settings.Default.Plugin_Configurations = mw.PluginConfigurations;
                        Properties.Settings.Default.Save();

                        if (enabled) mw.Plugin_Load(config);
                        else mw.Plugin_Unload(config);
                    }  
                }
                else
                { 
                    foreach (var pluginConfig in mw.PluginConfigurations)
                    {
                        EnabledChanged_SubPlugins(pluginConfig, sender, enabled);
                    }
                }
            }
        }

        void EnabledChanged_SubPlugins(PluginConfiguration parent, PluginConfiguration sender, bool enabled)
        {
            if (parent.SubCategories != null)
            {
                foreach (var subCategory in parent.SubCategories)
                {
                    if (subCategory.PluginConfigurations != null)
                    {
                        var config = subCategory.PluginConfigurations.Find(x =>
                            x.Name == sender.Name &&
                            x.Parent == sender.Parent &&
                            x.Category == sender.Category
                            );
                        if (config != null)
                        {
                            if (config.Enabled != enabled)
                            {
                                config.Enabled = enabled;

                                Properties.Settings.Default.Plugin_Configurations = mw.PluginConfigurations;
                                Properties.Settings.Default.Save();

                                if (enabled) mw.Plugin_Load(config);
                                else mw.Plugin_Unload(config);
                            }
                        }

                        foreach (var subConfig in subCategory.PluginConfigurations)
                        {
                            EnabledChanged_SubPlugins(subConfig, sender, enabled);
                        }
                    }
                }
            } 
        }

        public void ClearInstalledItems()
        {
            ListItems.Clear();
        }

    }
}
