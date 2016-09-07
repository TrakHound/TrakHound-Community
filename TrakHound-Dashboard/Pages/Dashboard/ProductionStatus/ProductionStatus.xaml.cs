// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using TrakHound.Configurations;
using TrakHound_Dashboard.Pages.Dashboard.ProductionStatus.Controls;

namespace TrakHound_Dashboard.Pages.Dashboard.ProductionStatus
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
