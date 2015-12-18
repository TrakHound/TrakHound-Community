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

using TH_PlugIns_Client;

namespace TrakHound_Client.Options.Pages.General
{
    /// <summary>
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class Page : UserControl, TH_Global.Page
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;
        }

        MainWindow mw;

        public string PageName { get { return "General"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Options/Pages/General/Home_01.png")); } }

        #region "Properties"

        public int DeviceUpdateInterval
        {
            get { return (int)GetValue(DeviceUpdateIntervalProperty); }
            set 
            { 
                SetValue(DeviceUpdateIntervalProperty, value);
            }
        }

        public static readonly DependencyProperty DeviceUpdateIntervalProperty =
            DependencyProperty.Register("DeviceUpdateInterval", typeof(int), typeof(Page), new PropertyMetadata(3000));


        public TimeSpan DeviceUpdateInterval_TimeSpan
        {
            get { return (TimeSpan)GetValue(DeviceUpdateInterval_TimeSpanProperty); }
            set { SetValue(DeviceUpdateInterval_TimeSpanProperty, value); }
        }

        public static readonly DependencyProperty DeviceUpdateInterval_TimeSpanProperty =
            DependencyProperty.Register("DeviceUpdateInterval_TimeSpan", typeof(TimeSpan), typeof(Page), new PropertyMetadata(TimeSpan.FromMilliseconds(3000)));


        #endregion

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //DeviceUpdateInterval_TimeSpan = TimeSpan.FromMilliseconds(DeviceUpdateInterval);
            //if (mw != null) mw.clientUpdateInterval = DeviceUpdateInterval;
        }

    }
}
