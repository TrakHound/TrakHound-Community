// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TH_DeviceCompare.Controls.DeviceDisplay
{
    /// <summary>
    /// Interaction logic for Column.xaml
    /// </summary>
    public partial class Column : UserControl
    {
        public Column(TH_DeviceCompare.DeviceDisplay parent)
        {
            InitializeComponent();
            DataContext = this;

            ParentDisplay = parent;
        }

        public int Index;

        public TH_DeviceCompare.DeviceDisplay ParentDisplay
        {
            get { return (TH_DeviceCompare.DeviceDisplay)GetValue(ParentDisplayProperty); }
            set { SetValue(ParentDisplayProperty, value); }
        }

        public static readonly DependencyProperty ParentDisplayProperty =
            DependencyProperty.Register("ParentDisplay", typeof(TH_DeviceCompare.DeviceDisplay), typeof(Column), new PropertyMetadata(null));
       
        
        ObservableCollection<Cell> _cells;
        public ObservableCollection<Cell> Cells
        {
            get
            {
                if (_cells == null) _cells = new ObservableCollection<Cell>();
                return _cells;
            }
            set
            {
                _cells = value;
            }
        }

        #region "IsSelected"

        public delegate void Clicked_Handler(int Index);
        public event Clicked_Handler Clicked;

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(Column), new PropertyMetadata(false));

        private void Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(Index);
        }

        #endregion

    }
}
