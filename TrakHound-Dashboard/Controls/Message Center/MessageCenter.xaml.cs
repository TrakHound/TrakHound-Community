// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using TrakHound.API.Users;
using TrakHound.Tools;

namespace TrakHound_Dashboard.Controls.Message_Center
{
    /// <summary>
    /// Interaction logic for MessageCenter.xaml
    /// </summary>
    public partial class MessageCenter : UserControl
    {
        public MessageCenter()
        {
            InitializeComponent();
            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;
        }

        public MainWindow mw;

        public int UnreadMessages
        {
            get { return (int)GetValue(UnreadMessagesProperty); }
            set { SetValue(UnreadMessagesProperty, value); }
        }

        public static readonly DependencyProperty UnreadMessagesProperty =
            DependencyProperty.Register("UnreadMessages", typeof(int), typeof(MessageCenter), new PropertyMetadata(0));

        public void ClearMessages()
        {
            Notifications.Clear();
            DeviceAlerts.Clear();
            Warnings.Clear();
            Errors.Clear();

            CheckForMessages();
        }


        private ObservableCollection<Message> notifications;
        public ObservableCollection<Message> Notifications
        {
            get
            {
                if (notifications == null)
                    notifications = new ObservableCollection<Message>();

                return notifications;
            }

            set
            {
                notifications = value;
            }
        }

        private ObservableCollection<Message> devicealerts;
        public ObservableCollection<Message> DeviceAlerts
        {
            get
            {
                if (devicealerts == null)
                    devicealerts = new ObservableCollection<Message>();

                return devicealerts;
            }

            set
            {
                devicealerts = value;
            }
        }

        private ObservableCollection<Message> warnings;
        public ObservableCollection<Message> Warnings
        {
            get
            {
                if (warnings == null)
                    warnings = new ObservableCollection<Message>();

                return warnings;
            }

            set
            {
                warnings = value;
            }
        }

        private ObservableCollection<Message> errors;
        public ObservableCollection<Message> Errors
        {
            get
            {
                if (errors == null)
                    errors = new ObservableCollection<Message>();

                return errors;
            }

            set
            {
                errors = value;
            }
        }

        public void AddMessage(MessageData data)
        {
            this.Dispatcher.BeginInvoke(new Action<MessageData>(AddMessage_GUI), MainWindow.PRIORITY_BACKGROUND, new object[] { data });
        }

        void AddMessage_GUI(MessageData data)
        {        
            Message m = new Message(data);
            m.Clicked += Message_Clicked;
            m.CloseClicked += Message_CloseClicked;

            Notifications.Add(m);

            m.Shown = true;

            CheckForMessages();
        }

        void Message_Clicked(Message message)
        {
            message.Data.Read = true;

            if (message.Data != null)
            {
                if (message.Data.Action != null)
                {
                    message.Data.Action(message.Data.ActionParameter);
                }

                if (mw != null && mw.CurrentUser != null && message.Data != null && message.Data.Remote)
                {
                    var info = new UpdateMessageInfo();
                    info.UserConfig = mw.CurrentUser;
                    info.MessageId = message.Data.Id;

                    ThreadPool.QueueUserWorkItem(new WaitCallback(MessageClicked_Worker), info);
                }
            }

            CheckForMessages();
        }

        private class UpdateMessageInfo
        {
            public UserConfiguration UserConfig { get; set; }

            public string MessageId { get; set; }
        }

        private void MessageClicked_Worker(object o)
        {
            if (o != null)
            {
                var info = (UpdateMessageInfo)o;

                var messageInfos = new List<TrakHound.API.Messages.MessageInfo>();
                var messageInfo = new TrakHound.API.Messages.MessageInfo();
                messageInfo.Id = info.MessageId;
                messageInfos.Add(messageInfo);

                TrakHound.API.Messages.Update(info.UserConfig, messageInfos);
            }
        }


        void Message_CloseClicked(Message message)
        {
            if (mw != null && mw.CurrentUser != null && message.Data != null && message.Data.Remote)
            {
                var info = new CloseMessageInfo();
                info.UserConfig = mw.CurrentUser;
                info.MessageId = message.Data.Id;

                ThreadPool.QueueUserWorkItem(new WaitCallback(CloseMessage_Worker), info);
            }

            message.Shown = false;
            CheckForMessages();
        }

        private class CloseMessageInfo
        {
            public UserConfiguration UserConfig { get; set; }
            public string MessageId { get; set; }
        }

        private void CloseMessage_Worker(object o)
        {
            if (o != null)
            {
                var info = (CloseMessageInfo)o;

                var messageInfos = new List<TrakHound.API.Messages.MessageInfo>();
                var messageInfo = new TrakHound.API.Messages.MessageInfo();
                messageInfo.Id = info.MessageId;
                messageInfos.Add(messageInfo);

                TrakHound.API.Messages.Remove(info.UserConfig, messageInfos);
            }
        }

        public void CheckForMessages()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 500;
            timer.Elapsed += CheckForMessages_TIMER_Elapsed;
            timer.Enabled = true;
        }

        void CheckForMessages_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ((System.Timers.Timer)sender).Enabled = false;
            this.Dispatcher.BeginInvoke(new Action(CheckForMessages_GUI));
        }

        void CheckForMessages_GUI()
        {
            if (Notifications.ToList().FindAll(x => x.Shown == true).Count > 0 ||
               DeviceAlerts.ToList().FindAll(x => x.Shown == true).Count > 0 ||
               Warnings.ToList().FindAll(x => x.Shown == true).Count > 0 ||
               Errors.ToList().FindAll(x => x.Shown == true).Count > 0) NoMessages = false;
            else NoMessages = true;

            if (Notifications.ToList().FindAll(x => x.Shown == true).Count > 0) NoNotifications = false;
            else NoNotifications = true;

            if (DeviceAlerts.ToList().FindAll(x => x.Shown == true).Count > 0) NoDeviceAlerts = false;
            else NoDeviceAlerts = true;

            if (Warnings.ToList().FindAll(x => x.Shown == true).Count > 0) NoWarnings = false;
            else NoWarnings = true;

            if (Errors.ToList().FindAll(x => x.Shown == true).Count > 0) NoErrors = false;
            else NoErrors = true;

            foreach (Message msg in Notifications.ToList().FindAll(x => x.Shown == false)) Notifications.Remove(msg);

            // Get Unread Message Count
            mw.NotificationsCount = Notifications.ToList().Count(x => x.Read == false);
            mw.NotificationsCount += DeviceAlerts.ToList().Count(x => x.Read == false);
            mw.NotificationsCount += Warnings.ToList().Count(x => x.Read == false);
            mw.NotificationsCount += Errors.ToList().Count(x => x.Read == false);
        }


        public bool NoMessages
        {
            get { return (bool)GetValue(NoMessagesProperty); }
            set { SetValue(NoMessagesProperty, value); }
        }

        public static readonly DependencyProperty NoMessagesProperty =
            DependencyProperty.Register("NoMessages", typeof(bool), typeof(MessageCenter), new PropertyMetadata(true));

        public bool NoNotifications
        {
            get { return (bool)GetValue(NoNotificationsProperty); }
            set { SetValue(NoNotificationsProperty, value); }
        }

        public static readonly DependencyProperty NoNotificationsProperty =
            DependencyProperty.Register("NoNotifications", typeof(bool), typeof(MessageCenter), new PropertyMetadata(true));

        public bool NoDeviceAlerts
        {
            get { return (bool)GetValue(NoDeviceAlertsProperty); }
            set { SetValue(NoDeviceAlertsProperty, value); }
        }

        public static readonly DependencyProperty NoDeviceAlertsProperty =
            DependencyProperty.Register("NoDeviceAlerts", typeof(bool), typeof(MessageCenter), new PropertyMetadata(true));

        public bool NoWarnings
        {
            get { return (bool)GetValue(NoWarningsProperty); }
            set { SetValue(NoWarningsProperty, value); }
        }

        public static readonly DependencyProperty NoWarningsProperty =
            DependencyProperty.Register("NoWarnings", typeof(bool), typeof(MessageCenter), new PropertyMetadata(true));

        public bool NoErrors
        {
            get { return (bool)GetValue(NoErrorsProperty); }
            set { SetValue(NoErrorsProperty, value); }
        }

        public static readonly DependencyProperty NoErrorsProperty =
            DependencyProperty.Register("NoErrors", typeof(bool), typeof(MessageCenter), new PropertyMetadata(true));



        public bool Shown
        {
            get { return (bool)GetValue(ShownProperty); }
            set { SetValue(ShownProperty, value); }
        }

        public static readonly DependencyProperty ShownProperty =
            DependencyProperty.Register("Shown", typeof(bool), typeof(MessageCenter), new PropertyMetadata(true));

        public void Hide()
        {

            if (!IsMouseOver) Shown = false;

        }

        private void ClearAll_Clicked(TrakHound_UI.Button bt)
        {
            var result = TrakHound_UI.MessageBox.Show("Are you sure you want to delete All messages?", "Delete All Messages", TrakHound_UI.MessageBoxButtons.YesNo);
            if (result == TrakHound_UI.MessageBoxDialogResult.Yes)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (mw != null && mw.CurrentUser != null)
                    {
                        var ids = new List<string>();

                        foreach (Message msg in Notifications) if (msg.Data.Remote) ids.Add(msg.Data.Id);
                        foreach (Message msg in DeviceAlerts) if (msg.Data.Remote) ids.Add(msg.Data.Id);
                        foreach (Message msg in Warnings) if (msg.Data.Remote) ids.Add(msg.Data.Id);
                        foreach (Message msg in Errors) if (msg.Data.Remote) ids.Add(msg.Data.Id);

                        if (ids.Count > 0)
                        {
                            var info = new ClearMessagesInfo();
                            info.UserConfig = mw.CurrentUser;
                            info.MessageIds = ids.ToArray();

                            ThreadPool.QueueUserWorkItem(new WaitCallback(ClearMessages_Worker), info);
                        }
                    }
                }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });

                foreach (Message msg in Notifications) msg.Shown = false;
                foreach (Message msg in DeviceAlerts) msg.Shown = false;
                foreach (Message msg in Warnings) msg.Shown = false;
                foreach (Message msg in Errors) msg.Shown = false;
            }

            CheckForMessages();
        }

        private class ClearMessagesInfo
        {
            public UserConfiguration UserConfig { get; set; }
            public string[] MessageIds { get; set; }
        }

        private void ClearMessages_Worker(object o)
        {
            if (o != null)
            {
                var info = (ClearMessagesInfo)o;

                var infos = new List<TrakHound.API.Messages.MessageInfo>();

                foreach (var id in info.MessageIds)
                {
                    var messageInfo = new TrakHound.API.Messages.MessageInfo();
                    messageInfo.Id = id;
                    infos.Add(messageInfo);
                }

                TrakHound.API.Messages.Remove(info.UserConfig, infos);
            }
        }

    }

}
