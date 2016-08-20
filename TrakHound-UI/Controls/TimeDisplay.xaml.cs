using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TrakHound_UI
{
    /// <summary>
    /// Interaction logic for TimeDisplay.xaml
    /// </summary>
    public partial class TimeDisplay : UserControl
    {
        public TimeDisplay()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public TimeSpan Time
        {
            get { return (TimeSpan)GetValue(TimeProperty); }
            set
            {
                SetValue(TimeProperty, value);
                ProcessTime();
            }
        }

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(TimeSpan), typeof(TimeDisplay), new PropertyMetadata(TimeSpan.Zero, new PropertyChangedCallback(Value_PropertyChanged)));

        private static void Value_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var o = obj as TimeDisplay;
            if (o != null) o.ProcessTime();
        }

        private void ProcessTime()
        {
            ShowDate = Time > TimeSpan.FromDays(1);
        }

        public bool ShowDate
        {
            get { return (bool)GetValue(ShowDateProperty); }
            set { SetValue(ShowDateProperty, value); }
        }

        public static readonly DependencyProperty ShowDateProperty =
            DependencyProperty.Register("ShowDate", typeof(bool), typeof(TimeDisplay), new PropertyMetadata(false));
        

        public new Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public new static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(TimeDisplay), new PropertyMetadata(new SolidColorBrush(Colors.White)));


    }
}
