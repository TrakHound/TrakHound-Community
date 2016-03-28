using System;

using System.Collections.ObjectModel;
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

namespace TH_DevicePage.Controls
{
    /// <summary>
    /// Interaction logic for ProductionStatusPanel.xaml
    /// </summary>
    public partial class ProductionStatusPanel : UserControl
    {
        public ProductionStatusPanel()
        {
            InitializeComponent();
            root.DataContext = this;

            LoadChartData();
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

        void LoadChartData()
        {
            ChartData.Add(new KeyValuePair<string, int>("Full Production", 60));
            ChartData.Add(new KeyValuePair<string, int>("Limited Production", 10));
            ChartData.Add(new KeyValuePair<string, int>("Idle", 25));
            ChartData.Add(new KeyValuePair<string, int>("Alert", 5));
        }

    }
}
