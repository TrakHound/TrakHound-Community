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
    /// Interaction logic for TimeSlider.xaml
    /// </summary>
    public partial class TimeSlider : UserControl
    {
        public TimeSlider()
        {
            InitializeComponent();
            root.DataContext = this;
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TimeSlider), new PropertyMetadata(null));


        public string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register("HelpText", typeof(string), typeof(TimeSlider), new PropertyMetadata(null));


        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(TimeSlider), new PropertyMetadata(12d));




        public long Value
        {
            get { return (long)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(long), typeof(TimeSlider), new PropertyMetadata(0L, new PropertyChangedCallback(Value_PropertyChanged)));


        public string Value_Text
        {
            get { return (string)GetValue(Value_TextProperty); }
            set { SetValue(Value_TextProperty, value); }
        }

        public static readonly DependencyProperty Value_TextProperty =
            DependencyProperty.Register("Value_Text", typeof(string), typeof(TimeSlider), new PropertyMetadata("00:00", new PropertyChangedCallback(Value_Text_PropertyChanged)));


        public string Value_FormattedString
        {
            get { return (string)GetValue(Value_FormattedStringProperty); }
            set { SetValue(Value_FormattedStringProperty, value); }
        }

        public static readonly DependencyProperty Value_FormattedStringProperty =
            DependencyProperty.Register("Value_FormattedString", typeof(string), typeof(TimeSlider), new PropertyMetadata(null));



        private static void Value_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var o = obj as TimeSlider;
            if (o != null)
            {
                var ts = TimeSpan.FromMilliseconds((long)e.NewValue);

                o.Value_Text = ts.ToString();
                o.Value_FormattedString = TH_Global.Functions.TimeSpan_Functions.ToFormattedString(ts);
            }
        }

        private static void Value_Text_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var o = obj as TimeSlider;
            if (o != null)
            {
                string text = e.NewValue.ToString();

                if (text != String.Empty)
                {
                    TimeSpan ts = GetTimeSpanFromString(text);
                    o.Value_FormattedString = TH_Global.Functions.TimeSpan_Functions.ToFormattedString(ts);
                    if (ts.TotalMilliseconds < long.MaxValue)
                    {
                        o.Value = Convert.ToInt64(ts.TotalMilliseconds);
                    }
                }
            }
        }

        private static TimeSpan GetTimeSpanFromString(string s)
        {
            TimeSpan result = TimeSpan.Zero;
            if (TimeSpan.TryParse(s, out result)) return result;
            else
            {
                s = s.Trim();
                //Milliseconds
                if (s.Length > 2)
                {
                    string unit = s.Substring(s.Length - 2, 2);
                    if (unit == "ms")
                    {
                        double ms;
                        if (double.TryParse(s.Substring(0, s.Length - 2), out ms))
                        {
                            result = TimeSpan.FromMilliseconds(ms);
                        }
                    }
                }
                //Seconds
                if (result == TimeSpan.Zero && s.Length > 1)
                {
                    string unit = s.Substring(s.Length - 1, 1);
                    if (unit == "s")
                    {
                        double seconds;
                        if (double.TryParse(s.Substring(0, s.Length - 1), out seconds))
                        {
                            result = TimeSpan.FromSeconds(seconds);
                        }
                    }
                }
            }
            return result;
        }




        public long Maximum
        {
            get { return (long)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(long), typeof(TimeSlider), new PropertyMetadata(10L));


        public long Minimum
        {
            get { return (long)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(long), typeof(TimeSlider), new PropertyMetadata(0L));


        public long LargeChange
        {
            get { return (long)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        public static readonly DependencyProperty LargeChangeProperty =
            DependencyProperty.Register("LargeChange", typeof(long), typeof(TimeSlider), new PropertyMetadata(1L));


        public long TickFrequency
        {
            get { return (long)GetValue(TickFrequencyProperty); }
            set { SetValue(TickFrequencyProperty, value); }
        }

        public static readonly DependencyProperty TickFrequencyProperty =
            DependencyProperty.Register("TickFrequency", typeof(long), typeof(TimeSlider), new PropertyMetadata(1L));




        public bool TextIsFocused
        {
            get { return (bool)GetValue(TextIsFocusedProperty); }
            set { SetValue(TextIsFocusedProperty, value); }
        }

        public static readonly DependencyProperty TextIsFocusedProperty =
            DependencyProperty.Register("TextIsFocused", typeof(bool), typeof(TimeSlider), new PropertyMetadata(false));



        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = false;
                    }
                }
            }
        }
    }
}
