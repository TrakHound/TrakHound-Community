// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Shell;

using TH_Database;
using TH_UserManagement.Management;

namespace TrakHound_Client
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ISingleInstanceApp
    {

        public MainWindow()
        {
            init();
        }

        private const int DEFAULT_ANIMATION_FRAMERATE = 60;

        public void init()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += currentDomain_UnhandledException;

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = DEFAULT_ANIMATION_FRAMERATE });

            Log_Initialize();
            Splash_Initialize();


            Splash_UpdateStatus("...Initializing", 10);

            InitializeComponent();
            DataContext = this;

            this.SourceInitialized += new EventHandler(win_SourceInitialized);

            Application.Current.MainWindow = this;

            // Read Database Plugins (stores to static list in TH_Database.Global.Plugins)
            DatabasePluginReader.ReadPlugins();

            DeviceManager_Initialize();

            // Initialize Pages
            //Splash_UpdateStatus("...Creating Pages", 40);
            //Pages_Initialize();

            // Set border thickness (maybe make this a static resource in XAML?)
            ResizeBorderThickness = 1;

            //LoadDevices_Initialize();

            // Read Users and Login
            Splash_UpdateStatus("...Logging in User", 60);
            ReadUserManagementSettings();

            LoginMenu.rememberMeType = RememberMeType.Client;
            LoginMenu.LoadRememberMe();

            Splash_UpdateStatus("...Loading Plugins", 70);
            LoadPlugins();

            Splash_UpdateStatus("...Finishing Up", 100);

            WelcomeMessage();

            //// Wait for the minimum splash time to elapse, then close the splash dialog
            ////while (SplashWait) { System.Threading.Thread.Sleep(200); }
            Splash_Close();
        }

        private void Main_Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Main_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Plugins_Closed();

            Properties.Settings.Default.Save();
        }

        private void Main_Window_Closed(object sender, EventArgs e)
        {
            TH_Global.FileLocations.CleanTempDirectory(1);
        }

        void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show(e.ExceptionObject.ToString());
        }
      

    }

}
