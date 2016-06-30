using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace UI_Tools.LoadingAnimation
{
    /// <summary>
    /// Interaction logic for Wheel.xaml
    /// </summary>
    public partial class Wheel : UserControl
    {
        public Wheel()
        {
            InitializeComponent();
            canvas.DataContext = this;

            Init();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool visible = (bool)e.NewValue;

            if (visible) Start();
            else Stop();
        }

        Storyboard storyboard = null;

        private void Init()
        {
            storyboard = new Storyboard();
            storyboard.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
            storyboard.RepeatBehavior = RepeatBehavior.Forever;

            var animation = new DoubleAnimation();

            animation.From = 0;
            animation.To = 360;

            animation.Duration = storyboard.Duration;
            animation.RepeatBehavior = storyboard.RepeatBehavior;

            Storyboard.SetTarget(animation, canvas);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(RotateTransform.Angle)"));

            storyboard.Children.Add(animation);

            animation.Freeze();
            storyboard.Freeze();
        }

        private void Start()
        {
            if (storyboard != null) storyboard.Begin();
        }

        private void Stop()
        {
            if (storyboard != null) storyboard.Stop();
        }



        public SolidColorBrush Foreground
        {
            get { return (SolidColorBrush)GetValue(ForegroundProperty); }
            set
            {
                SetValue(ForegroundProperty, value);

                if (value != null) Color = value.Color;
                else Color = Colors.Transparent;
            }
        }

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(SolidColorBrush), typeof(Wheel), new PropertyMetadata(new SolidColorBrush(Colors.Black), new PropertyChangedCallback(Foreground_PropertyChanged)));

        private static void Foreground_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var w = obj as Wheel;
            if (w != null)
            {
                w.Color = w.Foreground.Color;
                w.SetBrushes(w.Color);
            }
        }


        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(Wheel), new PropertyMetadata(Colors.Black, new PropertyChangedCallback(Color_PropertyChanged)));

        private static void Color_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var w = obj as Wheel;
            if (w != null) w.SetBrushes(w.Color);
        }


        private void SetBrushes(Color color)
        {
            var alpha = color.A;

            byte alpha1 = Convert.ToByte(Math.Max(0, alpha - (alpha * 0.20)));
            byte alpha2 = Convert.ToByte(Math.Max(0, alpha - (alpha * 0.60)));
            byte alpha3 = Convert.ToByte(Math.Max(0, alpha - (alpha * 0.99)));

            Color1 = Color.FromArgb(alpha, color.R, color.G, color.B);
            Color2 = Color.FromArgb(alpha1, color.R, color.G, color.B);
            Color3 = Color.FromArgb(alpha2, color.R, color.G, color.B);
            Color4 = Color.FromArgb(alpha3, color.R, color.G, color.B);
        }


        public Color Color1
        {
            get { return (Color)GetValue(Color1Property); }
            set { SetValue(Color1Property, value); }
        }

        public static readonly DependencyProperty Color1Property =
            DependencyProperty.Register("Color1", typeof(Color), typeof(Wheel), new PropertyMetadata(Colors.Black));


        public Color Color2
        {
            get { return (Color)GetValue(Color2Property); }
            set { SetValue(Color2Property, value); }
        }

        public static readonly DependencyProperty Color2Property =
            DependencyProperty.Register("Color2", typeof(Color), typeof(Wheel), new PropertyMetadata(Color.FromArgb(170, 0, 0, 0)));


        public Color Color3
        {
            get { return (Color)GetValue(Color3Property); }
            set { SetValue(Color3Property, value); }
        }

        public static readonly DependencyProperty Color3Property =
            DependencyProperty.Register("Color3", typeof(Color), typeof(Wheel), new PropertyMetadata(Color.FromArgb(136, 0, 0, 0)));


        public Color Color4
        {
            get { return (Color)GetValue(Color4Property); }
            set { SetValue(Color4Property, value); }
        }

        public static readonly DependencyProperty Color4Property =
            DependencyProperty.Register("Color4", typeof(Color), typeof(Wheel), new PropertyMetadata(Color.FromArgb(17, 0, 0, 0)));

    }

    public class HalfSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double height = (double)value;

            return height / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
