// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TrakHound_UI
{
    /// <summary>
    /// Interaction logic for TimeProgress.xaml
    /// </summary>
    public partial class TimeProgress : UserControl, IComparable
    {
        public TimeProgress()
        {
            InitializeComponent();
            root.DataContext = this;

            UpdateValue();
        }

        public int Index { get; set; }

        public object DataObject { get; set; }


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TimeProgress), new PropertyMetadata("Time Progress"));


        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TimeProgress), new PropertyMetadata(false));


        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(string), typeof(TimeProgress), new PropertyMetadata("00:00:00"));


        public string Percentage
        {
            get { return (string)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("Percentage", typeof(string), typeof(TimeProgress), new PropertyMetadata(null));


        
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
                UpdateValue();
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(TimeProgress), new PropertyMetadata(0d, new PropertyChangedCallback(Value_PropertyChanged)));


        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set
            {
                SetValue(MaximumProperty, value);
                UpdateValue(false);
            }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(TimeProgress), new PropertyMetadata(1d, new PropertyChangedCallback(Maximum_PropertyChanged)));


        private static void Value_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var tp = obj as TimeProgress;
            if (tp != null) tp.UpdateValue();
        }

        private static void Maximum_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var tp = obj as TimeProgress;
            if (tp != null)
            {
                tp.UpdateValue(false);
            }
        }

        private bool full;

        private void UpdateValue(bool animate = true)
        {
            bool prevAnimate = AnimateValueChange;
            AnimateValueChange = animate;

            if (Maximum > 0)
            {
                double percentage = Math.Min(1, Value / Maximum);
                Percentage = percentage.ToString("P1");

                Time = TimeSpan.FromSeconds(Value).ToString(@"hh\:mm\:ss");

                if (percentage >= 1) full = true;
                else full = false;

                if (full) BarValue = Maximum;
                else BarValue = Value;

                BarMaximum = Maximum;
            }
            else
            {
                Percentage = "0.0%";
                Time = "00:00:00";

                BarValue = 0;
                BarMaximum = 1;
            }

            AnimateValueChange = prevAnimate;
        }

        #region "Bar Properties"

        public double BarValue
        {
            get { return (double)GetValue(BarValueProperty); }
            set { SetValue(BarValueProperty, value); }
        }

        public static readonly DependencyProperty BarValueProperty =
            DependencyProperty.Register("BarValue", typeof(double), typeof(TimeProgress), new PropertyMetadata(0d));


        public double BarMaximum
        {
            get { return (double)GetValue(BarMaximumProperty); }
            set { SetValue(BarMaximumProperty, value); }
        }

        public static readonly DependencyProperty BarMaximumProperty =
            DependencyProperty.Register("BarMaximum", typeof(double), typeof(TimeProgress), new PropertyMetadata(1d));

        #endregion

        public Brush BarBrush
        {
            get { return (SolidColorBrush)GetValue(BarBrushProperty); }
            set { SetValue(BarBrushProperty, value); }
        }

        public static readonly DependencyProperty BarBrushProperty =
            DependencyProperty.Register("BarBrush", typeof(Brush), typeof(TimeProgress), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(170, 255, 255, 255))));



        public Brush BarBackgroundBrush
        {
            get { return (Brush)GetValue(BarBackgroundBrushProperty); }
            set { SetValue(BarBackgroundBrushProperty, value); }
        }

        public static readonly DependencyProperty BarBackgroundBrushProperty =
            DependencyProperty.Register("BarBackgroundBrush", typeof(Brush), typeof(TimeProgress), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(50, 255, 255, 255))));



        public double BarHeight
        {
            get { return (double)GetValue(BarHeightProperty); }
            set { SetValue(BarHeightProperty, value); }
        }

        public static readonly DependencyProperty BarHeightProperty =
            DependencyProperty.Register("BarHeight", typeof(double), typeof(TimeProgress), new PropertyMetadata(10d));


        public bool AnimateValueChange
        {
            get { return (bool)GetValue(AnimateValueChangeProperty); }
            set { SetValue(AnimateValueChangeProperty, value); }
        }

        public static readonly DependencyProperty AnimateValueChangeProperty =
            DependencyProperty.Register("AnimateValueChange", typeof(bool), typeof(TimeProgress), new PropertyMetadata(true));



        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var tp = obj as TimeProgress;
            if (tp != null)
            {
                if (tp > this) return -1;
                else if (tp < this) return 1;
                else return 0;
            }
            else return 1;
        }

        #region "Operator Overrides"

        #region "Private"

        static bool EqualTo(TimeProgress tp1, TimeProgress tp2)
        {
            if (!object.ReferenceEquals(tp1, null) && object.ReferenceEquals(tp2, null)) return false;
            if (object.ReferenceEquals(tp1, null) && !object.ReferenceEquals(tp2, null)) return false;
            if (object.ReferenceEquals(tp1, null) && object.ReferenceEquals(tp2, null)) return true;

            return tp1.Index == tp2.Index;
        }

        static bool NotEqualTo(TimeProgress tp1, TimeProgress tp2)
        {
            if (!object.ReferenceEquals(tp1, null) && object.ReferenceEquals(tp2, null)) return true;
            if (object.ReferenceEquals(tp1, null) && !object.ReferenceEquals(tp2, null)) return true;
            if (object.ReferenceEquals(tp1, null) && object.ReferenceEquals(tp2, null)) return false;

            return tp1.Index != tp2.Index;
        }

        static bool LessThan(TimeProgress tp1, TimeProgress tp2)
        {
            int tp1Index = tp1.Index;
            int tp2Index = tp2.Index;

            if (tp1Index > tp2Index) return false;
            else if (tp1Index == tp2Index) return false;
            else return true;
        }

        static bool GreaterThan(TimeProgress tp1, TimeProgress tp2)
        {
            int tp1Index = tp1.Index;
            int tp2Index = tp2.Index;

            if (tp1Index < tp2Index) return false;
            else if (tp1Index == tp2Index) return false;
            else return true;
        }

        #endregion

        public static bool operator ==(TimeProgress tp1, TimeProgress tp2)
        {
            return EqualTo(tp1, tp2);
        }

        public static bool operator !=(TimeProgress tp1, TimeProgress tp2)
        {
            return NotEqualTo(tp1, tp2);
        }


        public static bool operator <(TimeProgress tp1, TimeProgress tp2)
        {
            return LessThan(tp1, tp2);
        }

        public static bool operator >(TimeProgress tp1, TimeProgress tp2)
        {
            return GreaterThan(tp1, tp2);
        }


        public static bool operator <=(TimeProgress tp1, TimeProgress tp2)
        {
            return LessThan(tp1, tp2) || EqualTo(tp1, tp2);
        }

        public static bool operator >=(TimeProgress tp1, TimeProgress tp2)
        {
            return GreaterThan(tp1, tp2) || EqualTo(tp1, tp2);
        }

        #endregion


    }
}
