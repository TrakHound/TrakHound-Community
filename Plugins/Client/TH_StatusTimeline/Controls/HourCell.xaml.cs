using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TH_StatusTimeline.Controls
{
    /// <summary>
    /// Interaction logic for HourCell.xaml
    /// </summary>
    public partial class HourCell : UserControl
    {
        public HourCell()
        {
            InitializeComponent();
        }

        public HourData HourData
        {
            get { return (HourData)GetValue(HourDataProperty); }
            set { SetValue(HourDataProperty, value); }
        }

        public static readonly DependencyProperty HourDataProperty =
            DependencyProperty.Register("HourData", typeof(HourData), typeof(HourCell), new PropertyMetadata(null));
    }
}
