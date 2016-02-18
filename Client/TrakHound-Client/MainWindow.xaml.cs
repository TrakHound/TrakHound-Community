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

using Microsoft.Shell;

using System.Threading;

using System.Data;
using System.Xml;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using WinInterop = System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Drawing.Printing;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using TH_Configuration;
using TH_DeviceManager;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Client;
using TH_WPF;
using TH_Updater;
using TH_UserManagement;
using TH_UserManagement.Management;

using TrakHound_Client.Controls;

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

        public void init()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += currentDomain_UnhandledException;

            Log_Initialize();
            Splash_Initialize();


            Splash_UpdateStatus("...Initializing", 10);

            InitializeComponent();
            DataContext = this;

            this.SourceInitialized += new EventHandler(win_SourceInitialized);

            Application.Current.MainWindow = this;

            // Read Database Plugins (stores to static list in TH_Database.Global.Plugins)
            DatabasePluginReader.ReadPlugins();

            devicemanager = new DeviceManager(DeviceManagerType.Client);

            // Initialize Pages
            Splash_UpdateStatus("...Creating Pages", 40);
            Pages_Initialize();

            // Set border thickness (maybe make this a static resource in XAML?)
            ResizeBorderThickness = 1;

            LoadDevices_Initialize();

            // Read Users and Login
            Splash_UpdateStatus("...Logging in User", 60);
            ReadUserManagementSettings();

            LoginMenu.rememberMeType = RememberMeType.Client;
            LoginMenu.LoadRememberMe();

            Splash_UpdateStatus("...Loading Plugins", 70);
            LoadPlugins();

            Splash_UpdateStatus("...Finishing Up", 100);

            WelcomeMessage();

            // Wait for the minimum splash time to elapse, then close the splash dialog
            //while (SplashWait) { System.Threading.Thread.Sleep(200); }
            Splash_Close();
        }

        private void Main_Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Main_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Plugins_Closing();

            DevicesMonitor_Close();

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
