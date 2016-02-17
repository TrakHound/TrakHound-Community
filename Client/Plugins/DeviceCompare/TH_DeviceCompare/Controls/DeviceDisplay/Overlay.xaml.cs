// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using TH_Configuration;

namespace TH_DeviceCompare.Controls.DeviceDisplay
{
    /// <summary>
    /// Interaction logic for Column_Overlay.xaml
    /// </summary>
    public partial class Overlay : UserControl
    {
        public Overlay(Configuration config)
        {
            InitializeComponent();
            DataContext = this;

            configuration = config;
        }

        Configuration configuration;


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Overlay), new PropertyMetadata(true));



        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(Overlay), new PropertyMetadata(false));
      

        public string ConnectionStatus
        {
            get { return (string)GetValue(ConnectionStatusProperty); }
            set { SetValue(ConnectionStatusProperty, value); }
        }

        public static readonly DependencyProperty ConnectionStatusProperty =
            DependencyProperty.Register("ConnectionStatus", typeof(string), typeof(Overlay), new PropertyMetadata(null));


        public bool ConnectionError
        {
            get { return (bool)GetValue(ConnectionErrorProperty); }
            set { SetValue(ConnectionErrorProperty, value); }
        }

        public static readonly DependencyProperty ConnectionErrorProperty =
            DependencyProperty.Register("ConnectionError", typeof(bool), typeof(Overlay), new PropertyMetadata(false));
 

    }
}
