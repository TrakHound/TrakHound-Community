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

using System.Data;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace TH_History.Controls
{
    /// <summary>
    /// Interaction logic for HistoryItem.xaml
    /// </summary>
    public partial class HistoryItem : UserControl
    {
        public HistoryItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public HistoryItem(GraphInfo info)
        {
            InitializeComponent();
            DataContext = this;

            CreateGraph(info);

        }

        #region "Properties"

        public string GraphTitle
        {
            get { return (string)GetValue(GraphTitleProperty); }
            set { SetValue(GraphTitleProperty, value); }
        }

        public static readonly DependencyProperty GraphTitleProperty =
            DependencyProperty.Register("GraphTitle", typeof(string), typeof(HistoryItem), new PropertyMetadata(null));

        #endregion

        void CreateGraph(GraphInfo info)
        {
            GraphTitle = info.Name;

            List<KeyValuePair<string, double>> data = CreateData(info);

            if (data.Count > 0)
            {
                PlotModel pm = new PlotModel();
                pm.IsLegendVisible = false;
                pm.PlotAreaBorderThickness = new OxyThickness(0);

                pm.PlotMargins = new OxyThickness(10);
                pm.Padding = new OxyThickness(20, 0, 20, 50);

                pm.Axes.Add(Create_XAxis(data));
                pm.Axes.Add(Create_YAxis());

                ColumnSeries series = new ColumnSeries();
                series.FillColor = OxyColor.FromArgb(170, 255, 255, 255);

                foreach (KeyValuePair<string, double> row in data)
                {
                    series.Items.Add(new ColumnItem(row.Value));
                }

                pm.Series.Add(series);

                graph_PV.Model = pm;
            }
        }

        /// <summary>
        /// Create Data for Graphs
        /// </summary>
        /// <param name="info"></param>
        static List<KeyValuePair<string, double>> CreateData(GraphInfo info)
        {
            DataTable dt = info.Table;

            DataColumn categoryColumn = dt.Columns[info.CategoryColumn];
            DataColumn dataColumn = dt.Columns[info.DataColumn];
            Type dataType = dataColumn.DataType;

            List<string> categories = dt.AsEnumerable().Select(x => x.Field<string>(categoryColumn)).ToList();
            List<object> data = dt.AsEnumerable().Select(x => x.Field<object>(dataColumn)).ToList();

            List<KeyValuePair<string, double>> graphData = new List<KeyValuePair<string, double>>();

            for (int x = 0; x <= categories.Count - 1; x++)
            {
                double val = double.MinValue;
                double.TryParse(data[x].ToString(), out val);
                if (val > double.MinValue)
                {
                    KeyValuePair<string, double> kvp = new KeyValuePair<string, double>(categories[x], val);
                    graphData.Add(kvp);
                }
            }

            return graphData;
        }

        #region "Axes"

        static CategoryAxis Create_XAxis(List<KeyValuePair<string, double>> data)
        {
            CategoryAxis Result = new CategoryAxis();
            Result.Position = AxisPosition.Bottom;

            int index = 1;

            foreach (KeyValuePair<string, double> row in data)
            {
                Result.Labels.Add(row.Key);
                index += 1;
            }

            Result.FontSize = 8;
            Result.TextColor = OxyColor.FromArgb(51, 255, 255, 255);

            Result.GapWidth = 0.1;

            Result.AxisTickToLabelDistance = 0;

            Result.IsPanEnabled = false;
            Result.IsZoomEnabled = false;

            return Result;
        }

        static LinearAxis Create_YAxis()
        {
            LinearAxis Result = new LinearAxis();
            Result.Position = AxisPosition.Left;

            //Result.IntervalLength = 0.2;
            //Result.MajorStep = 0.2;
            //Result.MinorStep = Result.MajorStep;
            //Result.FontSize = 8;

            //Result.Minimum = 0;
            //Result.Maximum = 1;

            Result.MajorTickSize = 3;

            Result.MajorGridlineStyle = LineStyle.Solid;
            Result.MajorGridlineThickness = 1;

            Result.TextColor = OxyColor.FromArgb(51, 255, 255, 255);
            Result.AxislineColor = OxyColor.FromArgb(102, 0, 0, 0);
            Result.MajorGridlineColor = OxyColor.FromArgb(102, 0, 0, 0);
            Result.TicklineColor = OxyColor.FromArgb(102, 0, 0, 0);

            //Result.LabelFormatter = x => x.ToString("P0");

            Result.IsPanEnabled = false;
            Result.IsZoomEnabled = false;

            return Result;
        }

        #endregion

    }
}
