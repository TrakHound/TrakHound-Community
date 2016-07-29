// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound.Tools;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;

using TrakHound.Server.Plugins.Shifts;

namespace TrakHound_Device_Manager.Pages.Shifts
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, IConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Title { get { return "Shifts"; } }

        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Device-Manager;component/Resources/Timer_01.png"));
                    _image.Freeze();
                }

                return _image;
            }
        }

        public bool Loaded { get; set; }

        public event SettingChanged_Handler SettingChanged;


        public event SendData_Handler SendData;

        public void GetSentData(EventData data)
        {

        }


        #region "Load Configuration"

        public void LoadConfiguration(DataTable dt)
        {
            InitializeHourTimes();

            configurationTable = dt;

            Loading = true;
            DisplayShifts = false;

            ShiftItems.Clear();

            shifts = GetShifts(dt);

            foreach (Shift shift in shifts)
            {
                this.Dispatcher.BeginInvoke(new Action<Shift>(ShiftListItem_Add), priority, new object[] { shift });
            }

            if (shifts.Count > 0) DisplayShifts = true;

            Loading = false;
        }

        public List<Shift> shifts;

        public static List<Shift> GetShifts(DataTable dt)
        {
            var result = new List<Shift>();

            string address = "/Shifts/";
            string oldAddress = "/ShiftData/Shifts/";
            string filter = "address LIKE '" + address + "*' OR address LIKE '" + oldAddress + "*'";
            //string filter = "address LIKE '" + address + "*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();
            temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

            // Get Shifts
            foreach (DataRow row in temp_dt.Rows)
            {
                Shift shift = GetShiftFromRow(result, row);
                if (shift != null) result.Add(shift);
            }

            // Get Breaks
            foreach (Shift shift in result)
            {
                string shiftfilter = "address LIKE '" + address + "Shift||" + shift.id.ToString("00") + "/";
                dv = dt.AsDataView();
                dv.RowFilter = shiftfilter + "*'";
                temp_dt = dv.ToTable();
                temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

                foreach (DataRow row in temp_dt.Rows)
                {
                    Segment b = GetBreakFromRow(shift, row);
                    if (b != null) shift.Breaks.Add(b);
                }
            }

            return result;
        }
        
        static Shift GetShiftFromRow(List<Shift> shifts, DataRow row)
        {
            Shift result = null;

            string adr = row["address"].ToString();

            string lastNode = DataTable_Functions.TrakHound.GetLastNode(row);

            if (lastNode != null)
            {
                if (lastNode.ToLower() == "shift")
                {
                    int eventIndex = adr.IndexOf("Shift");
                    int slashIndex = adr.IndexOf('/', eventIndex) + 1;
                    int separatorIndex = adr.IndexOf("||", slashIndex);

                    if (separatorIndex > slashIndex)
                    {
                        string name = DataTable_Functions.TrakHound.GetRowAttribute("name", row);

                        // Begin Time
                        ShiftTime begintime;
                        ShiftTime.TryParse(DataTable_Functions.TrakHound.GetRowAttribute("begintime", row), out begintime);

                        // End Time
                        ShiftTime endtime;
                        ShiftTime.TryParse(DataTable_Functions.TrakHound.GetRowAttribute("endtime", row), out endtime);

                        string strId = adr.Substring(separatorIndex + 2, 2);

                        int id;
                        if (int.TryParse(strId, out id))
                        {
                            Shift shift = shifts.Find(x => x.id == id);
                            if (shift == null)
                            {
                                result = new Shift();
                                result.id = id;
                                result.name = name;
                                result.begintime = begintime;
                                result.endtime = endtime;
                            }
                        }
                    }
                }
            }

            return result;
        }

        static Segment GetBreakFromRow(Shift shift, DataRow row)
        {
            Segment result = null;

            string adr = row["address"].ToString();

            string lastNode = DataTable_Functions.TrakHound.GetLastNode(row);

            if (lastNode != null)
            {
                if (lastNode.ToLower() == "break")
                {
                    // Begin Time
                    ShiftTime begintime;
                    ShiftTime.TryParse(DataTable_Functions.TrakHound.GetRowAttribute("begintime", row), out begintime);

                    // End Time
                    ShiftTime endtime;
                    ShiftTime.TryParse(DataTable_Functions.TrakHound.GetRowAttribute("endtime", row), out endtime);

                    string strId = DataTable_Functions.TrakHound.GetRowAttribute("id", row);
                    if (strId != null)
                    {
                        int id = -1;
                        if (int.TryParse(strId, out id))
                        {
                            Segment b = shift.Breaks.Find(x => x.id == id);
                            if (b == null)
                            {
                                result = new Segment();
                                result.type = "Break";
                                result.id = id;
                                result.begintime = begintime;
                                result.endtime = endtime;
                            }
                        }
                    }
                }

            }

            return result;
        }

        #endregion

        #region "Save Configuration"

        public void SaveConfiguration(DataTable dt)
        {
            // Clear all shift rows first (so that Ids can be sequentially assigned)
            DataTable_Functions.TrakHound.DeleteRows("/Shifts/*", "address", dt);
            DataTable_Functions.TrakHound.DeleteRows("/ShiftData/*", "address", dt);

            if (shifts != null)
            {
                foreach (Shift s in shifts)
                {
                    SaveShift(s, dt);
                }
            }

            configurationTable = dt;
        }

        static void SaveShift(Shift s, DataTable dt)
        {
            //int id = 0;
            //string adr = "/Shifts/Shift";
            //string test = adr + id.ToString("00");

            int id = DataTable_Functions.TrakHound.GetUnusedAddressId("/Shifts/Shift", dt);
            string adr = "/Shifts/Shift||" + id.ToString("00");

            //DataTable_Functions.TrakHound.GetUnusedAddressId(adr, dt);
            //adr = test;

            s.id = id;

            // Save Attributes
            string attr = "";
            attr += "id||" + s.id.ToString("00") + ";";
            attr += "name||" + s.name + ";";
            attr += "begintime||" + s.begintime.ToFullString() + ";";
            attr += "endtime||" + s.endtime.ToFullString() + ";";

            DataTable_Functions.UpdateTableValue(dt, "address", adr, "attributes", attr);

            for (var i = 0; i < s.Breaks.Count; i++)
            {
                var segment = s.Breaks[i];

                string segadr = adr + "/Segments/Break||" + i.ToString("00");

                string segattr = "";
                segattr += "id||" + i.ToString("00") + ";";
                segattr += "begintime||" + segment.begintime.ToFullString() + ";";
                segattr += "endtime||" + segment.endtime.ToFullString() + ";";

                DataTable_Functions.UpdateTableValue(dt, "address", segadr, "attributes", segattr);
            }
        }

        //static void SaveShift(Shift s, DataTable dt)
        //{
        //    int id = 0;
        //    string adr = "/Shifts/Shift||";
        //    string test = adr + id.ToString("00");

        //    //// Reassign Id (to keep everything in sequence)
        //    //if (dt != null)
        //    //{
        //    //    while (DataTable_Functions.GetTableValue(dt, "address", test, ") != null)
        //    //    {
        //    //        id += 1;
        //    //        test = adr + id.ToString("00");
        //    //    }
        //    //}

        //    DataTable_Functions.TrakHound.GetUnusedAddressId(test, dt);
        //    adr = test;

        //    s.id = id;

        //    // Save Attributes
        //    string attr = "";
        //    attr += "id||" + s.id.ToString("00") + ";";
        //    attr += "name||" + s.name + ";";
        //    //attr += "begintime||" + s.begintime.To24HourString() + ";";
        //    //attr += "endtime||" + s.endtime.To24HourString() + ";";
        //    attr += "begintime||" + s.begintime.ToFullString() + ";";
        //    attr += "endtime||" + s.endtime.ToFullString() + ";";

        //    //Table_Functions.UpdateTableValue(null, attr, adr, dt);
        //    DataTable_Functions.UpdateTableValue(dt, "address", adr, "attributes", attr);

        //    // Save Segments
        //    List<Segment> segments = CreateSegments(s);

        //    foreach (Segment segment in segments)
        //    {
        //        string segadr = adr + "/Segments/" + String_Functions.UppercaseFirst(segment.type) + "||" + segment.id.ToString("00");

        //        string segattr = "";
        //        segattr += "id||" + segment.id.ToString("00") + ";";
        //        //segattr += "begintime||" + segment.begintime.To24HourString() + ";";
        //        //segattr += "endtime||" + segment.endtime.To24HourString() + ";";
        //        segattr += "begintime||" + segment.begintime.ToFullString() + ";";
        //        segattr += "endtime||" + segment.endtime.ToFullString() + ";";

        //        //Table_Functions.UpdateTableValue(null, segattr, segadr, dt);
        //        DataTable_Functions.UpdateTableValue(dt, "address", segadr, "attributes", segattr);
        //    }
        //}

        //static List<Segment> CreateSegments(Shift shift)
        //{
        //    List<Segment> result = new List<Segment>();

        //    var interval = new ShiftTime();
        //    interval.minute = 5;

        //    ShiftTime begintime = shift.begintime.Copy();
        //    ShiftTime endtime = shift.endtime.Copy();
        //    //if (endtime < begintime) endtime.dayOffset = begintime.dayOffset + 1;
        //    if (endtime < begintime) endtime.hour += 24;

        //    ShiftTime time = begintime.Copy();

        //    int nextBreakIndex = 0;

        //    List<Segment> breaks = shift.Breaks;
        //    if (breaks != null)
        //    {
        //        // Make sure the Segments(breaks) have values set
        //        List<Segment> sortedBreaks = new List<Segment>();
        //        foreach (Segment b in breaks) if (b.begintime != null || b.endtime != null) sortedBreaks.Add(b);

        //        sortedBreaks.Sort((a, b) => a.begintime.CompareTo(b.begintime));

        //        breaks = sortedBreaks;
        //    }

        //    int id = 0;

        //    while (time < endtime)
        //    {
        //        ShiftTime prev = time.Copy();

        //        bool breakfound = false;
        //        string type = "Work";

        //        if (breaks != null)
        //        {
        //            if (nextBreakIndex < breaks.Count)
        //            {
        //                // If break is same hour but less than 'time' (ex. time=11:00 and break.begintime=11:30)
        //                if (time.hour == breaks[nextBreakIndex].begintime.hour && time < breaks[nextBreakIndex].begintime)
        //                {
        //                    time = breaks[nextBreakIndex].begintime;
        //                    breakfound = true;
        //                }
        //                else if (time >= breaks[nextBreakIndex].begintime)
        //                {
        //                    time = breaks[nextBreakIndex].endtime;
        //                    nextBreakIndex += 1;
        //                    breakfound = true;
        //                    type = "Break";
        //                }
        //            }
        //        }

        //        if (!breakfound)
        //        {
        //            time = time.AddShiftTime(interval);

        //            //time.hour = 0;
        //            //time.minute += 5;
        //            //time.second = 0;

        //            //time.hour += 1;
        //            //time.minute = 0;
        //            //time.second = 0;

        //            if (time > endtime) time = endtime.Copy();
        //        }

        //        Segment segment = new Segment();
        //        segment.begintime = prev.Copy();
        //        segment.endtime = time.Copy();
        //        segment.type = type;
        //        segment.id = id;
        //        result.Add(segment);

        //        id += 1;
        //    }

        //    return result;
        //}

        #endregion


        #region "ShiftListItems"

        void ShiftListItem_Add(Shift shift)
        {
            Controls.ShiftListItem item = new Controls.ShiftListItem();
            item.ParentPage = this;
            item.ParentShift = shift;

            item.SettingChanged += ShiftListItem_SettingChanged;
            item.RemoveClicked += ShiftListItem_RemoveClicked;
            item.BreakRemoveClicked += ShiftListItem_BreakRemoveClicked;

            item.BeginHourItems = new ObservableCollection<ShiftTime>();
            foreach (ShiftTime time in HourItems) item.BeginHourItems.Add(time.Copy());

            item.EndHourItems = new ObservableCollection<ShiftTime>();
            foreach (ShiftTime time in HourItems) item.EndHourItems.Add(time.Copy());

            item.ShiftName = shift.name;

            item.BeginTime = shift.begintime.Copy();
            item.EndTime = shift.endtime.Copy();

            foreach (Segment b in shift.Breaks)
            {
                Controls.BreakListItem br = new Controls.BreakListItem();
                br.ParentPage = this;
                br.ParentShift = shift;
                br.ParentSegment = b;

                br.BeginHourItems = new ObservableCollection<ShiftTime>();
                foreach (ShiftTime time in HourItems) br.BeginHourItems.Add(time.Copy());

                br.EndHourItems = new ObservableCollection<ShiftTime>();
                foreach (ShiftTime time in HourItems) br.EndHourItems.Add(time.Copy());

                br.BeginTime = b.begintime.Copy();
                br.EndTime = b.endtime.Copy();

                br.SettingChanged += ShiftListItem_SettingChanged;
                br.RemoveClicked += ShiftListItem_BreakRemoveClicked;

                item.BreakItems.Add(br);
            }

            ShiftItems.Add(item);

            DisplayShifts = true;
        }

        void ShiftListItem_SettingChanged(string name)
        {
            if (!Loading) if (SettingChanged != null) SettingChanged(null, null, null);
        }

        void ShiftListItem_RemoveClicked(Controls.ShiftListItem item)
        {
            if (item.ParentShift != null)
            {
                if (shifts.Contains(item.ParentShift))
                {
                    shifts.Remove(item.ParentShift);
                }
            }

            if (ShiftItems.Contains(item)) ShiftItems.Remove(item);

            if (ShiftItems.Count > 0) DisplayShifts = true;
            else DisplayShifts = false;

            if (!Loading) if (SettingChanged != null) SettingChanged(null, null, null);
        }

        void ShiftListItem_BreakRemoveClicked(Controls.BreakListItem item)
        {
            if (item.ParentShift != null)
            {
                if (item.ParentShift.Breaks.Contains(item.ParentSegment))
                {
                    item.ParentShift.Breaks.Remove(item.ParentSegment);
                }

                int index = ShiftItems.ToList().FindIndex(x => x.ShiftName.ToLower() == item.ParentShift.name.ToLower());
                if (index >= 0)
                {
                    Controls.ShiftListItem sli = shiftitems[index];

                    if (sli.BreakItems.Contains(item)) sli.BreakItems.Remove(item);
                }
            }

            if (!Loading) if (SettingChanged != null) SettingChanged(null, null, null);
        }

        private void AddShift_Clicked(TrakHound_UI.Button bt)
        {
            Shift shift = new Shift();
            shift.name = null;
            shift.begintime = new ShiftTime();
            shift.endtime = new ShiftTime();

            shifts.Add(shift);

            ShiftListItem_Add(shift);

            if (!Loading) if (SettingChanged != null) SettingChanged(null, null, null);
        }

        #endregion

        #region "Hour Times"

        ObservableCollection<ShiftTime> houritems;
        public ObservableCollection<ShiftTime> HourItems
        {
            get
            {
                if (houritems == null)
                    houritems = new ObservableCollection<ShiftTime>();
                return houritems;
            }

            set
            {
                houritems = value;
            }
        }

        void InitializeHourTimes()
        {
            ShiftTime time = new ShiftTime();
            time.hour = 0;
            time.minute = 0;
            time.second = 0;

            ShiftTime interval = new ShiftTime();
            interval.hour = 0;
            interval.minute = 15;
            interval.second = 0;

            ShiftTime endtime = new ShiftTime();
            endtime.hour = 24;
            endtime.minute = 0;
            endtime.second = 0;

            HourItems.Clear();

            while (time < endtime)
            {
                HourItems.Add(time);
                time = time.AddShiftTime(interval);
            }
        }

        #endregion


        bool Loading = false;

        void ChangeSetting(string address, string name, string val)
        {
            if (!Loading)
            {
                string newVal = val;
                string oldVal = null;

                if (configurationTable != null)
                {
                    oldVal = DataTable_Functions.GetTableValue(configurationTable, "address", address, "value");
                }

                if (SettingChanged != null) SettingChanged(name, oldVal, newVal);
            }
        }

        DataTable configurationTable;

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        public bool DisplayShifts
        {
            get { return (bool)GetValue(DisplayShiftsProperty); }
            set { SetValue(DisplayShiftsProperty, value); }
        }

        public static readonly DependencyProperty DisplayShiftsProperty =
            DependencyProperty.Register("DisplayShifts", typeof(bool), typeof(Page), new PropertyMetadata(false));

        ObservableCollection<Controls.ShiftListItem> shiftitems;
        public ObservableCollection<Controls.ShiftListItem> ShiftItems
        {
            get
            {
                if (shiftitems == null)
                    shiftitems = new ObservableCollection<Controls.ShiftListItem>();
                return shiftitems;
            }

            set
            {
                shiftitems = value;
            }
        }

    }

    public class Shift
    {
        public Shift() { Breaks = new List<Segment>(); }

        public int id { get; set; }
        public string name { get; set; }
        public ShiftTime begintime { get; set; }
        public ShiftTime endtime { get; set; }

        public List<Segment> Breaks { get; set; }
    }

    public class Segment
    {
        public int id { get; set; }
        public string type { get; set; }
        public ShiftTime begintime { get; set; }
        public ShiftTime endtime { get; set; }
    }
}
