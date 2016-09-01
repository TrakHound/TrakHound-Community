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
using TH_ProductionStatus.Controls;

using TrakHound.Configurations;

namespace TH_ProductionStatus
{

    public partial class ProductionStatus : UserControl
    {
        public ProductionStatus()
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
