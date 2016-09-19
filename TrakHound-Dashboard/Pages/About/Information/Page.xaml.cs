// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;

namespace TrakHound_Dashboard.Pages.About.Information
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, IPage
    {

        public Page()
        {
            InitializeComponent();

            DataContext = this;

            PageContent = this;

            // Build Information
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            BuildVersion = "v" + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString();
            FullBuildVersion = "v" + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();

        }

        public string Title { get { return "Information"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Pages/About/Information/Information_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        public event SendData_Handler SendData;

        public void GetSentData(EventData data) { }


        public object PageContent { get; set; }


        public string BuildVersion
        {
            get { return (string)GetValue(BuildVersionProperty); }
            set { SetValue(BuildVersionProperty, value); }
        }

        public static readonly DependencyProperty BuildVersionProperty =
            DependencyProperty.Register("BuildVersion", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string FullBuildVersion
        {
            get { return (string)GetValue(FullBuildVersionProperty); }
            set { SetValue(FullBuildVersionProperty, value); }
        }

        public static readonly DependencyProperty FullBuildVersionProperty =
            DependencyProperty.Register("FullBuildVersion", typeof(string), typeof(Page), new PropertyMetadata(null));

    }
}
