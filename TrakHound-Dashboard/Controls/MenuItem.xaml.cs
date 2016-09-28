// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TrakHound_Dashboard.Menus
{
    /// <summary>
    /// Interaction logic for MenuItem.xaml
    /// </summary>
    public partial class MenuItem : UserControl
    {
        public MenuItem()
        {
            InitializeComponent();
            DataContext = this;
        }


        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(MenuItem), new PropertyMetadata(null));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MenuItem), new PropertyMetadata(null));


        public object DataObject { get; set; }

        public delegate void Clicked_Handler(object obj);
        public event Clicked_Handler Clicked;

        private void root_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Clicked?.Invoke(DataObject);
        }

    }
}
