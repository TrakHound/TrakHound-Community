// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;

namespace TH_SQLite_Config
{
    /// <summary>
    /// Interaction logic for Button.xaml
    /// </summary>
    public partial class Button : UserControl
    {
        public Button()
        {
            InitializeComponent();
            DataContext = this;
        }


        public string DatabasePath
        {
            get { return (string)GetValue(DatabasePathProperty); }
            set { SetValue(DatabasePathProperty, value); }
        }

        public static readonly DependencyProperty DatabasePathProperty =
            DependencyProperty.Register("DatabasePath", typeof(string), typeof(Button), new PropertyMetadata(null));

    }
}
