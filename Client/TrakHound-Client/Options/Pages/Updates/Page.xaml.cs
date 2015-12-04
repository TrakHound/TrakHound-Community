﻿// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

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

using TH_PlugIns_Client_Control;
using TH_Updater;
using TH_Global;

namespace TrakHound_Client.Options.Pages.Updates
{
    /// <summary>
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class Page : UserControl, OptionsPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;

            updateBehavior = ProcessUpdateBehavior();

            if (updateBehavior == 0) AutoUpdater_Start();
            if (updateBehavior == 1) ManualUpdater_Start();

            Download_BT.AlternateStyle = true;
        }

        int updateBehavior = 0;

        MainWindow mw;

        public string PageName { get { return "Updates"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Arrow_Up_01.png")); } }


        void AutoUpdater_Start()
        {
            UpdateCheck updateCheck = new UpdateCheck();
            updateCheck.AppInfoReceived += AutoUpdater_AppInfoReceived;
            updateCheck.Start("http://www.feenux.com/trakhound/appinfo/th/client-appinfo.txt");
        }

        void AutoUpdater_AppInfoReceived(UpdateCheck.AppInfo info)
        {
            if (info != null)
            {
                // Print Auto Update info to Console
                Console.WriteLine("---- Auto-Update Info ----");
                Console.WriteLine("TrakHound - Client");
                Console.WriteLine("Release Type : " + info.releaseType);
                Console.WriteLine("Version : " + info.version);
                Console.WriteLine("Build Date : " + info.buildDate);
                Console.WriteLine("Download URL : " + info.downloadUrl);
                Console.WriteLine("Update URL : " + info.updateUrl);
                Console.WriteLine("File Size : " + info.size);
                Console.WriteLine("--------------------------");

                // Run Updater
                Updater updater = new Updater();
                updater.assembly = Assembly.GetExecutingAssembly();
                updater.Start(info.updateUrl);

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
                    Console.WriteLine("Update Available : " + latestVersion.ToString());

                    // Add Notification to Message Center
                    Message_Center.Message_Data mData = new Message_Center.Message_Data();
                    mData.title = "Version " + latestVersion.ToString() + " is Available";
                    mData.text = "Click to Update";

                    mw.messageCenter.AddNotification(mData);

                    
                    ClientCheckResult = "Version " + latestVersion.ToString() + " is available";
                    ClientCheckBrush = new SolidColorBrush(Color.FromRgb(0, 128, 255));
                    ClientCheckImage = null;
                    //ClientCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Arrow_Down_01.png"));

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
            updateCheck.Start("http://www.feenux.com/trakhound/appinfo/th/client-appinfo.txt");
        }

        void ManualUpdater_AppInfoReceived(UpdateCheck.AppInfo info)
        {
            appInfo = info;

            if (info != null)
            {
                // Print Auto Update info to Console
                Console.WriteLine("---- Manual-Update Info ----");
                Console.WriteLine("TrakHound - Client");
                Console.WriteLine("Release Type : " + info.releaseType);
                Console.WriteLine("Version : " + info.version);
                Console.WriteLine("Build Date : " + info.buildDate);
                Console.WriteLine("Download URL : " + info.downloadUrl);
                Console.WriteLine("Update URL : " + info.updateUrl);
                Console.WriteLine("File Size : " + info.size);
                Console.WriteLine("--------------------------");

                //// Run Updater
                //Updater updater = new Updater();
                //updater.assembly = Assembly.GetExecutingAssembly();
                //updater.Start(info.updateUrl);

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
                    Console.WriteLine("Update Available : " + latestVersion.ToString());

                    // Add Notification to Message Center
                    Message_Center.Message_Data mData = new Message_Center.Message_Data();
                    mData.title = "Version " + latestVersion.ToString() + " is Available";
                    mData.text = "Click to Update";

                    mw.messageCenter.AddNotification(mData);


                    ClientCheckResult = "Version " + latestVersion.ToString() + " is available";
                    ClientCheckBrush = new SolidColorBrush(Color.FromRgb(0, 128, 255));
                    ClientCheckImage = null;
                    //ClientCheckImage = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Arrow_Down_01.png"));

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

            // Load Page Plugin Configurations
            if (Properties.Settings.Default.PagePlugIn_Configurations != null)
            {
                List<PlugInConfiguration> configs = Properties.Settings.Default.PagePlugIn_Configurations.ToList();

                foreach (PlugInConfiguration config in configs)
                {
                    if (mw != null)
                    {
                        if (mw.PagePlugIns != null)
                        {
                            try
                            {
                                Lazy<Control_PlugIn> LCP = mw.PagePlugIns.Find(x => x.Value.Title.ToUpper() == config.name.ToUpper());
                                if (LCP != null)
                                {
                                    Control_PlugIn CP = LCP.Value;

                                    UpdateItem ui = new UpdateItem();
                                    ui.PluginTitle = config.name;
                                    ui.PluginImage = CP.Image;

                                    // Build Information
                                    Assembly assembly = Assembly.GetAssembly(CP.GetType());
                                    Version version = assembly.GetName().Version;

                                    ui.PluginVersion = "v" + version.ToString();

                                    // Author Info
                                    ui.PluginAuthor = CP.Author;
                                    ui.PluginAuthorInfo = CP.AuthorText;

                                    // Update Info
                                    ui.UpdateFileUrl = CP.UpdateFileURL;

                                    Plugin_STACK.Children.Add(ui);
                                }
                            }
                            catch { }
                        }
                    }

                    if (config.SubCategories != null)
                    {
                        foreach (PlugInConfigurationCategory subcat in config.SubCategories)
                        {
                            foreach (PlugInConfiguration subConfig in subcat.PlugInConfigurations)
                            {
                                Lazy<Control_PlugIn> sLCP = mw.PagePlugIns.Find(x => x.Value.Title.ToUpper() == subConfig.name.ToUpper());
                                if (sLCP != null)
                                {
                                    try
                                    {
                                        Control_PlugIn sCP = sLCP.Value;

                                        UpdateItem ui = new UpdateItem();
                                        ui.PluginTitle = subConfig.name;
                                        ui.PluginImage = sCP.Image;

                                        // Build Information
                                        Assembly assembly = Assembly.GetAssembly(sCP.GetType());
                                        Version version = assembly.GetName().Version;

                                        ui.PluginVersion = "v" + version.ToString();

                                        // Author Info
                                        ui.PluginAuthor = sCP.Author;
                                        ui.PluginAuthorInfo = sCP.AuthorText;

                                        // Update Info
                                        ui.UpdateFileUrl = sCP.UpdateFileURL;

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
                Console.WriteLine("TrakHound-Client.Options.Pages.Update.Page.SetRegistryKey() : " + ex.Message);
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
                Console.WriteLine("TrakHound-Client.Options.Pages.Updates.Page.GetRegistryKey() : " + ex.Message);
            }

            return Result;
        }

        #endregion

        //private void CheckForUpdates_Button_Clicked(Controls.TH_Button bt)
        //{
        //    ManualUpdater_Start();

        //    foreach (UpdateItem ui in Plugin_STACK.Children.OfType<UpdateItem>().ToList())
        //    {
        //        ui.CheckUpdateVersion();
        //    }
        //}

        private void CheckForUpdates_Clicked(TH_WPF.Button_01 bt)
        {
            ManualUpdater_Start();

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

        private void Download_Button_Clicked(Controls.TH_Button bt)
        {
            if (appInfo != null)
            {
                // Run Updater
                Updater updater = new Updater();
                updater.assembly = Assembly.GetExecutingAssembly();
                updater.Finished += updater_Finished;
                updater.Start(appInfo.updateUrl);
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
