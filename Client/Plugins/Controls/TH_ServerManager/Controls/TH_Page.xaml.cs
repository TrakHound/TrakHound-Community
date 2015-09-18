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

using System.Windows.Markup;
using System.Collections.ObjectModel;

namespace TH_ServerManager.Controls
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    [ContentProperty("Children")]
    public partial class TH_Page : UserControl
    {
        public TH_Page()
        {
            InitializeComponent();
            DataContext = this;
            //Child = Child_BD.Child;
        }

        ObservableCollection<UIElement> children = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> Children { get { return children; } set { children = value; } }

        //public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
        //    "Children",
        //    typeof(UIElement),
        //    typeof(TH_Page),
        //    new PropertyMetadata());

        //public UIElement Child
        //{
        //    get { return (UIElement)GetValue(ChildProperty.DependencyProperty); }
        //    private set { SetValue(ChildProperty, value); }
        //}

    }
}
