using System.Collections.ObjectModel;
using System.Windows.Controls;
using TrakHound;
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
            row.Clicked += Row_Clicked;
            Rows.Add(row);
        }

        private void AddRow(DeviceConfiguration config, int index)
        {
            var row = new Controls.Row();
            row.Configuration = config;
            row.Clicked += Row_Clicked;
            Rows.Insert(index, row);
        }

        private void Row_Clicked(Controls.Row row)
        {
            var data = new EventData(this);
            data.Id = "OPEN_DEVICE_DETAILS";
            data.Data01 = row.Device;
            SendData?.Invoke(data);
        }
    }
}
