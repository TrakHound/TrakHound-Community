// Copyright (c) 2016 Feenux LLC, All Rights Reserved.
//
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TrakHound_Dashboard.Controls
{
    /// <summary>
    /// Interaction logic for TH_ToolBarItem.xaml
    /// </summary>
    public partial class TH_ToolBarItem : UserControl
    {
        public TH_ToolBarItem()
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
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(TH_ToolBarItem), new PropertyMetadata(null));


        public delegate void Clicked_Handler();
        public event Clicked_Handler Clicked;

        private void root_GRID_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked();
        }
    }
}
