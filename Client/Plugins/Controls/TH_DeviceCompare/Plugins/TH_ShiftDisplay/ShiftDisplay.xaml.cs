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

using System.IO;
using System.Collections.ObjectModel;
using System.Globalization;

using TH_Configuration;
using TH_Device_Client;
using TH_PlugIns_Client_Control;

using TH_DeviceCompare.Components;
using TH_DeviceCompare.Controls;

namespace TH_ShiftDisplay
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ShiftDisplay : UserControl, Control_PlugIn
    {
        public ShiftDisplay()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Shift Display"; } }

        public string Description { get { return "Display Shift Information"; } }

        public ImageSource Image { get { return null; } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return new BitmapImage(new Uri("pack://application:,,,/TH_ShiftDisplay;component/Resources/TrakHound_Logo_10_200px.png")); } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\License\" + "License.txt"); } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return null; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return "Device Compare"; } }
        public string DefaultParentCategory { get { return "Components"; } }

        public bool AcceptsPlugIns { get { return true; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ShowInAppMenu { get { return true; } }

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Update(ReturnData rd)
        {
            //this.Dispatcher.BeginInvoke(new Action<ReturnData>(Update_GUI), Priority_Background, new object[] { rd });
        }

        public void Closing() { }

        public void Show()
        {
            if (ShowRequested != null)
            {
                PluginShowInfo info = new PluginShowInfo();
                info.Page = this;
                ShowRequested(info);
            }
        }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {

        }

        public event DataEvent_Handler DataEvent;

        public event PlugInTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Device Properties"

        private List<Device_Client> lDevices;
        public List<Device_Client> Devices
        {
            get
            {
                return lDevices;
            }
            set
            {
                lDevices = value;

                //DeviceDisplays = new List<DeviceDisplay>();

                //foreach (Device_Client device in Devices) CreateDeviceDisplay(device);
            }
        }

        #endregion

        #region "Options"

        public OptionsPage Options { get; set; }

        #endregion

        public object RootParent { get; set; }

        #endregion

        #region "Shift Display"


        #region "Segment Indicators"

        ObservableCollection<ShiftSegmentIndicator> segmentindicators;
        public ObservableCollection<ShiftSegmentIndicator> SegmentIndicators
        {
            get
            {
                if (segmentindicators == null) segmentindicators = new ObservableCollection<ShiftSegmentIndicator>();
                return segmentindicators;
            }
            set
            {
                segmentindicators = value;
            }
        }

        #endregion


        public string Shift_Name
        {
            get { return (string)GetValue(Shift_NameProperty); }
            set { SetValue(Shift_NameProperty, value); }
        }

        public static readonly DependencyProperty Shift_NameProperty =
            DependencyProperty.Register("Shift_Name", typeof(string), typeof(ShiftDisplay), new PropertyMetadata(null));

        public string Shift_Times
        {
            get { return (string)GetValue(Shift_TimesProperty); }
            set { SetValue(Shift_TimesProperty, value); }
        }

        public static readonly DependencyProperty Shift_TimesProperty =
            DependencyProperty.Register("Shift_Times", typeof(string), typeof(ShiftDisplay), new PropertyMetadata(null));



        public string Month
        {
            get { return (string)GetValue(MonthProperty); }
            set { SetValue(MonthProperty, value); }
        }

        public static readonly DependencyProperty MonthProperty =
            DependencyProperty.Register("Month", typeof(string), typeof(ShiftDisplay), new PropertyMetadata(null));

        public string Day
        {
            get { return (string)GetValue(DayProperty); }
            set { SetValue(DayProperty, value); }
        }

        public static readonly DependencyProperty DayProperty =
            DependencyProperty.Register("Day", typeof(string), typeof(ShiftDisplay), new PropertyMetadata(null));



        public int Bar_Maximum
        {
            get { return (int)GetValue(Bar_MaximumProperty); }
            set { SetValue(Bar_MaximumProperty, value); }
        }

        public static readonly DependencyProperty Bar_MaximumProperty =
            DependencyProperty.Register("Bar_Maximum", typeof(int), typeof(ShiftDisplay), new PropertyMetadata(0));


        public int Bar_Value
        {
            get { return (int)GetValue(Bar_ValueProperty); }
            set { SetValue(Bar_ValueProperty, value); }
        }

        public static readonly DependencyProperty Bar_ValueProperty =
            DependencyProperty.Register("Bar_Value", typeof(int), typeof(ShiftDisplay), new PropertyMetadata(0));


        #endregion

    }


    public class ProgressToAngleConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double progress = (double)values[0];
            System.Windows.Controls.ProgressBar bar = values[1] as System.Windows.Controls.ProgressBar;

            return 359.999 * (progress / (bar.Maximum - bar.Minimum));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Arc : Shape
    {
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(Arc), new UIPropertyMetadata(0.0, new PropertyChangedCallback(UpdateArc)));

        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register("EndAngle", typeof(double), typeof(Arc), new UIPropertyMetadata(90.0, new PropertyChangedCallback(UpdateArc)));


        protected static void UpdateArc(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Arc arc = d as Arc;
            arc.InvalidateVisual();
        }

        protected override Geometry DefiningGeometry
        {
            get { return GetArcGeometry(); }
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            drawingContext.DrawGeometry(null, new Pen(Stroke, StrokeThickness), GetArcGeometry());
        }

        private Geometry GetArcGeometry()
        {
            Point startPoint = PointAtAngle(Math.Min(StartAngle, EndAngle));
            Point endPoint = PointAtAngle(Math.Max(StartAngle, EndAngle));

            Size arcSize = new Size(Math.Max(0, (RenderSize.Width - StrokeThickness) / 2),
                Math.Max(0, (RenderSize.Height - StrokeThickness) / 2));
            bool isLargeArc = Math.Abs(EndAngle - StartAngle) > 180;

            StreamGeometry geom = new StreamGeometry();
            using (StreamGeometryContext context = geom.Open())
            {
                context.BeginFigure(startPoint, false, false);
                context.ArcTo(endPoint, arcSize, 0, isLargeArc, SweepDirection.Counterclockwise, true, false);
            }
            geom.Transform = new TranslateTransform(StrokeThickness / 2, StrokeThickness / 2);
            return geom;
        }

        private Point PointAtAngle(double angle)
        {
            double radAngle = angle * (Math.PI / 180);
            double xr = (RenderSize.Width - StrokeThickness) / 2;
            double yr = (RenderSize.Height - StrokeThickness) / 2;

            double x = xr + xr * Math.Cos(radAngle);
            double y = yr - yr * Math.Sin(radAngle);

            return new Point(x, y);
        }
    }

    public class FriendlyTimeConverter : IValueConverter
    {
        public FriendlyTimeConverter()
        {

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                TimeSpan ts = TimeSpan.FromSeconds((double)value);
                //return String.Format("{00}:{00}:{1:D2}", ts.Hours, ts.Minutes, ts.Seconds);
                return ts.ToString();
            }
            else
            {
                return "00:00";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
