// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Linq;
using System.Windows;

using TrakHound.API;
using TrakHound.Tools;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {

        public int NotificationsCount
        {
            get { return (int)GetValue(NotificationsCountProperty); }
            set { SetValue(NotificationsCountProperty, value); }
        }

        public static readonly DependencyProperty NotificationsCountProperty =
            DependencyProperty.Register("NotificationsCount", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        private void MessageCenter_ToolBarItem_Clicked(TrakHound_UI.Button bt)
        {
            messageCenter.Shown = !messageCenter.Shown;
        }


        private void StartMessageMonitor()
        {
            var timer = new System.Timers.Timer();
            timer.Interval = 10000;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var timer = (System.Timers.Timer)sender;
            timer.Interval = 30000;
            timer.Enabled = false;

            if (_currentuser != null)
            {
                var messages = Messages.Get(_currentuser);
                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            int i = messageCenter.Notifications.ToList().FindIndex(x => x.Data.Id == message.Id);
                            if (i >= 0)
                            {
                                var msg = messageCenter.Notifications[i];

                                if (message.ReadTimestamp.HasValue) msg.Read = true;
                                else msg.Read = false;
                            }
                            else
                            {
                                var msg = new Controls.Message_Center.MessageData(message);
                                messageCenter.AddMessage(msg);
                            }

                            messageCenter.CheckForMessages();

                        }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });

                    }
                }

                timer.Enabled = true;
            }
            else
            {
                timer.Enabled = false;
                timer = null;
            }
        }
    }
}
