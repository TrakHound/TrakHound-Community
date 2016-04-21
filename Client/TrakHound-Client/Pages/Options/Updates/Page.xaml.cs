// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TH_Global;
using TH_Global.Functions;
using TH_Global.Updates;

namespace TrakHound_Client.Pages.Options.Updates
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, IPage
    {

        public Page()
        {
            InitializeComponent();
            DataContext = this;

            Load();

            RegistryCheck_Initialize();
        }

        public string Title { get { return "Updates"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Update_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }


        private void Load()
        {
            var config = UpdateConfiguration.Read();

            UpdateCheckInterval = GetMillisecondsFromMinutes(config.UpdateCheckInterval);

            UpdatesEnabled = config.Enabled;
        }

        private System.Timers.Timer settingChangedTimer;

        public void SettingChanged()
        {
            if (settingChangedTimer != null) settingChangedTimer.Enabled = false;

            settingChangedTimer = new System.Timers.Timer();
            settingChangedTimer.Interval = 1000;
            settingChangedTimer.Elapsed += SettingChangedTimer_Elapsed;
            settingChangedTimer.Enabled = true;
        }

        private void SettingChangedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var timer = (System.Timers.Timer)sender;
            timer.Enabled = false;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                Save();
            }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });           
        }

        private void Save()
        {
            var config = new UpdateConfiguration();

            config.UpdateCheckInterval = GetMinutesFromMilliseconds(UpdateCheckInterval);

            config.Enabled = UpdatesEnabled;

            config.CheckNow = checkNow;
            config.ApplyNow = applyNow;
            config.ClearUpdateQueue = clearUpdateQueue;

            UpdateConfiguration.Create(config);
        }

        private static int GetMinutesFromMilliseconds(long ms)
        {
            var ts = TimeSpan.FromMilliseconds(ms);
            return Convert.ToInt32(ts.TotalMinutes);
        }

        private static long GetMillisecondsFromMinutes(int minutes)
        {
            var ts = TimeSpan.FromMinutes(minutes);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }


        private bool checkNow;
        private bool applyNow;
        private bool clearUpdateQueue;


        private System.Timers.Timer registryCheckTimer;

        private void RegistryCheck_Initialize()
        {
            if (registryCheckTimer != null) registryCheckTimer.Enabled = false;

            registryCheckTimer = new System.Timers.Timer();
            registryCheckTimer.Interval = 5000;
            registryCheckTimer.Elapsed += RegistryCheckTimer_Elapsed;
            registryCheckTimer.Enabled = true;
        }

        private void RegistryCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var timer = (System.Timers.Timer)sender;
            timer.Enabled = false;

            int availableUpdates = 0;

            var appNames = Registry_Functions.GetKeyNames();
            if (appNames != null)
            {
                foreach (var appName in appNames)
                {
                    var valueNames = Registry_Functions.GetValueNames(appName);
                    if (valueNames != null)
                    {
                        var updatePath = valueNames.ToList().Exists(x => x == "update_path");
                        if (updatePath) availableUpdates++;
                    }
                }
            }

            Dispatcher.BeginInvoke(new Action(() => { AvailableUpdates = availableUpdates; }));

            timer.Enabled = true;
        }

        #region "Dependency Properties"

        private static void Value_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var o = obj as Page;
            if (o != null) o.SettingChanged();
        }

        private const long HOUR_MS = 3600000;

        public long UpdateCheckInterval
        {
            get { return (long)GetValue(UpdateCheckIntervalProperty); }
            set { SetValue(UpdateCheckIntervalProperty, value); }
        }

        public static readonly DependencyProperty UpdateCheckIntervalProperty =
            DependencyProperty.Register("UpdateCheckInterval", typeof(long), typeof(Page), new PropertyMetadata(HOUR_MS, new PropertyChangedCallback(Value_PropertyChanged)));


        public bool UpdatesEnabled
        {
            get { return (bool)GetValue(UpdatesEnabledProperty); }
            set { SetValue(UpdatesEnabledProperty, value); }
        }

        public static readonly DependencyProperty UpdatesEnabledProperty =
            DependencyProperty.Register("UpdatesEnabled", typeof(bool), typeof(Page), new PropertyMetadata(false, new PropertyChangedCallback(Value_PropertyChanged)));


        public int AvailableUpdates
        {
            get { return (int)GetValue(AvailableUpdatesProperty); }
            set { SetValue(AvailableUpdatesProperty, value); }
        }

        public static readonly DependencyProperty AvailableUpdatesProperty =
            DependencyProperty.Register("AvailableUpdates", typeof(int), typeof(Page), new PropertyMetadata(0));


        public bool UpdatesSetToApply
        {
            get { return (bool)GetValue(UpdatesSetToApplyProperty); }
            set { SetValue(UpdatesSetToApplyProperty, value); }
        }

        public static readonly DependencyProperty UpdatesSetToApplyProperty =
            DependencyProperty.Register("UpdatesSetToApply", typeof(bool), typeof(Page), new PropertyMetadata(false));

        #endregion


        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = false;
                    }
                }
            }
        }


        private void CheckForUpdates_Clicked(TH_WPF.Button bt)
        {
            UpdatesSetToApply = false;

            checkNow = true;
            SettingChanged();
        }

        private void ApplyUpdates_Clicked(TH_WPF.Button bt)
        {
            applyNow = true;
            SettingChanged();

            UpdatesSetToApply = true;
        }

        private void ClearUpdatesQueue_Clicked(TH_WPF.Button bt)
        {
            checkNow = false;
            applyNow = false;
            clearUpdateQueue = true;
            SettingChanged();
        }

        private void RestoreDefaults_Clicked(TH_WPF.Button bt)
        {
            checkNow = false;
            applyNow = false;
            clearUpdateQueue = false;

            UpdateCheckInterval = HOUR_MS;

            UpdatesEnabled = true;
        }

    }
}
