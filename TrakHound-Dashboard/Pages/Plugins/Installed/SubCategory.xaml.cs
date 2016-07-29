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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.ObjectModel;

namespace TrakHound_Dashboard.Pages.Plugins.Installed
{
    /// <summary>
    /// Interaction logic for CategoryLabel.xaml
    /// </summary>
    public partial class Subcategory : UserControl
    {
        public Subcategory()
        {
            InitializeComponent();
            root_GRID.DataContext = this;
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Subcategory), new PropertyMetadata(null));


        ObservableCollection<ListItem> _listItems;
        public ObservableCollection<ListItem> ListItems
        {
            get
            {
                if (_listItems == null) _listItems = new ObservableCollection<ListItem>();
                return _listItems;
            }
            set
            {
                _listItems = value;
            }
        }


    }
}
