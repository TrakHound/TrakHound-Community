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

using TH_PlugIns_Server;

namespace TH_InstanceTable
{
    /// <summary>
    /// Interaction logic for ConfigPage.xaml
    /// </summary>
    public partial class Configuration_Page : UserControl, ConfigurationPage
    {
        public Configuration_Page()
        {
            InitializeComponent();
        }

        public string PageName { get { return "Instance Table"; } }

        public ImageSource Image { get { return null; } }

    }
}
