using System.Collections.ObjectModel;
using System.Windows.Controls;

using TrakHound.Configurations;
using TrakHound_Overview.Pages.Dashboard.OverrideStatus.Controls;

namespace TrakHound_Overview.Pages.Dashboard.OverrideStatus
{
    /// <summary>
    /// Interaction logic for StatusTimeline.xaml
    /// </summary>
    public partial class Plugin : UserControl
    {
        public Plugin()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        ObservableCollection<Row> _rows;
        public ObservableCollection<Row> Rows
        {
            get
            {
                if (_rows == null) _rows = new ObservableCollection<Row>();
                return _rows;
            }
            set
            {
                _rows = value;
            }
        }

        private void AddRow(DeviceConfiguration config)
        {
            var row = new Row();
            row.Configuration = config;
            Rows.Add(row);
        }

    }
}
