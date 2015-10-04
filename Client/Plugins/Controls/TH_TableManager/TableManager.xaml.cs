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
using TH_Device_Client;
using TH_Global;
using TH_MySQL;
using TH_PlugIns_Client_Control;
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

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/tablemanager-appinfo.txt"; } }

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

        public void Update(ReturnData rd) {  }

        public void Closing() {  }

        //public void Show() { if (ShowRequested != null) ShowRequested(); }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {

        }

        public event DataEvent_Handler DataEvent;

        public event PlugInTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Device Properties"

        private List<Device_Client> lDevices;
        public List<Device_Client> Devices 
        {
            get { return lDevices; }
            set
            {
                lDevices = value;

                DeviceList.Clear();

                foreach (Device_Client device in lDevices)
                {
                    ListButton lb = new ListButton();
                    lb.Text = device.configuration.Description.Description;
                    lb.Selected += lb_Device_Selected;
                    lb.DataObject = device;
                    DeviceList.Add(lb);
                }
            }
        }

        private int lSelectedDeviceIndex;
        public int SelectedDeviceIndex { get; set; }

        #endregion

        #region "Options"

        public OptionsPage Options { get; set; }

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

            if (TableList_WORKER != null) TableList_WORKER.Abort();

            TableList_WORKER = new Thread(new ParameterizedThreadStart(LoadTableList_Worker));
            TableList_WORKER.Start(config);
        }

        void LoadTableList_Worker(object o)
        {
            Configuration config = (Configuration)o;

            string[] tableNames = Global.Table_List(config.SQL);

            this.Dispatcher.BeginInvoke(new Action<string[]>(LoadTableList_Finished), Priority, new object[] { tableNames });
        }

        void LoadTableList_Finished(string[] tableNames)
        {
            TableList.Clear();

            foreach (string tableName in tableNames)
            {
                TH_WPF.ListButton lb = new TH_WPF.ListButton();
                lb.Text = tableName;
                lb.Selected += lb_Table_Selected;
                this.Dispatcher.BeginInvoke(new Action<ListButton>(LoadTableList_AddItem), Priority, new object[] { lb });
            }

            if (tableNames.Length > 0) TableListShown = true;
            else TableListShown = false;
        }

        void LoadTableList_AddItem(ListButton lb)
        {
            TableList.Add(lb);
        }

        void lb_Table_Selected(TH_WPF.ListButton LB)
        {
            foreach (TH_WPF.ListButton olb in TableList.OfType<TH_WPF.ListButton>()) if (olb != LB) olb.IsSelected = false;
            LB.IsSelected = true;

            Int64 page = 0;

            TableInfoShown = false;
            SelectedTableName = null;
            TableRowCount = null;

            this.Dispatcher.BeginInvoke(new Action<string, Configuration>(LoadInfo), Priority, new object[] { LB.Text, selectedDevice.configuration });
            this.Dispatcher.BeginInvoke(new Action<string, Int64, Configuration>(LoadTable), Priority, new object[] { LB.Text, page, selectedDevice.configuration });
        }

        #endregion

        const System.Windows.Threading.DispatcherPriority Priority = System.Windows.Threading.DispatcherPriority.ContextIdle;

        Device_Client selectedDevice = null;

        void lb_Device_Selected(TH_WPF.ListButton lb)
        {
            foreach (TH_WPF.ListButton olb in DeviceList.OfType<TH_WPF.ListButton>()) if (olb != lb) olb.IsSelected = false;
            lb.IsSelected = true;

            
            Device_Client device = (Device_Client)lb.DataObject;

            selectedDevice = device;

            LoadTableList(device.configuration);
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

            Int64 rowCount = Global.Table_RowCount(ltp.config.SQL, ltp.tablename);

            Int64 tablesize = Global.Table_Size(ltp.config.SQL, ltp.tablename);

            this.Dispatcher.BeginInvoke(new Action<Int64, Int64>(LoadInfo_Finished), Priority, new object[] { rowCount, tablesize });
        }

        void LoadInfo_Finished(Int64 rowCount, Int64 tablesize)
        {
            TableInfoShown = true;

            RowCount = rowCount;
            TableRowCount = rowCount.ToString("n0");
            TableSize = Formatting.SizeSuffix(tablesize);

            CreatePageNumbers(rowCount, 1);
        }

        private void TableInfoRefresh_Button_Clicked(Controls.Button bt)
        {
            LoadInfo(SelectedTableName, selectedDevice.configuration);
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
            LoadTable(SelectedTableName, (Int64)bt.data, selectedDevice.configuration);
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


        Thread Table_WORKER;

        void LoadTable(string tableName, Int64 page, Configuration config)
        {
            TableDataView = null;

            TableDataLoading = true;

            LoadTableParameters ltp = new LoadTableParameters();
            ltp.tablename = tableName;
            ltp.page = page;
            ltp.config = config;

            if (Table_WORKER != null) Table_WORKER.Abort();

            Table_WORKER = new Thread(new ParameterizedThreadStart(LoadTable_Worker));
            Table_WORKER.Start(ltp);
        }

        void LoadTable_Worker(object loadTableParameters)
        {
            LoadTableParameters ltp = (LoadTableParameters)loadTableParameters;

            // Row Limit
            int limit = 100;

            // Calculate Offset based on selected page
            string offset = "";
            if (ltp.page > 1)
            {
                Int64 o = ltp.page * limit;
                offset = " OFFSET " + o.ToString();
            }
                
            // Get MySQL table
            DataTable dt = Global.Table_Get(ltp.config.SQL, ltp.tablename, "LIMIT " + limit.ToString() + offset);

            this.Dispatcher.BeginInvoke(new Action<DataTable>(LoadTable_Finished), Priority, new object[] { dt });
        }

        void LoadTable_Finished(DataTable dt)
        {
            TableDataLoading = false;

            if (dt != null)
            {
                TableDataView = dt.AsDataView();
            }
        }

        #endregion

        private void DropTables_Button_Clicked(Controls.Button bt)
        {

            if (MessageBox.Show("Delete ALL tables in " + selectedDevice.configuration.DataBaseName + "?", "Delete Tables", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string[] tablenames = Global.Table_List(selectedDevice.configuration.SQL);

                Global.Table_Drop(selectedDevice.configuration.SQL, tablenames);
            }

        }

        #endregion

        private void TableList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TableListShown = false;

            // Table Info
            TableInfoShown = false;
            SelectedTableName = null;
            TableRowCount = null;

            // Table Data
            TableDataView = null;
        }


    }

}
