// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;

using TrakHound;
using TrakHound_UI.Windows;
using TrakHound.API;

namespace TrakHound_Dashboard
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            init();
        }

        private const int DEFAULT_ANIMATION_FRAMERATE = 60;

        public const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;
        public const System.Windows.Threading.DispatcherPriority PRIORITY_CONTEXT_IDLE = System.Windows.Threading.DispatcherPriority.ContextIdle;
        public const System.Windows.Threading.DispatcherPriority PRIORITY_APPLICATION_IDLE = System.Windows.Threading.DispatcherPriority.ApplicationIdle;

        public void init()
        {
            // Set Unhandled Exception handler
            AppDomain.CurrentDomain.UnhandledException += currentDomain_UnhandledException;

            // Set Animation Framerate
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = DEFAULT_ANIMATION_FRAMERATE });

            //Log_Initialize();
            Splash_Initialize();

            Splash_UpdateStatus("...Initializing", 10);

            FileLocations.CreateAllDirectories();

            ApiConfiguration.Set(ApiConfiguration.Read());

            InitializeComponent();
            DataContext = this;

            //developerConsole.CurrentOutput = ApplicationNames.TRAKHOUND_DASHBOARD;

            Application.Current.MainWindow = this;

            DeviceManager_Initialize();


            // Read Users and Login
            Splash_UpdateStatus("...Logging in User", 60);
            Users_Initialize();

            Splash_UpdateStatus("...Loading Plugins", 70);
            LoadPlugins();

            Splash_UpdateStatus("...Finishing Up", 100);

            WelcomeMessage();
            CheckVersion();

            Splash_Close();

            ServerMonitor_Initialize();
        }

        private void Main_Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Main_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var tabHeader in TabHeaders)
            {
                var tabPage = tabHeader.Page;
                if (tabPage != null)
                {
                    if (tabPage.PageContent != null) tabPage.PageContent.Closing();
                }
            }

            Plugins_Closed();

            Properties.Settings.Default.Save();
        }

        private void Main_Window_Closed(object sender, EventArgs e)
        {
            FileLocations.CleanTempDirectory(1);
        }

        void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            SendBugReport((Exception)e.ExceptionObject);

            Program.CloseApp = true;
            if (e.IsTerminating) Close();
        }

        private void SendBugReport(Exception ex)
        {
            var bugInfo = new Bugs.BugInfo(ex);
            bugInfo.Application = ApplicationNames.TRAKHOUND_DASHBOARD;
            bugInfo.Type = 0;

            var bugInfos = new List<Bugs.BugInfo>();
            bugInfos.Add(bugInfo);

            if (Bugs.Send(_currentuser, bugInfos))
            {
                var window = new BugReportSent();
                window.ShowDialog();
            }
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
                var message = new Controls.Message_Center.MessageData();
                message.Title = "TrakHound Updated to " + version.ToString();
                message.Text = "TrakHound Successfully Updated from " + last + " to " + version.ToString();
                message.Type = Messages.MessageType.TRAKHOUND_UPDATE;
                message.Image = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Update_01.png"));
                messageCenter.AddMessage(message);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_SHOWME)
            {
                ShowMe();
            }

            return IntPtr.Zero;
        }

        private void ShowMe()
        {
            if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;

            // get our current "TopMost" value (ours will always be false though)
            bool top = Topmost;
            // make our form jump to the top of everything
            Topmost = true;
            // set it back to whatever it was
            Topmost = top;
        }

        private void TopBar_UserInfo_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AccountManager_Open();
        }

    }

}
