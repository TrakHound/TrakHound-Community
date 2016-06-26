using System.Collections.ObjectModel;
using System.Windows.Controls;

using TH_Configuration;
using TH_StatusTimes.Controls;

namespace TH_StatusTimes
{
    /// <summary>
    /// Interaction logic for StatusTimeline.xaml
    /// </summary>
    public partial class StatusTimes : UserControl
    {
        public StatusTimes()
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
