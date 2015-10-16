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

namespace TH_WPF.TimeLine
{
    /// <summary>
    /// Interaction logic for TickMark.xaml
    /// </summary>
    public partial class TickMark : UserControl
    {
        public TickMark()
        {
            InitializeComponent();
            DataContext = this;
        }


        public int PaddingLeft
        {
            get { return (int)GetValue(PaddingLeftProperty); }
            set { SetValue(PaddingLeftProperty, value); }
        }

        public static readonly DependencyProperty PaddingLeftProperty =
            DependencyProperty.Register("PaddingLeft", typeof(int), typeof(TickMark), new PropertyMetadata(0));


        public int PaddingRight
        {
            get { return (int)GetValue(PaddingRightProperty); }
            set { SetValue(PaddingRightProperty, value); }
        }

        public static readonly DependencyProperty PaddingRightProperty =
            DependencyProperty.Register("PaddingRight", typeof(int), typeof(TickMark), new PropertyMetadata(0));

        
    }
}
