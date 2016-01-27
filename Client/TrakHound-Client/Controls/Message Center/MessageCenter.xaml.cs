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

        public void AddNotification(Message_Data data)
        {
            this.Dispatcher.BeginInvoke(new Action<Message_Data>(AddNotification_GUI), Priority, new object[] { data });
        }

        void AddNotification_GUI(Message_Data data)
        {
             Message m = new Message();
            m.Message_Type = Message.MessageType.notification;
            m.Message_Title = data.title;
            m.Message_Text = data.text;
            m.Message_AdditionalInfo = data.additionalInfo;
            m.Time = DateTime.Now.ToShortTimeString();
            m.CloseClicked += Notification_CloseClicked;        
            Notifications.Add(m);
            m.Shown = true;

            CheckForMessages();
        }

        public void AddDeviceAlert(Message_Data data)
        {
            this.Dispatcher.BeginInvoke(new Action<Message_Data>(AddDeviceAlert_GUI), Priority, new object[] { data });
        }

        void AddDeviceAlert_GUI(Message_Data data)
        {
            Message m = new Message();
            m.Message_Type = Message.MessageType.devicealert;
            m.Message_Title = data.title;
            m.Message_Text = data.text;
            m.Message_AdditionalInfo = data.additionalInfo;
            m.Time = DateTime.Now.ToShortTimeString();
            m.CloseClicked += DeviceAlert_CloseClicked;
            DeviceAlerts.Add(m);
            m.Shown = true;

            CheckForMessages();
        }

        public void AddWarning(Message_Data data)
        {
            this.Dispatcher.BeginInvoke(new Action<Message_Data>(AddWarning_GUI), Priority, new object[] { data });
        }

        void AddWarning_GUI(Message_Data data)
        {
            Message m = new Message();
            m.Message_Type = Message.MessageType.warning;
            m.Message_Title = data.title;
            m.Message_Text = data.text;
            m.Message_AdditionalInfo = data.additionalInfo;
            m.Time = DateTime.Now.ToShortTimeString();
            m.CloseClicked += Warning_CloseClicked;
            Warnings.Add(m);
            m.Shown = true;

            CheckForMessages();
        }

        public void AddError(Message_Data data)
        {
            this.Dispatcher.BeginInvoke(new Action<Message_Data>(AddError_GUI), Priority, new object[] { data });
        }

        void AddError_GUI(Message_Data data)
        {
            Message m = new Message();
            m.Message_Type = Message.MessageType.error;
            m.Message_Title = data.title;
            m.Message_Text = data.text;
            m.Message_AdditionalInfo = data.additionalInfo;
            m.Time = DateTime.Now.ToShortTimeString();
            m.CloseClicked += Error_CloseClicked;
            Errors.Add(m);
            m.Shown = true;

            CheckForMessages();
        }

        void Notification_CloseClicked(Message message)
        {
            message.Shown = false;
            CheckForMessages();
        }

        void DeviceAlert_CloseClicked(Message message)
        {
            message.Shown = false;
            CheckForMessages();
        }

        void Warning_CloseClicked(Message message)
        {
            message.Shown = false;
            CheckForMessages();
        }

        void Error_CloseClicked(Message message)
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

        //private void ClearAll_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    //Notifications.Clear();
        //    //DeviceAlerts.Clear();
        //    //Warnings.Clear();
        //    //Errors.Clear();

        //    foreach (Message msg in Notifications) msg.Shown = false;
        //    foreach (Message msg in DeviceAlerts) msg.Shown = false;
        //    foreach (Message msg in Warnings) msg.Shown = false;
        //    foreach (Message msg in Errors) msg.Shown = false;

        //    CheckForMessages();
        //}

        private void ClearAll_Clicked(TH_WPF.Button bt)
        {
            foreach (Message msg in Notifications) msg.Shown = false;
            foreach (Message msg in DeviceAlerts) msg.Shown = false;
            foreach (Message msg in Warnings) msg.Shown = false;
            foreach (Message msg in Errors) msg.Shown = false;

            CheckForMessages();
        }

    }

    public class Message_Data
    {
        public string title { get; set; }
        public string text { get; set; }
        public string additionalInfo { get; set; }
    }
}
