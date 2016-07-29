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
using System.Collections.ObjectModel;

using TrakHound;
using TrakHound.Configurations;
using TrakHound.Plugins;
using TrakHound.Plugins.Client;

namespace TrakHound_Dashboard.Pages.Dashboard.ControllerStatus
{
    /// <summary>
    /// Interaction logic for StatusTimeline.xaml
    /// </summary>
    public partial class ControllerStatus : UserControl
    {
        public ControllerStatus()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        ObservableCollection<Controls.Row> _rows;
        public ObservableCollection<Controls.Row> Rows
        {
            get
            {
                if (_rows == null) _rows = new ObservableCollection<Controls.Row>();
                return _rows;
            }
            set
            {
                _rows = value;
            }
        }

        private void AddRow(DeviceConfiguration config)
        {
            var row = new Controls.Row();
            row.Configuration = config;
            Rows.Add(row);
        }

    }
}
