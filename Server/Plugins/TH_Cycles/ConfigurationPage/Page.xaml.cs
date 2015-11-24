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

using TH_PlugIns_Server;
using TH_Configuration;
using TH_UserManagement;
using TH_UserManagement.Management;

using TH_GeneratedData.ConfigurationPage;

namespace TH_Cycles.ConfigurationPage
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

        public string PageName { get { return "Cycles"; } }

        public ImageSource Image { get { return null; } }

        public event SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            configurationTable = dt;

            GeneratedEvents.Clear();

            genEvents = TH_GeneratedData.ConfigurationPage.Page.GetGeneratedEvents(dt);

            if (genEvents != null)
            {
                foreach (TH_GeneratedData.ConfigurationPage.Page.Event ev in genEvents)
                {
                    GeneratedEvents.Add(TH_Global.Formatting.UppercaseFirst(ev.name.Replace('_', ' ')));
                }
            }

            LoadData(dt);
        }

        public void SaveConfiguration(DataTable dt)
        {
            string eventName = event_COMBO.Text;
            string capturelink = capturelink_COMBO.Text;

            Table_Functions.UpdateTableValue(eventName.Replace(' ', '_').ToLower(), "/Cycles/Event", dt);

            Table_Functions.UpdateTableValue(capturelink.Replace(' ', '_').ToLower(), "/Cycles/CycleIdLink", dt);
        }

        DataTable configurationTable;


        void LoadData(DataTable dt)
        {
            string address = "/Cycles/";

            string filter = "address LIKE '" + address + "*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();
            temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

            // Get Events
            foreach (DataRow row in temp_dt.Rows)
            {
                string node = Table_Functions.GetLastNode(row);
                switch (node.ToLower())
                {
                    case "event": SelectedEvent = TH_Global.Formatting.UppercaseFirst(row["value"].ToString().Replace('_', ' ')); break;

                    case "cycleidlink": SelectedCaptureLink = TH_Global.Formatting.UppercaseFirst(row["value"].ToString().Replace('_', ' ')); break;
                }
            }
        }



        ObservableCollection<object> capturelinks;
        public ObservableCollection<object> CaptureLinks
        {
            get
            {
                if (capturelinks == null)
                    capturelinks = new ObservableCollection<object>();
                return capturelinks;
            }

            set
            {
                capturelinks = value;
            }
        }


        public object SelectedEvent
        {
            get { return (object)GetValue(SelectedEventProperty); }
            set { SetValue(SelectedEventProperty, value); }
        }

        public static readonly DependencyProperty SelectedEventProperty =
            DependencyProperty.Register("SelectedEvent", typeof(object), typeof(Page), new PropertyMetadata(null));


        public object SelectedCaptureLink
        {
            get { return (object)GetValue(SelectedCaptureLinkProperty); }
            set { SetValue(SelectedCaptureLinkProperty, value); }
        }

        public static readonly DependencyProperty SelectedCaptureLinkProperty =
            DependencyProperty.Register("SelectedCaptureLink", typeof(object), typeof(Page), new PropertyMetadata(null));

        


        private void Event_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItem = null;

            ComboBox cmbox = (ComboBox)sender;
            if (cmbox.SelectedItem != null) selectedItem = cmbox.SelectedItem.ToString();

            if (selectedItem != null)
            {
                CaptureLinks.Clear();

                if (genEvents != null)
                {
                    TH_GeneratedData.ConfigurationPage.Page.Event ev = genEvents.Find(x => TH_Global.Formatting.UppercaseFirst(x.name.Replace('_', ' ')).ToLower() == selectedItem.ToLower());
                    if (ev != null)
                    {
                        if (ev.captureItems != null)
                        {
                            foreach (TH_GeneratedData.ConfigurationPage.Page.CaptureItem item in ev.captureItems)
                            {
                                CaptureLinks.Add(item.name);
                            }
                        }
                    }
                }
            }

            if (cmbox.IsKeyboardFocused || cmbox.IsMouseCaptured) if (SettingChanged != null) SettingChanged("Cycle Event", null, null);
        }

        private void CaptureLink_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbox = (ComboBox)sender;

            if (cmbox.IsKeyboardFocused || cmbox.IsMouseCaptured) if (SettingChanged != null) SettingChanged("Cycle Value", null, null);
        }


        ObservableCollection<object> generatedevents;
        public ObservableCollection<object> GeneratedEvents
        {
            get
            {
                if (generatedevents == null)
                    generatedevents = new ObservableCollection<object>();
                return generatedevents;
            }

            set
            {
                generatedevents = value;
            }
        }

        List<TH_GeneratedData.ConfigurationPage.Page.Event> genEvents;

        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = false;
                    }
                }
            }
        }

        //#region "Generated Events"

        //#region "Sub Classes"

        //public class Event
        //{
        //    public Event() { values = new List<Value>(); }

        //    public List<Value> values { get; set; }

        //    public int id { get; set; }
        //    public string name { get; set; }

        //    public Result Default { get; set; }

        //    public string description { get; set; }
        //}

        //public class Value
        //{
        //    public Value() { triggers = new List<Trigger>(); }

        //    public List<Trigger> triggers { get; set; }

        //    public int id { get; set; }

        //    public Result result { get; set; }
        //}

        //public class Result
        //{
        //    public int numval { get; set; }
        //    public string value { get; set; }
        //}

        //#endregion

        //ObservableCollection<object> generatedevents;
        //public ObservableCollection<object> GeneratedEvents
        //{
        //    get
        //    {
        //        if (generatedevents == null)
        //            generatedevents = new ObservableCollection<object>();
        //        return generatedevents;
        //    }

        //    set
        //    {
        //        generatedevents = value;
        //    }
        //}

        //List<Event> genEvents;

        //List<Event> GetGeneratedEvents(DataTable dt)
        //{
        //    List<Event> result = new List<Event>();

        //    string address = "/GeneratedData/GeneratedEvents/";

        //    string filter = "address LIKE '" + address + "*'";
        //    DataView dv = dt.AsDataView();
        //    dv.RowFilter = filter;
        //    DataTable temp_dt = dv.ToTable();
        //    temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

        //    // Get Events
        //    foreach (DataRow row in temp_dt.Rows)
        //    {
        //        Event e = GetEventFromRow(result, row);
        //        if (e != null) result.Add(e);
        //    }

        //    // Get Values
        //    foreach (Event e in result)
        //    {
        //        string eventfilter = "address LIKE '" + address + "Event||" + e.id.ToString("00") + "/";
        //        dv = dt.AsDataView();
        //        dv.RowFilter = eventfilter + "*'";
        //        temp_dt = dv.ToTable();
        //        temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

        //        foreach (DataRow row in temp_dt.Rows)
        //        {
        //            Value v = GetValueFromRow(e, row);
        //            if (v != null) e.values.Add(v);

        //            GetDefaultFromRow(e, row);
        //        }

        //        foreach (Value v in e.values)
        //        {
        //            filter = eventfilter + "Value||" + v.id.ToString("00") + "/" + "*'";
        //            dv = dt.AsDataView();
        //            dv.RowFilter = filter;
        //            temp_dt = dv.ToTable();
        //            temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

        //            foreach (DataRow row in temp_dt.Rows)
        //            {
        //                //Trigger t = GetTriggerFromRow(v, row);
        //                //if (t != null) v.triggers.Add(t);

        //                GetResultFromRow(v, row);
        //            }
        //        }
        //    }

        //    return result;
        //}

        //Event GetEventFromRow(List<Event> genEvents, DataRow row)
        //{
        //    Event result = null;

        //    string adr = row["address"].ToString();

        //    string lastNode = Table_Functions.GetLastNode(row);

        //    if (lastNode != null)
        //    {
        //        if (lastNode.ToLower() == "event")
        //        {
        //            int eventIndex = adr.IndexOf("Event");
        //            int slashIndex = adr.IndexOf('/', eventIndex) + 1;
        //            int separatorIndex = adr.IndexOf("||", slashIndex);

        //            if (separatorIndex > slashIndex)
        //            {
        //                string name = Table_Functions.GetAttribute("name", row);
        //                string strId = adr.Substring(separatorIndex + 2, 2);

        //                int id;
        //                if (int.TryParse(strId, out id))
        //                {
        //                    Event e = genEvents.Find(x => x.id == id);
        //                    if (e == null)
        //                    {
        //                        result = new Event();
        //                        result.id = id;
        //                        result.name = name;
        //                        result.description = Table_Functions.GetAttribute("description", row);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

        //Value GetValueFromRow(Event e, DataRow row)
        //{
        //    Value result = null;

        //    string adr = row["address"].ToString();

        //    string lastNode = Table_Functions.GetLastNode(row);

        //    if (lastNode != null)
        //    {
        //        if (lastNode.ToLower() == "value")
        //        {
        //            string strId = Table_Functions.GetAttribute("id", row);
        //            if (strId != null)
        //            {
        //                int id = -1;
        //                if (int.TryParse(strId, out id))
        //                {
        //                    Value v = e.values.Find(x => x.id == id);
        //                    if (v == null)
        //                    {
        //                        result = new Value();
        //                        result.id = id;
        //                    }
        //                }
        //            }
        //        }

        //    }

        //    return result;
        //}

        //void GetDefaultFromRow(Event e, DataRow row)
        //{
        //    string adr = row["address"].ToString();

        //    if (adr.Contains("Default"))
        //    {
        //        string n = Table_Functions.GetAttribute("numval", row);
        //        if (n != null)
        //        {
        //            int numval;
        //            if (int.TryParse(n, out numval))
        //            {
        //                Result r = new Result();
        //                r.value = row["value"].ToString().Replace('_', ' '); ;
        //                r.numval = numval;
        //                e.Default = r;
        //            }
        //        }
        //    }
        //}

        //void GetResultFromRow(Value v, DataRow row)
        //{
        //    string adr = row["address"].ToString();

        //    if (adr.Contains("Result"))
        //    {
        //        string n = Table_Functions.GetAttribute("numval", row);
        //        if (n != null)
        //        {
        //            int numval;
        //            if (int.TryParse(n, out numval))
        //            {
        //                Result r = new Result();
        //                r.value = row["value"].ToString();
        //                r.numval = numval;
        //                v.result = r;
        //            }
        //        }
        //    }
        //}

        //#endregion

    }
}
