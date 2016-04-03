using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;

namespace TH_WPF.Charting
{
    /// <summary>
    /// Interaction logic for PieChart.xaml
    /// </summary>
    public partial class PieChart : UserControl
    {
        public PieChart()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        //private ObservableCollection<KeyValuePair<string, int>> _chartData;
        //public ObservableCollection<KeyValuePair<string, int>> ChartData
        //{
        //    get
        //    {
        //        if (_chartData == null) _chartData = new ObservableCollection<KeyValuePair<string, int>>();
        //        return _chartData;
        //    }
        //    set { _chartData = value; }
        //}



        public ObservableCollection<KeyValuePair<string, int>> ChartData
        {
            get { return (ObservableCollection<KeyValuePair<string, int>>)GetValue(ChartDataProperty); }
            set { SetValue(ChartDataProperty, value); }
        }

        public static readonly DependencyProperty ChartDataProperty =
            DependencyProperty.Register("ChartData", typeof(ObservableCollection<KeyValuePair<string, int>>), typeof(PieChart), new PropertyMetadata(null));


    }
}
