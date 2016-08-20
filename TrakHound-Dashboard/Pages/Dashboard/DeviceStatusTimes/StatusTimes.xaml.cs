using System.Collections.ObjectModel;
using System.Windows.Controls;

using TrakHound.Configurations;
using TH_StatusTimes.DeviceStatus.Controls;

namespace TH_StatusTimes.DeviceStatus
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
            var row = new Row();
            row.Configuration = config;
            Rows.Add(row);
        }

        private void AddRow(DeviceConfiguration config, int index)
        {
            var row = new Controls.Row();
            row.Configuration = config;
            Rows.Insert(index, row);
        }

    }
}
