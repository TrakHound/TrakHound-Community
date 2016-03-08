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
            DependencyProperty.Register("Maximum", typeof(double), typeof(ProgressBar), new PropertyMetadata(100d, new PropertyChangedCallback(Value_PropertyChanged)));

        private static void Value_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var pb = obj as ProgressBar;
            if (pb != null) pb.SetProgressValue(pb.Value);
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


        void SetProgressValue(double value)
        {
            if (Orientation == ProgressBarOrientation.Vertical) SetProgressHeight(value);
            else SetProgressWidth(value);
        }
        
        void SetProgressWidth(double value)
        {
            double controlWidth = this.ActualWidth;

            double val = Math.Min(Maximum, value);

            // Get ProgressWidth by calculating proportion of Value and Maximum
            val = (val * controlWidth) / Maximum;

            if (AnimateValueChange) Animate(val, ProgressWidthProperty);
            else ProgressWidth = val;
        }

        void SetProgressHeight(double value)
        {
            double controlHeight = this.ActualHeight;

            double val = Math.Min(Maximum, value);

            // Get ProgressWidth by calculating proportion of Value and Maximum
            val = (val * controlHeight) / Maximum;

            if (AnimateValueChange) Animate(val, ProgressHeightProperty);
            else ProgressHeight = val;           
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetProgressWidth(Value);
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
            DoubleAnimation animation = new DoubleAnimation();

            animation.From = (double)GetValue(dp);
            if (!double.IsNaN(to)) animation.To = Math.Max(0, to);
            else animation.To = 0;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            //animation.BeginTime = TimeSpan.FromMilliseconds(200);
            this.BeginAnimation(dp, animation);
        }
    }

    public enum ProgressBarOrientation
    {
        Horizontal,
        Vertical
    }
}
