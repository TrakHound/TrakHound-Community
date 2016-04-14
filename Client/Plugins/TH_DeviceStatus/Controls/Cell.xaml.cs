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

namespace TH_StatusTable.Controls
{
    /// <summary>
    /// Interaction logic for Cell.xaml
    /// </summary>
    public partial class Cell : UserControl
    {
        public Cell()
        {
            InitializeComponent();
            root.DataContext = this;
        }


        public HourData HourData
        {
            get { return (HourData)GetValue(HourDataProperty); }
            set { SetValue(HourDataProperty, value); }
        }

        public static readonly DependencyProperty HourDataProperty =
            DependencyProperty.Register("HourData", typeof(HourData), typeof(Cell), new PropertyMetadata(null));




        //public SegmentData Data00
        //{
        //    get { return (SegmentData)GetValue(Data00Property); }
        //    set { SetValue(Data00Property, value); }
        //}

        //public static readonly DependencyProperty Data00Property =
        //    DependencyProperty.Register("Data00", typeof(SegmentData), typeof(Cell), new PropertyMetadata(null));


        //public SegmentData Data01
        //{
        //    get { return (SegmentData)GetValue(Data01Property); }
        //    set { SetValue(Data01Property, value); }
        //}

        //public static readonly DependencyProperty Data01Property =
        //    DependencyProperty.Register("Data01", typeof(SegmentData), typeof(Cell), new PropertyMetadata(null));


        //public SegmentData Data02
        //{
        //    get { return (SegmentData)GetValue(Data02Property); }
        //    set { SetValue(Data02Property, value); }
        //}

        //public static readonly DependencyProperty Data02Property =
        //    DependencyProperty.Register("Data02", typeof(SegmentData), typeof(Cell), new PropertyMetadata(null));


        //public SegmentData Data03
        //{
        //    get { return (SegmentData)GetValue(Data03Property); }
        //    set { SetValue(Data03Property, value); }
        //}

        //public static readonly DependencyProperty Data03Property =
        //    DependencyProperty.Register("Data03", typeof(SegmentData), typeof(Cell), new PropertyMetadata(null));


        //public SegmentData Data04
        //{
        //    get { return (SegmentData)GetValue(Data04Property); }
        //    set { SetValue(Data04Property, value); }
        //}

        //public static readonly DependencyProperty Data04Property =
        //    DependencyProperty.Register("Data04", typeof(SegmentData), typeof(Cell), new PropertyMetadata(null));


        //public SegmentData Data05
        //{
        //    get { return (SegmentData)GetValue(Data05Property); }
        //    set { SetValue(Data05Property, value); }
        //}

        //public static readonly DependencyProperty Data05Property =
        //    DependencyProperty.Register("Data05", typeof(SegmentData), typeof(Cell), new PropertyMetadata(null));


        //public SegmentData Data06
        //{
        //    get { return (SegmentData)GetValue(Data06Property); }
        //    set { SetValue(Data06Property, value); }
        //}

        //public static readonly DependencyProperty Data06Property =
        //    DependencyProperty.Register("Data06", typeof(SegmentData), typeof(Cell), new PropertyMetadata(null));


        //public SegmentData Data07
        //{
        //    get { return (SegmentData)GetValue(Data07Property); }
        //    set { SetValue(Data07Property, value); }
        //}

        //public static readonly DependencyProperty Data07Property =
        //    DependencyProperty.Register("Data07", typeof(SegmentData), typeof(Cell), new PropertyMetadata(null));


        //public SegmentData Data08
        //{
        //    get { return (SegmentData)GetValue(Data08Property); }
        //    set { SetValue(Data08Property, value); }
        //}

        //public static readonly DependencyProperty Data08Property =
        //    DependencyProperty.Register("Data08", typeof(SegmentData), typeof(Cell), new PropertyMetadata(null));


        //public SegmentData Data09
        //{
        //    get { return (SegmentData)GetValue(Data09Property); }
        //    set { SetValue(Data09Property, value); }
        //}

        //public static readonly DependencyProperty Data09Property =
        //    DependencyProperty.Register("Data09", typeof(SegmentData), typeof(Cell), new PropertyMetadata(null));


        //public SegmentData Data10
        //{
        //    get { return (SegmentData)GetValue(Data10Property); }
        //    set { SetValue(Data10Property, value); }
        //}

        //public static readonly DependencyProperty Data10Property =
        //    DependencyProperty.Register("Data10", typeof(SegmentData), typeof(Cell), new PropertyMetadata(null));


        //public SegmentData Data11
        //{
        //    get { return (SegmentData)GetValue(Data11Property); }
        //    set { SetValue(Data11Property, value); }
        //}

        //public static readonly DependencyProperty Data11Property =
        //    DependencyProperty.Register("Data11", typeof(SegmentData), typeof(Cell), new PropertyMetadata(null));
        
    }
}
