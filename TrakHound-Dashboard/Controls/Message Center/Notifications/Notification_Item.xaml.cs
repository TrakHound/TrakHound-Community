// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TrakHound_Dashboard.Notifications
{
    /// <summary>
    /// Interaction logic for Notification_Item.xaml
    /// </summary>
    public partial class Notification_Item : UserControl
    {
        public Notification_Item()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Notification_Title
        {
            get { return (string)GetValue(Notification_TitleProperty); }
            set { SetValue(Notification_TitleProperty, value); }
        }

        public static readonly DependencyProperty Notification_TitleProperty =
            DependencyProperty.Register("Notification_Title", typeof(string), typeof(Notification_Item), new PropertyMetadata(null));


        public string Notification_Text
        {
            get { return (string)GetValue(Notification_TextProperty); }
            set { SetValue(Notification_TextProperty, value); }
        }

        public static readonly DependencyProperty Notification_TextProperty =
            DependencyProperty.Register("Notification_Text", typeof(string), typeof(Notification_Item), new PropertyMetadata(null));

        public string Notification_AdditionalInfo
        {
            get { return (string)GetValue(Notification_AdditionalInfoProperty); }
            set { SetValue(Notification_AdditionalInfoProperty, value); }
        }

        public static readonly DependencyProperty Notification_AdditionalInfoProperty =
            DependencyProperty.Register("Notification_AdditionalInfo", typeof(string), typeof(Notification_Item), new PropertyMetadata(null));


        public delegate void Clicked_Handler(Notification_Item ni);
        public event Clicked_Handler Clicked;
        public event Clicked_Handler CloseClicked;

        private void close_BD_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseClicked?.Invoke(this);
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Clicked?.Invoke(this);
        }

        private void MoreInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Notification_AdditionalInfo != null)
            {
                MessageBox.Show(Notification_AdditionalInfo);
            }
        }
    }
}
