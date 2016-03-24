// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;
using System.Collections.ObjectModel;
using System.IO;
using System.Data;

using TH_Configuration;
using TH_Database;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins;
using TH_Plugins.Client;
using TH_WPF;

using TH_TableManager.Controls;

namespace TH_TableManager
{
    /// <summary>
    /// Interaction logic for TableManager.xaml
    /// </summary>
    public partial class Plugin : UserControl
    {
        public Plugin()
        {
            InitializeComponent();
            DataContext = this;
        }

        ObservableCollection<ListButton> devicelist;
        public ObservableCollection<ListButton> DeviceList
        {
            get
            {
                if (devicelist == null)
                    devicelist = new ObservableCollection<ListButton>();
                return devicelist;
            }

            set
            {
                devicelist = value;
            }
        }

        #region "Properties"

        public bool LoadingDevices
        {
            get { return (bool)GetValue(LoadingDevicesProperty); }
            set { SetValue(LoadingDevicesProperty, value); }
        }

        public static readonly DependencyProperty LoadingDevicesProperty =
            DependencyProperty.Register("LoadingDevices", typeof(bool), typeof(Plugin), new PropertyMetadata(false));

        public bool LoggedIn
        {
            get { return (bool)GetValue(LoggedInProperty); }
            set { SetValue(LoggedInProperty, value); }
        }

        public static readonly DependencyProperty LoggedInProperty =
            DependencyProperty.Register("LoggedIn", typeof(bool), typeof(Plugin), new PropertyMetadata(false));

        #endregion

        #region "Table List"

        ObservableCollection<ListButton> tablelist;
        public ObservableCollection<ListButton> TableList
        {
            get
            {
                if (tablelist == null)
                    tablelist = new ObservableCollection<ListButton>();
                return tablelist;
            }

            set
            {
                tablelist = value;
            }
        }

        List<ListButton> selectedtables;
        List<ListButton> SelectedTables 
        {
            get 
            {
                if (selectedtables == null)
                {
                    selectedtables = new List<ListButton>();
                }
                    
                return selectedtables;
            }

            set
            {
                selectedtables = value;
            }
        }

        public bool TableIsSelected
        {
            get { return (bool)GetValue(TableIsSelectedProperty); }
            set { SetValue(TableIsSelectedProperty, value); }
        }

        public static readonly DependencyProperty TableIsSelectedProperty =
            DependencyProperty.Register("TableIsSelected", typeof(bool), typeof(Plugin), new PropertyMetadata(false));

       
        public bool TableListShown
        {
            get { return (bool)GetValue(TableListShownProperty); }
            set { SetValue(TableListShownProperty, value); }
        }

        public static readonly DependencyProperty TableListShownProperty =
            DependencyProperty.Register("TableListShown", typeof(bool), typeof(Plugin), new PropertyMetadata(false));

        Thread TableList_WORKER;

        void LoadTableList(Configuration config)
        {
            TableListShown = false;
            TableDataView = null;
            TableInfoShown = false;

            if (TableList_WORKER != null) TableList_WORKER.Abort();

            TableList_WORKER = new Thread(new ParameterizedThreadStart(LoadTableList_Worker));
            TableList_WORKER.Start(config);
        }

        void LoadTableList_Worker(object o)
        {
            Configuration config = (Configuration)o;

            string[] tableNames = TH_Database.Table.List(config.Databases_Client);

            if (tableNames != null)
            {
                var list = tableNames.ToList();
                list.Sort();

                if (!String.IsNullOrEmpty(config.DatabaseId))
                {
                    list = list.FindAll(x => x.StartsWith(config.DatabaseId));
                }

                tableNames = list.ToArray();
            }

            this.Dispatcher.BeginInvoke(new Action<string[]>(LoadTableList_Finished), Priority, new object[] { tableNames });
        }

        void LoadTableList_Finished(string[] tableNames)
        {
            TableList.Clear();

            if (tableNames != null)
            {
                foreach (string tableName in tableNames)
                {
                    TH_WPF.ListButton lb = new TH_WPF.ListButton();
                    lb.Text = tableName;
                    lb.Selected += lb_Table_Selected;
                    lb.MultiSelected += TableList_MultiSelected;
                    lb.MultiUnselected += TableList_MultiUnselected;
                    this.Dispatcher.BeginInvoke(new Action<ListButton>(LoadTableList_AddItem), Priority, new object[] { lb });
                }

                if (tableNames.Length > 0) TableListShown = true;
            }

        }


        void TableList_MultiSelected(ListButton LB)
        {
            SelectedTables.Add(LB);

            ProcessSelectedTables();
        }

        void TableList_MultiUnselected(ListButton LB)
        {
            int index = SelectedTables.ToList().FindIndex(x => x.Text.ToLower() == LB.Text.ToLower());
            if (index >= 0 && SelectedTables.Count > 1)
            {
                if (index == SelectedTables.Count - 2) SelectedTables.Remove(SelectedTables[index + 1]);
            }
                
            ProcessSelectedTables();
        }

        void ProcessSelectedTables()
        {
            if (SelectedTables.Count > 0) TableIsSelected = true;
            else TableIsSelected = false;

            foreach (ListButton lb in TableList)
            {
                if (SelectedTables.FindIndex(x => x.Text == lb.Text) >= 0) lb.IsSelected = true;
                else lb.IsSelected = false;
            }
        }

        void LoadTableList_AddItem(ListButton lb)
        {
            TableList.Add(lb);
        }

        void lb_Table_Selected(TH_WPF.ListButton LB)
        {
            SelectedTables.Clear();
            SelectedTables.Add(LB);

            foreach (TH_WPF.ListButton olb in TableList.OfType<TH_WPF.ListButton>()) if (olb != LB) olb.IsSelected = false;
            LB.IsSelected = true;

            Int64 page = 0;

            TableIsSelected = true;
            TableInfoShown = false;
            SelectedTableName = null;
            TableRowCount = null;

            this.Dispatcher.BeginInvoke(new Action<string, Configuration>(LoadInfo), Priority, new object[] { LB.Text, SelectedDevice });
            this.Dispatcher.BeginInvoke(new Action<string, Int64, Configuration>(LoadTable), Priority, new object[] { LB.Text, page, SelectedDevice });
        }

        #endregion

        void UpdateLoggedInChanged(EventData data)
        {
            if (data != null)
            {
                if (data.id.ToLower() == "userloggedin")
                {
                    LoggedIn = true;
                }

                if (data.id.ToLower() == "userloggedout")
                {
                    LoggedIn = false;
                }
            }
        }

        void UpdateDevicesLoading(EventData data)
        {
            if (data != null)
            {
                if (data.id.ToLower() == "loadingdevices")
                {
                    LoadingDevices = true;
                }

                if (data.id.ToLower() == "devicesloaded")
                {
                    LoadingDevices = false;
                }
            }
        }

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority = System.Windows.Threading.DispatcherPriority.ContextIdle;


        public Configuration SelectedDevice
        {
            get { return (Configuration)GetValue(SelectedDeviceProperty); }
            set { SetValue(SelectedDeviceProperty, value); }
        }

        public static readonly DependencyProperty SelectedDeviceProperty =
            DependencyProperty.Register("SelectedDevice", typeof(Configuration), typeof(Plugin), new PropertyMetadata(null));

        void lb_Device_Selected(TH_WPF.ListButton lb)
        {
            foreach (TH_WPF.ListButton olb in DeviceList.OfType<TH_WPF.ListButton>()) if (olb != lb) olb.IsSelected = false;
            lb.IsSelected = true;
        
            Configuration device = (Configuration)lb.DataObject;

            SelectedDevice = device;

            LoadTableList(device);
        }

        #region "Table View"

        class LoadTableParameters
        {
            public string tablename { get; set; }
            public Int64 page { get; set; }
            public Configuration config { get; set; }
        }

        #region "Table Info"

        public bool TableInfoShown
        {
            get { return (bool)GetValue(TableInfoShownProperty); }
            set { SetValue(TableInfoShownProperty, value); }
        }

        public static readonly DependencyProperty TableInfoShownProperty =
            DependencyProperty.Register("TableInfoShown", typeof(bool), typeof(Plugin), new PropertyMetadata(false));

        public string SelectedTableName
        {
            get { return (string)GetValue(SelectedTableNameProperty); }
            set { SetValue(SelectedTableNameProperty, value); }
        }

        public static readonly DependencyProperty SelectedTableNameProperty =
            DependencyProperty.Register("SelectedTableName", typeof(string), typeof(Plugin), new PropertyMetadata(null));

        
        public string TableRowCount
        {
            get { return (string)GetValue(TableRowCountProperty); }
            set { SetValue(TableRowCountProperty, value); }
        }

        public static readonly DependencyProperty TableRowCountProperty =
            DependencyProperty.Register("TableRowCount", typeof(string), typeof(Plugin), new PropertyMetadata(null));


        public string TableSize
        {
            get { return (string)GetValue(TableSizeProperty); }
            set { SetValue(TableSizeProperty, value); }
        }

        public static readonly DependencyProperty TableSizeProperty =
            DependencyProperty.Register("TableSize", typeof(string), typeof(Plugin), new PropertyMetadata(null));


        public string SelectedRowCount
        {
            get { return (string)GetValue(SelectedRowCountProperty); }
            set { SetValue(SelectedRowCountProperty, value); }
        }

        public static readonly DependencyProperty SelectedRowCountProperty =
            DependencyProperty.Register("SelectedRowCount", typeof(string), typeof(Plugin), new PropertyMetadata(null));


        Thread Info_WORKER;

        void LoadInfo(string tableName, Configuration config)
        {
            SelectedTableName = tableName;

            LoadTableParameters ltp = new LoadTableParameters();
            ltp.tablename = tableName;
            ltp.config = config;

            if (Info_WORKER != null) Info_WORKER.Abort();

            Info_WORKER = new Thread(new ParameterizedThreadStart(LoadInfo_Worker));
            Info_WORKER.Start(ltp);
        }

        void LoadInfo_Worker(object loadTableParameters)
        {
            LoadTableParameters ltp = (LoadTableParameters)loadTableParameters;

            Int64 rowCount = TH_Database.Table.GetRowCount(ltp.config.Databases_Client, ltp.tablename);

            Int64 tablesize = TH_Database.Table.GetSize(ltp.config.Databases_Client, ltp.tablename);

            this.Dispatcher.BeginInvoke(new Action<Int64, Int64>(LoadInfo_Finished), Priority, new object[] { rowCount, tablesize });
        }

        void LoadInfo_Finished(Int64 rowCount, Int64 tablesize)
        {
            TableInfoShown = true;

            RowCount = rowCount;
            TableRowCount = rowCount.ToString("n0");
            TableSize = String_Functions.FileSizeSuffix(tablesize);

            RowDisplayLimit = Math.Min(RowCount, rowLimits[rowLimitIndex]).ToString();

            LoadingRowDisplay = true;

            totalRows = rowCount;
            selectedPage = 1;

            CreatePageNumbers();
        }

        private void TableInfoRefresh_Button_Clicked(TH_WPF.Button bt)
        {
            LoadInfo(SelectedTableName, SelectedDevice);
        }

        Int64 RowCount;

        #endregion

        #region "Load Table"

        public DataView TableDataView
        {
            get { return (DataView)GetValue(TableDataViewProperty); }
            set { SetValue(TableDataViewProperty, value); }
        }

        public static readonly DependencyProperty TableDataViewProperty =
            DependencyProperty.Register("TableDataView", typeof(DataView), typeof(Plugin), new PropertyMetadata(null));

        public bool TableDataLoading
        {
            get { return (bool)GetValue(TableDataLoadingProperty); }
            set { SetValue(TableDataLoadingProperty, value); }
        }

        public static readonly DependencyProperty TableDataLoadingProperty =
            DependencyProperty.Register("TableDataLoading", typeof(bool), typeof(Plugin), new PropertyMetadata(false));

        public bool LoadingRowDisplay
        {
            get { return (bool)GetValue(LoadingRowDisplayProperty); }
            set { SetValue(LoadingRowDisplayProperty, value); }
        }

        public static readonly DependencyProperty LoadingRowDisplayProperty =
            DependencyProperty.Register("LoadingRowDisplay", typeof(bool), typeof(Plugin), new PropertyMetadata(false));

        public string LoadingRow
        {
            get { return (string)GetValue(LoadingRowProperty); }
            set { SetValue(LoadingRowProperty, value); }
        }

        public static readonly DependencyProperty LoadingRowProperty =
            DependencyProperty.Register("LoadingRow", typeof(string), typeof(Plugin), new PropertyMetadata(null));



        Thread Table_WORKER;

        LoadTableParameters selectedTableParameters;

        void LoadTable(string tableName, Int64 page, Configuration config)
        {
            TableDataView = null;
            LoadingRowDisplay = false;
            TableDataLoading = true;      
            LoadingRow = "0";

            LoadTableRowLimit();

            LoadTableParameters ltp = new LoadTableParameters();
            ltp.tablename = tableName;
            ltp.page = page;
            ltp.config = config;

            selectedTableParameters = ltp;

            if (Table_WORKER != null) Table_WORKER.Abort();

            Table_WORKER = new Thread(new ParameterizedThreadStart(LoadTable_Worker));
            Table_WORKER.Start(ltp);
        }

        void LoadTable()
        {
            if (selectedTableParameters != null)
            {
                TableDataView = null;
                TableDataLoading = true;
                LoadingRow = "0";

                RowDisplayLimit = Math.Min(RowCount, rowLimits[rowLimitIndex]).ToString();

                LoadTableRowLimit();

                if (Table_WORKER != null) Table_WORKER.Abort();

                Table_WORKER = new Thread(new ParameterizedThreadStart(LoadTable_Worker));
                Table_WORKER.Start(selectedTableParameters);
            }
        }

        void LoadTable_Worker(object loadTableParameters)
        {
            LoadTableParameters ltp = (LoadTableParameters)loadTableParameters;

            // Row Limit
            int limit = rowLimits[Properties.Settings.Default.RowDisplayIndex];

            // Calculate Offset based on selected page
            //string offset = "";
            Int64 page = Math.Max(0, ltp.page - 1);
            Int64 offset = page * limit;
            //offset = " OFFSET " + o.ToString();
                
            // Get MySQL table
            DataTable dt = TH_Database.Table.Get(ltp.config.Databases_Client, ltp.tablename, limit, offset);

            this.Dispatcher.BeginInvoke(new Action<DataTable>(LoadTable_Finished), Priority, new object[] { dt });
        }

        void LoadTable_Finished(DataTable dt)
        {
            if (dt != null)
            {
                DataTable table = new DataTable();
                table.TableName = dt.TableName;

                // Add Columns to DataView
                foreach (DataColumn column in dt.Columns)
                {
                    DataColumn col = new DataColumn();
                    col.ColumnName = column.ColumnName;
                    col.DataType = column.DataType;
                    table.Columns.Add(col);
                }

                TableDataView = table.AsDataView();

                //Add Rows to Table
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        this.Dispatcher.BeginInvoke(new Action<DataRow>(LoadTable_AddRow), Priority_Background, new object[] { row });
                    }
                }
                else TableDataLoading = false;
            }
            else TableDataLoading = false;

        }

        void LoadTable_AddRow(DataRow row)
        {
            if (TableDataView != null)
            {
                if (TableDataView.Table != null)
                {
                    DataRow newRow = TableDataView.Table.NewRow();
                    newRow.ItemArray = row.ItemArray;

                    LoadingRow = (TableDataView.Table.Rows.Count + 1).ToString();

                    TableDataView.Table.Rows.Add(newRow);

                    if (row.Table.Rows.IndexOf(row) == row.Table.Rows.Count - 1) TableDataLoading = false;
                }
            } 
        }

        #endregion

        private void DropTables_Button_Clicked(TH_WPF.Button bt)
        {
            var result = TH_WPF.MessageBox.Show("Delete Selected Tables?", "Drop Tables", MessageBoxButtons.YesNo);
            if (result == MessageBoxDialogResult.Yes)
            {
                string[] tablenames = SelectedTables.Select(x => x.Text).Distinct().ToArray();

                TH_Database.Table.Drop(SelectedDevice.Databases_Client, tablenames);

                LoadTableList(SelectedDevice);
            }
        }

        #endregion

        #region "Page Numbers"

        Int64 totalRows;
        Int64 selectedPage;
        const int maxPagesDisplayed = 11;

        Int64[] GetPageNumbers()
        {
            int rowLimit = rowLimits[Properties.Settings.Default.RowDisplayIndex];

            Int64 pageCount = Convert.ToInt64(Math.Max(1, Math.Ceiling((double)totalRows / rowLimit)));

            var pages = new List<Int64>();

            int previousPages = Convert.ToInt32(selectedPage - ((maxPagesDisplayed - 1) / 2));
            int nextPages = Convert.ToInt32(selectedPage + ((maxPagesDisplayed - 1) / 2));

            // Calculate First and Last Pages
            Int64 firstPage = Math.Max(1, previousPages);
            Int64 lastPage = Math.Min(pageCount, nextPages);

            if (firstPage > (selectedPage - (maxPagesDisplayed / 2))) lastPage += Math.Abs(previousPages - firstPage);
            if (lastPage < (selectedPage + (maxPagesDisplayed / 2))) firstPage -= Math.Abs(nextPages - lastPage);

            if (pageCount < maxPagesDisplayed)
            {
                firstPage = 1;
                lastPage = pageCount;
            }

            for (var x = firstPage; x <= lastPage; x++)
            {
                pages.Add(x);
            }

            return pages.ToArray();
        }

        void CreatePageNumbers()
        {
            PageNumbers_STACK.Children.Clear();

            Int64[] pages = GetPageNumbers();

            if (pages.Length > 0)
            {
                if (selectedPage > pages[0])
                {
                    PreviousPage_BT.DataObject = selectedPage - 1;
                    PreviousPage_BT.IsEnabled = true;
                }
                else PreviousPage_BT.IsEnabled = false;

                for (var x = 0; x <= pages.Length - 1; x++)
                {
                    Int64 pageNumber = pages[x];
                    var bt = new TH_WPF.Button();
                    bt.Text = pageNumber.ToString();
                    bt.DataObject = pageNumber;
                    if (selectedPage == pageNumber) bt.IsSelected = true;
                    bt.Clicked += PageButton_Clicked;
                    PageNumbers_STACK.Children.Add(bt);
                }

                if (selectedPage < pages[pages.Length - 1])
                {
                    NextPage_BT.DataObject = selectedPage + 1;
                    NextPage_BT.IsEnabled = true;
                }
                else NextPage_BT.IsEnabled = false;
            }
        }

        void PageButton_Clicked(TH_WPF.Button bt)
        {
            var selectedPageNumber = (Int64)bt.DataObject;

            LoadTable(SelectedTableName, selectedPageNumber, SelectedDevice);
            selectedPage = selectedPageNumber;
            CreatePageNumbers();
        }

        #endregion

        #region "Table Row Limit"

        ObservableCollection<string> rowlimits;
        public ObservableCollection<string> RowLimits
        {
            get
            {
                if (rowlimits == null)
                {
                    rowlimits = new ObservableCollection<string>();
                    foreach (int limit in rowLimits) rowlimits.Add(limit.ToString());
                }
                return rowlimits;
            }

            set
            {
                rowlimits = value;
            }
        }

        int[] rowLimits = new int[] { 50, 100, 500, 1000, 5000 };

        public string RowDisplayLimit
        {
            get { return (string)GetValue(RowDisplayLimitProperty); }
            set { SetValue(RowDisplayLimitProperty, value); }
        }

        public static readonly DependencyProperty RowDisplayLimitProperty =
            DependencyProperty.Register("RowDisplayLimit", typeof(string), typeof(Plugin), new PropertyMetadata(null));

        int rowLimitIndex = 0;

        private void RowDisplayLimit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType() == typeof(ComboBox))
            {
                ComboBox cb = (ComboBox)sender;
                rowLimitIndex = cb.SelectedIndex;
                SaveTableRowLimit(rowLimitIndex);

                if (selectedTableParameters != null) selectedTableParameters.page = 1;
                selectedPage = 1;

                LoadTable();
                CreatePageNumbers();
            } 
        }

        void LoadTableRowLimit()
        {
            int index = Properties.Settings.Default.RowDisplayIndex;
            if (index < RowLimit_COMBO.Items.Count)
            {
                RowLimit_COMBO.SelectedItem = RowLimit_COMBO.Items[index];
            }          
        }

        void SaveTableRowLimit(int limit)
        {
            Properties.Settings.Default.RowDisplayIndex = limit;
            Properties.Settings.Default.Save();
        }
         
        #endregion

        private void TableList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TableListShown = false;

            // Table Info
            TableInfoShown = false;
            SelectedTableName = null;
            TableRowCount = null;
            TableIsSelected = false;

            // Table Data
            TableDataView = null;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dg = (DataGrid)sender;

            SelectedRowCount = dg.SelectedItems.Count.ToString() + " Rows Selected";
        }

    }

}
