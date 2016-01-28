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
using System.Configuration;
using System.Collections.ObjectModel;

namespace TrakHound_Client.Controls.Message_Center
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

        public TrakHound_Client.MainWindow mw;

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

        const System.Windows.Threading.DispatcherPriority Priority = System.Windows.Threading.DispatcherPriority.Background;

        public void AddMessage(Message_Data data)
        {
            this.Dispatcher.BeginInvoke(new Action<Message_Data>(AddMessage_GUI), Priority, new object[] { data });
        }

        void AddMessage_GUI(Message_Data data)
        {        
            Message m = new Message(data);
            m.Clicked += Message_Clicked;
            m.CloseClicked += Message_CloseClicked;

            switch (data.Type)
            {
                case MessageType.notification:
                    Notifications.Add(m);
                    break;
                case MessageType.devicealert:
                    DeviceAlerts.Add(m);
                    break;
                case MessageType.warning:
                    Warnings.Add(m);
                    break;
                case MessageType.error:
                    Errors.Add(m);
                    break;
            }

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
            }
        }

        void Message_CloseClicked(Message message)
        {
            message.Shown = false;
            CheckForMessages();
        }

        void CheckForMessages()
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

            if (mw != null) mw.NotificationsCount =
                Notifications.ToList().FindAll(x => x.Shown == true).Count +
                DeviceAlerts.ToList().FindAll(x => x.Shown == true).Count +
                Warnings.ToList().FindAll(x => x.Shown == true).Count +
                Errors.ToList().FindAll(x => x.Shown == true).Count;

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

        private void ClearAll_Clicked(TH_WPF.Button bt)
        {
            foreach (Message msg in Notifications) msg.Shown = false;
            foreach (Message msg in DeviceAlerts) msg.Shown = false;
            foreach (Message msg in Warnings) msg.Shown = false;
            foreach (Message msg in Errors) msg.Shown = false;

            CheckForMessages();
        }

    }

    public enum MessageType
    {
        notification, devicealert, warning, error
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class Message_Data
    {
        public string Id { get; set; }
        public MessageType Type { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string AdditionalInfo { get; set; }
        public BitmapImage Image { get; set; }
        public System.Action<object> Action { get; set; }
        public object ActionParameter { get; set; }
        public bool Read { get; set; }
        public DateTime AddedTime { get; set; }
    }
}
