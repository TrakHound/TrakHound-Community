using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UI_Tools
{
    /// <summary>
    /// Interaction logic for MeterDisplay.xaml
    /// </summary>
    public partial class MeterDisplay : UserControl
    {
        public MeterDisplay()
        {
            InitializeComponent();
            root.DataContext = this;

            indicator.TotalLevelCount = 5;
            indicator.ActiveLevelCount = 0;
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);

                ValueText = value.ToString(ValueFormat);

                ProcessValue(value);
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(MeterDisplay), new PropertyMetadata(0d, new PropertyChangedCallback(ValuePropertyChanged)));

        private static void ValuePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            double value = (double)eventArgs.NewValue;

            var o = (MeterDisplay)dependencyObject;
            o.ValueText = value.ToString(o.ValueFormat);
            o.ProcessValue(value);
        }


        private void ProcessValue(double value)
        {
            int totalLevels = indicator.TotalLevelCount;
            int activeLevels = 0;

            if (value < Maximum)
            {
                double val = value;

                // Adjust value to have whole numbers (in case value is a decimal (ex. 0.01))
                double adjuster = 1;
                double adj = 10;
                while (val < 1 && val > 0)
                {
                    adjuster = adj;
                    val = val * adjuster;

                    adj = adj * 10;
                }

                if (totalLevels > 1)
                {
                    double increment = (Maximum * adjuster) / (totalLevels - 1);

                    val = val / increment;

                    if (val > 0) val = Math.Ceiling(val);
                    else val = 0;

                    activeLevels = (int)val;
                }
            }
            else
            {
                activeLevels = totalLevels;
            }

            

            indicator.ActiveLevelCount = activeLevels;
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(MeterDisplay), new PropertyMetadata(100d, new PropertyChangedCallback(MaximumPropertyChanged)));

        private static void MaximumPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            double max = (double)eventArgs.NewValue;

            var o = (MeterDisplay)dependencyObject;
            o.ProcessValue(o.Value);
        }


        public string ValueFormat
        {
            get { return (string)GetValue(ValueFormatProperty); }
            set { SetValue(ValueFormatProperty, value); }
        }

        public static readonly DependencyProperty ValueFormatProperty =
            DependencyProperty.Register("ValueFormat", typeof(string), typeof(MeterDisplay), new PropertyMetadata(null, new PropertyChangedCallback(ValueFormatPropertyChanged)));

        private static void ValueFormatPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            string valueFormat = (string)eventArgs.NewValue;

            var o = (MeterDisplay)dependencyObject;
            o.ValueText = o.Value.ToString(valueFormat);
        }

        public string ValueText
        {
            get { return (string)GetValue(ValueTextProperty); }
            set { SetValue(ValueTextProperty, value); }
        }

        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register("ValueText", typeof(string), typeof(MeterDisplay), new PropertyMetadata(null));


        public Brush ActiveLevelBrush
        {
            get { return (Brush)GetValue(ActiveLevelBrushProperty); }
            set { SetValue(ActiveLevelBrushProperty, value); }
        }

        public static readonly DependencyProperty ActiveLevelBrushProperty =
            DependencyProperty.Register("ActiveLevelBrush", typeof(Brush), typeof(MeterDisplay), new PropertyMetadata(new SolidColorBrush(Colors.Black)));


        public Brush InactiveLevelBrush
        {
            get { return (Brush)GetValue(InactiveLevelBrushProperty); }
            set { SetValue(InactiveLevelBrushProperty, value); }
        }

        public static readonly DependencyProperty InactiveLevelBrushProperty =
            DependencyProperty.Register("InactiveLevelBrush", typeof(Brush), typeof(MeterDisplay), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));



        public double IndicatorWidth
        {
            get { return (double)GetValue(IndicatorWidthProperty); }
            set { SetValue(IndicatorWidthProperty, value); }
        }

        public static readonly DependencyProperty IndicatorWidthProperty =
            DependencyProperty.Register("IndicatorWidth", typeof(double), typeof(MeterDisplay), new PropertyMetadata(75d));


    }
}
