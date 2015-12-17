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
using System.Windows.Shapes;

using System.Windows.Media.Animation;

using TH_PlugIns_Client_Control;
using TH_WPF;

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
            DataContext = this;
        }

        public string Status
        {
            get { return (string)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(Screen), new PropertyMetadata("Loading..."));


        public string Version
        {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(string), typeof(Screen), new PropertyMetadata(""));


        public void AddPagePlugin(Control_PlugIn CP)
        {

            ImageBrush NavIMGBRUSH = new ImageBrush();
            NavIMGBRUSH.ImageSource = CP.Image;
            NavIMGBRUSH.Stretch = Stretch.Uniform;

            Rectangle NavRECT = new Rectangle();
            NavRECT.Fill = new SolidColorBrush(Color_Functions.GetColorFromString("#aaffffff"));
            NavRECT.Height = 30;
            NavRECT.Width = 30;
            NavRECT.Margin = new Thickness(5);
            NavRECT.OpacityMask = NavIMGBRUSH;
            NavRECT.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            PagePlugins_STACK.Children.Add(NavRECT);

            PagePlugins_STACK.Visibility = System.Windows.Visibility.Visible;

        }

        public void AddGlobalPlugin(Control_PlugIn CP)
        {

            ImageBrush NavIMGBRUSH = new ImageBrush();
            NavIMGBRUSH.ImageSource = CP.Image;
            NavIMGBRUSH.Stretch = Stretch.Uniform;

            Rectangle NavRECT = new Rectangle();
            NavRECT.Fill = new SolidColorBrush(Color_Functions.GetColorFromString("#aaffffff"));
            NavRECT.Height = 30;
            NavRECT.Width = 30;
            NavRECT.Margin = new Thickness(5);
            NavRECT.OpacityMask = NavIMGBRUSH;
            NavRECT.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;

            GlobalPlugins_STACK.Children.Add(NavRECT);

            GlobalPlugins_STACK.Visibility = System.Windows.Visibility.Visible;

        }

    }
}
