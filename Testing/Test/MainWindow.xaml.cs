using System;
using System.Collections.ObjectModel;
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
using System.Reflection;

using TH_Global;
using TH_Global.Functions;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            LoadPieChartData();
        }

        private void LoadPieChartData()
        {
            //((PieSeries)mcChart.Series[0]).ItemsSource =
            //    new KeyValuePair<string, int>[]{
            //new KeyValuePair<string, int>("Project Manager", 12),
            //new KeyValuePair<string, int>("CEO", 25),
            //new KeyValuePair<string, int>("Software Engg.", 5),
            //new KeyValuePair<string, int>("Team Leader", 6),
            //new KeyValuePair<string, int>("Project Leader", 10),
            //new KeyValuePair<string, int>("Developer", 4) };

            ChartData.Add(new KeyValuePair<string, int>("Project Manager", 12));
            ChartData.Add(new KeyValuePair<string, int>("CEO", 25));
            ChartData.Add(new KeyValuePair<string, int>("Software Engg.", 5));
            ChartData.Add(new KeyValuePair<string, int>("Team Leader", 6));
            ChartData.Add(new KeyValuePair<string, int>("Project Leader", 10));
            ChartData.Add(new KeyValuePair<string, int>("Developer", 4));


        }


        private ObservableCollection<KeyValuePair<string, int>> _chartData;
        public ObservableCollection<KeyValuePair<string, int>> ChartData
        {
            get
            {
                if (_chartData == null) _chartData = new ObservableCollection<KeyValuePair<string, int>>();
                return _chartData;
            }
            set { _chartData = value; }
        }

    }
}
