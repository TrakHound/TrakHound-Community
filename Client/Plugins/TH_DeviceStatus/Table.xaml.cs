// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TH_StatusTable
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class StatusTable : UserControl
    {
        public StatusTable()
        {
            InitializeComponent();

            root.DataContext = this;

            CreateColumns(0, 24);
        }

        private void CreateColumns(int first, int count)
        {
            var columns = new List<DataGridColumn>();

            int last = first + count;
            for (var x = first; x <= last - 1; x++)
            {
                var column = new DataGridTemplateColumn();
                column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                column.MinWidth = 40;

                // Header
                var header = new Controls.Header();

                int hour = x;

                int startHour = x;
                string titleSuffix = "AM";
                if (startHour >= 12) titleSuffix = "PM";
                if (startHour > 12) startHour -= 12;
                if (startHour == 0) startHour = 12;

                DateTime start = new DateTime(1, 1, 1, hour, 0, 0);
                hour += 1;
                if (hour > 23) hour -= 24;
                DateTime end = new DateTime(1, 1, 1, hour, 0, 0);

                header.Text = startHour + " " + titleSuffix;
                header.TooltipHeader = start.ToShortTimeString() + " - " + end.ToShortTimeString();
                column.Header = header;

                // Add Cell
                //var cell = new FrameworkElementFactory(typeof(Controls.Cell));
                var cell = new FrameworkElementFactory(typeof(Controls.BetterCell));
                cell.SetBinding(Controls.BetterCell.HourDataProperty, new Binding("HourDatas[" + x.ToString() + "]"));

                


                // Set Template
                var template = new DataTemplate();
                template.VisualTree = cell;
                column.CellTemplate = template;
                //column.CellStyle = TryFindResource("HourColumnTemplate");
                

                columns.Add(column);
            }

            foreach (var column in columns)
            {
                Devices_DG.Columns.Add(column);
            }
        }

        private void TestColumns()
        {


        }
    }

}
