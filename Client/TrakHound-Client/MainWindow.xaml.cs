// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Shell;

using TH_Database;

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

        private const int DEFAULT_ANIMATION_FRAMERATE = 40;

        public const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;
        public const System.Windows.Threading.DispatcherPriority PRIORITY_CONTEXT_IDLE = System.Windows.Threading.DispatcherPriority.ContextIdle;
        public const System.Windows.Threading.DispatcherPriority PRIORITY_APPLICATION_IDLE = System.Windows.Threading.DispatcherPriority.ApplicationIdle;

        public void init()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += currentDomain_UnhandledException;

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = DEFAULT_ANIMATION_FRAMERATE });

            Log_Initialize();
            Splash_Initialize();

            Splash_UpdateStatus("...Initializing", 10);

            TH_Global.FileLocations.CreateAllDirectories();

            InitializeComponent();
            DataContext = this;

            developerConsole.CurrentOutput = CLIENT_NAME;

            //this.SourceInitialized += new EventHandler(win_SourceInitialized);

            Application.Current.MainWindow = this;

            // Read Database Plugins (stores to static list in TH_Database.Global.Plugins)
            DatabasePluginReader.ReadPlugins();

            DeviceManager_Initialize();


            // Read Users and Login
            Splash_UpdateStatus("...Logging in User", 60);
            Users_Initialize();

            Splash_UpdateStatus("...Loading Plugins", 70);
            LoadPlugins();

            Splash_UpdateStatus("...Finishing Up", 100);

            WelcomeMessage();
            CheckVersion();

            // Wait for the minimum splash time to elapse, then close the splash dialog
            Splash_Close();

            ServerMonitor_Initialize();
        }

        private void Main_Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Main_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Users_ClientClosing();

            Plugins_Closed();

            Properties.Settings.Default.Save();
        }

        private void Main_Window_Closed(object sender, EventArgs e)
        {
            TH_Global.FileLocations.CleanTempDirectory(1);
        }

        void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //System.Windows.MessageBox.Show(e.ExceptionObject.ToString());

            OpenBugReport();
        }

        private void CheckVersion()
        {
            // Build Information
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            string last = Properties.Settings.Default.LastVersion;
            if (last != "intial" && last != version.ToString())
            {
                Properties.Settings.Default.LastVersion = version.ToString();
                Properties.Settings.Default.Save();

                // Add Notification to Message Center
                var message = new Controls.Message_Center.Message_Data();
                message.Title = "TrakHound Updated to " + version.ToString();
                message.Text = "TrakHound Successfully Updated from " + last + " to " + version.ToString();
                message.Type = Controls.Message_Center.MessageType.notification;
                message.Image = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Update_01.png"));
                messageCenter.AddMessage(message);
            }
        }

    }

}
