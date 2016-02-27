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

        public void AddDataBar(DataBar dataBar)
        {
            DataBars.Add(dataBar);

            SetDataBarWidths();
        }

        void SetDataBarWidths()
        {
            double controlWidth = itemsControl.ActualWidth;
            double barWidth = DataBar.MaxBarWidth;

            if (controlWidth > 0 && DataBars.Count > 0)
            {
                int count = DataBars.Count;

                int margin = 4;

                barWidth = (controlWidth - (count * margin)) / count;
                barWidth = Math.Min(DataBar.MaxBarWidth, barWidth);
            }

            foreach (var dataBar in DataBars)
            {
                dataBar.BarWidth = barWidth;
            }
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Histogram), new PropertyMetadata(null));


        public string Id
        {
            get { return (string)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(string), typeof(Histogram), new PropertyMetadata(null));



        //public string shiftName
        //{
        //    get { return (string)GetValue(shiftNameProperty); }
        //    set { SetValue(shiftNameProperty, value); }
        //}

        //public static readonly DependencyProperty shiftNameProperty =
        //    DependencyProperty.Register("shiftName", typeof(string), typeof(Histogram), new PropertyMetadata(null));


        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetDataBarWidths();
        }
       
    }

    public class DataBar : TH_WPF.ProgressBar
    {
        public const double MaxBarWidth = 10d;

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


        //public bool CurrentSegment
        //{
        //    get { return (bool)GetValue(CurrentSegmentProperty); }
        //    set { SetValue(CurrentSegmentProperty, value); }
        //}

        //public static readonly DependencyProperty CurrentSegmentProperty =
        //    DependencyProperty.Register("CurrentSegment", typeof(bool), typeof(DataBar), new PropertyMetadata(false));


        //public string SegmentTimes
        //{
        //    get { return (string)GetValue(SegmentTimesProperty); }
        //    set { SetValue(SegmentTimesProperty, value); }
        //}

        //public static readonly DependencyProperty SegmentTimesProperty =
        //    DependencyProperty.Register("SegmentTimes", typeof(string), typeof(DataBar), new PropertyMetadata(null));


        public object ToolTipData
        {
            get { return (object)GetValue(ToolTipDataProperty); }
            set { SetValue(ToolTipDataProperty, value); }
        }

        public static readonly DependencyProperty ToolTipDataProperty =
            DependencyProperty.Register("ToolTipData", typeof(object), typeof(DataBar), new PropertyMetadata(null));


        public double BarWidth
        {
            get { return (double)GetValue(BarWidthProperty); }
            set { SetValue(BarWidthProperty, value); }
        }

        public static readonly DependencyProperty BarWidthProperty =
            DependencyProperty.Register("BarWidth", typeof(double), typeof(DataBar), new PropertyMetadata(MaxBarWidth));


        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(DataBar), new PropertyMetadata(false));



    }
}
