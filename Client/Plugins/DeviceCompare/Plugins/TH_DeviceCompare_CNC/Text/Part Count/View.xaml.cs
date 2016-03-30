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

namespace TH_DeviceCompare_CNC.Text.Part_Count
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

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(Plugin), new PropertyMetadata(null));


        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;


        void Update(EventData data)
        {
            if (data != null && data.data01 != null && data.data01.GetType() == typeof(Configuration))
            {
                // Snapshot Table Data
                if (data.id.ToLower() == "statusdata_parts")
                {
                    this.Dispatcher.BeginInvoke(new Action<object>(UpdateText), Priority_Context, new object[] { data.data02 });
                }
            }
        }


        void UpdateText(object partsData)
        {
            DataTable dt = partsData as DataTable;
            if (dt != null && dt.Columns.Contains("count"))
            {
                int count = 0;
                
                foreach (DataRow row in dt.Rows)
                {

                    string val = row["count"].ToString();

                    int i = 0;
                    if (int.TryParse(val, out i)) count += i;
                }

                Value = count.ToString();
            }
        }

    }
}
