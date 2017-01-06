// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            // Catch exception in case Value Format is not correct
            try
            {
                ValueText = Value.ToString(ValueFormat);
            }
            catch (Exception ex) { }
        }
        
        public string ValueText
        {
            get { return (string)GetValue(ValueTextProperty); }
            set { SetValue(ValueTextProperty, value); }
        }

        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register("ValueText", typeof(string), typeof(NumberDisplay), new PropertyMetadata("0.0"));


        public string ValueFormat
        {
            get { return (string)GetValue(ValueFormatProperty); }
            set { SetValue(ValueFormatProperty, value); }
        }

        public static readonly DependencyProperty ValueFormatProperty =
            DependencyProperty.Register("ValueFormat", typeof(string), typeof(NumberDisplay), new PropertyMetadata("N1", new PropertyChangedCallback(ValueFormatPropertyChanged)));


        private static void ValueFormatPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var o = (NumberDisplay)dependencyObject;
            o.ProcessValue(o.Value);
        }


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

    }
}
