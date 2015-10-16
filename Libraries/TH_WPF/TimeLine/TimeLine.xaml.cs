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

namespace TH_WPF.TimeLine
{
    /// <summary>
    /// Interaction logic for TimeLine.xaml
    /// </summary>
    public partial class TimeLine : UserControl
    {
        public TimeLine()
        {
            InitializeComponent();
            DataContext = this;
        }

        ObservableCollection<Segment> segments;
        public ObservableCollection<Segment> Segments
        {
            get
            {
                if (segments == null) segments = new ObservableCollection<Segment>();
                return segments;
            }
            set
            {
                segments = value;
            }
        }

        ObservableCollection<TickMark> tickmarks;
        public ObservableCollection<TickMark> TickMarks
        {
            get
            {
                if (tickmarks == null) tickmarks = new ObservableCollection<TickMark>();
                return tickmarks;
            }
            set
            {
                tickmarks = value;
            }
        }



        public int TickMarkCount
        {
            get { return (int)GetValue(TickMarkCountProperty); }
            set { SetValue(TickMarkCountProperty, value); }
        }

        public static readonly DependencyProperty TickMarkCountProperty =
            DependencyProperty.Register("TickMarkCount", typeof(int), typeof(TimeLine), new PropertyMetadata(0));



        public string shiftStart
        {
            get { return (string)GetValue(shiftStartProperty); }
            set { SetValue(shiftStartProperty, value); }
        }

        public static readonly DependencyProperty shiftStartProperty =
            DependencyProperty.Register("shiftStart", typeof(string), typeof(TimeLine), new PropertyMetadata(null));

        public string shiftEnd
        {
            get { return (string)GetValue(shiftEndProperty); }
            set { SetValue(shiftEndProperty, value); }
        }

        public static readonly DependencyProperty shiftEndProperty =
            DependencyProperty.Register("shiftEnd", typeof(string), typeof(TimeLine), new PropertyMetadata(null));

        

        public DateTime previousTimestamp = DateTime.MinValue;


        public string shiftName
        {
            get { return (string)GetValue(shiftNameProperty); }
            set { SetValue(shiftNameProperty, value); }
        }

        public static readonly DependencyProperty shiftNameProperty =
            DependencyProperty.Register("shiftName", typeof(string), typeof(TimeLine), new PropertyMetadata(null));

        public TimeSpan shiftDuration = TimeSpan.Zero;

        public List<Tuple<Color, string>> colors = new List<Tuple<Color, string>>();

        Tuple<DateTime, string> prevItem = null;

        List<Tuple<DateTime, string>> timelineData;

        public void Create(List<Tuple<DateTime, string>> data)
        {
            TickMarks.Clear();

            // Add Tick Marks
            TickMarkCount = Convert.ToInt16(Math.Round(shiftDuration.TotalHours, 0));

            // WorkingWidth = Control.ActualWidth - margins
            int width = Convert.ToInt16(this.ActualWidth) - 20;

            // Tick Width
            int tickWidth = Convert.ToInt16((double)width / TickMarkCount) - 3;

            if (width > 0)
            {
                for (int x = 0; x <= TickMarkCount; x++)
                {
                    TickMark tick = new TickMark();

                    tick.PaddingLeft = tickWidth;
                    if (x == 0) tick.PaddingLeft -= 1;

                    TickMarks.Add(tick);
                }

                Segments.Clear();

                foreach (Tuple<DateTime, string> item in data)
                {
                    if (prevItem != null)
                    {
                        // Get Color from "colors"
                        Color c = Colors.White;
                        Tuple<Color, string> colorItem = colors.Find(x => x.Item2.ToLower() == prevItem.Item2.ToLower());
                        if (colorItem != null) c = colorItem.Item1;
                        Color color = Color.FromRgb(c.R, c.G, c.B);

                        // Get Duration (value)
                        TimeSpan duration = item.Item1 - prevItem.Item1;

                        Segment segment = new Segment();
                        segment.Value = prevItem.Item2;
                        segment.Duration = duration.ToString();
                        segment.StartTimeStamp = prevItem.Item1.ToShortTimeString();
                        segment.EndTimeStamp = item.Item1.ToShortTimeString();

                        segment.Width = GetSegmentWidth(duration, shiftDuration);
                        segment.Color = new SolidColorBrush(color);

                        Segments.Add(segment);
                    }
                    prevItem = item;
                }
            }

            timelineData = data;
        }

        public void Update(List<Tuple<DateTime, string>> data)
        {
            if (data.Count > 0)
            {
                foreach (Tuple<DateTime, string> item in data)
                {
                    if (prevItem != null)
                    {
                        // Get Color from "colors"
                        Color c = Colors.White;
                        Tuple<Color, string> colorItem = colors.Find(x => x.Item2.ToLower() == item.Item2.ToLower());
                        if (colorItem != null) c = colorItem.Item1;
                        Color color = Color.FromRgb(c.R, c.G, c.B);

                        // Get Duration (value)
                        TimeSpan duration = item.Item1 - prevItem.Item1;

                        Segment segment = new Segment();

                        segment.Value = prevItem.Item2;
                        segment.Duration = duration.ToString();
                        segment.StartTimeStamp = prevItem.Item1.ToShortTimeString();
                        segment.EndTimeStamp = item.Item1.ToShortTimeString();

                        segment.Width = GetSegmentWidth(duration, shiftDuration);
                        segment.Color = new SolidColorBrush(color);

                        Segments.Add(segment);
                    }

                    prevItem = item;

                }

                timelineData.AddRange(data);
            }
        }

        double GetSegmentWidth(TimeSpan ts, TimeSpan shiftDuration)
        {
            if (shiftDuration > TimeSpan.Zero)
            {
                double controlWidth = Convert.ToInt16(this.ActualWidth) - 20;

                return (controlWidth * ts.TotalSeconds) / shiftDuration.TotalSeconds;
            }
            else return 0;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged && timelineData != null)
            {
                prevItem = null;
                Create(timelineData);
            }
        }

    }
}
