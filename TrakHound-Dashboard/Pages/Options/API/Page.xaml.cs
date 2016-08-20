// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Share_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }


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




        //public string CurrentHost
        //{
        //    get { return (string)GetValue(CurrentHostProperty); }
        //    set { SetValue(CurrentHostProperty, value); }
        //}

        //public static readonly DependencyProperty CurrentHostProperty =
        //    DependencyProperty.Register("CurrentHost", typeof(string), typeof(Page), new PropertyMetadata(null));


        //public bool ServersLoading
        //{
        //    get { return (bool)GetValue(ServersLoadingProperty); }
        //    set { SetValue(ServersLoadingProperty, value); }
        //}

        //public static readonly DependencyProperty ServersLoadingProperty =
        //    DependencyProperty.Register("ServersLoading", typeof(bool), typeof(Page), new PropertyMetadata(false));



        //private void FindServers()
        //{
        //    Servers.Clear();
        //    ServersLoading = true;

        //    var ping = new Network_Functions.PingNodes();
        //    ping.PingSuccessful += Ping_PingSuccessful;
        //    ping.Finished += Ping_Finished;
        //    ping.Start();
        //}

        //private void Ping_Finished()
        //{
        //    Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        ServersLoading = false;

        //    }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
        //}

        //private void Ping_PingSuccessful(System.Net.NetworkInformation.PingReply reply)
        //{
        //    ThreadPool.QueueUserWorkItem(new WaitCallback(FindServer), reply.Address);
        //}

        //private void FindServer(object o)
        //{
        //    var address = (IPAddress)o;

        //    var apiConfig = ConfigurationServer.Get(address);
        //    if (apiConfig != null)
        //    {
        //        apiConfig.Host = "http://" + address;

        //        TrakHound.Logging.Logger.Log("API Configuration Server Found @ " + apiConfig.Host, TrakHound.Logging.LogLineType.Notification);

        //        Dispatcher.BeginInvoke(new Action<ApiConfiguration>(AddServer), UI_Functions.PRIORITY_BACKGROUND, new object[] { apiConfig });
        //    } 
        //}


        //ObservableCollection<Controls.ServerItem> _servers;
        //public ObservableCollection<Controls.ServerItem> Servers
        //{
        //    get
        //    {
        //        if (_servers == null) _servers = new ObservableCollection<Controls.ServerItem>();
        //        return _servers;
        //    }
        //    set
        //    {
        //        _servers = value;
        //    }
        //}

        //private void AddServer(ApiConfiguration apiConfig)
        //{
        //    var item = new Controls.ServerItem(apiConfig);
        //    item.Clicked += Item_Clicked;

        //    var uri = new Uri(apiConfig.Host);
        //    uri = new Uri(uri, apiConfig.Path);

        //    if (CurrentHost == uri.ToString()) item.IsCurrent = true;

        //    Servers.Add(item);
        //}

        //private void Item_Clicked(ApiConfiguration apiConfig)
        //{
        //    ApiConfiguration.Create(apiConfig);

        //    ApiConfiguration.Set(ApiConfiguration.Read());
        //    CurrentHost = ApiConfiguration.ApiHost.ToString();

        //    FindServers();
        //}

        //private void Refresh_Clicked(TrakHound_UI.Button bt)
        //{
        //    FindServers();
        //}

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
