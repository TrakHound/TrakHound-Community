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

using System.IO;

using TH_PlugIns_Client_Control;

namespace TrakHound_Client.About.Pages.License
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, AboutPage
    {
        public Page()
        {
            InitializeComponent();

            DataContext = this;

            string LicensePath = AppDomain.CurrentDomain.BaseDirectory + @"\License.txt";
            LicenseText = File.ReadAllText(LicensePath);

            PageContent = this;
        }

        public string LicenseText
        {
            get { return (string)GetValue(LicenseTextProperty); }
            set { SetValue(LicenseTextProperty, value); }
        }

        public static readonly DependencyProperty LicenseTextProperty =
            DependencyProperty.Register("LicenseText", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string PageName { get { return "License"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/About/Pages/License/Key_03.png")); } }

        public object PageContent { get; set; }

    }
}
