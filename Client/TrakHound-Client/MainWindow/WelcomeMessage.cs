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
                Controls.Message_Center.Message_Data mData = new Controls.Message_Center.Message_Data();
                mData.Title = "Welcome to TrakHound!";
                mData.Text = "Click to view Help on Getting Started";
                mData.Type = Controls.Message_Center.MessageType.notification;
                mData.Image = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/TrakHound_Logo_Initials_10_30px.png"));
                mData.Action = new Action<object>(WelcomeMessage_Clicked);
                messageCenter.AddMessage(mData);

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
