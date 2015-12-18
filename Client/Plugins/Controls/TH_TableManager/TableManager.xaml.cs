// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

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
using TH_PlugIns_Client_Control;
using TH_UserManagement.Management;
using TH_WPF;

using TH_TableManager.Controls;

namespace TH_TableManager
{
    /// <summary>
    /// Interaction logic for TableManager.xaml
    /// </summary>
    public partial class TableManager : UserControl, Control_PlugIn
    {
        public TableManager()
        {
            InitializeComponent();
            DataContext = this;

            DatabasePluginReader dpr = new DatabasePluginReader();
        }

        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Table Manager"; } }

        public string Description { get { return "Display and Manage MySQL Tables associated with Device"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_TableManager;component/Resources/Table_01.png")); ; } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return new BitmapImage(new Uri("pack://application:,,,/TH_TableManager;component/Resources/TrakHound_Logo_10_200px.png")); } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\License\" + "License.txt"); } }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/tablemanager-appinfo.json"; } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return null; } }
        public string DefaultParentCategory { get { return null; } }

        public bool AcceptsPlugIns { get { return true; } }

        public bool OpenOnStartUp { get { return false; } }

        public bool ShowInAppMenu { get { return true; } }

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Methods"

        public void Initialize() { }

        public void Closing() {  }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {

        }

        public event DataEvent_Handler DataEvent;

        public event PlugInTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Device Properties"

        ObservableCollection<Configuration> Devices = new ObservableCollection<Configuration>();

        public void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine("DeviceCompare :: Devices :: " + e.Action.ToString());

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                Devices.Clear();
                DeviceList.Clear();
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                DeviceList.Clear();
            }

            if (e.NewItems != null)
            {
                foreach (Configuration newConfig in e.NewItems)
                {
                    Devices.Add(newConfig);

                    if (newConfig != null)
                    {
                        // Initialize Database Configurations
                        Global.Initialize(newConfig.Databases_Client);

                        Controls.DeviceButton db = new DeviceButton();
                        db.Description = newConfig.Description.Description;
                        db.Manufacturer = newConfig.Description.Manufacturer;
                        db.Model = newConfig.Description.Model;
                        db.Serial = newConfig.Description.Serial;
                        db.Id = newConfig.Description.Machine_ID;

                        db.Clicked += db_Clicked;

                        ListButton lb = new ListButton();
                        lb.ButtonContent = db;
                        lb.ShowImage = false;
                        lb.Selected += lb_Device_Selected;
                        lb.DataObject = newConfig;

                        db.Parent = lb;

                        DeviceList.Add(lb);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (Configuration oldConfig in e.OldItems)
                {
                    Devices.Add(oldConfig);
                }
            }
        }


        //private List<Device_Client> lDevices;
        //public List<Device_Client> Devices 
        //{
        //    get { return lDevices; }
        //    set
        //    {
        //        lDevices = value;

        //        DeviceList.Clear();

        //        foreach (Device_Client device in lDevices)
        //        {

        //            // Initialize Database Configurations
        //            Global.Initialize(device.configuration.Databases);

        //            Controls.DeviceButton db = new DeviceButton();
        //            db.Description = device.configuration.Description.Description;
        //            db.Manufacturer = device.configuration.Description.Manufacturer;
        //            db.Model = device.configuration.Description.Model;
        //            db.Serial = device.configuration.Description.Serial;
        //            db.Id = device.configuration.Description.Machine_ID;

        //            db.Clicked += db_Clicked;

        //            ListButton lb = new ListButton();
        //            lb.ButtonContent = db;
        //            lb.ShowImage = false;
        //            lb.Selected += lb_Device_Selected;
        //            lb.DataObject = device;

        //            db.Parent = lb;

        //            DeviceList.Add(lb);
        //        }
        //    }
        //}

        void db_Clicked(DeviceButton bt)
        {
            if (bt.Parent != null)
            {
                if (bt.Parent.GetType() == typeof(ListButton))
                {
                    ListButton lb = (ListButton)bt.Parent;

                    lb_Device_Selected(lb);
                }
            }
        }

        #endregion

        #region "Options"

        public OptionsPage Options { get; set; }

        #endregion

        #region "User"

        public UserConfiguration CurrentUser { get; set; }

        public Database_Settings UserDatabaseSettings { get; set; }

        #endregion

        public object RootParent { get; set; }

        #endregion

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
            DependencyProperty.Register("TableIsSelected", typeof(bool), typeof(TableManager), new PropertyMetadata(false));

       
        public bool TableListShown
        {
            get { return (bool)GetValue(TableListShownProperty); }
            set { SetValue(TableListShownProperty, value); }
        }

        public static readonly DependencyProperty TableListShownProperty =
            DependencyProperty.Register("TableListShown", typeof(bool), typeof(TableManager), new PropertyMetadata(false));

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
            //ProcessSelectedTables();

            foreach (TH_WPF.ListButton olb in TableList.OfType<TH_WPF.ListButton>()) if (olb != LB) olb.IsSelected = false;
            LB.IsSelected = true;

            Int64 page = 0;

            TableIsSelected = true;
            TableInfoShown = false;
            SelectedTableName = null;
            TableRowCount = null;

            this.Dispatcher.BeginInvoke(new Action<string, Configuration>(LoadInfo), Priority, new object[] { LB.Text, selectedDevice });
            this.Dispatcher.BeginInvoke(new Action<string, Int64, Configuration>(LoadTable), Priority, new object[] { LB.Text, page, selectedDevice });
        }

        #endregion

        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority = System.Windows.Threading.DispatcherPriority.ContextIdle;

        //Device_Client selectedDevice = null;

        Configuration selectedDevice = null;

        void lb_Device_Selected(TH_WPF.ListButton lb)
        {
            foreach (TH_WPF.ListButton olb in DeviceList.OfType<TH_WPF.ListButton>()) if (olb != lb) olb.IsSelected = false;
            lb.IsSelected = true;
        
            //Device_Client device = (Device_Client)lb.DataObject;

            Configuration device = (Configuration)lb.DataObject;

            selectedDevice = device;

            LoadTableList(device);

            

            //LoadTableList(device.configuration);
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
            DependencyProperty.Register("TableInfoShown", typeof(bool), typeof(TableManager), new PropertyMetadata(false));

        public string SelectedTableName
        {
            get { return (string)GetValue(SelectedTableNameProperty); }
            set { SetValue(SelectedTableNameProperty, value); }
        }

        public static readonly DependencyProperty SelectedTableNameProperty =
            DependencyProperty.Register("SelectedTableName", typeof(string), typeof(TableManager), new PropertyMetadata(null));

        
        public string TableRowCount
        {
            get { return (string)GetValue(TableRowCountProperty); }
            set { SetValue(TableRowCountProperty, value); }
        }

        public static readonly DependencyProperty TableRowCountProperty =
            DependencyProperty.Register("TableRowCount", typeof(string), typeof(TableManager), new PropertyMetadata(null));


        public string TableSize
        {
            get { return (string)GetValue(TableSizeProperty); }
            set { SetValue(TableSizeProperty, value); }
        }

        public static readonly DependencyProperty TableSizeProperty =
            DependencyProperty.Register("TableSize", typeof(string), typeof(TableManager), new PropertyMetadata(null));
     

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
            TableSize = Formatting.SizeSuffix(tablesize);

            RowDisplayLimit = Math.Min(RowCount, rowLimits[rowLimitIndex]).ToString();

            LoadingRowDisplay = true;

            CreatePageNumbers(rowCount, 1);
        }

        private void TableInfoRefresh_Button_Clicked(TH_WPF.Button_02 bt)
        {
            LoadInfo(SelectedTableName, selectedDevice);
        }

        Int64 RowCount;

        void CreatePageNumbers(Int64 rowCount, Int64 selectedPage)
        {
            PageNumbers_STACK.Children.Clear();

            Int64 pageCount = rowCount / 100;

            if (pageCount > 0)
            {
                int firstPage = 1;
                Int64 lastPage = pageCount + 1;

                int maxpages = 5; // should be an odd number
                Int64[] pageRange = new Int64[maxpages];

                //Create pageRange array
                if (selectedPage < maxpages) // ex. 1,2,3,4,5...100
                {
                    for (int i = 0; i <= pageRange.Length - 1; i++)
                        pageRange[i] = i + 1;
                }
                else if (selectedPage > (lastPage - maxpages)) // ex. 1...96,97,98,99,100
                {
                    for (int i = 0; i <= pageRange.Length - 1; i++)
                        pageRange[i] = lastPage - i;
                }
                else // ex. 1...10,11,12,13,14...100
                {
                    for (int i = 0; i <= pageRange.Length - 1; i++)
                    {
                        double midpoint = (double)maxpages / 2;
                        double mid = Math.Round(midpoint, 0, MidpointRounding.AwayFromZero);
                        Int64 start = selectedPage - (Int64)mid + 1;
                        pageRange[i] = start + i;
                    }
                }

                Int64 firstRangePage = pageRange[0];
                Int64 lastRangePage = pageRange[pageRange.Length - 1];


                // Create Page Number Buttons

                // Previous Page Button
                Controls.Button prev_bt = new Controls.Button();
                prev_bt.Text = "<";
                prev_bt.data = selectedPage - 1;
                prev_bt.Clicked += PageButton_Clicked;
                PageNumbers_STACK.Children.Add(prev_bt);

                // First Page Button
                if (selectedPage > maxpages)
                {
                    Controls.Button first_bt = new Controls.Button();
                    first_bt.Text = firstPage.ToString();
                    first_bt.data = firstPage;
                    if (selectedPage == firstPage) first_bt.IsSelected = true;
                    first_bt.Clicked += PageButton_Clicked;
                    PageNumbers_STACK.Children.Add(first_bt);

                    PageNumbers_STACK.Children.Add(CreateDotDotDot());
                }

                // Page Buttons
                for (int x = 0; x <= pageRange.Length - 1; x++)
                {
                    Controls.Button bt = new Controls.Button();
                    bt.Text = pageRange[x].ToString();
                    bt.data = pageRange[x];
                    if (selectedPage == pageRange[x]) bt.IsSelected = true;
                    bt.Clicked += PageButton_Clicked;
                    PageNumbers_STACK.Children.Add(bt);
                }

                // Last Page Button
                if (selectedPage < (lastPage - maxpages))
                {
                    PageNumbers_STACK.Children.Add(CreateDotDotDot());

                    Controls.Button last_bt = new Controls.Button();
                    last_bt.Text = lastPage.ToString();
                    last_bt.data = lastPage;
                    if (selectedPage == lastPage) last_bt.IsSelected = true;
                    last_bt.Clicked += PageButton_Clicked;
                    PageNumbers_STACK.Children.Add(last_bt);
                }

                // Next Page Button
                Controls.Button next_bt = new Controls.Button();
                next_bt.Text = ">";
                next_bt.data = selectedPage + 1;
                next_bt.Clicked += PageButton_Clicked;
                PageNumbers_STACK.Children.Add(next_bt);
            }
            else // only 1 page
            {
                // Single Page Button
                Controls.Button only_bt = new Controls.Button();
                only_bt.Text = "1";
                only_bt.IsSelected = true;
                PageNumbers_STACK.Children.Add(only_bt);
            }
        }

        void PageButton_Clicked(Controls.Button bt)
        {
            LoadTable(SelectedTableName, (Int64)bt.data, selectedDevice);
            CreatePageNumbers(RowCount, (Int64)bt.data);
        }

        TextBlock CreateDotDotDot()
        {
            TextBlock txt = new TextBlock();
            txt.Text = "..";
            txt.FontSize = 14;
            txt.Foreground = new SolidColorBrush(Color_Functions.GetColorFromString("#aaffffff"));
            txt.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            txt.Margin = new Thickness(2, 0, 2, 0);
            return txt;
        }

        #endregion

        #region "Load Table"

        public DataView TableDataView
        {
            get { return (DataView)GetValue(TableDataViewProperty); }
            set { SetValue(TableDataViewProperty, value); }
        }

        public static readonly DependencyProperty TableDataViewProperty =
            DependencyProperty.Register("TableDataView", typeof(DataView), typeof(TableManager), new PropertyMetadata(null));

        public bool TableDataLoading
        {
            get { return (bool)GetValue(TableDataLoadingProperty); }
            set { SetValue(TableDataLoadingProperty, value); }
        }

        public static readonly DependencyProperty TableDataLoadingProperty =
            DependencyProperty.Register("TableDataLoading", typeof(bool), typeof(TableManager), new PropertyMetadata(false));

        public bool LoadingRowDisplay
        {
            get { return (bool)GetValue(LoadingRowDisplayProperty); }
            set { SetValue(LoadingRowDisplayProperty, value); }
        }

        public static readonly DependencyProperty LoadingRowDisplayProperty =
            DependencyProperty.Register("LoadingRowDisplay", typeof(bool), typeof(TableManager), new PropertyMetadata(false));

        public string LoadingRow
        {
            get { return (string)GetValue(LoadingRowProperty); }
            set { SetValue(LoadingRowProperty, value); }
        }

        public static readonly DependencyProperty LoadingRowProperty =
            DependencyProperty.Register("LoadingRow", typeof(string), typeof(TableManager), new PropertyMetadata(null));



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
                //LoadingRowDisplay = false;
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
            int limit = GetTableRowLimit();

            // Calculate Offset based on selected page
            string offset = "";
            if (ltp.page > 1)
            {
                Int64 o = ltp.page * limit;
                offset = " OFFSET " + o.ToString();
            }
                
            // Get MySQL table
            DataTable dt = TH_Database.Table.Get(ltp.config.Databases_Client, ltp.tablename, "LIMIT " + limit.ToString() + offset);

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

        private void DropTables_Button_Clicked(TH_WPF.Button_02 bt)
        {

            if (MessageBox.Show("Delete Selected Tables?", "Drop Tables", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string[] tablenames = SelectedTables.Select(x => x.Text).Distinct().ToArray();

                TH_Database.Table.Drop(selectedDevice.Databases_Client, tablenames);

                LoadTableList(selectedDevice);
            }

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
            DependencyProperty.Register("RowDisplayLimit", typeof(string), typeof(TableManager), new PropertyMetadata(null));

        int rowLimitIndex = 0;

        private void RowDisplayLimit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType() == typeof(ComboBox))
            {
                ComboBox cb = (ComboBox)sender;
                rowLimitIndex = cb.SelectedIndex;
                SaveTableRowLimit(rowLimitIndex);

                LoadTable();
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

        int GetTableRowLimit()
        {
            if (Properties.Settings.Default.RowDisplayIndex == 1) return 100;
            if (Properties.Settings.Default.RowDisplayIndex == 2) return 500;
            if (Properties.Settings.Default.RowDisplayIndex == 3) return 1000;
            if (Properties.Settings.Default.RowDisplayIndex == 4) return 5000;
            else return 50;
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

    }

}
