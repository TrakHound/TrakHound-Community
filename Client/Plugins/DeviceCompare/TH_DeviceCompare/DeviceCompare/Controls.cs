// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using TH_Global.Functions;
using TH_Plugins.Client;

using TH_DeviceCompare.Controls;
using TH_DeviceCompare.Controls.DeviceDisplay;

namespace TH_DeviceCompare
{
    public partial class DeviceCompare
    {

        ObservableCollection<RowHeader> _rowHeaders;
        public ObservableCollection<RowHeader> RowHeaders
        {
            get
            {
                if (_rowHeaders == null) _rowHeaders = new ObservableCollection<RowHeader>();
                return _rowHeaders;
            }
            set
            {
                _rowHeaders = value;
            }
        }

        List<DeviceDisplay> _deviceDisplays;
        public List<DeviceDisplay> DeviceDisplays
        {
            get
            {
                if (_deviceDisplays == null) _deviceDisplays = new List<DeviceDisplay>();
                return _deviceDisplays;
            }
            set
            {
                _deviceDisplays = value;
            }
        }

        ObservableCollection<Header> _headers;
        public ObservableCollection<Header> Headers
        {
            get
            {
                if (_headers == null) _headers = new ObservableCollection<Header>();
                return _headers;
            }
            set
            {
                _headers = value;
            }
        }

        ObservableCollection<Column> _columns;
        public ObservableCollection<Column> Columns
        {
            get
            {
                if (_columns == null) _columns = new ObservableCollection<Column>();
                return _columns;
            }
            set
            {
                _columns = value;
            }
        }

        ObservableCollection<Overlay> _overlays;
        public ObservableCollection<Overlay> Overlays
        {
            get
            {
                if (_overlays == null) _overlays = new ObservableCollection<Overlay>();
                return _overlays;
            }
            set
            {
                _overlays = value;
            }
        }

        /// <summary>
        /// Event Handler for when a DeviceDisplay's Cell's Size (height) has changed so it can update other Device Displays
        /// </summary>
        /// <param name="sender"></param>
        void display_CellSizeChanged(Controls.DeviceDisplay.Cell sender)
        {
            SetCellHeights(sender);
        }

        /// <summary>
        /// Compare the Height of the 'sender' Cell to the other corresponding Cell in each Device Display. 
        /// Also set the height of the corresponding RowHeader
        /// </summary>
        /// <param name="sender"></param>
        void SetCellHeights(Cell sender)
        {
            double height = sender.DesiredSize.Height;
            int index = sender.Index;

            foreach (var deviceDisplay in DeviceDisplays)
            {
                var cellIndex = deviceDisplay.Cells.ToList().FindIndex(x => x.Index == index);
                if (cellIndex >= 0)
                {
                    var cell = deviceDisplay.Cells[cellIndex];
                    if (cell.DesiredSize.Height < sender.DesiredSize.Height)
                    {
                        cell.MinHeight = height;
                    }
                }
            }

            var headerIndex = RowHeaders.ToList().FindIndex(x => x.Index == index);
            if (headerIndex >= 0) RowHeaders[headerIndex].MinHeight = height;
        }

        #region "Row Headers"

        void AddRowHeaders(List<IClientPlugin> plugins, List<PluginConfiguration> configs)
        {
            RowHeaders.Clear();

            if (plugins != null)
            {
                foreach (var plugin in plugins)
                {
                    AddRowHeader(plugin, configs);
                }
            }
        }

        void AddRowHeader(IClientPlugin plugin, List<PluginConfiguration> configs)
        {
            var config = configs.Find(x => x.Name == plugin.Title);
            if (config != null)
            {
                config.EnabledChanged += RowHeader_ConfigEnabledChanged;

                if (config.Enabled)
                {
                    var header = new RowHeader();
                    header.Text = plugin.Title;
                    header.Index = RowHeaders.Count;
                    header.IndexChanged += RowHeader_IndexChanged;
                    header.ResetOrder += Header_ResetOrder;
                    RowHeaders.Add(header);
                }
            }
        }

        void RowHeader_ConfigEnabledChanged(PluginConfiguration config)
        {
            var category = SubCategories.Find(x => x.Name == "Components");
            if (category != null)
            {
                var plugin = Plugins.Find(x => x.Title == config.Name);
                if (plugin != null)
                {
                    AddRowHeader(plugin, category.PluginConfigurations);

                    foreach (var deviceDisplay in DeviceDisplays)
                    {
                        deviceDisplay.AddPlugin(plugin, category.PluginConfigurations);
                    }
                }
            }
        }

        /// <summary>
        /// Event Handler for when a RowHeader has requested to change it's Index (position in List)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newIndex"></param>
        /// <param name="oldIndex"></param>
        void RowHeader_IndexChanged(RowHeader sender, int newIndex, int oldIndex)
        {
            SortDataItems(sender, newIndex, oldIndex);
            SaveDataItemOrder();
        }

        private void Header_ResetOrder()
        {
            SortDataItems(true);
        }

        void RowHeaders_UnselectAll()
        {
            foreach (var header in RowHeaders)
            {
                header.IsSelected = false;
            }
        }

        void Rows_UnSelectAll()
        {
            foreach (Column column in Columns)
            {
                foreach (Cell cell in column.Cells) cell.IsSelected = false;
            }
        }

        void rh_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(RowHeader))
            {
                var rh = (RowHeader)sender;

                int index = RowHeaders.IndexOf(rh);

                foreach (Column column in Columns)
                {
                    for (int x = 0; x <= column.Cells.Count - 1; x++)
                    {
                        if (x != index) column.Cells[x].IsSelected = false;
                    }

                    column.Cells[index].IsSelected = true;
                }
            }
        }


        #endregion

        #region "Headers (Columns)"

        private void ColumnHeaderMinimize_Clicked(TH_WPF.Button bt)
        {
            HeaderViewType viewType = HeaderViewType.Large;

            foreach (var header in Headers)
            {
                header.ViewType = header.ViewType - 1;
                viewType = header.ViewType;
            }

            SaveHeaderView(viewType);
        }

        private void ColumnHeaderMaximize_Clicked(TH_WPF.Button bt)
        {
            HeaderViewType viewType = HeaderViewType.Large;

            foreach (var header in Headers)
            {
                header.ViewType = header.ViewType + 1;
                viewType = header.ViewType;
            }

            SaveHeaderView(viewType);
        }


        public HeaderViewType ViewType
        {
            get { return (HeaderViewType)GetValue(ViewTypeProperty); }
            set { SetValue(ViewTypeProperty, value); }
        }

        public static readonly DependencyProperty ViewTypeProperty =
            DependencyProperty.Register("ViewType", typeof(HeaderViewType), typeof(DeviceCompare), new PropertyMetadata(HeaderViewType.Large));


        void LoadHeaderView()
        {
            HeaderViewType viewType = (HeaderViewType)Properties.Settings.Default.HeaderView;
            ViewType = viewType;

            foreach (var header in Headers) header.ViewType = viewType;
        }

        void SaveHeaderView(HeaderViewType viewType)
        {
            ViewType = viewType;

            Properties.Settings.Default.HeaderView = (int)viewType;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region "Data Item Sorting"

        /// <summary>
        /// Change Index of a RowHeader and each related DataItem 
        /// </summary>
        /// <param name="sender">The RowHeader that was changed</param>
        /// <param name="newIndex"></param>
        /// <param name="oldIndex"></param>
        void SortDataItems(RowHeader sender, int newIndex, int oldIndex)
        {
            // Check if newIndex is valid
            if (newIndex >= 0 && newIndex <= RowHeaders.Count - 1)
            {
                // Move the RowHeader inside the collection based on the newIndex
                RowHeaders.Move(oldIndex, newIndex);

                // Move the Cell for each DeviceDisplay to match the index for the RowHeader
                foreach (var deviceDisplay in DeviceDisplays)
                {
                    deviceDisplay.Cells.Move(oldIndex, newIndex);
                }

                // Reset Indexes so they are in numerical order
                for (var x = 0; x <= RowHeaders.Count - 1; x++) RowHeaders[x].Index = x;
                foreach (var deviceDisplay in DeviceDisplays)
                {
                    for (var x = 0; x <= deviceDisplay.Cells.Count - 1; x++) deviceDisplay.Cells[x].Index = x;
                }
            }
        }

        /// <summary>
        /// Default Order for Data Items if not found in Application Settings
        /// </summary>
        private string[] defaultDataItemOrder = new string[]
        {
            "OEE",
            "Availability",
            "Performance",
            "Timeline (OEE)",
            "Production Status", 
            "Program Name",
            "Feedrate Override",
            "Rapidrate Override",
            "Spindle Override",
            "Emergency Stop",
            "Controller Mode",
            "Execution Mode",
            "Alarm",
            "Part Count",
        };

        /// <summary>
        /// Initial Sort of Data Items (this should be called when Device Displays are first created)
        /// </summary>
        void SortDataItems(bool reset = false)
        {
            string[] titles = null;

            // Get order array from Application Settings
            if (!reset) titles = Properties.Settings.Default.DataItemOrder;

            // If not found in Application Settings, then load from defaultDataItemOrder array
            if (titles == null) titles = defaultDataItemOrder;

            // Loop through order array and set Index for each DataItem that is found
            for (var i = 0; i <= titles.Length - 1; i++)
            {
                // Find RowHeader with matching title/text
                int index = RowHeaders.ToList().FindIndex(x => x.Text == titles[i]);
                if (index >= 0)
                {
                    // Set new indexes to RowHeader and Cells in each DeviceDisplay
                    RowHeaders[index].Index = i;
                    foreach (var deviceDisplay in DeviceDisplays)
                    {
                        index = deviceDisplay.Cells.ToList().FindIndex(x => x.Link == titles[i]);
                        if (index >= 0)
                        {
                            deviceDisplay.Cells[index].Index = i;
                        }
                    }
                }
            }

            // Sort Collections using new indexes (.Sort() is an custom extension method)
            RowHeaders.Sort();
            foreach (var deviceDisplay in DeviceDisplays)
            {
                deviceDisplay.Cells.Sort();
            }

            // Save the resulting order to Application Settings
            SaveDataItemOrder();
        }

        /// <summary>
        /// Save the order of the Data Items to Application Settings
        /// </summary>
        void SaveDataItemOrder()
        {
            var titles = new List<string>();
            for (var i = 0; i <= RowHeaders.Count - 1; i++)
            {
                titles.Add(RowHeaders[i].Text);
            }

            Properties.Settings.Default.DataItemOrder = titles.ToArray();
            Properties.Settings.Default.Save();
        }

        #endregion

    }

}
