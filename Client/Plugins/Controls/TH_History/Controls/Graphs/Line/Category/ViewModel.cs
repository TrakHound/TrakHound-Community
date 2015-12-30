using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace TH_DataCenter.Controls.Graphs.Line.Category
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

            var categoryAxis = new LinearAxis(AxisPosition.Bottom) { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Category" };
            PlotModel.Axes.Add(categoryAxis);

            var valueAxis = new LinearAxis(AxisPosition.Left, 0) { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Value" };
            PlotModel.Axes.Add(valueAxis);

        }

        public void LoadData(List<Data> data)
        {

            var lineSerie = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 3,
                MarkerStroke = OxyColors.Red,
                Smooth = false,
            };

            foreach (Data item in data)
            {
                var dataPoint = new DataPoint();
                dataPoint.X = item.Category;
                dataPoint.Y = item.Value;

                lineSerie.Points.Add(dataPoint);
            }

            PlotModel.Series.Add(lineSerie);

        }

    }
}
