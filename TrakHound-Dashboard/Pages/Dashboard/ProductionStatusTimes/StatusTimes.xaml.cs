using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using TH_StatusTimes.ProductionStatus.Controls;

using TrakHound.Configurations;

namespace TH_StatusTimes.ProductionStatus
{
    /// <summary>
    /// Interaction logic for StatusTimeline.xaml
    /// </summary>
    public partial class ProductionStatusTimes : UserControl
    {
        public ProductionStatusTimes()
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
