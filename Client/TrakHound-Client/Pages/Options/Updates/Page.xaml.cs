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
using Microsoft.Win32;
using System.Reflection;

using System.ComponentModel;
using System.Diagnostics;
using System.IO;

using TH_Plugins_Client;
using TH_Updater;
using TH_Global;

namespace TrakHound_Client.Pages.Options.Updates
{
    /// <summary>
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class Page : UserControl, TH_Global.IPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;

            updateBehavior = ProcessUpdateBehavior();

            if (updateBehavior == 0 || updateBehavior == 1) AutoUpdater_Start();
        }

        int updateBehavior = 0;

        MainWindow mw;

        public string Title { get { return "Updates"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Arrow_Up_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }


        void LaunchUpdater()
        {
            //string appStartPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + "trakhound-client.exe";

            //string appStartPath = @"F:\feenux\TrakHound\TrakHound\Client\AppStart\bin\Debug\AppStart.exe";

            string appStartPath = AppDomain.CurrentDomain.BaseDirectory + "\\Updater\\" + "AppStart.exe";

            if (File.Exists(appStartPath))
            {
                var p = new Process();

                p.StartInfo.FileName = appStartPath;
                p.StartInfo.Arguments = Process.GetCurrentProcess().ProcessName;

                p.Start();
            }
            else
            {
                Logger.Log("LaunchUpdater() :: Can't find " + appStartPath);
            }
        }


        void AutoUpdater_Start()
        {
            UpdateCheck updateCheck = new UpdateCheck();
            updateCheck.AppInfoReceived += AutoUpdater_AppInfoReceived;
            updateCheck.Start("http://www.feenux.com/trakhound/appinfo/th/client-appinfo.json");
        }

        void AutoUpdater_AppInfoReceived(UpdateCheck.AppInfo info)
        {
            if (info != null)
            {
                // Print Auto Update info to Console
                Logger.Log("---- Auto-Update Info ----");
                Logger.Log("TrakHound - Client");
                Logger.Log("Release Type : " + info.releaseType);
                Logger.Log("Version : " + info.version);
                Logger.Log("Build Date : " + info.buildDate);
                Logger.Log("Download URL : " + info.downloadUrl);
                Logger.Log("Update URL : " + info.updateUrl);
                Logger.Log("File Size : " + info.size);
                Logger.Log("--------------------------");

                this.Dispatcher.BeginInvoke(new Action<UpdateCheck.AppInfo>(AutoUpdater_AppInfoReceived_GUI), new object[] { info });
            }
        }

        void AutoUpdater_AppInfoReceived_GUI(UpdateCheck.AppInfo info)
        {
            // Check if version is Up-to-date
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            ClientVersion = "v" + version.ToString();

            Version latestVersion = null;
            Version.TryParse(info.version, out latestVersion);

            ClientUpdateShown = false;

            if (latestVersion != null)
            {
                if (version < latestVersion)
                {
                    // Run Updater
                    Updater updater = new Updater();
                    updater.assembly = Assembly.GetExecutingAssembly();
                    updater.Start(info.updateUrl);

                    Logger.Log("Update Available : " + latestVersion.ToString());

                    // Add Notification to Message Center
                    Controls.Message_Center.Message_Data mData = new Controls.Message_Center.Message_Data();
                    mData.Title = "Version " + latestVersion.ToString() + " is Available";
                    mData.Text = "Reopen TrakHound to apply update";

                    mw.messageCenter.AddMessage(mData);

                    
                    ClientCheckResult = "Version " + latestVersion.ToString() + " is available";
                    ClientCheckBrush = new SolidColorBrush(Color.FromRgb(0, 128, 255));
                    ClientCheckImage = null;

                    ClientUpdateShown = true;
                }
                else
                {
                    ClientCheckResult = "Up to Date";
                    ClientCheckBrush = new SolidColorBrush(Colors.Green);
                    ClientCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/CheckMark_01.png"));
                }
            }
            else
            {
                ClientCheckResult = "Error during Update Check";
                ClientCheckBrush = new SolidColorBrush(Colors.Red);
                ClientCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/X_01.png"));
            }
        }


        UpdateCheck.AppInfo appInfo;

        void ManualUpdater_Start()
        {
            UpdateCheck updateCheck = new UpdateCheck();
            updateCheck.AppInfoReceived += ManualUpdater_AppInfoReceived;
            updateCheck.Start("http://www.feenux.com/trakhound/appinfo/th/client-appinfo.json");
        }

        void ManualUpdater_AppInfoReceived(UpdateCheck.AppInfo info)
        {
            appInfo = info;

            if (info != null)
            {
                // Print Auto Update info to Console
                Logger.Log("---- Manual-Update Info ----");
                Logger.Log("TrakHound - Client");
                Logger.Log("Release Type : " + info.releaseType);
                Logger.Log("Version : " + info.version);
                Logger.Log("Build Date : " + info.buildDate);
                Logger.Log("Download URL : " + info.downloadUrl);
                Logger.Log("Update URL : " + info.updateUrl);
                Logger.Log("File Size : " + info.size);
                Logger.Log("--------------------------");

                this.Dispatcher.BeginInvoke(new Action<UpdateCheck.AppInfo>(ManualUpdater_AppInfoReceived_GUI), new object[] { info });
            }
        }

        void ManualUpdater_AppInfoReceived_GUI(UpdateCheck.AppInfo info)
        {
            // Check if version is Up-to-date
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            ClientVersion = "v" + version.ToString();

            Version latestVersion = null;
            Version.TryParse(info.version, out latestVersion);

            ClientUpdateShown = false;

            if (latestVersion != null)
            {
                if (version < latestVersion)
                {
                    Logger.Log("Update Available : " + latestVersion.ToString());

                    // Add Notification to Message Center
                    Controls.Message_Center.Message_Data mData = new Controls.Message_Center.Message_Data();
                    mData.Title = "Version " + latestVersion.ToString() + " is Available";
                    mData.Text = "Click to Update";

                    mw.messageCenter.AddMessage(mData);

                    ClientCheckResult = "Version " + latestVersion.ToString() + " is available";
                    ClientCheckBrush = new SolidColorBrush(Color.FromRgb(0, 128, 255));
                    ClientCheckImage = null;

                    ClientUpdateShown = true;
                }
                else
                {
                    ClientCheckResult = "Up to Date";
                    ClientCheckBrush = new SolidColorBrush(Colors.Green);
                    ClientCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/CheckMark_01.png"));
                }
            }
            else
            {
                ClientCheckResult = "Error during Update Check";
                ClientCheckBrush = new SolidColorBrush(Colors.Red);
                ClientCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/X_01.png"));
            }
        }

        public void LoadPluginConfigurations()
        {
            Plugin_STACK.Children.Clear();

            if (mw != null)
            {
                var configs = mw.PluginConfigurations;

                foreach (var config in configs)
                {
                    if (mw.Plugins != null)
                    {
                        try
                        {
                            var plugin = mw.Plugins.Find(x =>
                                x.Title == config.Name &&
                                x.DefaultParent == config.Parent &&
                                x.DefaultParentCategory == config.Category
                                );
                            if (plugin != null)
                            {
                                UpdateItem ui = new UpdateItem();
                                ui.PluginTitle = config.Name;
                                ui.PluginImage = plugin.Image;

                                // Build Information
                                Assembly assembly = Assembly.GetAssembly(plugin.GetType());
                                Version version = assembly.GetName().Version;

                                ui.assembly = assembly;
                                ui.version = version;
                                ui.PluginVersion = "v" + version.ToString();

                                // Author Info
                                ui.PluginAuthor = plugin.Author;
                                ui.PluginAuthorInfo = plugin.AuthorText;

                                // Update Info
                                ui.UpdateFileUrl = plugin.UpdateFileURL;

                                Plugin_STACK.Children.Add(ui);
                            }
                        }
                        catch (Exception ex) { Logger.Log("Updates.Page.LoadPluginConfigurations() :: Exception :: " + ex.Message); }
                    }

                    if (config.SubCategories != null)
                    {
                        foreach (PluginConfigurationCategory subcat in config.SubCategories)
                        {
                            foreach (PluginConfiguration subConfig in subcat.PluginConfigurations)
                            {
                                var plugin = mw.Plugins.Find(x =>
                                    x.Title == subConfig.Name &&
                                    x.DefaultParent == subConfig.Parent &&
                                    x.DefaultParentCategory == subConfig.Category
                                    );
                                if (plugin != null)
                                {
                                    try
                                    {
                                        UpdateItem ui = new UpdateItem();
                                        ui.PluginTitle = subConfig.Name;
                                        ui.PluginImage = plugin.Image;

                                        // Build Information
                                        Assembly assembly = Assembly.GetAssembly(plugin.GetType());
                                        Version version = assembly.GetName().Version;

                                        ui.assembly = assembly;
                                        ui.version = version;
                                        ui.PluginVersion = "v" + version.ToString();

                                        // Author Info
                                        ui.PluginAuthor = plugin.Author;
                                        ui.PluginAuthorInfo = plugin.AuthorText;

                                        // Update Info
                                        ui.UpdateFileUrl = plugin.UpdateFileURL;

                                        Plugin_STACK.Children.Add(ui);
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
            }
        }

        //public void LoadPluginConfigurations()
        //{
        //    Plugin_STACK.Children.Clear();

        //    // Load Plugin Configurations
        //    if (Properties.Settings.Default.Plugin_Configurations != null)
        //    {
        //        List<PluginConfiguration> configs = Properties.Settings.Default.Plugin_Configurations.ToList();

        //        foreach (PluginConfiguration config in configs)
        //        {
        //            if (mw != null)
        //            {
        //                if (mw.Plugins != null)
        //                {
        //                    try
        //                    {
        //                        var plugin = mw.Plugins.Find(x => x.Title == config.Name);
        //                        if (plugin != null)
        //                        {
        //                            UpdateItem ui = new UpdateItem();
        //                            ui.PluginTitle = config.Name;
        //                            ui.PluginImage = plugin.Image;

        //                            // Build Information
        //                            Assembly assembly = Assembly.GetAssembly(plugin.GetType());
        //                            Version version = assembly.GetName().Version;

        //                            ui.assembly = assembly;
        //                            ui.version = version;
        //                            ui.PluginVersion = "v" + version.ToString();

        //                            // Author Info
        //                            ui.PluginAuthor = plugin.Author;
        //                            ui.PluginAuthorInfo = plugin.AuthorText;

        //                            // Update Info
        //                            ui.UpdateFileUrl = plugin.UpdateFileURL;

        //                            Plugin_STACK.Children.Add(ui);
        //                        }
        //                    }
        //                    catch (Exception ex) { Logger.Log("Updates.Page.LoadPluginConfigurations() :: Exception :: " + ex.Message); }
        //                }
        //            }

        //            if (config.SubCategories != null)
        //            {
        //                foreach (PluginConfigurationCategory subcat in config.SubCategories)
        //                {
        //                    foreach (PluginConfiguration subConfig in subcat.PluginConfigurations)
        //                    {
        //                        var plugin = mw.Plugins.Find(x => x.Title == subConfig.Name);
        //                        if (plugin != null)
        //                        {
        //                            try
        //                            {
        //                                UpdateItem ui = new UpdateItem();
        //                                ui.PluginTitle = subConfig.Name;
        //                                ui.PluginImage = plugin.Image;

        //                                // Build Information
        //                                Assembly assembly = Assembly.GetAssembly(plugin.GetType());
        //                                Version version = assembly.GetName().Version;

        //                                ui.assembly = assembly;
        //                                ui.version = version;
        //                                ui.PluginVersion = "v" + version.ToString();

        //                                // Author Info
        //                                ui.PluginAuthor = plugin.Author;
        //                                ui.PluginAuthorInfo = plugin.AuthorText;

        //                                // Update Info
        //                                ui.UpdateFileUrl = plugin.UpdateFileURL;

        //                                Plugin_STACK.Children.Add(ui);
        //                            }
        //                            catch { }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}


        public string ClientVersion
        {
            get { return (string)GetValue(ClientVersionProperty); }
            set { SetValue(ClientVersionProperty, value); }
        }

        public static readonly DependencyProperty ClientVersionProperty =
            DependencyProperty.Register("ClientVersion", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string ClientCheckResult
        {
            get { return (string)GetValue(ClientCheckResultProperty); }
            set { SetValue(ClientCheckResultProperty, value); }
        }

        public static readonly DependencyProperty ClientCheckResultProperty =
            DependencyProperty.Register("ClientCheckResult", typeof(string), typeof(Page), new PropertyMetadata(null));


        public SolidColorBrush ClientCheckBrush
        {
            get { return (SolidColorBrush)GetValue(ClientCheckBrushProperty); }
            set { SetValue(ClientCheckBrushProperty, value); }
        }

        public static readonly DependencyProperty ClientCheckBrushProperty =
            DependencyProperty.Register("ClientCheckBrush", typeof(SolidColorBrush), typeof(Page), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public ImageSource ClientCheckImage
        {
            get { return (ImageSource)GetValue(ClientCheckImageProperty); }
            set { SetValue(ClientCheckImageProperty, value); }
        }

        public static readonly DependencyProperty ClientCheckImageProperty =
            DependencyProperty.Register("ClientCheckImage", typeof(ImageSource), typeof(Page), new PropertyMetadata(null));


        public bool ClientUpdateShown
        {
            get { return (bool)GetValue(ClientUpdateShownProperty); }
            set { SetValue(ClientUpdateShownProperty, value); }
        }

        public static readonly DependencyProperty ClientUpdateShownProperty =
            DependencyProperty.Register("ClientUpdateShown", typeof(bool), typeof(Page), new PropertyMetadata(false));


        public bool ClientInstalledShown
        {
            get { return (bool)GetValue(ClientInstalledShownProperty); }
            set { SetValue(ClientInstalledShownProperty, value); }
        }

        public static readonly DependencyProperty ClientInstalledShownProperty =
            DependencyProperty.Register("ClientInstalledShown", typeof(bool), typeof(Page), new PropertyMetadata(false));

 
        #region "Update Behavior"

        int tryCount = 0;

        int ProcessUpdateBehavior()
        {
            int Result = -1;

            tryCount += 1;

            object updateBehavior = GetRegistryKey("Update_Behavior");

            if (updateBehavior != null)
            {
                int val = -1;
                int.TryParse(updateBehavior.ToString(), out val);
                if (val >= 0)
                {
                    switch (val)
                    {
                        case 1: SemiAuto_RADIO.IsChecked = true; break;

                        case 2: Manual_RADIO.IsChecked = true; break;

                        default: Auto_RADIO.IsChecked = true; break;
                    }

                    Result = val;
                }
            }
            else
            {
                SetRegistryKey("Update_Behavior", 0);
                // Try max of 3 times before giving up (don't want to get stuck in a loop)
                if (tryCount < 4) ProcessUpdateBehavior();
            }

            return Result;
        }

        private void Auto_RadioButton_Checked(object sender, RoutedEventArgs e) { SetRegistryKey("Update_Behavior", 0); }

        private void SemiAuto_RadioButton_Checked(object sender, RoutedEventArgs e) { SetRegistryKey("Update_Behavior", 1); }

        private void Manual_RadioButton_Checked(object sender, RoutedEventArgs e) { SetRegistryKey("Update_Behavior", 2); }

        static void SetRegistryKey(string keyName, object keyValue)
        {
            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Create/Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.CreateSubKey("TrakHound");

                // Create/Open CURRENT_USER/Software/TrakHound/[keyName] Key
                RegistryKey updateKey = rootKey.CreateSubKey(keyName);

                // Update value for [keyName] to [keyValue]
                updateKey.SetValue(keyName, keyValue, RegistryValueKind.String);
            }
            catch (Exception ex)
            {
                Logger.Log("TrakHound-Client.Pages.Options.Update.Page.SetRegistryKey() : " + ex.Message);
            }
        }

        static string GetRegistryKey(string keyName)
        {
            string Result = null;

            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound");

                // Open CURRENT_USER/Software/TrakHound/[keyName] Key
                RegistryKey updateKey = rootKey.OpenSubKey(keyName);

                if (updateKey != null)
                {
                    // Read value for [keyName] to [keyValue]
                    object val = updateKey.GetValue(keyName, 0);
                    if (val != null) Result = val.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("TrakHound-Client.Pages.Options.Updates.Page.GetRegistryKey() : " + ex.Message);
            }

            return Result;
        }

        #endregion

        private void CheckForUpdates_Clicked(TH_WPF.Button bt)
        {
            AutoUpdater_Start();

            foreach (UpdateItem ui in Plugin_STACK.Children.OfType<UpdateItem>().ToList())
            {
                ui.CheckUpdateVersion();
            }
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPluginConfigurations();

            if (updateBehavior < 2)
            {
                foreach (UpdateItem ui in Plugin_STACK.Children.OfType<UpdateItem>().ToList())
                {
                    ui.CheckUpdateVersion();
                }
            }  
        }

        void updater_Finished()
        {
            this.Dispatcher.BeginInvoke(new Action(updater_Finished_GUI));
        }

        void updater_Finished_GUI()
        {
            ClientUpdateShown = false;
            ClientInstalledShown = true;
            ClientCheckResult = "";
        }
    }

}
