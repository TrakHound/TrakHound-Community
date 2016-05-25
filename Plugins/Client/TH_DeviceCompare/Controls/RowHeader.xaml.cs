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

namespace TH_DeviceCompare.Controls
{
    /// <summary>
    /// Interaction logic for Row_Header.xaml
    /// </summary>
    public partial class RowHeader : UserControl, IComparable
    {
        public RowHeader()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        /// <summary>
        /// Text that is displayed to User
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(RowHeader), new PropertyMetadata("---"));


        #region "IsSelected"

        public delegate void Clicked_Handler(int index);
        public event Clicked_Handler Clicked;

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(RowHeader), new PropertyMetadata(false));

        private void Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(Index);
        }

        #endregion

        #region "Index"

        /// <summary>
        /// Index of this control within it's parent container (used for sorting and reordering)
        /// </summary>
        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set
            {
                SetValue(IndexProperty, value);

                if (value <= 0) MoveUpEnabled = false;
                else MoveUpEnabled = true;
            }
        }

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(RowHeader), new PropertyMetadata(0));


        public delegate void IndexChanged_Hander(RowHeader sender, int newIndex, int oldIndex);
        public event IndexChanged_Hander IndexChanged;

        public delegate void ResetOrder_Handler();
        public event ResetOrder_Handler ResetOrder;


        public bool MoveUpEnabled
        {
            get { return (bool)GetValue(MoveUpEnabledProperty); }
            set { SetValue(MoveUpEnabledProperty, value); }
        }

        public static readonly DependencyProperty MoveUpEnabledProperty =
            DependencyProperty.Register("MoveUpEnabled", typeof(bool), typeof(RowHeader), new PropertyMetadata(true));


        public bool MoveDownEnabled
        {
            get { return (bool)GetValue(MoveDownEnabledProperty); }
            set { SetValue(MoveDownEnabledProperty, value); }
        }

        public static readonly DependencyProperty MoveDownEnabledProperty =
            DependencyProperty.Register("MoveDownEnabled", typeof(bool), typeof(RowHeader), new PropertyMetadata(true));


        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            int oldIndex = Index;
            int newIndex = Index - 1;

            if (IndexChanged != null) IndexChanged(this, newIndex, oldIndex);
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            int oldIndex = Index;
            int newIndex = Index + 1;

            if (IndexChanged != null) IndexChanged(this, newIndex, oldIndex);
        }

        private void ResetOrder_Click(object sender, RoutedEventArgs e)
        {
            if (ResetOrder != null) ResetOrder();
        }

        #endregion

        #region "IComparable Interfaces"

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var rh = obj as RowHeader;
            if (rh != null)
            {
                if (rh > this) return -1;
                else if (rh < this) return 1;
                else return 0;
            }
            else return 1;
        }

        #region "Operator Overrides"

        #region "Private"

        static bool EqualTo(RowHeader rh1, RowHeader rh2)
        {
            if (!object.ReferenceEquals(rh1, null) && object.ReferenceEquals(rh2, null)) return false;
            if (object.ReferenceEquals(rh1, null) && !object.ReferenceEquals(rh2, null)) return false;
            if (object.ReferenceEquals(rh1, null) && object.ReferenceEquals(rh2, null)) return true;

            return rh1.Index == rh2.Index;
        }

        static bool NotEqualTo(RowHeader rh1, RowHeader rh2)
        {
            if (!object.ReferenceEquals(rh1, null) && object.ReferenceEquals(rh2, null)) return true;
            if (object.ReferenceEquals(rh1, null) && !object.ReferenceEquals(rh2, null)) return true;
            if (object.ReferenceEquals(rh1, null) && object.ReferenceEquals(rh2, null)) return false;

            return rh1.Index != rh2.Index;
        }

        static bool LessThan(RowHeader rh1, RowHeader rh2)
        {
            int rh1Index = rh1.Index;
            int rh2Index = rh2.Index;

            if (rh1Index > rh2Index) return false;
            else if (rh1Index == rh2Index) return false;
            else return true;
        }

        static bool GreaterThan(RowHeader rh1, RowHeader rh2)
        {
            int rh1Index = rh1.Index;
            int rh2Index = rh2.Index;

            if (rh1Index < rh2Index) return false;
            else if (rh1Index == rh2Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(RowHeader rh1, RowHeader rh2)
        {
            return EqualTo(rh1, rh2);
        }

        public static bool operator !=(RowHeader rh1, RowHeader rh2)
        {
            return NotEqualTo(rh1, rh2);
        }


        public static bool operator <(RowHeader rh1, RowHeader rh2)
        {
            return LessThan(rh1, rh2);
        }

        public static bool operator >(RowHeader rh1, RowHeader rh2)
        {
            return GreaterThan(rh1, rh2);
        }


        public static bool operator <=(RowHeader rh1, RowHeader rh2)
        {
            return LessThan(rh1, rh2) || EqualTo(rh1, rh2);
        }

        public static bool operator >=(RowHeader rh1, RowHeader rh2)
        {
            return GreaterThan(rh1, rh2) || EqualTo(rh1, rh2);
        }

        #endregion

        #endregion

    }
}
