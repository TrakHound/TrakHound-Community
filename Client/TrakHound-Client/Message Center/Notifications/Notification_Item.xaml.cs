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

namespace TrakHound_Client.Notifications
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
            if (CloseClicked != null) CloseClicked(this);
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this);
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
