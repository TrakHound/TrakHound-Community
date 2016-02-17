// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
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

            mw = Application.Current.MainWindow as MainWindow;
        }

        MainWindow mw;

        public string PageName { get { return "Installed"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/CheckMark_01.png")); } }


        private void AddSubConfigurationListItem(PluginConfigurationCategory category, SubCategory subCategory)
        {
            foreach (PluginConfiguration config in category.PluginConfigurations)
            {
                var plugin = mw.Plugins.Find(x => x.Title == config.Name);
                if (plugin != null)
                {
                    try
                    {
                        ListItem item = new ListItem();
                        item.Plugin_Title = config.Name;
                        item.Plugin_Description = config.Description;
                        item.config = config;
                        item.Plugin_Enabled = config.Enabled;
                        item.Plugin_Image = plugin.Image;

                        item.EnabledChanged += li_EnabledChanged;

                        // Author Info
                        item.Author_Name = plugin.Author;
                        item.Author_Text = plugin.AuthorText;
                        item.Author_Image = Image_Functions.SetImageSize(plugin.AuthorImage, 0, 30);

                        // Version Info
                        Assembly sassembly = Assembly.GetAssembly(plugin.GetType());
                        if (sassembly != null)
                        {
                            Version sversion = sassembly.GetName().Version;
                            item.Plugin_Version = "v" + sversion.Major.ToString() + "." + sversion.Minor.ToString() + "." + sversion.Build.ToString();
                        }

                        AddSubConfigurations(config, item);

                        subCategory.ListItems.Add(item);
                    }
                    catch (Exception ex)
                    {
                        Controls.Message_Center.Message_Data mData = new Controls.Message_Center.Message_Data();
                        mData.Title = "Plugin Error";
                        mData.Text = "Error during plugin Configuration Load";
                        mData.AdditionalInfo = ex.Message;

                        mw.messageCenter.AddMessage(mData);
                    }
                }
            }
        }

        private void AddSubConfigurations(PluginConfiguration config, ListItem item)
        {
            if (config.SubCategories != null)
            {
                foreach (PluginConfigurationCategory category in config.SubCategories)
                {
                    SubCategory subCategory = new SubCategory();
                    subCategory.Text = category.Name;

                    AddSubConfigurationListItem(category, subCategory);

                    item.SubCategories.Add(subCategory);
                }
            }
        }

        public void AddInstalledItem(PluginConfiguration config)
        {
            if (mw != null)
            {
                var plugin = mw.Plugins.Find(x => x.Title == config.Name);
                if (plugin != null)
                {
                    ListContainer lc = new ListContainer();

                    ListItem li = new ListItem();
                    li.Plugin_Title = config.Name;
                    li.Plugin_Description = config.Description;
                    li.config = config;
                    li.Plugin_Enabled = config.Enabled;
                    li.Plugin_Image = plugin.Image;

                    li.EnabledChanged += li_EnabledChanged;
                    

                    // Author Info
                    li.Author_Name = plugin.Author;
                    li.Author_Text = plugin.AuthorText;
                    li.Author_Image = Image_Functions.SetImageSize(plugin.AuthorImage, 0, 30);

                    // Version Info
                    Assembly assembly = Assembly.GetAssembly(plugin.GetType());
                    if (assembly != null)
                    {
                        Version version = assembly.GetName().Version;
                        li.Plugin_Version = "v" + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString();
                    }

                    AddSubConfigurations(config, li);

                    lc.RootPlugin_GRID.Children.Add(li);

                    Installed_STACK.Children.Add(lc);
                }
            }
        }

        void li_EnabledChanged(PluginConfiguration sender, bool enabled)
        {
            if (mw != null)
            {
                var config = mw.PluginConfigurations.Find(x => x.Name == sender.Name);
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
                        var config = subCategory.PluginConfigurations.Find(x => x.Name == sender.Name);
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
            Installed_STACK.Children.Clear();
        }

        public void LoadPluginConfigurations()
        {
            //if (mw != null)
            //{
            //    var configs = mw.PluginConfigurations;
                
            //    foreach (var config in configs)
            //    {



            //    }

            //}
            


            // Load Plugin Configurations
            //if (Properties.Settings.Default.Plugin_Configurations != null)
            //{
            //    List<PluginConfiguration> configs = Properties.Settings.Default.Plugin_Configurations.ToList();

            //    foreach (ListContainer lc in Installed_STACK.Children.OfType<ListContainer>())
            //    {
            //        foreach (ListItem root_li in lc.RootPlugin_GRID.Children.OfType<ListItem>())
            //        {
            //            PluginConfiguration config = configs.Find(x => x.Name.ToUpper() == root_li.Plugin_Title.ToUpper());
            //            if (config != null)
            //            {
            //                root_li.Plugin_Enabled = config.Enabled;
            //            }

            //            List<ListItem> subitems = lc.SubPlugins_STACK.Children.OfType<ListItem>().ToList();

            //            foreach (PluginConfigurationCategory subCat in config.SubCategories)
            //            {
            //                foreach (PluginConfiguration subConfig in subCat.PluginConfigurations)
            //                {
            //                    ListItem sub_li = subitems.Find(x => x.Plugin_Title.ToUpper() == subConfig.Name.ToUpper());
            //                    if (sub_li != null)
            //                    {
            //                        sub_li.Plugin_Enabled = subConfig.Enabled;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
        }

        //public void LoadPluginConfigurations()
        //{


        //    // Load Plugin Configurations
        //    if (Properties.Settings.Default.Plugin_Configurations != null)
        //    {
        //        List<PluginConfiguration> configs = Properties.Settings.Default.Plugin_Configurations.ToList();

        //        foreach (ListContainer lc in Installed_STACK.Children.OfType<ListContainer>())
        //        {
        //            foreach (ListItem root_li in lc.RootPlugin_GRID.Children.OfType<ListItem>())
        //            {
        //                PluginConfiguration config = configs.Find(x => x.Name.ToUpper() == root_li.Plugin_Title.ToUpper());
        //                if (config != null)
        //                {
        //                    root_li.Plugin_Enabled = config.Enabled;
        //                }

        //                List<ListItem> subitems = lc.SubPlugins_STACK.Children.OfType<ListItem>().ToList();

        //                foreach (PluginConfigurationCategory subCat in config.SubCategories)
        //                {
        //                    foreach (PluginConfiguration subConfig in subCat.PluginConfigurations)
        //                    {
        //                        ListItem sub_li = subitems.Find(x => x.Plugin_Title.ToUpper() == subConfig.Name.ToUpper());
        //                        if (sub_li != null)
        //                        {
        //                            sub_li.Plugin_Enabled = subConfig.Enabled;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            //Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            //dlg.InitialDirectory = Environment.GetEnvironmentVariable("system");
            //dlg.Multiselect = true;
            //dlg.Title = "Browse for Client Plugins";
            //dlg.Filter = "Client Plugin (*.dll)|*.dll";

            //dlg.ShowDialog();

            //try
            //{
            //    foreach (string filename in dlg.FileNames.ToList())
            //    {
            //        string pluginPath = TH_Global.FileLocations.TrakHound + @"\PlugIns\";

            //        string name = System.IO.Path.GetFileNameWithoutExtension(filename);
            //        string ext = System.IO.Path.GetExtension(filename);

            //        string suffix = "";
            //        int suffixNum = 0;

            //        string test = pluginPath + name + suffix + ext;

            //        while (File.Exists(test))
            //        {
            //            suffixNum += 1;
            //            suffix = "_" + suffixNum.ToString("00");
            //            test = pluginPath + name + suffix + ext;
            //        }

            //        File.Copy(filename, test);
            //    }

            //    if (mw != null) mw.Plugins_Find();

            //    LoadPluginConfigurations();
            //}
            //catch (Exception ex) 
            //{
            //    Console.WriteLine("Browse_Click() : " + ex.Message);
            //}
        }

    }
}
