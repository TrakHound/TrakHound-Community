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

        private void AddRow(DeviceDescription device)
        {
            if (device != null && !Rows.ToList().Exists(o => o.Device.UniqueId == device.UniqueId))
            {
                var row = new Row(device);
                Rows.Add(row);
            }
        }

        private void AddRow(DeviceDescription device, int index)
        {
            if (device != null && !Rows.ToList().Exists(o => o.Device.UniqueId == device.UniqueId))
            {
                var row = new Row(device);
                Rows.Insert(index, row);
            }
        }

    }
}
