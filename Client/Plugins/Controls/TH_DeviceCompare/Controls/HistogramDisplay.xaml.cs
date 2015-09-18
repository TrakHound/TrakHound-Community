// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

using TH_Configuration;

namespace TH_DeviceCompare.Controls
{
    /// <summary>
    /// Interaction logic for HistogramDisplay.xaml
    /// </summary>
    public partial class HistogramDisplay : UserControl
    {
        public HistogramDisplay()
        {
            InitializeComponent();
        }

        public void CreateData(List<KeyValuePair<string, double>> data)
        {
            PlotModel pm = new PlotModel();
            pm.IsLegendVisible = false;
            pm.PlotAreaBorderThickness = new OxyThickness(0);

            pm.PlotMargins = new OxyThickness(10);
            pm.Padding = new OxyThickness(20, 0, 20, 50);

            pm.Axes.Add(Create_XAxis(data));
            pm.Axes.Add(Create_YAxis());

            int index = 0;

            ColumnSeries series = new ColumnSeries();
            series.FillColor = OxyColor.FromArgb(170, 255, 255, 255);
            series.ColumnWidth = 10;

            foreach (KeyValuePair<string, double> row in data)
            {
                series.Items.Add(new ColumnItem(row.Value, index));
                index += 1;
            }

            pm.Series.Add(series);

            historgram_PV.Model = pm;
        }

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

            Result.GapWidth = 2;
            Result.AxisTickToLabelDistance = 0;
            Result.Angle = 60;

            Result.IsPanEnabled = false;
            Result.IsZoomEnabled = false;

            return Result;
        }

        LinearAxis Create_YAxis()
        {
            LinearAxis Result = new LinearAxis();
            Result.Position = AxisPosition.Left;

            Result.IntervalLength = 0.2;
            Result.MajorStep = 0.2;
            Result.MinorStep = Result.MajorStep;
            Result.FontSize = 8;
            
            Result.Minimum = 0;
            Result.Maximum = 1;

            Result.MajorTickSize = 3;

            Result.MajorGridlineStyle = LineStyle.Solid;
            Result.MajorGridlineThickness = 1;

            Result.TextColor = OxyColor.FromArgb(51, 255, 255, 255);
            Result.AxislineColor = OxyColor.FromArgb(102, 0, 0, 0);
            Result.MajorGridlineColor = OxyColor.FromArgb(102, 0, 0, 0);
            Result.TicklineColor = OxyColor.FromArgb(102, 0, 0, 0);

            Result.LabelFormatter = x => x.ToString("P0");

            Result.IsPanEnabled = false;
            Result.IsZoomEnabled = false;

            return Result;
        }

    }
}
