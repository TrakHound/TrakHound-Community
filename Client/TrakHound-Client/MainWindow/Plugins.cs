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

        List<PluginConfiguration> EnabledPlugins;

        void LoadPlugins()
        {
            EnabledPlugins = new List<PluginConfiguration>();

            Plugins_Find();

            Plugins_Load();
        }

        #region "Pages"

        Plugin_Container plugin_container;

        class Plugin_Container
        {
            // Store Plugins
            [ImportMany(typeof(IClientPlugin))]
            public IEnumerable<Lazy<IClientPlugin>> plugins { get; set; }
        }

        public List<Lazy<IClientPlugin>> plugins;

        public void Plugins_Find()
        {
            plugins = new List<Lazy<IClientPlugin>>();

            string path;

            // Load from System Directory first (easier for user to navigate to 'C:\TrakHound\')
            path = TH_Global.FileLocations.TrakHound + @"\Plugins\";
            if (Directory.Exists(path)) PagePlugins_Find_Recursive(path);

            // Load from App root Directory (doesn't overwrite plugins found in System Directory)
            path = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(path)) PagePlugins_Find_Recursive(path);

            path = AppDomain.CurrentDomain.BaseDirectory + @"Plugins\";
            if (Directory.Exists(path)) PagePlugins_Find_Recursive(path);

            // Add Buttons for Plugins on PlugIn Options page
            if (Properties.Settings.Default.Plugin_Configurations != null && pluginsPage != null)
            {
                pluginsPage.ClearInstalledItems();

                Properties.Settings.Default.Plugin_Configurations.Sort((a, b) => a.Name.CompareTo(b.Name));

                foreach (PluginConfiguration config in Properties.Settings.Default.Plugin_Configurations.ToList())
                {
                    pluginsPage.AddInstalledItem(config);
                }
            }
        }

        List<string> DefaultEnablePlugins = new List<string> { "dashboard", "device compare", "table manager", "status data" };

        void PagePlugins_Find_Recursive(string Path)
        {
            try
            {
                plugin_container = new Plugin_Container();

                var PageCatalog = new DirectoryCatalog(Path);
                var PageContainer = new CompositionContainer(PageCatalog);
                PageContainer.SatisfyImportsOnce(plugin_container);

                if (plugin_container.plugins != null)
                {
                    List<PluginConfiguration> configs;

                    if (Properties.Settings.Default.Plugin_Configurations != null)
                    {
                        configs = Properties.Settings.Default.Plugin_Configurations.ToList();
                    }
                    else
                    {
                        configs = new List<PluginConfiguration>();
                    }

                    foreach (Lazy<IClientPlugin> lplugin in plugin_container.plugins.ToList())
                    {
                        try
                        {
                            IClientPlugin plugin = lplugin.Value;

                            Console.WriteLine(plugin.Title + " Found in '" + Path + "'");

                            PluginConfiguration config = configs.Find(x => x.Name.ToUpper() == plugin.Title.ToUpper());
                            if (config == null)
                            {
                                Console.WriteLine("Plugin Configuration created for " + plugin.Title);
                                config = new PluginConfiguration();
                                config.Name = plugin.Title;
                                config.Description = plugin.Description;

                                // Automatically enable basic Plugins by TrakHound
                                if (DefaultEnablePlugins.Contains(config.Name.ToLower()))
                                {
                                    config.Enabled = true;
                                    Console.WriteLine("Default TrakHound Plugin Initialized as 'Enabled'");
                                }
                                else config.Enabled = false;

                                config.Parent = plugin.DefaultParent;
                                config.Category = plugin.DefaultParentCategory;

                                config.SubCategories = plugin.SubCategories;

                                configs.Add(config);
                            }
                            else Console.WriteLine("Plugin Configuration found for " + plugin.Title);

                            if (config.Parent == null) config.EnabledChanged += PageConfig_EnabledChanged;

                            plugins.Add(lplugin);

                        }
                        catch (Exception ex)
                        {
                            Controls.Message_Center.Message_Data mData = new Controls.Message_Center.Message_Data();
                            mData.Title = "PlugIn Error";
                            mData.Text = "Error during plugin intialization";
                            mData.AdditionalInfo = ex.Message;
                            mData.Type = Controls.Message_Center.MessageType.error;

                            messageCenter.AddMessage(mData);
                        }
                    }


                    // Create a copy of configs since we are modifying it
                    List<PluginConfiguration> tempConfigs = new List<PluginConfiguration>();
                    tempConfigs.AddRange(configs);

                    foreach (PluginConfiguration config in tempConfigs)
                    {
                        if (configs.Contains(config))
                        {
                            if (config.Parent != null)
                            {
                                if (config.Category != null)
                                {
                                    PluginConfiguration match1 = configs.Find(x => x.Name.ToUpper() == config.Parent.ToUpper());
                                    if (match1 != null)
                                    {
                                        PluginConfigurationCategory match2 = match1.SubCategories.Find(x => x.Name.ToUpper() == config.Category.ToUpper());
                                        if (match2 != null)
                                        {
                                            configs.Remove(config);
                                            if (match2.PluginConfigurations.Find(x => x.Name.ToUpper() == config.Name.ToUpper()) == null)
                                            {
                                                match2.PluginConfigurations.Add(config);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Properties.Settings.Default.Plugin_Configurations = configs;
                    Properties.Settings.Default.Save();
                }

                foreach (string directory in Directory.GetDirectories(Path, "*", SearchOption.AllDirectories))
                {
                    PagePlugins_Find_Recursive(directory);
                }
            }
            catch (Exception ex)
            {
                Controls.Message_Center.Message_Data mData = new Controls.Message_Center.Message_Data();
                mData.Title = "Plugin Load Error";
                mData.Text = "Error loading Plugins from " + Path;
                mData.AdditionalInfo = ex.Message;
                mData.Type = Controls.Message_Center.MessageType.error;

                messageCenter.AddMessage(mData);
            }
        }

        void PageConfig_EnabledChanged(PluginConfiguration config)
        {
            if (config.Enabled) Plugins_Load(config);
            else Plugins_Unload(config);

            Properties.Settings.Default.Save();

        }

        public void Plugins_Load()
        {
            if (Properties.Settings.Default.Plugin_Configurations != null)
            {
                var configs = Properties.Settings.Default.Plugin_Configurations.ToList();
                //configs.Sort((a, b) => a.Name.CompareTo(b.Name));

                foreach (var config in configs)
                {
                    Plugins_Load(config);
                }
            }
        }

        public void Plugins_Load(PluginConfiguration config)
        {
            if (config != null)
            {
                if (!EnabledPlugins.Contains(config))
                {
                    if (config.Enabled)
                    {
                        if (plugins != null)
                        {
                            Lazy<IClientPlugin> lplugin = plugins.Find(x => x.Value.Title.ToUpper() == config.Name.ToUpper());
                            if (lplugin != null)
                            {
                                try
                                {
                                    IClientPlugin plugin = lplugin.Value;

                                    plugin.DataEvent += Plugin_DataEvent;
                                    plugin.ShowRequested += Plugin_ShowRequested;
                                    plugin.SubCategories = config.SubCategories;

                                    plugin.Plugins = new List<IClientPlugin>();

                                    if (plugin.SubCategories != null)
                                    {
                                        foreach (PluginConfigurationCategory subcategory in plugin.SubCategories)
                                        {
                                            foreach (PluginConfiguration subConfig in subcategory.PluginConfigurations)
                                            {
                                                Lazy<IClientPlugin> clplugin = plugins.Find(x => x.Value.Title.ToUpper() == subConfig.Name.ToUpper());
                                                if (clplugin != null)
                                                {
                                                    plugin.Plugins.Add(clplugin.Value);
                                                }
                                            }
                                        }
                                    }

                                    plugin.Initialize();

                                    AddAppToList(plugin);

                                    if (plugin.OpenOnStartUp)
                                    {
                                        AddPageAsTab(plugin, plugin.Title, plugin.Image);
                                    }

                                    Plugins_CreateOptionsPage(plugin);

                                    EnabledPlugins.Add(config);
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
                }
            }
        }

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

        void Plugin_DataEvent(DataEvent_Data de_d)
        {
            if (Properties.Settings.Default.Plugin_Configurations != null)
            {
                List<PluginConfiguration> configs = Properties.Settings.Default.Plugin_Configurations.ToList();

                foreach (PluginConfiguration config in configs)
                {
                    if (config.Enabled)
                    {
                        Lazy<IClientPlugin> lplugin = plugins.ToList().Find(x => x.Value.Title == config.Name);
                        if (lplugin != null)
                        {
                            IClientPlugin plugin = lplugin.Value;
                            plugin.Update_DataEvent(de_d);
                        }
                    }
                }
            }
        }

        public void Plugins_Unload(PluginConfiguration config)
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

                    if (EnabledPlugins.Contains(config)) EnabledPlugins.Remove(config);
                }
            }
        }

        void Plugins_CreateOptionsPage(IClientPlugin plugin)
        {

            if (plugin.Options != null) optionsManager.AddPage(plugin.Options);

        }

        void UpdatePluginDevices(List<Configuration> devices)
        {
            System.Diagnostics.Stopwatch stpw = new System.Diagnostics.Stopwatch();
            stpw.Start();

            if (plugins != null)
            {
                foreach (Lazy<IClientPlugin> lplugin in plugins.ToList())
                {
                    IClientPlugin plugin = lplugin.Value;

                    plugin.Devices = devices;
                }
            }

            stpw.Stop();

            Console.WriteLine("UpdatePluginDevices() : " + stpw.ElapsedMilliseconds.ToString() + "ms");
        }


        void UpdatePluginUser(UserConfiguration userConfig, Database_Settings userDatabaseSettings)
        {
            if (plugins != null)
            {
                foreach (Lazy<IClientPlugin> lplugin in plugins.ToList())
                {
                    IClientPlugin plugin = lplugin.Value;

                    this.Dispatcher.BeginInvoke(new Action<IClientPlugin, UserConfiguration, Database_Settings>(UpdatePluginUser), Priority, new object[] { plugin, userConfig, userDatabaseSettings });
                }
            }
        }

        void UpdatePluginUser(IClientPlugin plugin, UserConfiguration userConfig, Database_Settings userDatabaseSettings)
        {
            plugin.UserDatabaseSettings = userDatabaseSettings;
            plugin.CurrentUser = userConfig;

        }

        #endregion

    }
}
