using System;
using System.Windows;

namespace TrakHound_Client
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

        private void MessageCenter_ToolBarItem_Clicked(TH_WPF.Button bt)
        {
            messageCenter.Shown = !messageCenter.Shown;
        }

    }
}
