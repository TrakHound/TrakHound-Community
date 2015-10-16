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

namespace TH_WPF.TimeLine
{
    /// <summary>
    /// Interaction logic for Segment.xaml
    /// </summary>
    public partial class Segment : UserControl
    {
        public Segment()
        {
            InitializeComponent();
            DataContext = this;
        }


        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(Segment), new PropertyMetadata(new SolidColorBrush(Colors.White)));


        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(Segment), new PropertyMetadata(null));


        public string Duration
        {
            get { return (string)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(string), typeof(Segment), new PropertyMetadata(null));


        public string StartTimeStamp
        {
            get { return (string)GetValue(StartTimeStampProperty); }
            set { SetValue(StartTimeStampProperty, value); }
        }

        public static readonly DependencyProperty StartTimeStampProperty =
            DependencyProperty.Register("StartTimeStamp", typeof(string), typeof(Segment), new PropertyMetadata(null));


        public string EndTimeStamp
        {
            get { return (string)GetValue(EndTimeStampProperty); }
            set { SetValue(EndTimeStampProperty, value); }
        }

        public static readonly DependencyProperty EndTimeStampProperty =
            DependencyProperty.Register("EndTimeStamp", typeof(string), typeof(Segment), new PropertyMetadata(null));

        
    }
}
