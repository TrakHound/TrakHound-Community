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

namespace TH_DevicePage.Controls
{
    /// <summary>
    /// Interaction logic for TextDisplay.xaml
    /// </summary>
    public partial class TextDisplay : UserControl
    {
        public TextDisplay()
        {
            InitializeComponent();
            root.DataContext = this;
        }


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextDisplay), new PropertyMetadata(null));
        
    }
}
