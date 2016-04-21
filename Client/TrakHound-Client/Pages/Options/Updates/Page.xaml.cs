// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
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
        }

        public string Title { get { return "Updates"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Update_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }


        private void Load()
        {
            var config = TH_Global.Updates.UpdateConfiguration.Read();

            //QueueWriteInterval = config.QueueWriteInterval;

            //DebugEnabled = config.Debug;
            //ErrorEnabled = config.Error;
            //NotificationEnabled = config.Notification;
            //WarningEnabled = config.Warning;

            //DebugRecycleDays = GetMillisecondsFromDays(config.DebugRecycleDays);
            //ErrorRecycleDays = GetMillisecondsFromDays(config.ErrorRecycleDays);
            //NotificationRecycleDays = GetMillisecondsFromDays(config.NotificationRecycleDays);
            //WarningRecycleDays = GetMillisecondsFromDays(config.WarningRecycleDays);
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
            var config = new TH_Global.Updates.UpdateConfiguration();

            //config.QueueWriteInterval = QueueWriteInterval;

            //config.Debug = DebugEnabled;
            //config.Error = ErrorEnabled;
            //config.Notification = NotificationEnabled;
            //config.Warning = WarningEnabled;

            //config.DebugRecycleDays = GetDaysFromMilliseconds(DebugRecycleDays);
            //config.ErrorRecycleDays = GetDaysFromMilliseconds(ErrorRecycleDays);
            //config.NotificationRecycleDays = GetDaysFromMilliseconds(NotificationRecycleDays);
            //config.WarningRecycleDays = GetDaysFromMilliseconds(WarningRecycleDays);

            //TH_Global.Logger.LoggerConfiguration.Create(config);
        }

        private static int GetDaysFromMilliseconds(long ms)
        {
            var ts = TimeSpan.FromMilliseconds(ms);
            return ts.Days;
        }

        private static long GetMillisecondsFromDays(int days)
        {
            var ts = TimeSpan.FromDays(days);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }


        #region "Dependency Properties"

        private static void Value_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var o = obj as Page;
            if (o != null) o.SettingChanged();
        }


        public int UpdateCheckInterval
        {
            get { return (int)GetValue(UpdateCheckIntervalProperty); }
            set { SetValue(UpdateCheckIntervalProperty, value); }
        }

        public static readonly DependencyProperty UpdateCheckIntervalProperty =
            DependencyProperty.Register("UpdateCheckInterval", typeof(int), typeof(Page), new PropertyMetadata(5000, new PropertyChangedCallback(Value_PropertyChanged)));


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





        public int QueueWriteInterval
        {
            get { return (int)GetValue(QueueWriteIntervalProperty); }
            set { SetValue(QueueWriteIntervalProperty, value); }
        }

        public static readonly DependencyProperty QueueWriteIntervalProperty =
            DependencyProperty.Register("QueueWriteInterval", typeof(int), typeof(Page), new PropertyMetadata(5000, new PropertyChangedCallback(Value_PropertyChanged)));



        public bool DebugEnabled
        {
            get { return (bool)GetValue(DebugEnabledProperty); }
            set { SetValue(DebugEnabledProperty, value); }
        }

        public static readonly DependencyProperty DebugEnabledProperty =
            DependencyProperty.Register("DebugEnabled", typeof(bool), typeof(Page), new PropertyMetadata(false, new PropertyChangedCallback(Value_PropertyChanged)));


        public bool ErrorEnabled
        {
            get { return (bool)GetValue(ErrorEnabledProperty); }
            set { SetValue(ErrorEnabledProperty, value); }
        }

        public static readonly DependencyProperty ErrorEnabledProperty =
            DependencyProperty.Register("ErrorEnabled", typeof(bool), typeof(Page), new PropertyMetadata(true, new PropertyChangedCallback(Value_PropertyChanged)));


        public bool NotificationEnabled
        {
            get { return (bool)GetValue(NotificationEnabledProperty); }
            set { SetValue(NotificationEnabledProperty, value); }
        }

        public static readonly DependencyProperty NotificationEnabledProperty =
            DependencyProperty.Register("NotificationEnabled", typeof(bool), typeof(Page), new PropertyMetadata(true, new PropertyChangedCallback(Value_PropertyChanged)));


        public bool WarningEnabled
        {
            get { return (bool)GetValue(WarningEnabledProperty); }
            set { SetValue(WarningEnabledProperty, value); }
        }

        public static readonly DependencyProperty WarningEnabledProperty =
            DependencyProperty.Register("WarningEnabled", typeof(bool), typeof(Page), new PropertyMetadata(true, new PropertyChangedCallback(Value_PropertyChanged)));


        private const long DAY_MS = 86400000;
        private const long WEEK_MS = 604800000;


        public long DebugRecycleDays
        {
            get { return (long)GetValue(DebugRecycleDaysProperty); }
            set { SetValue(DebugRecycleDaysProperty, value); }
        }

        public static readonly DependencyProperty DebugRecycleDaysProperty =
            DependencyProperty.Register("DebugRecycleDays", typeof(long), typeof(Page), new PropertyMetadata(WEEK_MS, new PropertyChangedCallback(Value_PropertyChanged)));


        public long ErrorRecycleDays
        {
            get { return (long)GetValue(ErrorRecycleDaysProperty); }
            set { SetValue(ErrorRecycleDaysProperty, value); }
        }

        public static readonly DependencyProperty ErrorRecycleDaysProperty =
            DependencyProperty.Register("ErrorRecycleDays", typeof(long), typeof(Page), new PropertyMetadata(WEEK_MS, new PropertyChangedCallback(Value_PropertyChanged)));


        public long NotificationRecycleDays
        {
            get { return (long)GetValue(NotificationRecycleDaysProperty); }
            set { SetValue(NotificationRecycleDaysProperty, value); }
        }

        public static readonly DependencyProperty NotificationRecycleDaysProperty =
            DependencyProperty.Register("NotificationRecycleDays", typeof(long), typeof(Page), new PropertyMetadata(DAY_MS, new PropertyChangedCallback(Value_PropertyChanged)));


        public long WarningRecycleDays
        {
            get { return (long)GetValue(WarningRecycleDaysProperty); }
            set { SetValue(WarningRecycleDaysProperty, value); }
        }

        public static readonly DependencyProperty WarningRecycleDaysProperty =
            DependencyProperty.Register("WarningRecycleDays", typeof(long), typeof(Page), new PropertyMetadata(DAY_MS, new PropertyChangedCallback(Value_PropertyChanged)));

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

        }

        private void ApplyUpdates_Clicked(TH_WPF.Button bt)
        {

        }

        private void ClearUpdatesQueue_Clicked(TH_WPF.Button bt)
        {

        }

        private void RestoreDefaults_Clicked(TH_WPF.Button bt)
        {
            QueueWriteInterval = 5000;

            DebugEnabled = false;
            ErrorEnabled = true;
            NotificationEnabled = true;
            WarningEnabled = true;

            DebugRecycleDays = WEEK_MS;
            ErrorRecycleDays = WEEK_MS;
            NotificationRecycleDays = DAY_MS;
            WarningRecycleDays = DAY_MS;
        }

    }
}
