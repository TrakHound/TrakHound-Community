// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TH_DeviceCompare.Controls.DeviceDisplay
{
    /// <summary>
    /// Interaction logic for Column.xaml
    /// </summary>
    public partial class Column : UserControl, IComparable
    {
        public Column(TH_DeviceCompare.DeviceDisplay parent)
        {
            InitializeComponent();
            DataContext = this;

            if (parent.Configuration != null)
            {
                Index = parent.Configuration.Index;
            }

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

        #region "IComparable"

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj.GetType() == typeof(Column))
            {
                var i = obj as Column;
                if (i != null)
                {
                    if (i > this) return -1;
                    else if (i < this) return 1;
                    else return 0;
                }
                else return 1;
            }
            else return 1;
        }

        #region "Private"

        static bool EqualTo(Column c1, Column c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;

            return c1.Index == c2.Index;
        }

        static bool NotEqualTo(Column c1, Column c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;

            return c1.Index != c2.Index;
        }

        static bool LessThan(Column c1, Column c2)
        {
            if (c1.Index > c2.Index) return false;
            else return true;
        }

        static bool GreaterThan(Column c1, Column c2)
        {
            if (c1.Index < c2.Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(Column c1, Column c2)
        {
            return EqualTo(c1, c2);
        }

        public static bool operator !=(Column c1, Column c2)
        {
            return NotEqualTo(c1, c2);
        }


        public static bool operator <(Column c1, Column c2)
        {
            return LessThan(c1, c2);
        }

        public static bool operator >(Column c1, Column c2)
        {
            return GreaterThan(c1, c2);
        }


        public static bool operator <=(Column c1, Column c2)
        {
            return LessThan(c1, c2) || EqualTo(c1, c2);
        }

        public static bool operator >=(Column c1, Column c2)
        {
            return GreaterThan(c1, c2) || EqualTo(c1, c2);
        }

        #endregion

    }
}
