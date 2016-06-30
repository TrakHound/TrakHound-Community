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

namespace UI_Tools.LevelIndicator
{
    /// <summary>
    /// Interaction logic for Segment.xaml
    /// </summary>
    public partial class Segment : UserControl
    {
        public Segment()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
