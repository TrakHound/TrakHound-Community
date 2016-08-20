// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {

        void WelcomeMessage()
        {
            if (Properties.Settings.Default.FirstOpen)
            {
                // Add Notification to Message Center
                var message = new Controls.Message_Center.MessageData();
                message.Title = "Welcome to TrakHound!";
                message.Text = "Click to view Help on Getting Started";
                message.Type = TrakHound.API.Messages.MessageType.TRAKHOUND_PRIVATE;
                message.Image = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/TrakHound_Logo_Initials_10_30px.png"));
                message.Action = new Action<object>(WelcomeMessage_Clicked);
                messageCenter.AddMessage(message);

                Properties.Settings.Default.FirstOpen = false;
                Properties.Settings.Default.Save();
            }
        }

        public void WelcomeMessage_Clicked(object obj)
        {
            System.Diagnostics.Process.Start("https://github.com/TrakHound/TrakHound/wiki");
        }

    }
}
