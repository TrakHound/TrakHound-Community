// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TH_WPF
{
    /// <summary>
    /// Interaction logic for ProgressBar.xaml
    /// </summary>
    public partial class ProgressBar : UserControl
    {
        public ProgressBar()
        {
            InitializeComponent();
            bd.DataContext = this;

            SetProgressValue(Value);
        }


        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set 
            {
                SetValue(ValueProperty, value);
                SetProgressValue(value);
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(ProgressBar), new PropertyMetadata(0d, new PropertyChangedCallback(Value_PropertyChanged)));


        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set 
            {
                SetValue(MaximumProperty, value);
                SetProgressValue(Value);
            }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(ProgressBar), new PropertyMetadata(100d, new PropertyChangedCallback(Maximum_PropertyChanged)));

        private static void Value_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var pb = obj as ProgressBar;
            if (pb != null) pb.SetProgressValue(pb.Value);
        }

        private static void Maximum_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var pb = obj as ProgressBar;
            if (pb != null) pb.SetProgressValue(pb.Value, false);
        }


        public ProgressBarOrientation Orientation
        {
            get { return (ProgressBarOrientation)GetValue(OrientationProperty); }
            set 
            { 
                SetValue(OrientationProperty, value);
                SetProgressValue(Value);
            }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(ProgressBarOrientation), typeof(ProgressBar), new PropertyMetadata(ProgressBarOrientation.Horizontal));


        public double ProgressWidth
        {
            get { return (double)GetValue(ProgressWidthProperty); }
            set { SetValue(ProgressWidthProperty, value); }
        }

        public static readonly DependencyProperty ProgressWidthProperty =
            DependencyProperty.Register("ProgressWidth", typeof(double), typeof(ProgressBar), new PropertyMetadata(0d));


        public double ProgressHeight
        {
            get { return (double)GetValue(ProgressHeightProperty); }
            set { SetValue(ProgressHeightProperty, value); }
        }

        public static readonly DependencyProperty ProgressHeightProperty =
            DependencyProperty.Register("ProgressHeight", typeof(double), typeof(ProgressBar), new PropertyMetadata(0d));


        private double percentage = 0;

        void SetProgressValue(double value, bool animate = true)
        {
            if (Orientation == ProgressBarOrientation.Vertical) SetProgressHeight(value, animate);
            else SetProgressWidth(value, animate);

            if (Maximum > 0) percentage = Value / Maximum;
            else percentage = 0;
        }
        
        void SetProgressWidth(double value, bool animate = true)
        {
            double controlWidth = this.ActualWidth;

            double val = Math.Min(Maximum, value);

            double p = 0;
            if (Maximum > 0) p = val / Maximum;

            // Get ProgressWidth by calculating proportion of Value and Maximum
            val = (val * controlWidth) / Maximum;

            if (ProgressWidth != val)
            {
                if (AnimateValueChange && animate && percentage != p) Animate(val, ProgressWidthProperty);
                else SetValue(ProgressWidthProperty, val);
            }
        }

        void SetProgressHeight(double value, bool animate = true)
        {
            double controlHeight = this.ActualHeight;

            double val = Math.Min(Maximum, value);

            double p = 0;
            if (Maximum > 0) p = val / Maximum;

            // Get ProgressWidth by calculating proportion of Value and Maximum
            val = (val * controlHeight) / Maximum;

            if (ProgressHeight != val)
            {
                if (AnimateValueChange && animate && percentage != p) Animate(val, ProgressHeightProperty);
                else ProgressHeight = val;
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetProgressWidth(Value, false);
        }




        public bool AnimateValueChange
        {
            get { return (bool)GetValue(AnimateValueChangeProperty); }
            set { SetValue(AnimateValueChangeProperty, value); }
        }

        public static readonly DependencyProperty AnimateValueChangeProperty =
            DependencyProperty.Register("AnimateValueChange", typeof(bool), typeof(ProgressBar), new PropertyMetadata(true));

        

        void Animate(double to, DependencyProperty dp)
        {
            var animation = new DoubleAnimation();

            animation.From = (double)GetValue(dp);
            if (!double.IsNaN(to)) animation.To = Math.Max(0, to);
            else animation.To = 0;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            animation.FillBehavior = FillBehavior.Stop;
            animation.Completed += Animation_Completed;
            animation.Freeze();
            BeginAnimation(dp, animation, HandoffBehavior.Compose);
        }

        private void Animation_Completed(object sender, EventArgs e)
        {
            SetProgressValue(Value, false);
        }
    }

    public enum ProgressBarOrientation
    {
        Horizontal,
        Vertical
    }
}
