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

namespace TH_DeviceCompare.Components
{
    /// <summary>
    /// Interaction logic for Column.xaml
    /// </summary>
    public partial class Column : UserControl
    {
        public Column()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Header header;

        public int Index;

        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set 
            { 
                SetValue(ConnectedProperty, value);

                foreach (Cell cell in Cells) cell.Connected = value;
            }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(Column), new PropertyMetadata(false));


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Column), new PropertyMetadata(true));

        ObservableCollection<Cell> cells;
        public ObservableCollection<Cell> Cells
        {
            get
            {
                if (cells == null) cells = new ObservableCollection<Cell>();
                return cells;
            }
            set
            {
                cells = value;
            }
        }

        #region "Mouse Over"

        public bool MouseOver
        {
            get { return (bool)GetValue(MouseOverProperty); }
            set { SetValue(MouseOverProperty, value); }
        }

        public static readonly DependencyProperty MouseOverProperty =
            DependencyProperty.Register("MouseOver", typeof(bool), typeof(Column), new PropertyMetadata(false));

        private void Control_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseOver = true;
            if (header != null) header.MouseOver = true;
        }

        private void Control_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseOver = false;
            if (header != null) header.MouseOver = false;
        }

        #endregion

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
