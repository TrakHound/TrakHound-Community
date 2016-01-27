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

namespace TrakHound_Client.Controls.Message_Center
{
    /// <summary>
    /// Interaction logic for Message.xaml
    /// </summary>
    
    public partial class Message : UserControl
    {
        public Message()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "Properties"

        public bool Shown
        {
            get { return (bool)GetValue(ShownProperty); }
            set { SetValue(ShownProperty, value); }
        }

        public static readonly DependencyProperty ShownProperty =
            DependencyProperty.Register("Shown", typeof(bool), typeof(Message), new PropertyMetadata(false));


        public enum MessageType
        {
            notification, devicealert, warning, error
        }

        public MessageType Message_Type
        {
            get { return (MessageType)GetValue(Message_TypeProperty); }
            set { SetValue(Message_TypeProperty, value); }
        }

        public static readonly DependencyProperty Message_TypeProperty =
            DependencyProperty.Register("Message_Type", typeof(MessageType), typeof(Message), new PropertyMetadata(MessageType.notification));


        public string Message_Title
        {
            get { return (string)GetValue(Message_TitleProperty); }
            set { SetValue(Message_TitleProperty, value); }
        }

        public static readonly DependencyProperty Message_TitleProperty =
            DependencyProperty.Register("Message_Title", typeof(string), typeof(Message), new PropertyMetadata(null));


        public string Message_Text
        {
            get { return (string)GetValue(Message_TextProperty); }
            set { SetValue(Message_TextProperty, value); }
        }

        public static readonly DependencyProperty Message_TextProperty =
            DependencyProperty.Register("Message_Text", typeof(string), typeof(Message), new PropertyMetadata(null));


        public string Message_AdditionalInfo
        {
            get { return (string)GetValue(Message_AdditionalInfoProperty); }
            set { SetValue(Message_AdditionalInfoProperty, value); }
        }

        public static readonly DependencyProperty Message_AdditionalInfoProperty =
            DependencyProperty.Register("Message_AdditionalInfo", typeof(string), typeof(Message), new PropertyMetadata(null));



        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(string), typeof(Message), new PropertyMetadata("00:00 am"));

        

        #endregion

        #region "Click Events"

        public delegate void Clicked_Handler(Message message);
        public event Clicked_Handler Clicked;
        public event Clicked_Handler CloseClicked;

        private void Close_Clicked(TH_WPF.Button bt)
        {
            if (CloseClicked != null) CloseClicked(this);
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this);
        }

        private void MoreInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Message_AdditionalInfo != null)
            {
                MessageBox.Show(Message_AdditionalInfo);
            }
        }

        #endregion

        #region "Mouse Over"

        public bool MouseOver
        {
            get { return (bool)GetValue(MouseOverProperty); }
            set { SetValue(MouseOverProperty, value); }
        }

        public static readonly DependencyProperty MouseOverProperty =
            DependencyProperty.Register("MouseOver", typeof(bool), typeof(Message), new PropertyMetadata(false));


        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseOver = true;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseOver = false;
        }

        #endregion

    }

}
