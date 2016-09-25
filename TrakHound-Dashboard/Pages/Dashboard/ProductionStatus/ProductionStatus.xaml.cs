// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using TrakHound;
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

        private void AddRow(DeviceDescription device)
        {
            if (device != null && !Rows.ToList().Exists(o => o.Device.UniqueId == device.UniqueId))
            {
                var row = new Row(device);
                row.Clicked += Row_Clicked;
                Rows.Add(row);
            }
        }

        private void AddRow(DeviceDescription device, int index)
        {
            if (device != null && !Rows.ToList().Exists(o => o.Device.UniqueId == device.UniqueId))
            {
                var row = new Row(device);
                row.Clicked += Row_Clicked;
                Rows.Insert(index, row);
            }
        }

        private void Row_Clicked(Controls.Row row)
        {
            var data = new EventData();
            data.Id = "OPEN_DEVICE_DETAILS";
            data.Data01 = row.Device;
            SendData?.Invoke(data);
        }
    }
}
