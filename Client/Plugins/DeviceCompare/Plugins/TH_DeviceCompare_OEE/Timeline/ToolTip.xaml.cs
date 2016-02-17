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

namespace TH_DeviceCompare_OEE.Timeline
{
    /// <summary>
    /// Interaction logic for OeeTimelineToolTip.xaml
    /// </summary>
    public partial class OeeToolTip : UserControl
    {
        public OeeToolTip()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Times
        {
            get { return (string)GetValue(TimesProperty); }
            set { SetValue(TimesProperty, value); }
        }

        public static readonly DependencyProperty TimesProperty =
            DependencyProperty.Register("Times", typeof(string), typeof(OeeToolTip), new PropertyMetadata(null));


        public string Oee
        {
            get { return (string)GetValue(OeeProperty); }
            set { SetValue(OeeProperty, value); }
        }

        public static readonly DependencyProperty OeeProperty =
            DependencyProperty.Register("Oee", typeof(string), typeof(OeeToolTip), new PropertyMetadata(null));


        public string Availability
        {
            get { return (string)GetValue(AvailabilityProperty); }
            set { SetValue(AvailabilityProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityProperty =
            DependencyProperty.Register("Availability", typeof(string), typeof(OeeToolTip), new PropertyMetadata(null));


        public string Performance
        {
            get { return (string)GetValue(PerformanceProperty); }
            set { SetValue(PerformanceProperty, value); }
        }

        public static readonly DependencyProperty PerformanceProperty =
            DependencyProperty.Register("Performance", typeof(string), typeof(OeeToolTip), new PropertyMetadata(null));

    }
}
