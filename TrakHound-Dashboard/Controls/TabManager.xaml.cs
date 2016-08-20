// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TrakHound_Dashboard.Controls
{
    /// <summary>
    /// Interaction logic for TabManager.xaml
    /// </summary>
    public partial class TabManager : UserControl
    {
        public TabManager()
        {
            InitializeComponent();
            DataContext = this;
        }

        List<TabItem> items;
        public List<TabItem> Items
        {
            get { return items; }
            set { items = value; }
        }

        public object CurrentPage
        {
            get { return (object)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(object), typeof(TabManager), new PropertyMetadata(null));
       

        public class TabItem
        {
            public string Title { get; set; }
        }
    }
}
