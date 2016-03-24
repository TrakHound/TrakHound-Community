// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using TH_Plugins.Client;

namespace TrakHound_Client.Menus.Plugins
{
    /// <summary>
    /// Interaction logic for AppItem.xaml
    /// </summary>
    public partial class PluginItem : UserControl
    {
        public PluginItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public IClientPlugin plugin;

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(PluginItem), new PropertyMetadata(null));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(PluginItem), new PropertyMetadata(null));


        public delegate void Clicked_Handler(PluginItem item);
        public event Clicked_Handler Clicked;

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this);
        }

    }
}
