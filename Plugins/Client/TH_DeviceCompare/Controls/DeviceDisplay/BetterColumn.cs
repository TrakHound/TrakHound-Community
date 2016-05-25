using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;

namespace TH_DeviceCompare.Controls.DeviceDisplay
{
    public class Column : Control, IComparable
    {
        static Column()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Column), new FrameworkPropertyMetadata(typeof(Column)));
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
