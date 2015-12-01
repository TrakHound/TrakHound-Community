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




        //public string StartTime
        //{
        //    get { return (string)GetValue(StartTimeProperty); }
        //    set { SetValue(StartTimeProperty, value); }
        //}

        //public static readonly DependencyProperty StartTimeProperty =
        //    DependencyProperty.Register("StartTime", typeof(string), typeof(TimeLine), new PropertyMetadata(null));


        //public string EndTime
        //{
        //    get { return (string)GetValue(EndTimeProperty); }
        //    set { SetValue(EndTimeProperty, value); }
        //}

        //public static readonly DependencyProperty EndTimeProperty =
        //    DependencyProperty.Register("EndTime", typeof(string), typeof(TimeLine), new PropertyMetadata(null));





        public DateTime StartTime
        {
            get { return (DateTime)GetValue(StartTimeProperty); }
            set 
            { 
                SetValue(StartTimeProperty, value);

                if (EndTime - StartTime > TimeSpan.FromDays(1))
                {
                    StartTimeText = StartTime.ToString();
                    EndTimeText = EndTime.ToString();
                }
                else
                {
                    StartTimeText = StartTime.ToShortTimeString();
                    EndTimeText = EndTime.ToShortTimeString();
                }

                TotalDuration = EndTime - StartTime;
            }
        }

        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register("StartTime", typeof(DateTime), typeof(TimeLine), new PropertyMetadata(new DateTime(0)));



        public DateTime EndTime
        {
            get { return (DateTime)GetValue(EndTimeProperty); }
            set 
            { 
                SetValue(EndTimeProperty, value);

                if (EndTime - StartTime > TimeSpan.FromDays(1))
                {
                    StartTimeText = StartTime.ToString();
                    EndTimeText = EndTime.ToString();
                }
                else
                {
                    StartTimeText = StartTime.ToShortTimeString();
                    EndTimeText = EndTime.ToShortTimeString();
                }

                TotalDuration = EndTime - StartTime;
            }
        }

        public static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register("EndTime", typeof(DateTime), typeof(TimeLine), new PropertyMetadata(new DateTime(0)));






        public string StartTimeText
        {
            get { return (string)GetValue(StartTimeTextProperty); }
            set { SetValue(StartTimeTextProperty, value); }
        }

        public static readonly DependencyProperty StartTimeTextProperty =
            DependencyProperty.Register("StartTimeText", typeof(string), typeof(TimeLine), new PropertyMetadata(null));



        public string EndTimeText
        {
            get { return (string)GetValue(EndTimeTextProperty); }
            set { SetValue(EndTimeTextProperty, value); }
        }

        public static readonly DependencyProperty EndTimeTextProperty =
            DependencyProperty.Register("EndTimeText", typeof(string), typeof(TimeLine), new PropertyMetadata(null));

        





        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(TimeLine), new PropertyMetadata(null));

        







        

        public DateTime previousTimestamp = DateTime.MinValue;

        public int SeriesCount { get; set; }

        public int prev_SeriesCount { get; set; }

        public List<int> Series = new List<int>();


        //public string shiftName
        //{
        //    get { return (string)GetValue(shiftNameProperty); }
        //    set { SetValue(shiftNameProperty, value); }
        //}

        //public static readonly DependencyProperty shiftNameProperty =
        //    DependencyProperty.Register("shiftName", typeof(string), typeof(TimeLine), new PropertyMetadata(null));





        TimeSpan totalDuration = TimeSpan.Zero;
        public TimeSpan TotalDuration
        {
            get { return totalDuration; }
            set
            {
                totalDuration = value;

                UpdateTickMarks();
                UpdateWidth();
            }
        }


        List<Tuple<Color, string>> colors = new List<Tuple<Color, string>>();
        public List<Tuple<Color, string>> Colors
        {
            get { return colors; }
            set
            {
                colors = value;

                UpdateColors();
            }
        }


        Tuple<DateTime, string, string> prevItem = null;

        List<Tuple<DateTime, string, string>> timelineData = new List<Tuple<DateTime, string, string>>();

        public void UpdateColors()
        {
            foreach (Segment s in Segments)
            {
                // Get Color from "colors"
                Color c = System.Windows.Media.Colors.White;
                Tuple<Color, string> colorItem = colors.Find(x => x.Item2.ToLower() == s.Value.ToLower());
                if (colorItem != null) c = colorItem.Item1;
                Color color = Color.FromRgb(c.R, c.G, c.B);

                s.Color = new SolidColorBrush(color);
            }
        }

        void UpdateWidth()
        {          
            foreach (Segment s in Segments)
            {
                s.Width = (GetSegmentWidth(s.Duration, TotalDuration));
            }
        }

        void UpdateTickMarks()
        {
            TickMarks.Clear();

            // Add Tick Marks
            if (TotalDuration.TotalHours < Int16.MaxValue && TotalDuration.TotalHours > Int16.MinValue)
            {
                TickMarkCount = Convert.ToInt16(Math.Round(TotalDuration.TotalHours, 0));

                // WorkingWidth = Control.ActualWidth - margins
                int width = Convert.ToInt16(this.ActualWidth) - 20;

                // Tick Width
                int tickWidth = 0;
                if (TickMarkCount > 0) tickWidth = Convert.ToInt16((double)width / TickMarkCount) - 3;

                if (width > 0)
                {
                    for (int x = 0; x <= TickMarkCount; x++)
                    {
                        TickMark tick = new TickMark();

                        tick.PaddingLeft = tickWidth;
                        if (x == 0) tick.PaddingLeft -= 1;

                        TickMarks.Add(tick);
                    }
                }
            } 
        }

        public void AddData(List<Tuple<DateTime, string, string>> data)
        {
            if (data != null)
            {
                if (data.Count > 0)
                {
                    foreach (Tuple<DateTime, string, string> item in data)
                    {
                        if (prevItem != null)
                        {
                            // Get Color from "colors"
                            Color c = System.Windows.Media.Colors.White;
                            Tuple<Color, string> colorItem = colors.Find(x => x.Item2.ToLower() == prevItem.Item2.ToLower());
                            if (colorItem != null) c = colorItem.Item1;
                            Color color = Color.FromRgb(c.R, c.G, c.B);

                            // Get Duration (value)
                            TimeSpan duration = item.Item1 - prevItem.Item1;

                            Segment segment = new Segment();
                            segment.Value = prevItem.Item2;
                            segment.ValueText = prevItem.Item3;
                            segment.Duration = duration;
                            segment.DurationText = duration.ToString();
                            segment.StartTimeStamp = prevItem.Item1.ToLocalTime().ToString();
                            segment.EndTimeStamp = item.Item1.ToLocalTime().ToString();

                            segment.Width = GetSegmentWidth(duration, TotalDuration);
                            segment.Color = new SolidColorBrush(color);

                            Segments.Add(segment);
                        }

                        prevItem = item;

                    }

                    timelineData.AddRange(data);
                }
            }
        }

        double GetSegmentWidth(TimeSpan ts, TimeSpan shiftDuration)
        {
            if (shiftDuration > TimeSpan.Zero)
            {
                double controlWidth = Convert.ToInt16(this.ActualWidth) - 20;

                double width = (controlWidth * ts.TotalSeconds) / shiftDuration.TotalSeconds;

                return Math.Max(width, 0);
            }
            else return 0;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged && timelineData != null)
            {
                prevItem = null;
                UpdateWidth();
            }
        }

    }
}
