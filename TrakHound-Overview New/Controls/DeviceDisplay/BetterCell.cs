using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrakHound_Overview.Controls.DeviceDisplay
{

    public class Cell : Control, IComparable
    {
        static Cell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Cell), new FrameworkPropertyMetadata(typeof(Cell)));
        }

        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(Cell), new PropertyMetadata(0));


        //public bool Connected
        //{
        //    get { return (bool)GetValue(ConnectedProperty); }
        //    set { SetValue(ConnectedProperty, value); }
        //}

        //public static readonly DependencyProperty ConnectedProperty =
        //    DependencyProperty.Register("Connected", typeof(bool), typeof(Cell), new PropertyMetadata(false));


        public string Link
        {
            get { return (string)GetValue(LinkProperty); }
            set { SetValue(LinkProperty, value); }
        }

        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.Register("Link", typeof(string), typeof(Cell), new PropertyMetadata(null));


        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(Cell), new PropertyMetadata(null));



        //#region "IsSelected"

        //public delegate void Clicked_Handler(int Index);
        //public event Clicked_Handler Clicked;

        //public bool IsSelected
        //{
        //    get { return (bool)GetValue(IsSelectedProperty); }
        //    set { SetValue(IsSelectedProperty, value); }
        //}

        //public static readonly DependencyProperty IsSelectedProperty =
        //    DependencyProperty.Register("IsSelected", typeof(bool), typeof(Cell), new PropertyMetadata(false));

        //private void Control_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (Clicked != null) Clicked(Index);
        //}

        //#endregion

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var s = obj as Cell;
            if (s != null)
            {
                if (s > this) return -1;
                else if (s < this) return 1;
                else return 0;
            }
            else return 1;
        }

        #region "Operator Overrides"

        #region "Private"

        static bool EqualTo(Cell c1, Cell c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return false;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;

            return c1.Index == c2.Index;
        }

        static bool NotEqualTo(Cell c1, Cell c2)
        {
            if (!object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && !object.ReferenceEquals(c2, null)) return true;
            if (object.ReferenceEquals(c1, null) && object.ReferenceEquals(c2, null)) return false;

            return c1.Index != c2.Index;
        }

        static bool LessThan(Cell c1, Cell c2)
        {
            int c1Index = c1.Index;
            int c2Index = c2.Index;

            if (c1Index > c2Index) return false;
            else if (c1Index == c2Index) return false;
            else return true;
        }

        static bool GreaterThan(Cell c1, Cell c2)
        {
            int c1Index = c1.Index;
            int c2Index = c2.Index;

            if (c1Index < c2Index) return false;
            else if (c1Index == c2Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(Cell c1, Cell c2)
        {
            return EqualTo(c1, c2);
        }

        public static bool operator !=(Cell c1, Cell c2)
        {
            return NotEqualTo(c1, c2);
        }


        public static bool operator <(Cell c1, Cell c2)
        {
            return LessThan(c1, c2);
        }

        public static bool operator >(Cell c1, Cell c2)
        {
            return GreaterThan(c1, c2);
        }


        public static bool operator <=(Cell c1, Cell c2)
        {
            return LessThan(c1, c2) || EqualTo(c1, c2);
        }

        public static bool operator >=(Cell c1, Cell c2)
        {
            return GreaterThan(c1, c2) || EqualTo(c1, c2);
        }

        #endregion

    }
}
