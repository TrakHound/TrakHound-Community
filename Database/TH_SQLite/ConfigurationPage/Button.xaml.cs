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

namespace TH_SQLite.ConfigurationPage
{
    /// <summary>
    /// Interaction logic for Button.xaml
    /// </summary>
    public partial class Button : UserControl
    {
        public Button()
        {
            InitializeComponent();
            DataContext = this;
        }


        public string DatabasePath
        {
            get { return (string)GetValue(DatabasePathProperty); }
            set { SetValue(DatabasePathProperty, value); }
        }

        public static readonly DependencyProperty DatabasePathProperty =
            DependencyProperty.Register("DatabasePath", typeof(string), typeof(Button), new PropertyMetadata(null));

    }
}
