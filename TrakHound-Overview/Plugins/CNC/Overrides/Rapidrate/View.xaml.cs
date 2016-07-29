// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using TrakHound.Configurations;
using TrakHound.Tools;
using TrakHound.Plugins;
using TrakHound_UI;

namespace TH_DeviceCompare_CNC.Overrides.Rapidrate
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Plugin : UserControl
    {
        public Plugin()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        const string overrideLink = "Rapidrate Override";


        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(Plugin), new PropertyMetadata(0d));


        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;


        void Update(EventData data)
        {
            if (data != null && data.Data01 != null && data.Data01.GetType() == typeof(DeviceConfiguration))
            {
                // Snapshot Table Data
                if (data.Id.ToLower() == "statusdata_snapshots")
                {
                    this.Dispatcher.BeginInvoke(new Action<object>(UpdateOverrides), Priority_Context, new object[] { data.Data02 });
                }
            }
        }

        private ObservableCollection<MeterDisplay> _overrides;
        public ObservableCollection<MeterDisplay> Overrides
        {
            get
            {
                if (_overrides == null) _overrides = new ObservableCollection<MeterDisplay>();
                return _overrides;
            }
            set
            {
                _overrides = value;
            }
        }

        void UpdateOverrides(object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                var rows = dt.Select("name LIKE 'Rapidrate Override%'");
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        string id = DataTable_Functions.GetRowValue("name", row);
                        string value = DataTable_Functions.GetRowValue("value", row);
                        string link = DataTable_Functions.GetRowValue("link", row);
                        if (id != null && value != null)
                        {
                            double ovr = 0;
                            if (double.TryParse(value, out ovr))
                            {
                                MeterDisplay match = null;

                                int i = Overrides.ToList().FindIndex(x => x.DataObject.ToString() == id);
                                if (i < 0)
                                {
                                    match = new MeterDisplay();
                                    match.Maximum = 1;
                                    match.Title = link;
                                    match.DataObject = id;
                                    Overrides.Add(match);
                                }
                                else match = Overrides[i];

                                match.Value = ovr * 0.01;
                            }
                        }
                    }
                }
            }
        }


        //void UpdateOverride(object snapshotData)
        //{
        //    DataTable dt = snapshotData as DataTable;
        //    if (dt != null)
        //    {
        //        string value = DataTable_Functions.GetTableValue(dt, "name", overrideLink, "value");

        //        if (value != null)
        //        {
        //            double ovr = 0;
        //            if (double.TryParse(value, out ovr))
        //            {
        //                Value = ovr * 0.01;
        //            }
        //        }
        //    }
        //}

    }
}
