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
            DependencyProperty.Register("Maximum", typeof(double), typeof(MeterDisplay), new PropertyMetadata(100d));


        public string ValueFormat
        {
            get { return (string)GetValue(ValueFormatProperty); }
            set { SetValue(ValueFormatProperty, value); }
        }

        public static readonly DependencyProperty ValueFormatProperty =
            DependencyProperty.Register("ValueFormat", typeof(string), typeof(MeterDisplay), new PropertyMetadata(null));



        public string ValueText
        {
            get { return (string)GetValue(ValueTextProperty); }
            set { SetValue(ValueTextProperty, value); }
        }

        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register("ValueText", typeof(string), typeof(MeterDisplay), new PropertyMetadata(null));

        


    }
}
