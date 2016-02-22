using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Controls;
using System.Windows.Media;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using System.IO;

using TH_Configuration;
using TH_Plugins_Client;
using TH_UserManagement.Management;
using TH_WPF;

using TrakHound_Client.Controls;

namespace TrakHound_Client
{
    public partial class MainWindow
    {

        private List<IClientPlugin> _plugins;
        /// <summary>
        /// Flat list of IClientPlugin objects that were found
        /// </summary>
        public List<IClientPlugin> Plugins
        {
            get
            {
                if (_plugins == null) _plugins = new List<IClientPlugin>();
                return _plugins;
            }
            set
            {
                _plugins = value;
            }
        }

        private List<PluginConfiguration> _pluginConfigurations;
        /// <summary>
        /// Hierarchical List of PluginConfiguration objects
        /// </summary>
        public List<PluginConfiguration> PluginConfigurations
        {
            get
            {
                if (_pluginConfigurations == null) _pluginConfigurations = new List<PluginConfiguration>();
                return _pluginConfigurations;
            }
            set
            {
                _pluginConfigurations = value;
            }
        }


        // List of plugins enabled by default
        List<string> defaultEnabledPlugins = new List<string> {
            "Dashboard", 

            "Device Compare",
            "OEE",
            "Availability",
            "Performance",
            "Timeline (OEE)",
            "Production Status",
            "Program Name",
            "Feedrate Override",
            "Rapidrate Override",
            "Spindle Override",
            "Emergency Stop",
            "Controller Mode",
            "Execution Mode",

            "Table Manager",
            "Status Data" 
        };

        void LoadPlugins()
        {
            Plugins = GetPlugins();

            PluginConfigurations = GetPluginConfigurations(Plugins); 

            foreach (var config in PluginConfigurations)
            {
                Plugin_Load(config);
            }

            Properties.Settings.Default.Plugin_Configurations = PluginConfigurations;
            Properties.Settings.Default.Save();
        }

        #region "IClientPlugins"

        /// <summary>
        /// Get a list of IClientPlugins
        /// </summary>
        /// <returns></returns>
        static List<IClientPlugin> GetPlugins()
        {
            var result = new List<IClientPlugin>();

            string path;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            path = TH_Global.FileLocations.TrakHound + @"\Plugins\";
            if (Directory.Exists(path)) AddPlugins(GetPlugins(path), result);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            path = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(path)) AddPlugins(GetPlugins(path), result);

            return result;
        }

        /// <summary>
        /// Get a list of plugins in the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static List<IClientPlugin> GetPlugins(string path)
        {
            var result = new List<IClientPlugin>();

            var plugins = PluginTools.FindPlugins(path);
            foreach (var plugin in plugins)
            {
                // Only add if not already in returned list
                if (result.Find(x =>
                    x.Title == plugin.Title &&
                    x.DefaultParent == plugin.DefaultParent &&
                    x.DefaultParentCategory == plugin.DefaultParentCategory
                    ) == null)
                {
                    result.Add(plugin);
                }
            }

            // Load from subdirectories
            var subdirectories = Directory.GetDirectories(path);
            if (subdirectories != null)
            {
                foreach (var subdirectory in subdirectories)
                {
                    result.AddRange(GetPlugins(subdirectory));
                }
            } 

            return result;
        }

        /// <summary>
        /// Add plugins to list making sure that plugins are not repeated in list
        /// </summary>
        /// <param name="newPlugins"></param>
        /// <param name="oldPlugins"></param>
        static void AddPlugins(List<IClientPlugin> newPlugins, List<IClientPlugin> oldPlugins)
        {
            foreach (var plugin in newPlugins)
            {
                if (oldPlugins.Find(x =>
                    x.Title == plugin.Title &&
                    x.DefaultParent == plugin.DefaultParent && 
                    x.DefaultParentCategory == plugin.DefaultParentCategory
                    ) == null) 
                    oldPlugins.Add(plugin);
            }
        }

        #endregion

        #region "PluginConfiguration"

        /// <summary>
        /// Get a list of PluginConfigurations
        /// </summary>
        /// <param name="plugins"></param>
        /// <returns></returns>
        List<PluginConfiguration> GetPluginConfigurations(List<IClientPlugin> plugins)
        {
            var result = new List<PluginConfiguration>();

            List<PluginConfiguration> configs = Properties.Settings.Default.Plugin_Configurations;

            // Get list (not actual setting)
            if (configs != null) configs = configs.ToList();
            else configs = new List<PluginConfiguration>();

            foreach (var plugin in plugins)
            {
                // See if config is already created
                var config = FindPluginConfiguration(plugin, configs);
                if (config == null)
                {
                    config = new PluginConfiguration();
                    config.Name = plugin.Title;
                    config.Description = plugin.Description;
                    config.SubCategories = plugin.SubCategories;

                    // Automatically enable basic Plugins by TrakHound
                    if (defaultEnabledPlugins.Find(x => x.ToLower() == config.Name.ToLower()) != null)
                    {
                        config.Enabled = true;
                    }
                    else config.Enabled = false;
                }

                config.EnabledChanged += config_EnabledChanged;

                config.Parent = plugin.DefaultParent;
                config.Category = plugin.DefaultParentCategory;

                if (FindPluginConfiguration(plugin, result) == null) result.Add(config);
            }

            result = ProcessPluginConfigurations(result);

            // Add Buttons for Plugins on Plugin Options page
            if (pluginsPage != null)
            {
                pluginsPage.ClearInstalledItems();

                result.Sort((a, b) => a.Name.CompareTo(b.Name));

                foreach (var config in result)
                {
                    pluginsPage.AddPlugin(config);
                }
            }

            return result;
        }

        /// <summary>
        /// Find a PluginConfiguration item in 'configs' using 'name' as an identifier
        /// Searches all subpluginconfigurations also
        /// </summary>
        /// <param name="name"></param>
        /// <param name="configs"></param>
        /// <returns></returns>
        PluginConfiguration FindPluginConfiguration(IClientPlugin plugin, List<PluginConfiguration> configs)
        {
            PluginConfiguration result = null;

            foreach (var config in configs)
            {
                // See if root is a match
                if (config.Name == plugin.Title &&
                    config.Parent == plugin.DefaultParent &&
                    config.Category == plugin.DefaultParentCategory 
                    ) result = config;
                // if root is not a match, then search subconfigs
                else
                {
                    if (config.SubCategories != null)
                    {
                        foreach (var subcategory in config.SubCategories)
                        {
                            var subConfig = FindPluginConfiguration(plugin, subcategory.PluginConfigurations);
                            if (subConfig != null)
                            {
                                result = subConfig;
                                break;
                            }
                        }
                    }
                }

                if (result != null) break;
            }

            return result;
        }

        /// <summary>
        /// Process the PluginConfigurations for SubCategories
        /// </summary>
        /// <param name="configs"></param>
        /// <returns></returns>
        List<PluginConfiguration> ProcessPluginConfigurations(List<PluginConfiguration> configs)
        {
            var result = new List<PluginConfiguration>();

            foreach (var config in configs)
            {
                // Find Parent and assign to Parent Category
                if (config.Parent != null && config.Category != null)
                {
                    // Find Parent in configs
                    var parent = configs.Find(x => x.Name.ToLower() == config.Parent.ToLower());
                    if (parent != null && parent.SubCategories != null)
                    {
                        // Find category in parent's subcategories
                        var category = parent.SubCategories.Find(x => x.Name.ToLower() == config.Category.ToLower());
                        if (category != null)
                        {
                            category.PluginConfigurations.Add(config);
                        }
                    }
                    // If Parent is not found (or if parent has no subcategories) just add to root
                    else result.Add(config);
                }
                // If no parent is defined then add to root
                else result.Add(config);
            }

            return result;
        }

        void config_EnabledChanged(PluginConfiguration sender)
        {
            Properties.Settings.Default.Plugin_Configurations = PluginConfigurations;
            Properties.Settings.Default.Save();
        }

        #endregion

        /// <summary>
        /// Load Plugin if Enabled
        /// </summary>
        /// <param name="config"></param>
        public void Plugin_Load(PluginConfiguration config)
        {
            ///Check if Enabled
            if (config.Enabled)
            {
                var plugin = Plugins.Find(x =>
                    x.Title == config.Name &&
                    x.DefaultParent == config.Parent &&
                    x.DefaultParentCategory == config.Category
                    );
                if (plugin != null)
                {
                    try
                    {
                        // Assign event handlers
                        plugin.DataEvent += Plugin_DataEvent;
                        plugin.ShowRequested += Plugin_ShowRequested;

                        // Process SubPlugins
                        plugin.SubCategories = config.SubCategories;
                        Plugin_LoadSubPlugins(config);

                        // Initialize plugin
                        plugin.Initialize();

                        // Add to Plugins List Menu
                        AddAppToList(plugin);

                        // If set to OpenOnStartUp then Open new Tab
                        if (plugin.OpenOnStartUp) AddPageAsTab(plugin, plugin.Title, plugin.Image);

                        // Create an Options page (if exists)
                        Plugin_CreateOptionsPage(plugin);
                    }
                    catch (Exception ex)
                    {
                        Controls.Message_Center.Message_Data mData = new Controls.Message_Center.Message_Data();
                        mData.Title = "Plugin Error";
                        mData.Text = "Error during plugin load";
                        mData.AdditionalInfo = ex.Message;
                        mData.Type = Controls.Message_Center.MessageType.error;

                        messageCenter.AddMessage(mData);
                    }
                }
            }
        }

        /// <summary>
        /// Process the SubPlugins
        /// </summary>
        /// <param name="plugin"></param>
        void Plugin_LoadSubPlugins(IClientPlugin plugin)
        {
            plugin.Plugins = new List<IClientPlugin>();

            if (plugin.SubCategories != null)
            {
                foreach (PluginConfigurationCategory subcategory in plugin.SubCategories)
                {
                    foreach (PluginConfiguration subConfig in subcategory.PluginConfigurations)
                    {
                        var subplugin = Plugins.Find(x =>
                            x.Title == subConfig.Name &&
                            x.DefaultParent == subConfig.Parent &&
                            x.DefaultParentCategory == subConfig.Category
                            );
                        if (subplugin != null)
                        {
                            Plugin_LoadSubPlugins(subplugin);

                            plugin.Plugins.Add(subplugin);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add the 'sub' plugins to the PluginConfiguration
        /// </summary>
        /// <param name="config"></param>
        void Plugin_LoadSubPlugins(PluginConfiguration config)
        {
            var plugin = Plugins.Find(x =>
                x.Title == config.Name &&
                x.DefaultParent == config.Parent &&
                x.DefaultParentCategory == config.Category
                );
            if (plugin != null)
            {
                plugin.Plugins = new List<IClientPlugin>();

                if (config.SubCategories != null)
                {
                    foreach (PluginConfigurationCategory subcategory in config.SubCategories)
                    {
                        foreach (PluginConfiguration subConfig in subcategory.PluginConfigurations)
                        {
                            var subplugin = Plugins.Find(x =>
                                x.Title == subConfig.Name &&
                                x.DefaultParent == subConfig.Parent &&
                                x.DefaultParentCategory == subConfig.Category
                                );
                            if (subplugin != null)
                            {
                                Plugin_LoadSubPlugins(subConfig);

                                plugin.Plugins.Add(subplugin);
                            }
                        }
                    }
                }
            }

            
        }

        /// <summary>
        /// Plugin has requested to be shown
        /// </summary>
        /// <param name="info"></param>
        void Plugin_ShowRequested(PluginShowInfo info)
        {
            IClientPlugin plugin = null;

            if (info.Page.GetType() == typeof(IClientPlugin))
            {
                plugin = (IClientPlugin)info.Page;
            }

            string title = info.PageTitle;
            if (info.PageTitle == null && plugin != null) title = plugin.Title;

            ImageSource image = info.PageImage;
            if (info.PageImage == null && plugin != null) image = plugin.Image;

            object page = info.Page;

            AddPageAsTab(page, title, image);
        }

        /// <summary>
        /// Plugin has sent a DataEvent_Data object to other plugins
        /// </summary>
        /// <param name="de_d"></param>
        void Plugin_DataEvent(DataEvent_Data de_d)
        {
            foreach (var config in PluginConfigurations)
            {
                if (config.Enabled)
                {
                    var plugin = Plugins.Find(x =>
                        x.Title == config.Name &&
                        x.DefaultParent == config.Parent &&
                        x.DefaultParentCategory == config.Category
                        );
                    if (plugin != null)
                    {
                        plugin.Update_DataEvent(de_d);
                    }
                }
            }
        }

        /// <summary>
        /// Plugin has been signaled to be unloaded
        /// </summary>
        /// <param name="config"></param>
        public void Plugin_Unload(PluginConfiguration config)
        {
            if (config != null)
            {
                if (!config.Enabled)
                {
                    // Remove TabItem
                    foreach (TH_TabItem ti in Pages_TABCONTROL.Items.OfType<TH_TabItem>().ToList())
                    {
                        if (ti.Header != null)
                        {
                            if (ti.Header.ToString().ToUpper() == config.Name.ToUpper())
                            {
                                if (ti.Content.GetType() == typeof(Grid))
                                {
                                    Grid grid = ti.Content as Grid;
                                    grid.Children.Clear();
                                }
                                Pages_TABCONTROL.Items.Remove(ti);
                            }
                        }
                    }

                    if (optionsManager != null)
                    {
                        foreach (ListButton lb in optionsManager.Pages.ToList())
                        {
                            if (lb.Text.ToUpper() == config.Name.ToUpper())
                            {
                                optionsManager.Pages.Remove(lb);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create an Options page for the plugin and add it to the Options Manager
        /// </summary>
        /// <param name="plugin"></param>
        void Plugin_CreateOptionsPage(IClientPlugin plugin)
        {
            if (plugin.Options != null) optionsManager.AddPage(plugin.Options);
        }

        /// <summary>
        /// Update the devices list for each plugin
        /// </summary>
        /// <param name="devices"></param>
        void Plugins_UpdateDevices(List<Configuration> devices)
        {
            foreach (var plugin in Plugins)
            {
                plugin.Devices = devices;
            }
        }

        /// <summary>
        /// Update the Current User for each plugin
        /// </summary>
        /// <param name="userConfig"></param>
        void Plugins_UpdateUser(UserConfiguration userConfig)
        {
            var de_d = new DataEvent_Data();

            if (userConfig != null)
            {
                de_d.id = "userloggedin";
            }
            else
            {
                de_d.id = "userloggedout";
            }

            Plugin_DataEvent(de_d);
        }

        /// <summary>
        /// Signal plugins to close
        /// </summary>
        void Plugins_Closing()
        {
            foreach (var plugin in Plugins)
            {
                try
                {
                    plugin.Closing();
                }
                catch (Exception ex) { }
            }
        }

    }
}
