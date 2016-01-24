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

            devicemanager = new DeviceManager(DeviceManagerType.Client);

            InitializeComponent();
            DataContext = this;

            Splash_UpdateStatus("...Initializing");
            this.SourceInitialized += new EventHandler(win_SourceInitialized);

            Application.Current.MainWindow = this;

            // Initialize Pages
            Pages_Initialize();

            // Set border thickness (maybe make this a static resource in XAML?)
            ResizeBorderThickness = 1;

            LoadDevices_Initialize();

            // Read Users and Login
            Splash_UpdateStatus("...Logging in User");
            ReadUserManagementSettings();
            devicemanager.UserDatabaseSettings = UserDatabaseSettings;

            LoginMenu.rememberMeType = RememberMeType.Client;
            LoginMenu.UserDatabaseSettings = UserDatabaseSettings;
            LoginMenu.LoadRememberMe();

            Splash_UpdateStatus("...Loading Plugins");
            LoadPlugins();

            // Wait for the minimum splash time to elapse, then close the splash dialog
            while (SplashWait) { System.Threading.Thread.Sleep(200); }
            Splash_Close();
        }

        void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            System.Windows.MessageBox.Show(e.ExceptionObject.ToString());
        }

    }

}
