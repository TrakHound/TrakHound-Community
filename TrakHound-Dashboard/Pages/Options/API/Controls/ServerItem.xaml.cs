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

using TrakHound.API;

namespace TrakHound_Dashboard.Pages.Options.API.Controls
{
    /// <summary>
    /// Interaction logic for Item.xaml
    /// </summary>
    public partial class ServerItem : UserControl
    {
        public ServerItem(ApiConfiguration apiConfig)
        {
            InitializeComponent();
            DataContext = this;

            ApiConfiguration = apiConfig;

            var uri = new Uri(apiConfig.Host);
            uri = new Uri(uri, apiConfig.Path);

            Host = uri.ToString();
        }

        public ApiConfiguration ApiConfiguration { get; set; }

        public string Host
        {
            get { return (string)GetValue(HostProperty); }
            set { SetValue(HostProperty, value); }
        }

        public static readonly DependencyProperty HostProperty =
            DependencyProperty.Register("Host", typeof(string), typeof(ServerItem), new PropertyMetadata(null));


        public bool IsCurrent
        {
            get { return (bool)GetValue(IsCurrentProperty); }
            set { SetValue(IsCurrentProperty, value); }
        }

        public static readonly DependencyProperty IsCurrentProperty =
            DependencyProperty.Register("IsCurrent", typeof(bool), typeof(ServerItem), new PropertyMetadata(false));




        public delegate void Clicked_Handler(ApiConfiguration apiConfig);
        public event Clicked_Handler Clicked;

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(ApiConfiguration);
        }
    }
}
