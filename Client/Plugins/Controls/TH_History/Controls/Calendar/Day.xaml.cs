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

namespace TH_History.Controls.Calendar
{
    /// <summary>
    /// Interaction logic for Day.xaml
    /// </summary>
    public partial class Day : UserControl
    {
        public Day(int dayNumber)
        {
            InitializeComponent();
            DataContext = this;

            DayNumber = dayNumber.ToString();
        }

        public string DayNumber
        {
            get { return (string)GetValue(DayNumberProperty); }
            set { SetValue(DayNumberProperty, value); }
        }

        public static readonly DependencyProperty DayNumberProperty =
            DependencyProperty.Register("DayNumber", typeof(string), typeof(Day), new PropertyMetadata(null));


        public bool NotCurrentMonth
        {
            get { return (bool)GetValue(NotCurrentMonthProperty); }
            set { SetValue(NotCurrentMonthProperty, value); }
        }

        public static readonly DependencyProperty NotCurrentMonthProperty =
            DependencyProperty.Register("NotCurrentMonth", typeof(bool), typeof(Day), new PropertyMetadata(false));

        

    }
}
