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
using System.Collections.ObjectModel;

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

            //UpdaterCommunications_Initialize();

            Load();

            //RegistryCheck_Initialize();
            GetInstalledApplications();
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

        #region "Update Items"

        ObservableCollection<UpdateItem> _updateItems;
        public ObservableCollection<UpdateItem> UpdateItems
        {
            get
            {
                if (_updateItems == null)
                    _updateItems = new ObservableCollection<UpdateItem>();
                return _updateItems;
            }

            set
            {
                _updateItems = value;
            }
        }


        private void GetInstalledApplications()
        {
            var appNames = Registry_Functions.GetKeyNames();
            if (appNames != null)
            {
                foreach (var appName in appNames)
                {
                    var valueNames = Registry_Functions.GetValueNames(appName);

                    var item = new UpdateItem();
                    item.ApplyClicked += Item_ApplyClicked;
                    item.CheckForUpdatesClicked += Item_CheckForUpdatesClicked;

                    item.ApplicationName = appName;
                    item.ApplicationTitle = GetValueData(valueNames.ToList().Find(x => x == "title"), appName);
                    item.ApplicationSubtitle = GetValueData(valueNames.ToList().Find(x => x == "subtitle"), appName);

                    UpdateItems.Add(item);
                }
            }
        }

        private void Item_CheckForUpdatesClicked(UpdateItem item)
        {
            item.Loading = true;
            item.Status = "Retrieving Update Information..";

            var message = new WCF_Functions.MessageData("check");
            message.Data01 = item.ApplicationName;
            SendMessage(message);
        }

        private void Item_ApplyClicked(UpdateItem item)
        {
            item.Loading = false;
            item.Status = "Restart TrakHound to Apply Update";

            var message = new WCF_Functions.MessageData("apply");
            message.Data01 = item.ApplicationName;
            SendMessage(message);
        }

        private static string GetValueData(string valueName, string appName)
        {
            return Registry_Functions.GetValue(valueName, appName);
        }

        #endregion


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

        private void CheckForDownloadedUpdates()
        {
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
                        if (updatePath)
                        {
                            var item = new UpdateItem();
                            item.AvailableVersion = valueNames.ToList().Find(x => x == "update_version");
                            UpdateItems.Add(item);

                            availableUpdates++;
                        }
                    }
                }
            }

            AvailableUpdates = availableUpdates;
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
            SendMessage(new WCF_Functions.MessageData("check"));

            UpdatesSetToApply = false;
        }

        private void ApplyUpdates_Clicked(TH_WPF.Button bt)
        {
            SendMessage(new WCF_Functions.MessageData("apply"));

            UpdatesSetToApply = true;
        }

        private void ClearUpdatesQueue_Clicked(TH_WPF.Button bt)
        {
            SendMessage(new WCF_Functions.MessageData("clear"));
        }

        private void RestoreDefaults_Clicked(TH_WPF.Button bt)
        {


            UpdateCheckInterval = HOUR_MS;

            UpdatesEnabled = true;
        }

        #region "WCF Updater Communications"

        /// <summary>
        /// Worker class run on a separate thread and send back the MessageData using an event
        /// </summary>
        private class MessageWorker : WCF_Functions.IMessageCallback
        {
            WCF_Functions.IMessage messageProxy;

            public MessageWorker()
            {
                messageProxy = WCF_Functions.Client.GetWithCallback(UpdateConfiguration.UPDATER_PIPE_NAME, this);
            }

            public void SendMessage(WCF_Functions.MessageData data)
            {
                try
                {
                    if (messageProxy != null) messageProxy.SendData(data);
                }
                catch (Exception ex) { TH_Global.Logger.Log("Exception : " + ex.Message); }
            }

            public delegate void MessageRecieved_Handler(WCF_Functions.MessageData data);
            public event MessageRecieved_Handler MessageRecieved;

            public void OnCallback(WCF_Functions.MessageData data)
            {
                if (MessageRecieved != null) MessageRecieved(data);
            }
        }

        MessageWorker messageWorker;

        private void SendMessage(WCF_Functions.MessageData data)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(SendMessage_Worker, data);
        }

        private void SendMessage_Worker(object o)
        {
            messageWorker = new MessageWorker();
            messageWorker.MessageRecieved += MessageWorker_MessageRecieved;
            messageWorker.SendMessage((WCF_Functions.MessageData)o);

            //int timeout = 500;
            //int t = 0;
            //while (t < timeout)
            //{
            //    t++;

            //    System.Threading.Thread.Sleep(10);
            //}

            //TH_Global.Logger.Log("SendMessage_Worker Exited", TH_Global.Logger.LogLineType.Notification);
        }

        private void MessageWorker_MessageRecieved(WCF_Functions.MessageData data)
        {
            //TH_Global.Logger.Log("MessageWorker_MessageRecieved", TH_Global.Logger.LogLineType.Notification);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                switch (data.Id)
                {
                    case "download_progress_percentage": DownloadProgressPercentage(data); break;

                    case "download_completed": DownloadCompleted(data); break;

                    case "update_ready": UpdateReady(data); break;

                    case "up_to_date": UpToDate(data); break;
                }

            }));
            
        }

        private void DownloadProgressPercentage(WCF_Functions.MessageData data)
        {
            // data.Data01 = Application Name
            // data.Data02 = Download Progress (int)

            string name = data.Data01.ToString();
            int percentage = (int)data.Data02;

            int index = UpdateItems.ToList().FindIndex(x => x.ApplicationName == name);
            if (index >= 0)
            {
                var item = UpdateItems[index];
                item.ProgressValue = percentage;
                item.Status = "Downloading..";
            }
        }


        private void DownloadCompleted(WCF_Functions.MessageData data)
        {
            // data.Data01 = Application Name

            string name = data.Data01.ToString();

            int index = UpdateItems.ToList().FindIndex(x => x.ApplicationName == name);
            if (index >= 0)
            {
                var item = UpdateItems[index];
                item.ProgressValue = 100;
                item.Status = "Extracting Files..";
            }
        }


        private void UpdateReady(WCF_Functions.MessageData data)
        {
            // data.Data01 = Application Name
            // data.Data02 = Version

            string name = data.Data01.ToString();
            string version = data.Data02.ToString();

            int index = UpdateItems.ToList().FindIndex(x => x.ApplicationName == name);
            if (index >= 0)
            {
                var item = UpdateItems[index];
                item.ProgressValue = 120;
                item.Loading = false;
                item.Status = version + " Update Ready";
            }
        }


        private void UpToDate(WCF_Functions.MessageData data)
        {
            // data.Data01 = Application Name

            string name = data.Data01.ToString();

            int index = UpdateItems.ToList().FindIndex(x => x.ApplicationName == name);
            if (index >= 0)
            {
                var item = UpdateItems[index];
                item.Status = "Up to Date";
            }
        }

        #endregion

    }
}
