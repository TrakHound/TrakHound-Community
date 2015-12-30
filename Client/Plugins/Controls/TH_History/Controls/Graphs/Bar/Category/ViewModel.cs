using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace TH_DataCenter.Controls.Graphs.Bar.Category
{
    public class ViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private PlotModel plotModel;
        public PlotModel PlotModel
        {
            get { return plotModel; }
            set { plotModel = value; OnPropertyChanged("PlotModel"); }
        }

        public ViewModel()
        {
            PlotModel = new PlotModel();
            SetUpModel();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        CategoryAxis categoryAxis;
        LinearAxis linearAxis;

        void SetUpModel()
        {

            PlotModel.LegendTitle = "Legend";
            PlotModel.LegendOrientation = LegendOrientation.Horizontal;
            PlotModel.LegendPlacement = LegendPlacement.Outside;
            PlotModel.LegendPosition = LegendPosition.TopRight;
            PlotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            PlotModel.LegendBorder = OxyColors.Black;

            //var dateAxis = new DateTimeAxis(AxisPosition.Bottom, "Date", "dd/MM/yy HH:mm") { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot };
            //PlotModel.Axes.Add(dateAxis);

            //var categoryAxis = new CategoryAxis();
            //categoryAxis.Position = AxisPosition.Bottom;
            //PlotModel.Axes.Add(categoryAxis);

            categoryAxis = new CategoryAxis();
            categoryAxis.Title = "Category";
            //categoryAxis.MinorStep = 1;
            PlotModel.Axes.Add(categoryAxis);

            linearAxis = new LinearAxis();
            linearAxis.Title = "Linear";
            //linearAxis.AbsoluteMinimum = 0;
            PlotModel.Axes.Add(linearAxis);

        }

        public void LoadData(List<Data> data, System.Windows.Media.Color color)
        {

            var columnSerie = new ColumnSeries();

            columnSerie.FillColor = OxyColor.FromRgb(color.R, color.G, color.B);

            foreach (Data item in data)
            {
                var columnItem = new ColumnItem();
                columnItem.Value = item.Value;

                // Add Labels for CategoryAxis
                if (!categoryAxis.Labels.Contains(item.Category)) categoryAxis.Labels.Add(item.Category);
                if (!categoryAxis.ActualLabels.Contains(item.Category)) categoryAxis.ActualLabels.Add(item.Category);
                
                columnSerie.Items.Add(columnItem);
            }

            PlotModel.Series.Add(columnSerie);

        }

    }
}
