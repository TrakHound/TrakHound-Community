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
    /// Interaction logic for DayOfWeekTitle.xaml
    /// </summary>
    public partial class DayOfWeekTitle : UserControl
    {
        public DayOfWeekTitle(string dayOfWeekName)
        {
            InitializeComponent();
            DataContext = this;
            DayOfWeekName = dayOfWeekName;
        }

        public string DayOfWeekName
        {
            get { return (string)GetValue(DayOfWeekNameProperty); }
            set { SetValue(DayOfWeekNameProperty, value); }
        }

        public static readonly DependencyProperty DayOfWeekNameProperty =
            DependencyProperty.Register("DayOfWeekName", typeof(string), typeof(DayOfWeekTitle), new PropertyMetadata(null));
 
    }
}
