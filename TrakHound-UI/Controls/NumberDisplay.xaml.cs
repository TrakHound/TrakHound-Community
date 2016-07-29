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

namespace TrakHound_UI
{
    /// <summary>
    /// Interaction logic for NumberDisplay.xaml
    /// </summary>
    public partial class NumberDisplay : UserControl
    {
        public NumberDisplay()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        #region "Previous Values"

        private double previousvalue;
        public double PreviousValue
        {
            get { return previousvalue; }
            set
            {
                previousvalue = value;

                PreviousValue3 = PreviousValue2;
                PreviousValue2 = PreviousValue1;
                PreviousValue1 = previousvalue.ToString(Value_Format);
            }
        }

        public string PreviousValue1
        {
            get { return (string)GetValue(PreviousValue1Property); }
            set { SetValue(PreviousValue1Property, value); }
        }

        public static readonly DependencyProperty PreviousValue1Property =
            DependencyProperty.Register("PreviousValue1", typeof(string), typeof(NumberDisplay), new PropertyMetadata(null));

        public string PreviousValue2
        {
            get { return (string)GetValue(PreviousValue2Property); }
            set { SetValue(PreviousValue2Property, value); }
        }

        public static readonly DependencyProperty PreviousValue2Property =
            DependencyProperty.Register("PreviousValue2", typeof(string), typeof(NumberDisplay), new PropertyMetadata(null));

        public string PreviousValue3
        {
            get { return (string)GetValue(PreviousValue3Property); }
            set { SetValue(PreviousValue3Property, value); }
        }

        public static readonly DependencyProperty PreviousValue3Property =
            DependencyProperty.Register("PreviousValue3", typeof(string), typeof(NumberDisplay), new PropertyMetadata(null));

        #endregion


        private double _value = 0;
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);

                ProcessValue(value);
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(NumberDisplay), new PropertyMetadata(0d, new PropertyChangedCallback(ValuePropertyChanged)));

        private static void ValuePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var o = (NumberDisplay)dependencyObject;
            o.ProcessValue((double)eventArgs.NewValue);
        }

        public void ProcessValue(double d)
        {
            if (_value != d)
            {
                if (d > _value)
                {
                    ValueIncreasing = true;
                    ValueDecreasing = false;
                }
                else if (d < _value)
                {
                    ValueIncreasing = false;
                    ValueDecreasing = true;
                }

                PreviousValue = _value;
            }

            ValueTimer_Start();
        }

        System.Timers.Timer value_TIMER;

        void ValueTimer_Start()
        {
            if (value_TIMER != null) value_TIMER.Enabled = false;

            value_TIMER = new System.Timers.Timer();
            value_TIMER.Interval = 25;
            value_TIMER.Elapsed += ValueTimer_Update;
            value_TIMER.Enabled = true;
        }

        void ValueTimer_Update(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(ValueTimer_Update_GUI), System.Windows.Threading.DispatcherPriority.Background, new object[] { } );
        }

        void ValueTimer_Update_GUI()
        {
            // Get Difference
            double diff = Math.Abs(Value - _value);
            double interval = diff * 0.25;

            if (_value < Value) _value += interval;
            else if (_value > Value) _value -= interval;
            else value_TIMER.Enabled = false;

            ValueText = _value.ToString(Value_Format);
        }

        public string ValueText
        {
            get { return (string)GetValue(ValueTextProperty); }
            set { SetValue(ValueTextProperty, value); }
        }

        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register("ValueText", typeof(string), typeof(NumberDisplay), new PropertyMetadata("0.0"));



        public string Value_Format
        {
            get { return (string)GetValue(Value_FormatProperty); }
            set { SetValue(Value_FormatProperty, value); }
        }

        public static readonly DependencyProperty Value_FormatProperty =
            DependencyProperty.Register("Value_Format", typeof(string), typeof(NumberDisplay), new PropertyMetadata(null));


        public bool ValueIncreasing
        {
            get { return (bool)GetValue(ValueIncreasingProperty); }
            set { SetValue(ValueIncreasingProperty, value); }
        }

        public static readonly DependencyProperty ValueIncreasingProperty =
            DependencyProperty.Register("ValueIncreasing", typeof(bool), typeof(NumberDisplay), new PropertyMetadata(false));

        public bool ValueDecreasing
        {
            get { return (bool)GetValue(ValueDecreasingProperty); }
            set { SetValue(ValueDecreasingProperty, value); }
        }

        public static readonly DependencyProperty ValueDecreasingProperty =
            DependencyProperty.Register("ValueDecreasing", typeof(bool), typeof(NumberDisplay), new PropertyMetadata(false));




        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(NumberDisplay), new PropertyMetadata(new SolidColorBrush(Colors.Black)));



        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(NumberDisplay), new PropertyMetadata(null));



        public Brush ArrowBrush
        {
            get { return (Brush)GetValue(ArrowBrushProperty); }
            set { SetValue(ArrowBrushProperty, value); }
        }

        public static readonly DependencyProperty ArrowBrushProperty =
            DependencyProperty.Register("ArrowBrush", typeof(Brush), typeof(NumberDisplay), new PropertyMetadata(new SolidColorBrush(Colors.Black)));


    }
}
