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
using System.Threading;

using TH_Configuration;
using TH_Global.Functions;
using TH_PlugIns_Server;
using TH_MTC_Data.Components;
using TH_MTC_Requests;
using TH_UserManagement.Management;

namespace TH_GeneratedData.ConfigurationPage
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

        public string PageName { get { return "Generated Data"; } }

        public ImageSource Image { get { return null; } }

        public event SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            loading = true;

            configurationTable = dt;

            LoadSnapshotItems(configurationTable);

            List<Event> genEvents;

            genEvents = GetGeneratedEvents(dt);

            if (genEvents.Count > 0) DisplayEvents = true;
            else DisplayEvents = false;

            CreateGeneratedEvents(genEvents);

            GeneratedEvents.Clear();
            foreach (Event e in genEvents) GeneratedEvents.Add(e);

            LoadAgentSettings(dt);

            loading = false;
        }

        public void SaveConfiguration(DataTable dt)
        {

            SaveSnapshotData(dt);

            SaveGeneratedEvents(dt);

        }

        bool loading = false;

        void ChangeSetting(string address, string name, string val)
        {
            string newVal = val;
            string oldVal = null;

            if (configurationTable != null)
            {
                oldVal = Table_Functions.GetTableValue(address, configurationTable);
            }

            if (!loading) if (SettingChanged != null) SettingChanged(name, oldVal, newVal);
        }


        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        DataTable configurationTable;
       

        #region "MTC Data Items"  
     
        ObservableCollection<CollectedItem> collecteditems;
        public ObservableCollection<CollectedItem> CollectedItems
        {
            get
            {
                if (collecteditems == null)
                    collecteditems = new ObservableCollection<CollectedItem>();
                return collecteditems;
            }

            set
            {
                collecteditems = value;
            }
        }

        public class CollectedItem
        {
            public string id { get; set; }
            public string name { get; set; }

            public string display { get; set; }

            public override string ToString()
            {
                return display;
            }
        }

        void LoadAgentSettings(DataTable dt)
        {
            string prefix = "/Agent/";

            string ip = Table_Functions.GetTableValue(prefix + "IP_Address", dt);
            string p = Table_Functions.GetTableValue(prefix + "Port", dt);
            string devicename = Table_Functions.GetTableValue(prefix + "Device_Name", dt);

            int port;
            int.TryParse(p, out port);

            CollectedItems.Clear();

            RunProbe(ip, port, devicename);
        }

        void RunProbe(string url, int port, string deviceName)
        {
            // Create Configuration with agent settings
            Configuration config = new Configuration();
            Agent_Settings agentSettings = new Agent_Settings();

            agentSettings.IP_Address = url;
            agentSettings.Port = port;
            agentSettings.Device_Name = deviceName;

            config.Agent = agentSettings;

            // Run a Probe request
            Probe probe = new Probe();
            probe.configuration = config;
            probe.ProbeFinished += probe_ProbeFinished;
            probe.ProbeError += probe_ProbeError;
            probe.Start();
        }

        void probe_ProbeFinished(ReturnData returnData, Probe sender)
        {
            if (returnData != null)
            {
                foreach (Device device in returnData.devices)
                {
                    DataItemCollection dataItems = Tools.GetDataItemsFromDevice(device);

                    List<DataItem> items = new List<DataItem>();

                    // Conditions
                    foreach (DataItem dataItem in dataItems.Conditions) items.Add(dataItem);

                    // Events
                    foreach (DataItem dataItem in dataItems.Events) items.Add(dataItem);

                    // Samples
                    foreach (DataItem dataItem in dataItems.Samples) items.Add(dataItem);

                    this.Dispatcher.BeginInvoke(new Action<List<DataItem>>(AddDataItems), priority, new object[] { items });
                }
            }

            // Set 'Loading' to false
            this.Dispatcher.BeginInvoke(new Action(ProbeFinished), priority, null);
        }

        void AddDataItems(List<DataItem> items)
        {
            List<CollectedItem> list = new List<CollectedItem>();

            foreach (DataItem item in items)
            {
                CollectedItem ci = new CollectedItem();
                ci.id = item.id;
                ci.name = item.name;

                if (ci.name != null) ci.display = ci.id + " : " + ci.name;
                else ci.display = ci.id;

                list.Add(ci);
            }

            list.Sort((x, y) => string.Compare(x.id, y.id));

            foreach (CollectedItem item in list) CollectedItems.Add(item);

        }

        void ProbeFinished()
        {
            foreach (Controls.Snapshot_Item item in SnapshotItems)
            {
                this.Dispatcher.BeginInvoke(new Action<Controls.Snapshot_Item>(SnapshotItem_UpdateCollectedLink), priority, new object[] { item });
            }

            foreach (Controls.Event ev in events)
            {
                foreach (Controls.Value v in ev.Values)
                {
                    foreach (Controls.Trigger t in v.Triggers)
                    {
                        this.Dispatcher.BeginInvoke(new Action<Controls.Trigger>(Trigger_UpdateCollectedLink), priority, new object[] { t });
                    } 
                }

                foreach (Controls.CaptureItem ci in ev.CaptureItems)
                {
                    this.Dispatcher.BeginInvoke(new Action<Controls.CaptureItem>(CaptureItem_UpdateCollectedLink), priority, new object[] { ci });
                }
            }
        }

        void probe_ProbeError(Probe.ErrorData errorData)
        {

            // Set 'Loading' to false
            this.Dispatcher.BeginInvoke(new Action(ProbeFinished), priority, null);

        }

        #endregion

        #region "Snapshot Data"

        public class Snapshot
        {
            public string name { get; set; }
            public string type { get; set; }
            public string link { get; set; }
        }

        public bool DisplaySnapshots
        {
            get { return (bool)GetValue(DisplaySnapshotsProperty); }
            set { SetValue(DisplaySnapshotsProperty, value); }
        }

        public static readonly DependencyProperty DisplaySnapshotsProperty =
            DependencyProperty.Register("DisplaySnapshots", typeof(bool), typeof(Page), new PropertyMetadata(false));


        ObservableCollection<Controls.Snapshot_Item> snapshotitems;
        public ObservableCollection<Controls.Snapshot_Item> SnapshotItems
        {
            get
            {
                if (snapshotitems == null)
                    snapshotitems = new ObservableCollection<Controls.Snapshot_Item>();
                return snapshotitems;
            }

            set
            {
                snapshotitems = value;
            }
        }


        void SaveSnapshotData(DataTable dt)
        {
            string prefix = "/GeneratedData/SnapShotData/";

            // Clear all snapshot rows first (so that Ids can be sequentially assigned)
            string filter = "address LIKE '" + prefix + "*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();
            foreach (DataRow row in temp_dt.Rows)
            {
                DataRow dbRow = dt.Rows.Find(row["address"]);
                if (dbRow != null) dt.Rows.Remove(dbRow);
            }

            List<CollectedItem> linkitems = CollectedItems.ToList();

            // Loop through SnapshotItems and add each item back to table with sequential id's
            foreach (Controls.Snapshot_Item item in SnapshotItems)
            {
                if (item.ParentSnapshot != null)
                {
                    Snapshot snapshot = item.ParentSnapshot;

                    if (snapshot.name != null && snapshot.link != null)
                    {
                        int id = 0;
                        string adr = "/GeneratedData/SnapShotData/" + TH_Global.Formatting.UppercaseFirst(snapshot.type) + "||";
                        string test = adr + id.ToString("00");

                        // Reassign Id (to keep everything in sequence)
                        if (configurationTable != null)
                        {
                            while (Table_Functions.GetTableValue(test, dt) != null)
                            {
                                id += 1;
                                test = adr + id.ToString("00");
                            }
                        }

                        adr = test;

                        string attr = "";
                        attr += "id||" + id.ToString("00") + ";";
                        attr += "name||" + snapshot.name + ";";

                        string link = snapshot.link;
                        if (item.SelectedType.ToLower() == "collected")
                        {
                            CollectedItem ci = linkitems.Find(x => x.display == link);
                            if (ci != null) link = ci.id;
                        }
                        else if (item.SelectedType.ToLower() == "generated")
                        {
                            link = link.Replace(' ', '_').ToLower();
                        }

                        attr += "link||" + link + ";";

                        Table_Functions.UpdateTableValue(null, attr, adr, dt);

                    }
                }
            }
        }

        void LoadSnapshotItems(DataTable dt)
        {
            Console.WriteLine("Loading Snapshots");

            string address = "/GeneratedData/SnapShotData/";

            string filter = "address LIKE '" + address + "*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();
            temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

            SnapshotItems.Clear();

            foreach (DataRow row in temp_dt.Rows)
            {
                Snapshot snapshot = new Snapshot();
                snapshot.name = Table_Functions.GetAttribute("name", row);
                snapshot.type = Table_Functions.GetLastNode(row);
                snapshot.link = Table_Functions.GetAttribute("link", row);

                SnapshotItem_Add(snapshot);
            }

            if (SnapshotItems.Count > 0) DisplaySnapshots = true;
            else DisplaySnapshots = false;
        }

        #region "SnapshotItem"

        void SnapshotItem_Add(Snapshot snapshot)
        {
            Controls.Snapshot_Item item = new Controls.Snapshot_Item();
            item.Loading = true;

            item.ParentPage = this;
            item.ParentSnapshot = snapshot;

            item.TypeItems = GenerateTypeList();

            item.NameText = snapshot.name;

            item.SelectedType = snapshot.type;
            switch (item.SelectedType.ToLower())
            {
                case "collected": 
                    //CollectedItem ci = CollectedItems.ToList().Find(x => x.id == snapshot.link);
                    //if (ci != null) item.collectedlink_COMBO.Text = ci.display;
                    item.collectedlink_COMBO.Text = snapshot.link;
                    break;

                case "generated":
                    item.generatedlink_COMBO.Text = TH_Global.Formatting.UppercaseFirst(snapshot.link.Replace('_', ' ')); 
                    break;

                case "variable": item.variable_TXT.Text = snapshot.link; break;
            }

            item.SettingChanged += SnapshotItem_SettingChanged;
            item.RemoveClicked += SnapshotItem_RemoveClicked;
            item.RefreshClicked += SnapshotItem_RefreshClicked;

            item.Loading = false;

            SnapshotItems.Add(item);

            DisplaySnapshots = true;
        }

        ObservableCollection<string> GenerateTypeList()
        {
            ObservableCollection<string> result = new ObservableCollection<string>();

            result.Add("Collected");
            result.Add("Generated");
            result.Add("Variable");

            return result;
        }

        void SnapshotItem_RemoveClicked(Controls.Snapshot_Item item)
        {
            this.Dispatcher.BeginInvoke(new Action<Controls.Snapshot_Item>(SnapshotItem_Remove), priority, new object[] { item });
        }

        void SnapshotItem_Remove(Controls.Snapshot_Item item)
        {
            SnapshotItems.Remove(item);
        }

        void SnapshotItem_RefreshClicked(Controls.Snapshot_Item item)
        {
            if (configurationTable != null)
            {
                List<Event> genEvents;

                genEvents = GetGeneratedEvents(configurationTable);

                GeneratedEvents.Clear();
                foreach (Event e in genEvents) GeneratedEvents.Add(e);

                //GeneratedEvents = GetGeneratedEvents(configurationTable);
            }
        }

        void SnapshotItem_SettingChanged()
        {
            if (!loading) if (SettingChanged != null) SettingChanged(null, null, null);
        }

        void SnapshotItem_UpdateCollectedLink(Controls.Snapshot_Item item)
        {
            Page.CollectedItem ci = CollectedItems.ToList().Find(x => x.id == item.collectedlink_COMBO.Text);
            if (ci != null) item.collectedlink_COMBO.Text = ci.display;
        }

        #endregion

        List<Controls.Snapshot_Item> Snapshot_RemoveList = new List<Controls.Snapshot_Item>();
    
        private void AddValue_Clicked(TH_WPF.Button_01 bt)
        {
            Snapshot snapshot = new Snapshot();
            snapshot.type = "Collected";

            SnapshotItem_Add(snapshot);
        }

        #endregion

        #region "Generated Events"

        ObservableCollection<Event> generatedevents;
        public ObservableCollection<Event> GeneratedEvents
        {
            get
            {
                if (generatedevents == null)
                    generatedevents = new ObservableCollection<Event>();
                return generatedevents;
            }

            set
            {
                generatedevents = value;
            }
        }


        public bool DisplayEvents
        {
            get { return (bool)GetValue(DisplayEventsProperty); }
            set { SetValue(DisplayEventsProperty, value); }
        }

        public static readonly DependencyProperty DisplayEventsProperty =
            DependencyProperty.Register("DisplayEvents", typeof(bool), typeof(Page), new PropertyMetadata(false));

        
        ObservableCollection<TH_WPF.CollapseButton> eventbuttons;
        public ObservableCollection<TH_WPF.CollapseButton> EventButtons
        {
            get
            {
                if (eventbuttons == null)
                    eventbuttons = new ObservableCollection<TH_WPF.CollapseButton>();
                return eventbuttons;
            }

            set
            {
                eventbuttons = value;
            }
        }

        public List<Controls.Event> events;

        #region "Save"

        void SaveGeneratedEvents(DataTable dt)
        {
            string prefix = "/GeneratedData/GeneratedEvents/";

            // Clear all generated event rows first (so that Ids can be sequentially assigned)
            string filter = "address LIKE '" + prefix + "*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();
            foreach (DataRow row in temp_dt.Rows)
            {
                DataRow dbRow = dt.Rows.Find(row["address"]);
                if (dbRow != null) dt.Rows.Remove(dbRow);
            }

            if (GeneratedEvents != null)
            {
                foreach (Event e in GeneratedEvents)
                {
                    SaveEvent(e, dt);
                }
            }
        }

        void SaveEvent(Event e, DataTable dt)
        {

            int id = 0;
            string adr = "/GeneratedData/GeneratedEvents/Event||";
            string test = adr + id.ToString("00");

            // Reassign Id (to keep everything in sequence)
            if (configurationTable != null)
            {
                while (Table_Functions.GetTableValue(test, dt) != null)
                {
                    id += 1;
                    test = adr + id.ToString("00");
                }
            }

            adr = test;

            e.id = id;

            string attr = "";
            attr += "id||" + e.id.ToString("00") + ";";
            attr += "name||" + e.name.Replace(' ', '_').ToLower() + ";";
            attr += "description||" + e.description + ";";



            Table_Functions.UpdateTableValue(null, attr, adr, dt);

            foreach (Value v in e.values) SaveValue(v, e, dt);

            foreach (CaptureItem ci in e.captureItems) SaveCaptureItems(ci, e, dt);

            if (e.Default != null)
            {
                string addr = adr + "/Default";
                attr = "";
                attr += "numval||" + e.Default.numval.ToString() + ";";
                string val = e.Default.value;
                Table_Functions.UpdateTableValue(val, attr, addr, dt);
            }
        }

        void SaveValue(Value v, Event e, DataTable dt)
        {
            int id = 0;
            string adr = "/GeneratedData/GeneratedEvents/Event||" + e.id.ToString("00") + "/Value||";
            string test = adr + id.ToString("00");


            // Reassign Id (to keep everything in sequence)
            if (configurationTable != null)
            {
                while (Table_Functions.GetTableValue(test, dt) != null)
                {
                    id += 1;
                    test = adr + id.ToString("00");
                }
            }

            adr = test;

            v.id = id;


            // Save Root
            string attr = "";
            attr += "id||" + v.id.ToString("00") + ";";
            Table_Functions.UpdateTableValue(null, attr, adr, dt);

            // Save Triggers
            foreach (Trigger t in v.triggers) SaveTrigger(t, v, e, dt);

            // Save Result
            if (v.result != null)
            {
                string addr = adr + "/Result";
                attr = "";
                attr += "numval||" + v.result.numval.ToString() + ";";
                string val = v.result.value;
                Table_Functions.UpdateTableValue(val, attr, addr, dt);
            }
        }

        void SaveTrigger(Trigger t, Value v, Event e, DataTable dt)
        {
            if (t.link != null && t.modifier != null && t.value != null)
            {
                int id = 0;
                string adr = "/GeneratedData/GeneratedEvents/Event||" + e.id.ToString("00");
                adr += "/Value||" + v.id.ToString("00") + "/Triggers";
                adr += "/Trigger||";
                string test = adr + id.ToString("00");


                // Reassign Id (to keep everything in sequence)
                if (configurationTable != null)
                {
                    while (Table_Functions.GetTableValue(test, dt) != null)
                    {
                        id += 1;
                        test = adr + id.ToString("00");
                    }
                }

                adr = test;

                t.id = id;

                // Save Root
                string attr = "";
                attr += "id||" + t.id.ToString("00") + ";";

                string link = t.link;
                List<CollectedItem> linkitems = CollectedItems.ToList();
                CollectedItem dataitem = linkitems.Find(x => x.display == link);
                if (dataitem != null) link = dataitem.id;

                attr += "link||" + link + ";";


                attr += "value||" + t.value + ";";



                Table_Functions.UpdateTableValue(null, attr, adr, dt);
            }
        }

        void SaveCaptureItems(CaptureItem ci, Event e, DataTable dt)
        {
            int id = 0;
            string adr = "/GeneratedData/GeneratedEvents/Event||" + e.id.ToString("00") + "/Capture/Item||";
            string test = adr + id.ToString("00");

            // Reassign Id (to keep everything in sequence)
            if (configurationTable != null)
            {
                while (Table_Functions.GetTableValue(test, dt) != null)
                {
                    id += 1;
                    test = adr + id.ToString("00");
                }
            }

            adr = test;

            ci.id = id;

            // Save Root
            string attr = "";
            attr += "id||" + ci.id.ToString("00") + ";";
            attr += "name||" + ci.name.Replace(' ','_').ToLower() + ";";

            string link = ci.link;
            List<CollectedItem> linkitems = CollectedItems.ToList();
            CollectedItem dataitem = linkitems.Find(x => x.display == link);
            if (dataitem != null) link = dataitem.id;

            attr += "link||" + link;
            Table_Functions.UpdateTableValue(null, attr, adr, dt);
        }

        #endregion

        #region "Load"

        public static List<Event> GetGeneratedEvents(DataTable dt)
        {
            List<Event> result = new List<Event>();

            string address = "/GeneratedData/GeneratedEvents/";

            string filter = "address LIKE '" + address + "*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();
            temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

            // Get Events
            foreach (DataRow row in temp_dt.Rows)
            {
                Event e = GetEventFromRow(result, row);
                if (e != null) result.Add(e);
            }

            // Get Values
            foreach (Event e in result)
            {
                string eventfilter = "address LIKE '" + address + "Event||" + e.id.ToString("00") + "/";
                dv = dt.AsDataView();
                dv.RowFilter = eventfilter + "*'";
                temp_dt = dv.ToTable();
                temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

                foreach (DataRow row in temp_dt.Rows)
                {
                    Value v = GetValueFromRow(e, row);
                    if (v != null) e.values.Add(v);

                    GetDefaultFromRow(e, row);

                    // Get Capture Items
                    CaptureItem ci = GetCaptureItemFromRow(e, row);
                    if (ci != null) e.captureItems.Add(ci);
                }

                foreach (Value v in e.values)
                {
                    filter = eventfilter + "Value||" + v.id.ToString("00") + "/" + "*'";
                    dv = dt.AsDataView();
                    dv.RowFilter = filter;
                    temp_dt = dv.ToTable();
                    temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

                    foreach (DataRow row in temp_dt.Rows)
                    {
                        Trigger t = GetTriggerFromRow(v, row);
                        if (t != null) v.triggers.Add(t);

                        GetResultFromRow(v, row);
                    }
                }
            }

            return result;
        }

        static Event GetEventFromRow(List<Event> genEvents, DataRow row)
        {
            Event result = null;

            string adr = row["address"].ToString();

            string lastNode = Table_Functions.GetLastNode(row);

            if (lastNode != null)
            {
                if (lastNode.ToLower() == "event")
                {
                    int eventIndex = adr.IndexOf("Event");
                    int slashIndex = adr.IndexOf('/', eventIndex) + 1;
                    int separatorIndex = adr.IndexOf("||", slashIndex);

                    if (separatorIndex > slashIndex)
                    {
                        string name = Table_Functions.GetAttribute("name", row);
                        string strId = adr.Substring(separatorIndex + 2, 2);

                        int id;
                        if (int.TryParse(strId, out id))
                        {
                            Event e = genEvents.Find(x => x.id == id);
                            if (e == null)
                            {
                                result = new Event();
                                result.id = id;
                                result.name = name;
                                result.description = Table_Functions.GetAttribute("description", row);
                            }
                        }
                    }
                }
            }

            return result;
        }

        static Value GetValueFromRow(Event e, DataRow row)
        {
            Value result = null;

            string adr = row["address"].ToString();

            string lastNode = Table_Functions.GetLastNode(row);

            if (lastNode != null)
            {
                if (lastNode.ToLower() == "value")
                {
                    string strId = Table_Functions.GetAttribute("id", row);
                    if (strId != null)
                    {
                        int id = -1;
                        if (int.TryParse(strId, out id))
                        {
                            Value v = e.values.Find(x => x.id == id);
                            if (v == null)
                            {
                                result = new Value();
                                result.id = id;
                            }
                        }
                    }
                }

            }

            return result;
        }

        static void GetDefaultFromRow(Event e, DataRow row)
        {
            string adr = row["address"].ToString();

            if (adr.Contains("Default"))
            {
                string n = Table_Functions.GetAttribute("numval", row);
                if (n != null)
                {
                    int numval;
                    if (int.TryParse(n, out numval))
                    {
                        Result r = new Result();
                        r.value = row["value"].ToString().Replace('_', ' '); ;
                        r.numval = numval;
                        e.Default = r;
                    }
                }
            }
        }

        static void GetResultFromRow(Value v, DataRow row)
        {
            string adr = row["address"].ToString();

            if (adr.Contains("Result"))
            {
                string n = Table_Functions.GetAttribute("numval", row);
                if (n != null)
                {
                    int numval;
                    if (int.TryParse(n, out numval))
                    {
                        Result r = new Result();
                        r.value = row["value"].ToString();
                        r.numval = numval;
                        v.result = r;
                    }
                }
            }
        }

        static Trigger GetTriggerFromRow(Value v, DataRow row)
        {
            Trigger result = null;

            string adr = row["address"].ToString();

            if (adr.Contains("Trigger"))
            {
                int eventIndex = adr.IndexOf("Trigger");
                int slashIndex = adr.IndexOf('/', eventIndex) + 1;
                int separatorIndex = adr.IndexOf("||", slashIndex);

                if (separatorIndex > slashIndex)
                {
                    string val = row["value"].ToString();
                    string strId = adr.Substring(separatorIndex + 2, 2);

                    int id;
                    if (int.TryParse(strId, out id))
                    {
                        Trigger t = v.triggers.Find(x => x.id == id);
                        if (t == null)
                        {
                            result = new Trigger();
                            result.id = id;
                            result.link = Table_Functions.GetAttribute("link", row);
                            result.value = Table_Functions.GetAttribute("value", row);

                            string modifier = "Equal To";
                            string mod = Table_Functions.GetAttribute("modifier", row);
                            if (mod != null)
                            {
                                switch (mod.ToLower())
                                {
                                    case "not": modifier = "Not Equal To"; break;
                                    case "greater_than": modifier = "Greater Than"; break;
                                    case "less_than": modifier = "Less Than"; break;
                                    case "contains": modifier = "Contains"; break;
                                    case "contains_match_case": modifier = "Contains Match Case"; break;
                                    case "contains_whole_word": modifier = "Contains Whole Word"; break;
                                    case "contains_whole_word_match_case": modifier = "Contains Whole Word Match Case"; break;
                                }
                            }

                            result.modifier = modifier;
                        }
                    }
                }
            }

            return result;
        }

        static CaptureItem GetCaptureItemFromRow(Event e, DataRow row)
        {
            CaptureItem result = null;

            string adr = row["address"].ToString();

            if (adr.Contains("Capture"))
            {
                string node = Table_Functions.GetLastNode(row);
                if (node.ToLower() == "item")
                {
                    string strId = Table_Functions.GetAttribute("id", row);
                    int id;
                    if (int.TryParse(strId, out id))
                    {
                        result = new CaptureItem();
                        result.id = id;
                        result.name = Table_Functions.GetAttribute("name", row);
                        result.link = Table_Functions.GetAttribute("link", row);
                    }
                }
            }

            return result;
        }

        #region "Controls"

        void CreateGeneratedEvents(List<Event> genEvents)
        {
            EventButtons.Clear();
            events = new List<Controls.Event>();

            first = true;

            foreach (Event e in genEvents)
            {
                this.Dispatcher.BeginInvoke(new Action<Event>(AddEvent), priority, new object[] { e });
            }
        }

        bool first = true;

        void AddEvent(Event e)
        {
            Controls.Event ev = CreateEvent(e);

            Controls.EventButton event_bt = new Controls.EventButton();
            event_bt.EventName = TH_Global.Formatting.UppercaseFirst(e.name.Replace('_', ' '));
            event_bt.SettingChanged += event_bt_SettingChanged;
            event_bt.RemoveClicked += Event_RemoveClicked;
            event_bt.ParentEvent = e;

            TH_WPF.CollapseButton bt = new TH_WPF.CollapseButton();
            bt.ButtonContent = event_bt;

            if (first) bt.IsExpanded = true;
            first = false;

            bt.PageContent = ev;

            events.Add(ev);

            EventButtons.Add(bt);
        }

        void event_bt_SettingChanged()
        {
            if (!loading) if (SettingChanged != null) SettingChanged(null, null, null);
        }

        #region "Event"

        Controls.Event CreateEvent(Event e)
        {
            Controls.Event result = new Controls.Event();
            result.ParentPage = this;
            result.ParentEvent = e;

            result.Description = e.description;

            result.SettingChanged += Event_SettingChanged;
            result.AddValueClicked += Event_AddValueClicked;
            result.AddCaptureItemClicked += Event_AddCaptureItemClicked;

            foreach (Value v in e.values)
            {
                Controls.Value val = CreateValue(v, e);
                result.Values.Add(val);
            }

            foreach (CaptureItem ci in e.captureItems)
            {
                Controls.CaptureItem cap = CreateCaptureItem(ci, e);
                result.CaptureItems.Add(cap);
            }

            // Default
            Controls.Default def = new Controls.Default();
            def.ParentResult = e.Default;
            def.ValueName = e.Default.value;
            def.SettingChanged += def_SettingChanged;
            result.DefaultValue = def;

            return result;
        }

        void Event_AddCaptureItemClicked(Controls.Event e)
        {
            CaptureItem ci = new CaptureItem();

            if (e.ParentEvent != null)
            {
                Controls.CaptureItem item = CreateCaptureItem(ci, e.ParentEvent);
                e.ParentEvent.captureItems.Add(ci);
                e.CaptureItems.Add(item);
            }

            if (SettingChanged != null) SettingChanged(null, null, null);
        }

        void def_SettingChanged()
        {
            ChangeSetting(null, null, null);
        }

        void Event_SettingChanged()
        {
            ChangeSetting(null, null, null);
        }

        void Event_AddValueClicked(Controls.Event e)
        {
            if (e.ParentEvent != null)
            {
                Value val = new Value();

                e.ParentEvent.values.Add(val);

                e.Values.Add(CreateValue(val, e.ParentEvent));
            }

            ChangeSetting(null, null, null);
        }

        #endregion

        #region "Value"

        Controls.Value CreateValue(Value v, Event e)
        {
            Controls.Value result = new Controls.Value();

            result.ParentEvent = e;
            result.ParentValue = v;

            result.SettingChanged += result_SettingChanged;
            result.RemoveClicked += Value_RemoveClicked;
            result.AddTriggerClicked += Value_AddTriggerClicked;

            if (v.result != null)
            {
                result.ValueName = v.result.value.Replace('_', ' ');
            }

            foreach (Trigger t in v.triggers)
            {
                Controls.Trigger tr = CreateTrigger(t, v, e);
                result.Triggers.Add(tr);
            }

            return result;
        }

        void result_SettingChanged()
        {
            ChangeSetting(null, null, null);
        }

        void Value_AddTriggerClicked(Controls.Value val)
        {
            if (val.ParentValue != null && val.ParentEvent != null)
            {
                if (val.ParentEvent.values.Contains(val.ParentValue))
                {
                    Controls.Event e = events.Find(x => x.ParentEvent == val.ParentEvent);
                    if (e != null)
                    {
                        int index = e.Values.ToList().FindIndex(x => x.ParentValue == val.ParentValue);
                        if (index >= 0)
                        {
                            Controls.Value v = e.Values[index];
                            if (v != null)
                            {
                                Trigger t = new Trigger();
                                val.ParentValue.triggers.Add(t);

                                Controls.Trigger tr = CreateTrigger(t, val.ParentValue, val.ParentEvent);
                                tr.SelectedModifier = "Equal To";
                                val.Triggers.Add(tr);
                            }
                        }
                    }
                }
            }
        }

        void Value_RemoveClicked(Controls.Value val)
        {
            if (val.ParentValue != null && val.ParentEvent != null)
            {
                if (val.ParentEvent.values.Contains(val.ParentValue))
                {
                    val.ParentEvent.values.Remove(val.ParentValue);

                    Controls.Event e = events.Find(x => x.ParentEvent == val.ParentEvent);
                    if (e != null)
                    {
                        if (e.Values.Contains(val)) e.Values.Remove(val);
                    }
                }
            }
        }

        #endregion

        #region "Trigger"

        Controls.Trigger CreateTrigger(Trigger t, Value v, Event e)
        {
            Controls.Trigger result = new Controls.Trigger();

            result.ParentPage = this;
            result.ParentEvent = e;
            result.ParentValue = v;
            result.ParentTrigger = t;

            result.SettingChanged += Trigger_SettingChanged;
            result.RemoveClicked += Trigger_RemoveClicked;

            foreach (CollectedItem item in CollectedItems)
            {
                result.DataItems.Add(item.id);
            }

            result.link_COMBO.Text = t.link;

            //result.SelectedLink = t.link;
            result.SelectedModifier = t.modifier;

            result.Value = t.value;

            return result;
        }

        void Trigger_SettingChanged()
        {
            ChangeSetting(null, null, null);
        }

        void Trigger_RemoveClicked(Controls.Trigger t)
        {
            if (t.ParentTrigger != null && t.ParentValue != null && t.ParentEvent != null)
            {
                t.ParentValue.triggers.Remove(t.ParentTrigger);

                if (t.ParentEvent.values.Contains(t.ParentValue))
                {
                    Controls.Event e = events.Find(x => x.ParentEvent == t.ParentEvent);
                    if (e != null)
                    {
                        Controls.Value v = e.Values.ToList().Find(x => x.ParentValue == t.ParentValue);
                        if (v != null)
                        {
                            if (v.Triggers.Contains(t)) v.Triggers.Remove(t);
                        }
                    }
                }
            }
        }

        void Trigger_UpdateCollectedLink(Controls.Trigger item)
        {
            Page.CollectedItem ci = CollectedItems.ToList().Find(x => x.id == item.link_COMBO.Text);
            if (ci != null) item.link_COMBO.Text = ci.display;
        }

        #endregion

        #region "Capture Item"

        Controls.CaptureItem CreateCaptureItem(CaptureItem ci, Event e)
        {
            Controls.CaptureItem result = new Controls.CaptureItem();

            result.ParentPage = this;
            result.ParentEvent = e;
            result.ParentCaptureItem = ci;

            result.SettingChanged += CaptureItem_SettingChanged;
            result.RemoveClicked += CaptureItem_RemoveClicked;

            result.CaptureName = TH_Global.Formatting.UppercaseFirst(ci.name.Replace('_',' '));

            result.link_COMBO.Text = ci.link;

            return result;
        }

        void CaptureItem_SettingChanged()
        {
            ChangeSetting(null, null, null);
        }

        void CaptureItem_RemoveClicked(Controls.CaptureItem item)
        {
            if (item.ParentEvent != null)
            {
                item.ParentEvent.captureItems.Remove(item.ParentCaptureItem);

                Controls.Event e = events.Find(x => x.ParentEvent == item.ParentEvent);
                if (e != null)
                {
                    if (e.CaptureItems.Contains(item)) e.CaptureItems.Remove(item);
                }
            }

            if (SettingChanged != null) SettingChanged(null, null, null);
        }

        void CaptureItem_UpdateCollectedLink(Controls.CaptureItem item)
        {
            Page.CollectedItem ci = CollectedItems.ToList().Find(x => x.id == item.link_COMBO.Text);
            if (ci != null) item.link_COMBO.Text = ci.display;
        }

        #endregion

        #endregion

        private void AddEvent_Clicked(TH_WPF.Button_01 bt)
        {
            Event e = new Event();
            e.name = "EVENT";

            e.Default = new Result();
            e.Default.value = "DEFAULT";

            GeneratedEvents.Add(e);

            AddEvent(e);
            //LoadGeneratedEvents_GUI(e);

            if (EventButtons.Count > 0)
            {
                TH_WPF.CollapseButton cbt = EventButtons[EventButtons.Count - 1];

                foreach (TH_WPF.CollapseButton ocbt in EventButtons.OfType<TH_WPF.CollapseButton>().ToList()) if (ocbt != cbt) ocbt.IsExpanded = false;
                cbt.IsExpanded = true;

                cbt.BringIntoView();
            }

            DisplayEvents = true;

            ChangeSetting(null, null, null);
        }

        void Event_RemoveClicked(Controls.EventButton bt)
        {
            if (bt.ParentEvent != null)
            {
                int index = GeneratedEvents.ToList().FindIndex(x => x == bt.ParentEvent);
                if (index >= 0)
                {
                    Event e = GeneratedEvents[index];

                    GeneratedEvents.Remove(e);

                    index = EventButtons.ToList().FindIndex(x => x.ButtonContent == bt);
                    if (index >= 0)
                    {
                        EventButtons.RemoveAt(index);
                    }
                }
            }

            if (EventButtons.Count > 0) DisplayEvents = true;
            else DisplayEvents = false;

            ChangeSetting(null, null, null);
        }

        #endregion

        public class Event
        {
            public Event() { values = new List<Value>(); captureItems = new List<CaptureItem>(); }

            public List<Value> values { get; set; }

            public List<CaptureItem> captureItems { get; set; }

            public int id { get; set; }
            public string name { get; set; }

            public Result Default { get; set; }

            public string description { get; set; }

            public override string ToString()
            {
                return TH_Global.Formatting.UppercaseFirst(name.Replace('_',' '));
            }
        }

        public class Value
        {
            public Value() { triggers = new List<Trigger>(); }

            public List<Trigger> triggers { get; set; }

            public int id { get; set; }

            public Result result { get; set; }
        }

        public class Trigger
        {
            public int id { get; set; }
            public int numval { get; set; }
            public string value { get; set; } 
            public string link { get; set; }

            public string modifier { get; set; }
        }

        public class Result
        {
            public int numval { get; set; }
            public string value { get; set; }         
        }

        public class CaptureItem
        {
            public int id { get; set; }
            public string name { get; set; }
            public string link { get; set; }
        }

        #endregion

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

    }
}
