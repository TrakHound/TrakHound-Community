// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;

using TH_Global;
using TH_Global.Functions;

namespace TrakHound_Client.Pages.About.Information
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, TH_Global.IPage
    {

        public Page()
        {
            InitializeComponent();

            DataContext = this;

            PageContent = this;

            // Build Information
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            Build_Version = "v" + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();

        }

        public string Title { get { return "Information"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Pages/About/Information/Information_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        public object PageContent { get; set; }


        public string Build_Version
        {
            get { return (string)GetValue(Build_VersionProperty); }
            set { SetValue(Build_VersionProperty, value); }
        }

        public static readonly DependencyProperty Build_VersionProperty =
            DependencyProperty.Register("Build_Version", typeof(string), typeof(Page), new PropertyMetadata(null));


    }
}
