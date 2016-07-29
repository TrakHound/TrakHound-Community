using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace TrakHound_UI.Histogram
{
    /// <summary>
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : UserControl
    {
        public Histogram()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        ObservableCollection<DataBar> _dataBars;
        public ObservableCollection<DataBar> DataBars
        {
            get
            {
                if (_dataBars == null) _dataBars = new ObservableCollection<DataBar>();
                return _dataBars;
            }
            set
            {
                _dataBars = value;
            }
        }

        ObservableCollection<Label> _labels;
        public ObservableCollection<Label> Labels
        {
            get
            {
                if (_labels == null) _labels = new ObservableCollection<Label>();
                return _labels;
            }
            set
            {
                _labels = value;
            }
        }

        public void AddDataBar(DataBar dataBar)
        {
            DataBars.Add(dataBar);

            SetDataBarWidths();
        }

        public void Refresh()
        {
            SetDataBarWidths();

            SetLabels();
        }

        void SetLabels()
        {
            Labels.Clear();

            double min = Minimum;
            
            var labels = new List<Label>();

            for (var x = min; x <= Maximum; x = x + MajorStep)
            {
                var txt = new Label();
                txt.Value = x;
                txt.Text = x.ToString(ValueFormat);

                labels.Add(txt);
            }

            // Get padding for above and below label
            // Subtract Text Height (12) from ActualHeight
            double controlHeight = ActualHeight - (labels.Count * 12);
            double count = labels.Count * 2;
            if (labels.Count > 2) count = (labels.Count - 1) * 2;
            double padding = controlHeight / count;

            labels = labels.OrderByDescending(x => x.Value).ToList();

            for (var x = 0; x <= labels.Count - 1; x++)
            {
                labels[x].TopPadding = Math.Max(0,padding);
                labels[x].BottomPadding = Math.Max(0, padding);

                if (x == 0) labels[x].TopPadding = 0;
                else if (x == labels.Count - 1) labels[x].BottomPadding = 0;

                Labels.Add(labels[x]);
            } 
        }


        void SetDataBarWidths()
        {
            double controlWidth = itemsControl.ActualWidth;
            double barWidth = MaxBarWidth;

            if (controlWidth > 0 && DataBars.Count > 0)
            {
                int count = DataBars.Count;

                double margin = BarMargin.Left + BarMargin.Right;

                barWidth = (controlWidth - (count * margin)) / count;
                //barWidth = controlWidth - (count * margin);
                barWidth = Math.Min(MaxBarWidth, barWidth);
            }

            foreach (var dataBar in DataBars)
            {
                dataBar.BarWidth = barWidth;
                dataBar.Margin = BarMargin;
            }
        }


        private static void PropertyValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var h = obj as Histogram;
            if (h != null) h.Refresh();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Refresh();
        }



        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Histogram), new PropertyMetadata(null));


        public string Id
        {
            get { return (string)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(string), typeof(Histogram), new PropertyMetadata(null));




        public double MaxBarWidth
        {
            get { return (double)GetValue(MaxBarWidthProperty); }
            set { SetValue(MaxBarWidthProperty, value); }
        }

        public static readonly DependencyProperty MaxBarWidthProperty =
            DependencyProperty.Register("MaxBarWidth", typeof(double), typeof(Histogram), new PropertyMetadata(DataBar.DEFAULT_BAR_WIDTH, new PropertyChangedCallback(PropertyValueChanged)));


        public double MajorStep
        {
            get { return (double)GetValue(MajorStepProperty); }
            set { SetValue(MajorStepProperty, value); }
        }

        public static readonly DependencyProperty MajorStepProperty =
            DependencyProperty.Register("MajorStep", typeof(double), typeof(Histogram), new PropertyMetadata(.5d, new PropertyChangedCallback(PropertyValueChanged)));


        public string ValueFormat
        {
            get { return (string)GetValue(ValueFormatProperty); }
            set { SetValue(ValueFormatProperty, value); }
        }

        public static readonly DependencyProperty ValueFormatProperty =
            DependencyProperty.Register("ValueFormat", typeof(string), typeof(Histogram), new PropertyMetadata("", new PropertyChangedCallback(PropertyValueChanged)));


        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(Histogram), new PropertyMetadata(10d, new PropertyChangedCallback(PropertyValueChanged)));


        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(Histogram), new PropertyMetadata(0d, new PropertyChangedCallback(PropertyValueChanged)));



        public Thickness BarMargin
        {
            get { return (Thickness)GetValue(BarMarginProperty); }
            set { SetValue(BarMarginProperty, value); }
        }

        public static readonly DependencyProperty BarMarginProperty =
            DependencyProperty.Register("BarMargin", typeof(Thickness), typeof(Histogram), new PropertyMetadata(new Thickness(2, 0, 2, 0)));



    }

    public class DataBar : TrakHound_UI.ProgressBar
    {

        public const double DEFAULT_BAR_WIDTH = 10d;



        public DataBar()
        {
            this.DataContext = this;
        }



        public object DataObject
        {
            get { return (object)GetValue(DataObjectProperty); }
            set { SetValue(DataObjectProperty, value); }
        }

        public static readonly DependencyProperty DataObjectProperty =
            DependencyProperty.Register("DataObject", typeof(object), typeof(DataBar), new PropertyMetadata(null));



        public string Id
        {
            get { return (string)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(string), typeof(DataBar), new PropertyMetadata(null));


        public object ToolTipData
        {
            get { return (object)GetValue(ToolTipDataProperty); }
            set { SetValue(ToolTipDataProperty, value); }
        }

        public static readonly DependencyProperty ToolTipDataProperty =
            DependencyProperty.Register("ToolTipData", typeof(object), typeof(DataBar), new PropertyMetadata(null));


        public double BarWidth
        {
            get { return (double)GetValue(BarWidthProperty); }
            set { SetValue(BarWidthProperty, value); }
        }

        public static readonly DependencyProperty BarWidthProperty =
            DependencyProperty.Register("BarWidth", typeof(double), typeof(DataBar), new PropertyMetadata(DEFAULT_BAR_WIDTH));

    
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(DataBar), new PropertyMetadata(false));



    }
}
