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
    /// Interaction logic for Segment.xaml
    /// </summary>
    public partial class Segment : UserControl
    {
        public Segment()
        {
            InitializeComponent();
            root.DataContext = this;
        }



        public SegmentData SegmentData
        {
            get { return (SegmentData)GetValue(SegmentDataProperty); }
            set { SetValue(SegmentDataProperty, value); }
        }

        public static readonly DependencyProperty SegmentDataProperty =
            DependencyProperty.Register("SegmentData", typeof(SegmentData), typeof(Segment), new PropertyMetadata(null));



        //public SegmentData Data
        //{
        //    get { return (SegmentData)GetValue(DataProperty); }
        //    set { SetValue(DataProperty, value); }
        //}

        //public static readonly DependencyProperty DataProperty =
        //    DependencyProperty.Register("Data", typeof(SegmentData), typeof(Segment), new PropertyMetadata(null));

    }
}
