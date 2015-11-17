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

using System.Data;
using System.Collections.ObjectModel;

using TH_Configuration.User;
using TH_Global.Functions;
using TH_PlugIns_Server;

namespace TH_ShiftTable.ConfigurationPage
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, TH_PlugIns_Server.ConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string PageName { get { return "Shifts"; } }

        public ImageSource Image { get { return null; } }

        public event SettingChanged_Handler SettingChanged;

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
            endtime.dayOffset = 1;
            endtime.hour = 0;
            endtime.minute = 0;
            endtime.second = 0;

            HourItems.Clear();

            while (time < endtime)
            {
                HourItems.Add(time);
                time = time.AddShiftTime(interval);
            }
        }

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        #region "Load Configuration"

        public void LoadConfiguration(DataTable dt)
        {
            InitializeHourTimes();

            Loading = true;
            DisplayShifts = false;
            ShiftItems.Clear();

            shifts = GetShifts(dt);

            foreach (Shift shift in shifts)
            {
                this.Dispatcher.BeginInvoke(new Action<Shift>(AddShiftItem), priority, new object[] { shift });
            }

            if (shifts.Count > 0) DisplayShifts = true;

            Loading = false;
        }

        void AddShiftItem(Shift shift)
        {
            Controls.ShiftListItem item = new Controls.ShiftListItem();
            item.ParentPage = this;
            item.ParentShift = shift;

            item.RemoveClicked += item_RemoveClicked;
            item.BreakRemoveClicked += item_BreakRemoveClicked;

            item.BeginHourItems = new ObservableCollection<ShiftTime>();
            foreach (ShiftTime time in HourItems) item.BeginHourItems.Add(time.Copy());

            item.EndHourItems = new ObservableCollection<ShiftTime>();
            foreach (ShiftTime time in HourItems) item.EndHourItems.Add(time.Copy());

            item.ShiftName = shift.name;

            //item.BeginTime = HourItems.ToList().Find(x => x == shift.begintime);
            //item.EndTime = HourItems.ToList().Find(x => x == shift.endtime);

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

                br.RemoveClicked += item_BreakRemoveClicked;

                item.BreakItems.Add(br);
            }

            ShiftItems.Add(item);
        }

        void item_RemoveClicked(Controls.ShiftListItem item)
        {
            if (item.ParentShift != null)
            {
                if (shifts.Contains(item.ParentShift))
                {
                    shifts.Remove(item.ParentShift);
                }
            }

            if (ShiftItems.Contains(item)) ShiftItems.Remove(item);
        }

        void item_BreakRemoveClicked(Controls.BreakListItem item)
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
        }

        string GetLastNode(DataRow row)
        {
            string result = null;

            string adr = row["address"].ToString();

            if (adr.Contains('/'))
            {
                string s = adr;

                // Remove Last forward slash
                if (s[s.Length - 1] == '/') s = s.Substring(0, s.Length - 1);

                // Get index of last forward slash
                int slashIndex = s.LastIndexOf('/') + 1;
                if (slashIndex < s.Length) s = s.Substring(slashIndex);

                // Remove Id
                if (s.Contains("||"))
                {
                    int separatorIndex = s.LastIndexOf("||");
                    s = s.Substring(0, separatorIndex);
                }

                result = s;
            }

            return result;
        }

        Shift GetShiftFromRow(List<Shift> shifts, DataRow row)
        {
            Shift result = null;

            string adr = row["address"].ToString();

            string lastNode = GetLastNode(row);

            if (lastNode != null)
            {
                if (lastNode.ToLower() == "shift")
                {
                    int eventIndex = adr.IndexOf("Shift");
                    int slashIndex = adr.IndexOf('/', eventIndex) + 1;
                    int separatorIndex = adr.IndexOf("||", slashIndex);

                    if (separatorIndex > slashIndex)
                    {
                        string name = GetAttribute("name", row);

                        // Begin Time
                        ShiftTime begintime;
                        ShiftTime.TryParse(GetAttribute("begintime", row), out begintime);

                        // End Time
                        ShiftTime endtime;
                        ShiftTime.TryParse(GetAttribute("endtime", row), out endtime);

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

        Segment GetBreakFromRow(Shift shift, DataRow row)
        {
            Segment result = null;

            string adr = row["address"].ToString();

            string lastNode = GetLastNode(row);

            if (lastNode != null)
            {
                if (lastNode.ToLower() == "break")
                {
                    // Begin Time
                    ShiftTime begintime;
                    ShiftTime.TryParse(GetAttribute("begintime", row), out begintime);

                    // End Time
                    ShiftTime endtime;
                    ShiftTime.TryParse(GetAttribute("endtime", row), out endtime);

                    string strId = GetAttribute("id", row);
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

        string GetAttribute(string name, DataRow row)
        {
            string line = row["attributes"].ToString();

            if (line.Contains(name))
            {
                int a = line.IndexOf(name);
                if (a >= 0)
                {
                    int b = line.IndexOf("||", a) + 2;
                    int c = line.IndexOf(";", a);

                    if (b >= 0 && (c - b) > 0)
                    {
                        return line.Substring(b, c - b);
                    }
                }
            }

            return null;
        }

        List<Shift> GetShifts(DataTable dt)
        {
            List<Shift> result = new List<Shift>();

            string address = "/ShiftData/Shifts/";

            string filter = "address LIKE '" + address + "*'";
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

        public List<Shift> shifts;

        #endregion

        #region "Save Configuration"

        public void SaveConfiguration(DataTable dt)
        {
            string prefix = "/ShiftData/Shifts/";

            // Clear all shift rows first (so that Ids can be sequentially assigned)
            string filter = "address LIKE '" + prefix + "*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();
            foreach (DataRow row in temp_dt.Rows)
            {
                DataRow dbRow = dt.Rows.Find(row["address"]);
                if (dbRow != null) dt.Rows.Remove(dbRow);
            }

            if (shifts != null)
            {
                foreach (Shift s in shifts)
                {
                    SaveShift(s, dt);
                }
            }
        }

        void SaveShift(Shift s, DataTable dt)
        {
            int id = 0;
            string adr = "/ShiftData/Shifts/Shift||";
            string test = adr + id.ToString("00");

            // Reassign Id (to keep everything in sequence)
            if (dt != null)
            {
                while (Table_Functions.GetTableValue(test, dt) != null)
                {
                    id += 1;
                    test = adr + id.ToString("00");
                }
            }

            adr = test;

            s.id = id;

            // Save Attributes
            string attr = "";
            attr += "id||" + s.id.ToString("00") + ";";
            attr += "name||" + s.name + ";";
            attr += "begintime||" + s.begintime.To24HourString() + ";";
            attr += "endtime||" + s.endtime.To24HourString() + ";";

            Table_Functions.UpdateTableValue(null, attr, adr, dt);

            // Save Segments
            List<Segment> segments = CreateSegments(s);

            foreach (Segment segment in segments)
            {
                string segadr = adr + "/" + TH_Global.Formatting.UppercaseFirst(segment.type) + "||" + segment.id.ToString("00");

                string segattr = "";
                segattr += "id||" + segment.id.ToString("00") + ";";
                segattr += "begintime||" + segment.begintime.To24HourString() + ";";
                segattr += "endtime||" + segment.endtime.To24HourString() + ";";

                Console.WriteLine(segment.type + "||" + segment.id.ToString("00") + " :: " + segment.begintime.To24HourString() + " - " + segment.endtime.To24HourString());

                Table_Functions.UpdateTableValue(null, segattr, segadr, dt);
            }
        }

        #endregion

        List<Segment> CreateSegments(Shift shift)
        {
            List<Segment> result = new List<Segment>();
            
            ShiftTime interval = new ShiftTime();
            interval.hour = 1;

            ShiftTime begintime = shift.begintime;
            ShiftTime endtime = shift.endtime;
            if (endtime < begintime) endtime.dayOffset += 1;

            ShiftTime time = begintime;

            int nextBreakIndex = 0;

            List<Segment> breaks = shift.Breaks;
            if (breaks != null)
            {
                // Make sure the Segments(breaks) have values set
                List<Segment> sortedBreaks = new List<Segment>();
                foreach (Segment b in breaks) if (b.begintime != null || b.endtime != null) sortedBreaks.Add(b);

                sortedBreaks.Sort((a, b) => a.begintime.CompareTo(b.begintime));

                breaks = sortedBreaks;
            }

            int id = 0;

            while (time < endtime)
            {
                ShiftTime prev = time.Copy();

                bool breakfound = false;

                if (breaks != null)
                {
                    if (nextBreakIndex < breaks.Count)
                    {
                        if (time >= breaks[nextBreakIndex].begintime)
                        {
                            time = breaks[nextBreakIndex].endtime;
                            nextBreakIndex += 1;
                            breakfound = true;
                        }   
                    }
                }

                string type = "Work";

                if (!breakfound)
                {
                    time.hour += 1;
                    time.minute = 0;
                    time.second = 0;
                }
                else type = "Break";

                Segment segment = new Segment();
                segment.begintime = prev;
                segment.endtime = time.Copy();
                segment.type = type;
                segment.id = id;
                result.Add(segment);

                Console.WriteLine(segment.type + "||" + segment.id.ToString("00") + " :: " + segment.begintime.To24HourString() + " - " + segment.endtime.To24HourString());

                id += 1;
            }

            return result;
        }


        bool Loading = false;

        //DataTable configurationTable;


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

        private void AddShift_Clicked(TH_WPF.Button_01 bt)
        {
            Shift shift = new Shift();

            DisplayShifts = true;

            Controls.ShiftListItem item = new Controls.ShiftListItem();

            item.ParentPage = this;
            item.ParentShift = shift;

            item.BeginHourItems = new ObservableCollection<ShiftTime>();
            foreach (ShiftTime time in HourItems) item.BeginHourItems.Add(time.Copy());

            item.EndHourItems = new ObservableCollection<ShiftTime>();
            foreach (ShiftTime time in HourItems) item.EndHourItems.Add(time.Copy());

            item.RemoveClicked += item_RemoveClicked;

            shifts.Add(shift);
            ShiftItems.Add(item);

        }


    }
}
