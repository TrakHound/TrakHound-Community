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
//using OxyPlot.Wpf.Series;

namespace TH_DeviceCompare.Controls
{
    /// <summary>
    /// Interaction logic for TimeLineDisplay.xaml
    /// </summary>
    public partial class TimeLineDisplay : UserControl
    {
        public TimeLineDisplay()
        {
            Start = new DateTime(2015, 8, 23, 0, 0, 0);
            End = new DateTime(2015, 8, 23, 23, 59, 59);

            Init();
        }

        public TimeLineDisplay(DateTime start, DateTime end)
        {
            Start = start;
            End = end;

            Init();
        }

        void Init()
        {
            InitializeComponent();
            DataContext = this;

            shiftStart = Start.ToShortTimeString();
            shiftEnd = End.ToShortTimeString();
        }

        DateTime Start;
        DateTime End;

        public string shiftStart
        {
            get { return (string)GetValue(shiftStartProperty); }
            set { SetValue(shiftStartProperty, value); }
        }

        public static readonly DependencyProperty shiftStartProperty =
            DependencyProperty.Register("shiftStart", typeof(string), typeof(TimeLineDisplay), new PropertyMetadata(null));

        public string shiftEnd
        {
            get { return (string)GetValue(shiftEndProperty); }
            set { SetValue(shiftEndProperty, value); }
        }

        public static readonly DependencyProperty shiftEndProperty =
            DependencyProperty.Register("shiftEnd", typeof(string), typeof(TimeLineDisplay), new PropertyMetadata(null));

        public void CreateData(List<Tuple<DateTime, string>> data, List<Tuple<Color, string>> colors)
        {
            PlotModel pm = new PlotModel();
            pm.IsLegendVisible = false;
            pm.PlotAreaBorderThickness = new OxyThickness(0);

            pm.PlotMargins = new OxyThickness(10);
            pm.Padding = new OxyThickness(0);

            pm.Axes.Add(Create_XAxis());
            pm.Axes.Add(Create_YAxis());

            Random rnd = new Random();

            Tuple<DateTime, string> prevItem = null;

            foreach (Tuple<DateTime, string> item in data)
            {
                if (prevItem != null)
                {
                    // Get Color from "colors"
                    Color c = Colors.White;
                    Tuple<Color, string> colorItem = colors.Find(x => x.Item2.ToLower() == item.Item2.ToLower());
                    if (colorItem != null) c = colorItem.Item1;

                    OxyColor oc = OxyColor.FromArgb(170, c.R, c.G, c.B);

                    // Get Duration (value)
                    TimeSpan duration = item.Item1 - prevItem.Item1;

                    // Add Series to PlotModel
                    BarSeries series = Times_Create_Series(item.Item2);
                    series.FillColor = oc;
                    series.Items.Add(new BarItem(duration.TotalSeconds, -1));
                    pm.Series.Add(series);
                }

                prevItem = item;

            }

            timeline_PV.Model = pm;
        }

        LinearAxis Create_XAxis()
        {
            LinearAxis Result = new LinearAxis();
            Result.Position = AxisPosition.Bottom;

            Result.IntervalLength = 3600;
            Result.MajorStep = 3600;
            Result.MajorGridlineStyle = LineStyle.LongDash;
            Result.MajorGridlineThickness = 1;

            Result.MinorStep = Result.MajorStep;

            TimeSpan totalDuration = End - Start;

            Result.Minimum = 0;
            Result.Maximum = Math.Round(totalDuration.TotalSeconds, 0);

            Result.AxislineColor = OxyColor.FromArgb(51, 0, 0, 0);
            Result.MajorGridlineColor = OxyColor.FromArgb(51, 0, 0, 0);
            Result.TicklineColor = OxyColor.FromArgb(51, 0, 0, 0);

            Result.LabelFormatter = x => "";

            Result.IsPanEnabled = false;
            Result.IsZoomEnabled = false;
            
            return Result;
        }

        static CategoryAxis Create_YAxis()
        {
            CategoryAxis Result = new CategoryAxis();
            Result.Position = AxisPosition.Left;

            Result.GapWidth = 0;

            Result.IsAxisVisible = false;
            Result.IsPanEnabled = false;
            Result.IsZoomEnabled = false;

            Result.MinorGridlineStyle = LineStyle.LongDash;
            Result.MinorGridlineThickness = 1;

            return Result;
        }

        BarSeries Times_Create_Series(string Title)
        {
            BarSeries Result = new BarSeries();
            Result.Title = Title;
            Result.BarWidth = 10;
            Result.IsStacked = true;

            return Result;
        }
     
    }

}
