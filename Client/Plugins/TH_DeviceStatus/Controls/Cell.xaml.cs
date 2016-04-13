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


        public int StatusLevel
        {
            get { return (int)GetValue(StatusLevelProperty); }
            set { SetValue(StatusLevelProperty, value); }
        }

        public static readonly DependencyProperty StatusLevelProperty =
            DependencyProperty.Register("StatusLevel", typeof(int), typeof(Cell), new PropertyMetadata(0));

    }
}
