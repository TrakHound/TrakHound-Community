using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using TrakHound.Configurations;
using TrakHound.DataManagement;
using TrakHound.Tables;
using TrakHound.Plugins;
using TrakHound.Tools;
using TrakHound_Dashboard.Pages.Cycles.Controls;
using TrakHound_UI;

namespace TrakHound_Dashboard.Pages.Cycles
{
    public partial class Plugin : UserControl
    {
        public Plugin()
        {
            InitializeComponent();
            DataContext = this;
        }

        public DeviceConfiguration Configuration { get; set; }

        private ObservableCollection<ListButton> _deviceList;
        public ObservableCollection<ListButton> DeviceList
        {
            get
            {
                if (_deviceList == null)
                    _deviceList = new ObservableCollection<ListButton>();
                return _deviceList;
            }

            set
            {
                _deviceList = value;
            }
        }

        private ObservableCollection<CycleRowInfo> _cycleRowInfos;
        public ObservableCollection<CycleRowInfo> CycleRowInfos
        {
            get
            {
                if (_cycleRowInfos == null)
                {
                    _cycleRowInfos = new ObservableCollection<CycleRowInfo>();
                }

                return _cycleRowInfos;
            }

            set
            {
                _cycleRowInfos = value;
            }
        }

        void Update(EventData data)
        {

        }

        #region "Date Selection"

        private List<DateTime> _selectedDates;
        public List<DateTime> SelectedDates
        {
            get
            {
                if (_selectedDates == null)
                {
                    _selectedDates = new List<DateTime>();
                }

                return _selectedDates;
            }

            set
            {
                _selectedDates = value;

                Load(Configuration, _selectedDates);
            }
        }

        private void Today_Clicked(TrakHound_UI.Button bt)
        {
            var dates = new List<DateTime>();
            dates.Add(DateTime.Now);
            SelectedDates = dates;
        }

        #endregion

        private void Load(DeviceConfiguration config, List<DateTime> dates)
        {
            if (config != null)
            {
                DataTable table = GetCyclesTable(config, dates);
                if (table != null)
                {
                    string[] cycleIds = DataTable_Functions.GetDistinctValues(table, "cycle_id");
                    if (cycleIds != null)
                    {
                        CycleRowInfos.Clear();

                        foreach (var cycleId in cycleIds)
                        {
                            var cycleInfo = CycleRowInfo.FromDataTable(cycleId, table);
                            if (cycleInfo != null) CycleRowInfos.Add(cycleInfo);
                        }
                    }
                }
            }
        }

        private static DataTable GetCyclesTable(DeviceConfiguration config, List<DateTime> dates)
        {
            DataTable result = null;

            if (dates != null && dates.Count > 0)
            {
                DateTime start = new DateTime(dates[0].Year, dates[0].Month, dates[0].Day, 0, 0, 0);
                DateTime end = new DateTime(dates[0].Year, dates[0].Month, dates[0].Day, 23, 59, 59);

                string filter = "WHERE NAME != 'UNAVAILABLE' AND START_TIME >= '" + start.ToString("yyyy-MM-dd HH:mm:ss") + "' AND STOP_TIME < '" + end.ToString("yyyy-MM-dd HH:mm:ss") + "'";

                result = Table.Get(Database.Configuration, Table.GetName(Names.Cycles, config.DatabaseId), filter);
                //result = Table.Get(config.Databases_Client, Global.GetTableName(TableNames.Cycles, config.DatabaseId), filter);
            }

            return result;
        }




        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;
            while (!(obj is DataGridRow) && obj != null) obj = VisualTreeHelper.GetParent(obj);
            if (obj is DataGridRow)
            {
                var row = obj as DataGridRow;

                if (row.DetailsVisibility == Visibility.Visible)
                {
                    row.DetailsVisibility = Visibility.Collapsed;
                }
                else
                {
                    row.DetailsVisibility = Visibility.Visible;
                }
            }
        }

        public static FrameworkElement GetTemplateChildByName(DependencyObject parent, string name)
        {
            int childnum = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childnum; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement && ((FrameworkElement)child).Name == name)
                {
                    return child as FrameworkElement;
                }
                else
                {
                    var s = GetTemplateChildByName(child, name);
                    if (s != null)
                        return s;
                }
            }
            return null;
        }

        private void dataGrid1_RowDetailsVisibilityChanged(object sender, DataGridRowDetailsEventArgs e)
        {
            //DataGridRow row = e.Row as DataGridRow;
            //FrameworkElement tb = GetTemplateChildByName(row, "RowHeaderToggleButton");
            //if (tb != null)
            //{
            //    if (row.DetailsVisibility == System.Windows.Visibility.Visible)
            //    {
            //        (tb as ToggleButton).IsChecked = true;
            //    }
            //    else
            //    {
            //        (tb as ToggleButton).IsChecked = false;
            //    }
            //}

        }

    }









    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    //public partial class Plugin : UserControl
    //{
    //    public Plugin()
    //    {
    //        InitializeComponent();
    //        DataContext = this;
    //    }

    //    ObservableCollection<ListButton> _deviceList;
    //    public ObservableCollection<ListButton> DeviceList
    //    {
    //        get
    //        {
    //            if (_deviceList == null)
    //                _deviceList = new ObservableCollection<ListButton>();
    //            return _deviceList;
    //        }

    //        set
    //        {
    //            _deviceList = value;
    //        }
    //    }

    //    void Update(EventData data)
    //    {
    //        //if (data != null)
    //        //{
    //        //    switch (data.Id.ToLower())
    //        //    {
    //        //        case "history_deviceselected":

    //        //            var config = data.Data01 as DeviceConfiguration;
    //        //            if (config != null)
    //        //            {
    //        //                configuration = config;

    //        //                // Assign list of Shift Segments to use for rest of functions
    //        //                shiftSegments = ReadShiftSegments(config);
    //        //            }

    //        //            break;


    //        //        case "history_datesselected":

    //        //            var dates = data.Data01 as List<DateTime>;
    //        //            if (dates != null)
    //        //            {
    //        //                SelectedDates = dates;

    //        //                //LoadData();
    //        //            }

    //        //            break;
    //        //    }
    //        //}
    //    }

    //    DeviceConfiguration configuration;

    //    public void Initialize(DeviceConfiguration config)
    //    {
    //        configuration = config;

    //        // Assign list of Shift Segments to use for rest of functions
    //        shiftSegments = ReadShiftSegments(config);
    //    }

    //    List<DateTime> selectedDates;
    //    public List<DateTime> SelectedDates
    //    {
    //        get
    //        {
    //            if (selectedDates == null)
    //            {
    //                selectedDates = new List<DateTime>();
    //            }

    //            return selectedDates;
    //        }

    //        set
    //        {
    //            selectedDates = value;

    //            LoadShiftNames();

    //            LoadCycles();
    //        }
    //    }


    //    ObservableCollection<FilterCheckBox> shiftNames;
    //    public ObservableCollection<FilterCheckBox> ShiftNames
    //    {
    //        get
    //        {
    //            if (shiftNames == null)
    //            {
    //                shiftNames = new ObservableCollection<FilterCheckBox>();
    //            }

    //            return shiftNames;
    //        }

    //        set
    //        {
    //            shiftNames = value;
    //        }
    //    }


    //    public object CycleData
    //    {
    //        get { return (object)GetValue(CycleDataProperty); }
    //        set { SetValue(CycleDataProperty, value); }
    //    }

    //    public static readonly DependencyProperty CycleDataProperty =
    //        DependencyProperty.Register("CycleData", typeof(object), typeof(Plugin), new PropertyMetadata(null));


    //    ObservableCollection<object> cycleNames;
    //    public ObservableCollection<object> CycleNames
    //    {
    //        get
    //        {
    //            if (cycleNames == null)
    //            {
    //                cycleNames = new ObservableCollection<object>();
    //            }

    //            return cycleNames;
    //        }

    //        set
    //        {
    //            cycleNames = value;
    //        }
    //    }

    //    ObservableCollection<CycleRowInfo> cycleRowInfos;
    //    public ObservableCollection<CycleRowInfo> CycleRowInfos
    //    {
    //        get
    //        {
    //            if (cycleRowInfos == null)
    //            {
    //                cycleRowInfos = new ObservableCollection<CycleRowInfo>();
    //            }

    //            return cycleRowInfos;
    //        }

    //        set
    //        {
    //            cycleRowInfos = value;
    //        }
    //    }


    //    public ListCollectionView CycleRowInfosView
    //    {
    //        get { return (ListCollectionView)GetValue(CycleRowInfosViewProperty); }
    //        set { SetValue(CycleRowInfosViewProperty, value); }
    //    }

    //    public static readonly DependencyProperty CycleRowInfosViewProperty =
    //        DependencyProperty.Register("CycleRowInfosView", typeof(ListCollectionView), typeof(Plugin), new PropertyMetadata(null));

    //    public void LoadCycles()
    //    {
    //        CycleRowInfos.Clear();

    //        List<string> shiftsToLoad = new List<string>();

    //        foreach (var shiftNameChk in ShiftNames)
    //        {
    //            if (shiftNameChk.IsChecked == true)
    //            {
    //                shiftsToLoad.Add(shiftNameChk.Content.ToString());
    //            }
    //        }

    //        AddCycleNames(SelectedDates, shiftsToLoad);
    //    }

    //    void AddCycles()
    //    {




    //    }

    //    #region "Cycle Names"

    //    void LoadCycleNames(string shiftName)
    //    {
    //        //CycleNames.Clear();

    //        //AddCycleNames(SelectedDates, shiftName);

    //        //foreach (DateTime date in SelectedDates)
    //        //{
    //        //    AddCycleNames(date, shiftName);
    //        //}
    //    }

    //    public void AddCycleNames(List<DateTime> dates, List<string> shiftNames)
    //    {
    //        CycleNames.Clear();

    //        if (configuration != null)
    //        {
    //            DataTable dt = GetCyclesTable(dates, shiftNames);

    //            if (dt != null)
    //            {
    //                var dv = dt.AsDataView();
    //                var distinctNames = dv.ToTable(true, "name");

    //                foreach (DataRow nameRow in distinctNames.Rows)
    //                {
    //                    string cycleName = nameRow[0].ToString();

    //                    //var data = new CycleData();

    //                    //// Set Cycle Name Title
    //                    //data.CycleName = cycleName;

    //                    var infos = new List<CycleRowInfo>();

    //                    // Get all of the Distinct Cycle Ids
    //                    dv = dt.AsDataView();
    //                    dv.RowFilter = "name='" + cycleName + "'";
    //                    var distinctCycles = dv.ToTable(true, "cycle_id");

    //                    foreach (DataRow cycleRow in distinctCycles.Rows)
    //                    {
    //                        string cycleId = cycleRow[0].ToString();

    //                        var info = CycleRowInfo.FromDataTable(cycleId, dt);
    //                        if (info != null)
    //                        {
    //                            infos.Add(info);
    //                        }
    //                    }

    //                    var chk = new FilterCheckBox();
    //                    chk.Content = cycleName;
    //                    chk.DataObject = infos;

    //                    chk.Checked += CycleName_Checked;
    //                    chk.Unchecked += CycleName_Unchecked;
    //                    chk.IsChecked = true;

    //                    CycleNames.Add(chk);
    //                }
    //            }
    //        }
    //    }

    //    void CycleName_Checked(object sender, RoutedEventArgs e)
    //    {
    //        var chk = (FilterCheckBox)sender;

    //        var o = chk.DataObject;
    //        if (o != null)
    //        {
    //            var infos = (List<CycleRowInfo>)o;

    //            foreach (var info in infos)
    //            {
    //                CycleRowInfos.Add(info);
    //            }
    //        }
    //    }

    //    void CycleName_Unchecked(object sender, RoutedEventArgs e)
    //    {
    //        var chk = (FilterCheckBox)sender;

    //        var o = chk.DataObject;
    //        if (o != null)
    //        {
    //            var infos = (List<CycleRowInfo>)o;

    //            foreach (var info in infos)
    //            {
    //                int index = CycleRowInfos.ToList().FindIndex(x => x.CycleId == info.CycleId && x.CycleInstanceId == info.CycleInstanceId);
    //                if (index >= 0) CycleRowInfos.RemoveAt(index);
    //            }
    //        }
    //    }

    //    #endregion





    //    //void ShiftName_Selected(ListButton LB)
    //    //{
    //    //    CycleNames.Clear();

    //    //    foreach (var date in SelectedDates)
    //    //    {
    //    //        AddCycles(date, LB.DataObject.ToString());
    //    //    }
    //    //}

    //    private static string GetTableName(string tablename, string id)
    //    {
    //        if (!String.IsNullOrEmpty(id)) return id + "_" + tablename;
    //        return tablename;
    //    }

    //    #region "Shift Names"

    //    void LoadShiftNames()
    //    {
    //        ShiftNames.Clear();

    //        var shiftNames = shiftSegments.Select(x => x.Shift).Distinct();
    //        if (shiftNames != null)
    //        {
    //            foreach (var shiftName in shiftNames)
    //            {
    //                var chk = new FilterCheckBox();
    //                chk.Content = shiftName;
    //                chk.Checked += ShiftName_Checked;
    //                chk.Unchecked += ShiftName_Unchecked;
    //                chk.IsChecked = true;
    //                ShiftNames.Add(chk);
    //            }
    //        }
    //    }

    //    void ShiftName_Checked(object sender, RoutedEventArgs e)
    //    {
    //        var chk = (CheckBox)sender;

    //        string shiftName = chk.Content.ToString();

    //        LoadCycleNames(shiftName);
    //    }

    //    void ShiftName_Unchecked(object sender, RoutedEventArgs e)
    //    {

    //    }

    //    #endregion

    //    #region "Shift Segments"

    //    class ShiftSegment
    //    {
    //        public string Shift { get; set; }
    //        public int ShiftId { get; set; }
    //        public int SegmentId { get; set; }
    //        public string Type { get; set; }

    //        public DateTime StartTime { get; set; }
    //        public DateTime EndTime { get; set; }

    //        public DateTime StartTimeUtc { get; set; }
    //        public DateTime EndTimeUtc { get; set; }
    //    }

    //    List<ShiftSegment> shiftSegments = new List<ShiftSegment>();

    //    static List<ShiftSegment> ReadShiftSegments(DeviceConfiguration config)
    //    {
    //        List<ShiftSegment> result = new List<ShiftSegment>();

    //        DataTable dt = Table.Get(config.Databases_Client, GetTableName(TableNames.ShiftSegments, config.DatabaseId));
    //        if (dt != null)
    //        {
    //            foreach (DataRow row in dt.Rows)
    //            {
    //                string shift = DataTable_Functions.GetRowValue("shift", row);

    //                int shiftId = -1;
    //                int.TryParse(DataTable_Functions.GetRowValue("shift_id", row), out shiftId);

    //                int segmentId = -1;
    //                int.TryParse(DataTable_Functions.GetRowValue("segment_id", row), out segmentId);

    //                string type = DataTable_Functions.GetRowValue("type", row);
    //                DateTime startTime = DateTime_Functions.Parse(DataTable_Functions.GetRowValue("start", row));
    //                DateTime endTime = DateTime_Functions.Parse(DataTable_Functions.GetRowValue("end", row));
    //                DateTime startTimeUtc = DateTime_Functions.Parse(DataTable_Functions.GetRowValue("start_utc", row));
    //                DateTime endTimeUtc = DateTime_Functions.Parse(DataTable_Functions.GetRowValue("end_utc", row));

    //                if (shift != null &&
    //                    shiftId >= 0 &&
    //                    segmentId >= 0)
    //                {
    //                    var segment = new ShiftSegment();
    //                    segment.Shift = shift;
    //                    segment.ShiftId = shiftId;
    //                    segment.SegmentId = segmentId;
    //                    segment.Type = type;
    //                    segment.StartTime = startTime;
    //                    segment.EndTime = endTime;
    //                    segment.StartTimeUtc = startTimeUtc;
    //                    segment.EndTimeUtc = endTimeUtc;
    //                    result.Add(segment);
    //                }
    //            }
    //        }

    //        return result;
    //    }

    //    List<ShiftSegment> GetShiftSegments(string shift)
    //    {
    //        if (shift != null) return shiftSegments.FindAll(x => x.Shift == shift);
    //        else return null;
    //    }

    //    #endregion

    //    void bt_Selected(ListButton LB)
    //    {
    //        CycleData = LB.DataObject;
    //    }


    //    DataTable GetCyclesTable(List<DateTime> dates, List<string> shiftNames)
    //    {
    //        DataTable result = null;

    //        if (shiftNames != null && dates != null && shiftSegments != null)
    //        {
    //            string query = CreateQuery_Where(dates, shiftSegments);
    //            if (query != null)
    //            {
    //                result = Table.Get(configuration.Databases_Client, GetTableName(TableNames.Cycles, configuration.DatabaseId), query);
    //            }
    //        }

    //        return result;
    //    }

    //    static string CreateQuery_Where(List<DateTime> dates, List<ShiftSegment> segments)
    //    {
    //        string result = null;

    //        if (segments != null)
    //        {
    //            result = "WHERE ";

    //            for (var d = 0; d <= dates.Count - 1; d++)
    //            {
    //                var date = dates[d];
    //                for (var x = 0; x <= segments.Count - 1; x++)
    //                {
    //                    var segment = segments[x];
    //                    result += "shift_id='" + GetShiftId(date, segment.ShiftId, segment.SegmentId) + "'";
    //                    if (x < segments.Count - 1) result += " OR ";
    //                }
    //                if (d < dates.Count - 1) result += " OR ";
    //            }
    //        }

    //        return result;
    //    }

    //    static string GetShiftId(DateTime date, int shiftId, int segmentId)
    //    {
    //        return date.ToString("yyyyMMdd") + "_" + shiftId.ToString("00") + "_" + segmentId.ToString("00");
    //    }

    //    private void ToggleButton_Click(object sender, RoutedEventArgs e)
    //    {
    //        DependencyObject obj = (DependencyObject)e.OriginalSource;
    //        while (!(obj is DataGridRow) && obj != null) obj = VisualTreeHelper.GetParent(obj);
    //        if (obj is DataGridRow)
    //        {
    //            var row = obj as DataGridRow;

    //            if (row.DetailsVisibility == Visibility.Visible)
    //            {
    //                row.DetailsVisibility = Visibility.Collapsed;
    //            }
    //            else
    //            {
    //                row.DetailsVisibility = Visibility.Visible;
    //            }
    //        }
    //    }

    //    public static FrameworkElement GetTemplateChildByName(DependencyObject parent, string name)
    //    {
    //        int childnum = VisualTreeHelper.GetChildrenCount(parent);
    //        for (int i = 0; i < childnum; i++)
    //        {
    //            var child = VisualTreeHelper.GetChild(parent, i);
    //            if (child is FrameworkElement && ((FrameworkElement)child).Name == name)
    //            {
    //                return child as FrameworkElement;
    //            }
    //            else
    //            {
    //                var s = GetTemplateChildByName(child, name);
    //                if (s != null)
    //                    return s;
    //            }
    //        }
    //        return null;
    //    }

    //    private void dataGrid1_RowDetailsVisibilityChanged(object sender, DataGridRowDetailsEventArgs e)
    //    {
    //        //DataGridRow row = e.Row as DataGridRow;
    //        //FrameworkElement tb = GetTemplateChildByName(row, "RowHeaderToggleButton");
    //        //if (tb != null)
    //        //{
    //        //    if (row.DetailsVisibility == System.Windows.Visibility.Visible)
    //        //    {
    //        //        (tb as ToggleButton).IsChecked = true;
    //        //    }
    //        //    else
    //        //    {
    //        //        (tb as ToggleButton).IsChecked = false;
    //        //    }
    //        //}

    //    }  
    //}
}
