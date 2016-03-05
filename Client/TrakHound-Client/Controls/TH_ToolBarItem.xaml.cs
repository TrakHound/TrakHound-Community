// Copyright (c) 2016 Feenux LLC, All Rights Reserved.
//
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//

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

namespace TrakHound_Client.Controls
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
