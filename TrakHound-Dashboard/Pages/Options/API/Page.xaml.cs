// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;

using TrakHound;
using TrakHound.API;

namespace TrakHound_Dashboard.Pages.Options.API
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, IPage
    {
        public Page()
        {
            InitializeComponent();
            root.DataContext = this;

            CurrentDataHost = ApiConfiguration.DataApiHost.ToString();
            CurrentAuthenticationHost = ApiConfiguration.AuthenticationApiHost.ToString();
        }

        public string Title { get { return "API"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Share_01.png"); } }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        public event SendData_Handler SendData;

        public void GetSentData(EventData data) { }


        public string CurrentDataHost
        {
            get { return (string)GetValue(CurrentDataHostProperty); }
            set { SetValue(CurrentDataHostProperty, value); }
        }

        public static readonly DependencyProperty CurrentDataHostProperty =
            DependencyProperty.Register("CurrentDataHost", typeof(string), typeof(Page), new PropertyMetadata(null));


        public string CurrentAuthenticationHost
        {
            get { return (string)GetValue(CurrentAuthenticationHostProperty); }
            set { SetValue(CurrentAuthenticationHostProperty, value); }
        }

        public static readonly DependencyProperty CurrentAuthenticationHostProperty =
            DependencyProperty.Register("CurrentAuthenticationHost", typeof(string), typeof(Page), new PropertyMetadata(null));
        

        private void Local_Clicked(TrakHound_UI.Button bt)
        {
            ApiConfiguration.SetLocal();
            CurrentDataHost = ApiConfiguration.LOCAL_API_HOST.ToString();
            CurrentAuthenticationHost = ApiConfiguration.CLOUD_API_HOST.ToString();
        }

        private void TrakHoundCloud_Clicked(TrakHound_UI.Button bt)
        {
            ApiConfiguration.SetTrakHoundCloud();
            CurrentDataHost = ApiConfiguration.CLOUD_API_HOST.ToString();
            CurrentAuthenticationHost = ApiConfiguration.CLOUD_API_HOST.ToString();
        }
    }
}
