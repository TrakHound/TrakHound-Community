// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

using TH_Configuration;
using TH_Global.Functions;
using TH_Plugins;

namespace TH_DeviceCompare_CNC.Overrides.SpindleSpeed
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

        const string overrideLink = "Spindle Override";


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
                    this.Dispatcher.BeginInvoke(new Action<object>(UpdateOverride), Priority_Context, new object[] { data.Data02 });
                }
            }
        }


        void UpdateOverride(object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "name", overrideLink, "value");

                if (value != null)
                {
                    double ovr = 0;
                    if (double.TryParse(value, out ovr))
                    {
                        Value = ovr * 0.01;
                    }
                }
            }
        }

    }
}
