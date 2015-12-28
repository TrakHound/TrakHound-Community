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

using System.Collections.ObjectModel;

namespace TH_WPF.Histogram
{
    /// <summary>
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : UserControl
    {
        public Histogram()
        {
            InitializeComponent();
            DataContext = this;
        }

        ObservableCollection<DataBar> databars;
        public ObservableCollection<DataBar> DataBars
        {
            get
            {
                if (databars == null) databars = new ObservableCollection<DataBar>();
                return databars;
            }
            set
            {
                databars = value;
            }
        }


        public string shiftName
        {
            get { return (string)GetValue(shiftNameProperty); }
            set { SetValue(shiftNameProperty, value); }
        }

        public static readonly DependencyProperty shiftNameProperty =
            DependencyProperty.Register("shiftName", typeof(string), typeof(Histogram), new PropertyMetadata(null));
       
    }

    public class DataBar : TH_WPF.ProgressBar
    {
        public DataBar()
        {
            this.DataContext = this;
        }

        public string Id
        {
            get { return (string)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(string), typeof(DataBar), new PropertyMetadata(null));


        public bool CurrentSegment
        {
            get { return (bool)GetValue(CurrentSegmentProperty); }
            set { SetValue(CurrentSegmentProperty, value); }
        }

        public static readonly DependencyProperty CurrentSegmentProperty =
            DependencyProperty.Register("CurrentSegment", typeof(bool), typeof(DataBar), new PropertyMetadata(false));



        public string SegmentTimes
        {
            get { return (string)GetValue(SegmentTimesProperty); }
            set { SetValue(SegmentTimesProperty, value); }
        }

        public static readonly DependencyProperty SegmentTimesProperty =
            DependencyProperty.Register("SegmentTimes", typeof(string), typeof(DataBar), new PropertyMetadata(null));



        public string ValueText
        {
            get { return (string)GetValue(ValueTextProperty); }
            set { SetValue(ValueTextProperty, value); }
        }

        public static readonly DependencyProperty ValueTextProperty =
            DependencyProperty.Register("ValueText", typeof(string), typeof(DataBar), new PropertyMetadata(null));


        public double BarWidth
        {
            get { return (double)GetValue(BarWidthProperty); }
            set { SetValue(BarWidthProperty, value); }
        }

        public static readonly DependencyProperty BarWidthProperty =
            DependencyProperty.Register("BarWidth", typeof(double), typeof(Histogram), new PropertyMetadata(8d));

    }
}
