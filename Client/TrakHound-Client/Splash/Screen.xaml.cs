// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;

namespace TrakHound_Client.Splash
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Screen : Window
    {
        public Screen()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public string Status1
        {
            get { return (string)GetValue(Status1Property); }
            set { SetValue(Status1Property, value); }
        }

        public static readonly DependencyProperty Status1Property =
            DependencyProperty.Register("Status1", typeof(string), typeof(Screen), new PropertyMetadata(null));


        public string Status2
        {
            get { return (string)GetValue(Status2Property); }
            set { SetValue(Status2Property, value); }
        }

        public static readonly DependencyProperty Status2Property =
            DependencyProperty.Register("Status2", typeof(string), typeof(Screen), new PropertyMetadata(null));


        public string Status3
        {
            get { return (string)GetValue(Status3Property); }
            set { SetValue(Status3Property, value); }
        }

        public static readonly DependencyProperty Status3Property =
            DependencyProperty.Register("Status3", typeof(string), typeof(Screen), new PropertyMetadata(null));



        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(string), typeof(Screen), new PropertyMetadata(""));


        public double LoadingProgress
        {
            get { return (double)GetValue(LoadingProgressProperty); }
            set { SetValue(LoadingProgressProperty, value); }
        }

        public static readonly DependencyProperty LoadingProgressProperty =
            DependencyProperty.Register("LoadingProgress", typeof(double), typeof(Screen), new PropertyMetadata(0d));


    }
}
