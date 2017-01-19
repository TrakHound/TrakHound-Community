// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

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

        public void init()
        {
            UpdateUserSettings();

            // Set Animation Framerate
            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = DEFAULT_ANIMATION_FRAMERATE });

            Splash_Initialize();

            Splash_UpdateStatus("...Initializing", 10);

            FileLocations.CreateAllDirectories();

            ApiConfiguration.Set(ApiConfiguration.Read());

            InitializeComponent();
            DataContext = this;

            Application.Current.MainWindow = this;

            DeviceManager_DeviceList_Initialize();

            // Read Users and Login
            Splash_UpdateStatus("...Logging in User", 40);
            Users_Initialize();

            Splash_UpdateStatus("...Loading Plugins", 60);
            LoadPlugins();
            Pages.DeviceManager.EditPage.GetPluginPageInfos();

            Splash_UpdateStatus("...Finishing Up", 100);

            AddWelcomeMessage();
            CheckVersion();

            Splash_Close();

            ServerMonitor_Initialize();
        }

        private void UpdateUserSettings()
        {
            if (Properties.Settings.Default.UpdateSettings)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateSettings = false;
                Properties.Settings.Default.Save();
            }
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

        private void CheckVersion()
        {
            // v1.4.3 Regenerate Device Configuration Notice
            Version version = null;
            if (Version.TryParse(Properties.Settings.Default.LastVersion, out version))
            {
                if (version < new Version("1.4.3"))
                {
                    var u = new UpdateNotification();
                    u.mw = this;
                    u.Show();
                    u.Focus();
                }
            }


            // Build Information
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            version = assembly.GetName().Version;

            string format = "{0}.{1}.{2}";
            string s = string.Format(format, version.Major, version.Minor, version.Build);

            string last = Properties.Settings.Default.LastVersion;
            if (last != "intial" && last != s)
            {
                Properties.Settings.Default.LastVersion = s;
                Properties.Settings.Default.Save();

                // Add Notification to Message Center
                var message = new Controls.Message_Center.MessageData();
                message.Title = "TrakHound Updated to " + s;
                message.Text = "TrakHound Successfully Updated from " + last + " to " + s;
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
