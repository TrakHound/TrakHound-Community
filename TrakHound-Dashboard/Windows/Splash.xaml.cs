// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;

namespace TrakHound_Dashboard.Windows
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash : Window
    {
        public Splash()
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
            DependencyProperty.Register("Status1", typeof(string), typeof(Splash), new PropertyMetadata(null));

        
        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(string), typeof(Splash), new PropertyMetadata(""));


        public double LoadingProgress
        {
            get { return (double)GetValue(LoadingProgressProperty); }
            set { SetValue(LoadingProgressProperty, value); }
        }

        public static readonly DependencyProperty LoadingProgressProperty =
            DependencyProperty.Register("LoadingProgress", typeof(double), typeof(Splash), new PropertyMetadata(0d));

    }
}
