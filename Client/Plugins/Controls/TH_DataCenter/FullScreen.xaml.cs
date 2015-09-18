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

namespace TH_DataCenter
{
    /// <summary>
    /// Interaction logic for FullScreen.xaml
    /// </summary>
    public partial class FullScreen : Window
    {
        public FullScreen()
        {
            InitializeComponent();
            DataContext = this;
        }

        public object WindowContent
        {
            get { return (object)GetValue(WindowContentProperty); }
            set { SetValue(WindowContentProperty, value); }
        }

        public static readonly DependencyProperty WindowContentProperty =
            DependencyProperty.Register("WindowContent", typeof(object), typeof(FullScreen), new PropertyMetadata(null));

        public delegate void FullScreenClosing_Handler(object windowcontent);
        public event FullScreenClosing_Handler FullScreenClosing;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            object content = WindowContent;

            WindowContent = null;

            if (FullScreenClosing != null) FullScreenClosing(content);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }

        
    }
}
