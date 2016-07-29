// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

using TrakHound.Configurations;
using TrakHound.Plugins;

namespace TH_DeviceCompare_OEE.Values.Performance
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


        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(Plugin), new PropertyMetadata(0d));

        public int Status
        {
            get { return (int)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(int), typeof(Plugin), new PropertyMetadata(0));

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;


        void Update(EventData data)
        {
            if (data != null && data.Data01 != null && data.Data01.GetType() == typeof(DeviceConfiguration))
            {
                // OEE Table Data
                if (data.Id.ToLower() == "statusdata_oee_segments")
                {
                    // OEE Values
                    this.Dispatcher.BeginInvoke(new Action<object>(OEEValues_Update), Priority_Context, new object[] { data.Data02 });
                }
            }
        }

        void OEEValues_Update(object oeedata)
        {
            var dt = oeedata as DataTable;
            if (dt != null)
            {
                var oeeData = OEEData.FromDataTable(dt);

                Value = oeeData.Performance;

                if (oeeData.Performance >= 0.75) Status = 2;
                else if (oeeData.Performance >= 0.5) Status = 1;
                else Status = 0;
            }
        }

    }
}
