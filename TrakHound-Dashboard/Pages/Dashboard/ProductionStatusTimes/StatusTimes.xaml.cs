using System.Collections.ObjectModel;
using System.Windows.Controls;

using TrakHound.Configurations;
using TH_StatusTimes.ProductionStatus.Controls;

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
            var row = new Row();
            row.Configuration = config;
            Rows.Add(row);
        }

    }
}
