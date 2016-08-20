// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

using TrakHound.Configurations;

namespace TrakHound_Dashboard.Pages.Dashboard.ControllerStatus
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

        ObservableCollection<Controls.Row> _rows;
        public ObservableCollection<Controls.Row> Rows
        {
            get
            {
                if (_rows == null) _rows = new ObservableCollection<Controls.Row>();
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
                var row = new Controls.Row();
                row.Configuration = config;
                Rows.Add(row);
            }
        }

        private void AddRow(DeviceConfiguration config, int index)
        {
            if (config != null && !Rows.ToList().Exists(o => o.Configuration.UniqueId == config.UniqueId))
            {
                var row = new Controls.Row();
                row.Configuration = config;
                Rows.Insert(index, row);
            }
        }

    }
}
