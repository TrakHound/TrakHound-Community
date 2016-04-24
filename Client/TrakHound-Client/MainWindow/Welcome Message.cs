using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrakHound_Client
{
    public partial class MainWindow
    {

        void WelcomeMessage()
        {
            if (Properties.Settings.Default.FirstOpen)
            {
                // Add Notification to Message Center
                var message = new Controls.Message_Center.Message_Data();
                message.Title = "Welcome to TrakHound!";
                message.Text = "Click to view Help on Getting Started";
                message.Type = Controls.Message_Center.MessageType.notification;
                message.Image = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/TrakHound_Logo_Initials_10_30px.png"));
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
