// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {

        private void AddWelcomeMessage()
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

        //private void AddRegenerateUpdateMessage()
        //{
        //    if (Properties.Settings.Default.FirstOpen)
        //    {
        //        // Add Notification to Message Center
        //        var message = new Controls.Message_Center.MessageData();
        //        message.Title = "Update Notifications for v1.4.3";
        //        message.Text = "Changes were made prior to v1.4.3 and Device Configurations may need to be updated to apply these changes. This Regenerate feature will however remove ";
        //        message.Type = TrakHound.API.Messages.MessageType.TRAKHOUND_PRIVATE;
        //        message.Image = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Update_01.png"));
        //        message.Action = new Action<object>(RegenerateUpdateMessage_Clicked);
        //        messageCenter.AddMessage(message);

        //        Properties.Settings.Default.FirstOpen = false;
        //        Properties.Settings.Default.Save();
        //    }
        //}

        //public void RegenerateUpdateMessage_Clicked(object obj)
        //{



        //    System.Diagnostics.Process.Start("https://github.com/TrakHound/TrakHound/wiki");
        //}

    }
}
