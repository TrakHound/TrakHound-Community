using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrakHound.Tools
{
    public static class List_Functions
    {
        public class ObservableCollectionEx<T> : ObservableCollection<T>
        {
            private bool _notificationSupressed = false;
            private bool _supressNotification = false;
            public bool SupressNotification
            {
                get
                {
                    return _supressNotification;
                }
                set
                {
                    _supressNotification = value;
                    if (_supressNotification == false && _notificationSupressed)
                    {
                        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                        _notificationSupressed = false;
                    }
                }
            }

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                if (SupressNotification)
                {
                    _notificationSupressed = true;
                    return;
                }
                base.OnCollectionChanged(e);
            }
        }


        public static void Sort(this IList o)
        {
            for (int i = o.Count - 1; i >= 0; i--)
            {
                for (int j = 1; j <= i; j++)
                {
                    object o1 = o[j - 1];
                    object o2 = o[j];
                    if (((IComparable)o1).CompareTo(o2) > 0)
                    {
                        o.Remove(o1);
                        o.Insert(j, o1);
                    }
                }
            }
        }

        public static void SortReverse(this IList o)
        {
            for (int i = o.Count - 1; i >= 0; i--)
            {
                for (int j = 1; j <= i; j++)
                {
                    object o1 = o[j - 1];
                    object o2 = o[j];
                    if (((IComparable)o1).CompareTo(o2) < 0)
                    {
                        o.Remove(o1);
                        o.Insert(j, o1);
                    }
                }
            }
        }

    }
}
