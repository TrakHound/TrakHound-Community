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
            grid.DataContext = this;
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

        

        //public bool Orientation
        //{
        //    get { return (bool)GetValue(OrientationProperty); }
        //    set 
        //    {
        //        SetValue(OrientationProperty, value);
        //        SetProgressValue(Value);
        //    }
        //}

        //public static readonly DependencyProperty OrientationProperty =
        //    DependencyProperty.Register("Orientation", typeof(bool), typeof(ProgressBar), new PropertyMetadata(false));

        
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

            double val = value;

            // Perform Checks
            if (val > Maximum) val = Maximum;
            if (Maximum <= 0)
            {
                Maximum = 1;
                val = Maximum;
            }

            // Get ProgressWidth by calculating proportion of Value and Maximum
            ProgressWidth = (val * controlWidth) / Maximum;
        }

        void SetProgressHeight(double value)
        {
            double controlHeight = this.ActualHeight;

            double val = value;

            // Perform Checks
            if (val > Maximum) val = Maximum;
            if (Maximum <= 0)
            {
                Maximum = 1;
                val = Maximum;
            }

            // Get ProgressWidth by calculating proportion of Value and Maximum
            ProgressHeight = (val * controlHeight) / Maximum;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetProgressWidth(Value);
        }

    }

    public enum ProgressBarOrientation
    {
        Horizontal,
        Vertical
    }
}
