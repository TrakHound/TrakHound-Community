// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using TrakHound;
using TrakHound.Logging;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;

namespace TrakHound_Dashboard
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
        
        private void LoadPlugins()
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
        private static List<IClientPlugin> GetPlugins()
        {
            var result = new List<IClientPlugin>();

            string path;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            path = Path.Combine(FileLocations.TrakHound, "Plugins");
            if (Directory.Exists(path)) AddPlugins(GetPlugins(path), result);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            path = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(path)) AddPlugins(GetPlugins(path), result);

            // Load from currenlty running assembly (ex. TrakHound-Dashboard)
            AddPlugins(GetPlugins(Assembly.GetEntryAssembly()), result);

            foreach (var plugin in result)
            {
                Logger.Log(plugin.Title + " Loaded", LogLineType.Notification);
            }

            return result;
        }

        /// <summary>
        /// Get a list of plugins in the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static List<IClientPlugin> GetPlugins(string path)
        {
            var result = new List<IClientPlugin>();

            var plugins = Reader.FindPlugins<IClientPlugin>(path, new ClientPlugin.PluginContainer());
            foreach (var plugin in plugins)
            {
                // Only add if not already in returned list
                if (result.Find(x =>
                    x.Title == plugin.Title &&
                    x.ParentPlugin == plugin.ParentPlugin &&
                    x.ParentPluginCategory == plugin.ParentPluginCategory
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
        /// Get a list of plugins in the specified Assembly
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static List<IClientPlugin> GetPlugins(Assembly assembly)
        {
            var result = new List<IClientPlugin>();

            var plugins = Reader.FindPlugins<IClientPlugin>(assembly, new ClientPlugin.PluginContainer());
            foreach (var plugin in plugins)
            {
                // Only add if not already in returned list
                if (result.Find(x =>
                    x.Title == plugin.Title &&
                    x.ParentPlugin == plugin.ParentPlugin &&
                    x.ParentPluginCategory == plugin.ParentPluginCategory
                    ) == null)
                {
                    result.Add(plugin);
                }
            }

            return result;
        }

        /// <summary>
        /// Add plugins to list making sure that plugins are not repeated in list
        /// </summary>
        /// <param name="newPlugins"></param>
        /// <param name="oldPlugins"></param>
        private static void AddPlugins(List<IClientPlugin> newPlugins, List<IClientPlugin> oldPlugins)
        {
            foreach (var plugin in newPlugins)
            {
                if (oldPlugins.Find(x =>
                    x.Title == plugin.Title &&
                    x.ParentPlugin == plugin.ParentPlugin && 
                    x.ParentPluginCategory == plugin.ParentPluginCategory
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
        private List<PluginConfiguration> GetPluginConfigurations(List<IClientPlugin> plugins)
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
                    config.Enabled = true;
                }

                config.SubCategories = plugin.SubCategories;

                config.EnabledChanged += config_EnabledChanged;

                config.Parent = plugin.ParentPlugin;
                config.Category = plugin.ParentPluginCategory;

                if (FindPluginConfiguration(plugin, result) == null) result.Add(config);
            }

            result = ProcessPluginConfigurations(result);

            return result;
        }

        /// <summary>
        /// Find a PluginConfiguration item in 'configs' using 'name' as an identifier
        /// Searches all subpluginconfigurations also
        /// </summary>
        /// <param name="name"></param>
        /// <param name="configs"></param>
        /// <returns></returns>
        private PluginConfiguration FindPluginConfiguration(IClientPlugin plugin, List<PluginConfiguration> configs)
        {
            PluginConfiguration result = null;

            foreach (var config in configs)
            {
                // See if root is a match
                if (config.Name == plugin.Title &&
                    config.Parent == plugin.ParentPlugin &&
                    config.Category == plugin.ParentPluginCategory
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
        private List<PluginConfiguration> ProcessPluginConfigurations(List<PluginConfiguration> configs)
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

        private void config_EnabledChanged(PluginConfiguration sender)
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
                    x.ParentPlugin == config.Parent &&
                    x.ParentPluginCategory == config.Category
                    );
                if (plugin != null)
                {
                    try
                    {
                        // Assign event handlers
                        plugin.SendData += Plugin_SendData;

                        // Process SubPlugins
                        plugin.SubCategories = config.SubCategories;
                        Plugin_LoadSubPlugins(config);

                        // Initialize plugin
                        plugin.Initialize();

                        // If set to OpenOnStartUp then Open new Tab
                        if (plugin.OpenOnStartUp) AddTab(plugin);

                        // Create an Options page (if exists)
                        Plugin_CreateOptionsPage(plugin);
                    }
                    catch (Exception ex)
                    {
                        var mData = new Controls.Message_Center.MessageData();
                        mData.Title = "Plugin Error";
                        mData.Text = "Error during plugin load";
                        mData.AdditionalInfo = ex.Message;
                        mData.Type = TrakHound.API.Messages.MessageType.TRAKHOUND_ALERT;

                        messageCenter.AddMessage(mData);
                    }
                }
            }
        }

        /// <summary>
        /// Process the SubPlugins
        /// </summary>
        /// <param name="plugin"></param>
        private void Plugin_LoadSubPlugins(IClientPlugin plugin)
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
                            x.ParentPlugin == subConfig.Parent &&
                            x.ParentPluginCategory == subConfig.Category
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
        private void Plugin_LoadSubPlugins(PluginConfiguration config)
        {
            var plugin = Plugins.Find(x =>
                x.Title == config.Name &&
                x.ParentPlugin == config.Parent &&
                x.ParentPluginCategory == config.Category
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
                                x.ParentPlugin == subConfig.Parent &&
                                x.ParentPluginCategory == subConfig.Category
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
        /// Plugin has sent a DataEvent_Data object to other plugins
        /// </summary>
        /// <param name="de_d"></param>
        private void Plugin_SendData(EventData data)
        {
            foreach (var config in PluginConfigurations)
            {
                if (config.Enabled)
                {
                    var plugin = Plugins.Find(x =>
                        x.Title == config.Name &&
                        x.ParentPlugin == config.Parent &&
                        x.ParentPluginCategory == config.Category
                        );
                    if (plugin != null)
                    {
                        var sendDataInfo = new SendDataInfo(plugin, data);

                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessSendData), sendDataInfo);
                    }
                }
            }

            SendEventData(data);
        }

        private class SendDataInfo
        {
            public SendDataInfo(IClientPlugin plugin, EventData data)
            {
                Plugin = plugin;
                Data = data;
            }

            public IClientPlugin Plugin { get; set; }
            public EventData Data { get; set; }
        }

        private void ProcessSendData(object o)
        {
            if (o != null)
            {
                var sendDataInfo = (SendDataInfo)o;

                try
                {
                    sendDataInfo.Plugin.GetSentData(sendDataInfo.Data);
                }
                catch (Exception ex) { Logger.Log("Plugin Error :: " + ex.Message); }
            }
        }

        /// <summary>
        /// Create an Options page for the plugin and add it to the Options Manager
        /// </summary>
        /// <param name="plugin"></param>
        private void Plugin_CreateOptionsPage(IClientPlugin plugin)
        {
            if (plugin.Options != null) Options_AddPage(plugin.Options);
        }

        /// <summary>
        /// Signal plugins to close
        /// </summary>
        private void Plugins_Closed()
        {
            foreach (var plugin in Plugins)
            {
                try
                {
                    plugin.Closed();
                }
                catch (Exception ex) { }
            }
        }

    }
}
