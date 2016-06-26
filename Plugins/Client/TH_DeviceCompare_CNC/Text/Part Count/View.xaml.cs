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
            DataContext = this;
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


        private bool useSnapshotForParts = false;

        void Update(EventData data)
        {
            if (data != null && data.Data01 != null && data.Data01.GetType() == typeof(DeviceConfiguration))
            {

                // Use Snapshot table if Part Count is given as a total for the day
                if (data.Id.ToLower() == "statusdata_snapshots")
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        int count = 0;

                        string val = GetTableValue(data.Data02, "Part Count");
                        if (val != null)
                        {
                            useSnapshotForParts = true;

                            int.TryParse(val, out count);

                            //info.PartCount = count;
                            Value = count.ToString();
                        }
                    }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
                }

                // Use the Parts table is Part Count is given as DISCRETE (number of parts per event) and not the total for the day
                if (data.Id.ToLower() == "statusdata_parts" && data.Data02 != null && !useSnapshotForParts)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var dt = data.Data02 as DataTable;
                        if (dt != null && dt.Columns.Contains("count"))
                        {
                            int count = 0;

                            foreach (DataRow row in dt.Rows)
                            {
                                string val = row["count"].ToString();

                                int i = 0;
                                if (int.TryParse(val, out i)) count += i;
                            }

                            //info.PartCount = count;
                            Value = count.ToString();
                        }
                    }), UI_Functions.PRIORITY_BACKGROUND, new object[] { });
                }
            }
        }

        //void Update(EventData data)
        //{
        //    if (data != null && data.Data01 != null && data.Data01.GetType() == typeof(Configuration))
        //    {
        //        // Snapshot Table Data
        //        if (data.Id.ToLower() == "statusdata_parts")
        //        {
        //            this.Dispatcher.BeginInvoke(new Action<object>(UpdateText), Priority_Context, new object[] { data.Data02 });
        //        }
        //    }
        //}


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

        private string GetTableValue(object obj, string key)
        {
            var dt = obj as DataTable;
            if (dt != null)
            {
                return DataTable_Functions.GetTableValue(dt, "name", key, "value");
            }
            return null;
        }

    }
}
