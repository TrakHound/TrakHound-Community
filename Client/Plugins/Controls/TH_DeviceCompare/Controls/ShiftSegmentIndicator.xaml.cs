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
    /// Interaction logic for ShiftSegmentIndicator.xaml
    /// </summary>
    public partial class ShiftSegmentIndicator : UserControl
    {
        public ShiftSegmentIndicator()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "Tool Tip"

        public string SegmentTimes
        {
            get { return (string)GetValue(SegmentTimesProperty); }
            set { SetValue(SegmentTimesProperty, value); }
        }

        public static readonly DependencyProperty SegmentTimesProperty =
            DependencyProperty.Register("SegmentTimes", typeof(string), typeof(ShiftSegmentIndicator), new PropertyMetadata(null));


        public string SegmentDuration
        {
            get { return (string)GetValue(SegmentDurationProperty); }
            set { SetValue(SegmentDurationProperty, value); }
        }

        public static readonly DependencyProperty SegmentDurationProperty =
            DependencyProperty.Register("SegmentDuration", typeof(string), typeof(ShiftSegmentIndicator), new PropertyMetadata(null));


        public string SegmentType
        {
            get { return (string)GetValue(SegmentTypeProperty); }
            set { SetValue(SegmentTypeProperty, value); }
        }

        public static readonly DependencyProperty SegmentTypeProperty =
            DependencyProperty.Register("SegmentType", typeof(string), typeof(ShiftSegmentIndicator), new PropertyMetadata(null));


        #endregion

        

        

        public string SegmentId
        {
            get { return (string)GetValue(SegmentIdProperty); }
            set { SetValue(SegmentIdProperty, value); }
        }

        public static readonly DependencyProperty SegmentIdProperty =
            DependencyProperty.Register("SegmentId", typeof(string), typeof(ShiftSegmentIndicator), new PropertyMetadata(null));

        

        public bool CurrentShift
        {
            get { return (bool)GetValue(CurrentShiftProperty); }
            set { SetValue(CurrentShiftProperty, value); }
        }

        public static readonly DependencyProperty CurrentShiftProperty =
            DependencyProperty.Register("CurrentShift", typeof(bool), typeof(ShiftSegmentIndicator), new PropertyMetadata(false));


        public bool BreakType
        {
            get { return (bool)GetValue(BreakTypeProperty); }
            set { SetValue(BreakTypeProperty, value); }
        }

        public static readonly DependencyProperty BreakTypeProperty =
            DependencyProperty.Register("BreakType", typeof(bool), typeof(ShiftSegmentIndicator), new PropertyMetadata(false));

        

        #region "Bar Properties"

        public double ProgressWidth
        {
            get { return (double)GetValue(ProgressWidthProperty); }
            set { SetValue(ProgressWidthProperty, value); }
        }

        public static readonly DependencyProperty ProgressWidthProperty =
            DependencyProperty.Register("ProgressWidth", typeof(double), typeof(ShiftSegmentIndicator), new PropertyMetadata(0d));

        public int BarValue
        {
            get { return (int)GetValue(BarValueProperty); }
            set { SetValue(BarValueProperty, value); }
        }

        public static readonly DependencyProperty BarValueProperty =
            DependencyProperty.Register("BarValue", typeof(int), typeof(ShiftSegmentIndicator), new PropertyMetadata(10));


        public int BarMaximum
        {
            get { return (int)GetValue(BarMaximumProperty); }
            set {  SetValue(BarMaximumProperty, value); }
        }

        public static readonly DependencyProperty BarMaximumProperty =
            DependencyProperty.Register("BarMaximum", typeof(int), typeof(ShiftSegmentIndicator), new PropertyMetadata(60));

        #endregion

        public SolidColorBrush BarBrush
        {
            get { return (SolidColorBrush)GetValue(BarBrushProperty); }
            set { SetValue(BarBrushProperty, value); }
        }

        public static readonly DependencyProperty BarBrushProperty =
            DependencyProperty.Register("BarBrush", typeof(SolidColorBrush), typeof(ShiftSegmentIndicator), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(170, 255, 255, 255))));

    }
}
