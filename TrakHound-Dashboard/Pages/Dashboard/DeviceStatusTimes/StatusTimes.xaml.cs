using System.Collections.ObjectModel;
using System.Windows.Controls;

using System.Linq;

using TrakHound.Configurations;
using TrakHound_Dashboard.Pages.Dashboard.DeviceStatusTimes.Controls;

namespace TrakHound_Dashboard.Pages.Dashboard.DeviceStatusTimes
{
    /// <summary>
    /// Interaction logic for StatusTimeline.xaml
    /// </summary>
    public partial class DeviceStatusTimes : UserControl
    {
        public DeviceStatusTimes()
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
            if (config != null && !Rows.ToList().Exists(o => o.Configuration.UniqueId == config.UniqueId))
            {
                var row = new Row(config);
                Rows.Add(row);
            }
        }

        private void AddRow(DeviceConfiguration config, int index)
        {
            if (config != null && !Rows.ToList().Exists(o => o.Configuration.UniqueId == config.UniqueId))
            {
                var row = new Row(config);
                Rows.Insert(index, row);
            }
        }

    }
}
