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

using TH_Configuration;
using TH_Configuration.User;
using TH_Global.Functions;
using TH_PlugIns_Server;
using TH_MTC_Data.Components;
using TH_MTC_Requests;

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

        List<Event> GeneratedEvents;

        public void LoadConfiguration(DataTable dt)
        {
            if (!Loading)
            {
                Loading = true;

                configurationTable = dt;

                GeneratedEvents = GetGeneratedEvents(dt);

                LoadAgentSettings(dt);
            }
        }

        public void SaveConfiguration(DataTable dt)
        {

            SaveSnapshotData(dt);

            SaveGeneratedEvents(dt);

        }

        bool Loading;

        void ChangeSetting(string address, string name, string val)
        {
            if (!Loading)
            {
                string newVal = val;
                string oldVal = null;

                if (configurationTable != null)
                {
                    oldVal = Table_Functions.GetTableValue(address, configurationTable);
                }

                if (SettingChanged != null) SettingChanged(name, oldVal, newVal);
            }
        }


        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;


        DataTable configurationTable;
       

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

        void item_TypeChanged(string type, Controls.Snapshot_Item item)
        {
            item.LinkItems.Clear();

            foreach (string link in GenerateLinkList(type))
            {
                item.LinkItems.Add(link);
            }
        }

        void AssignLinkItems(string type, Controls.Snapshot_Item item)
        {
            item.LinkItems = GenerateLinkList(type);
        }


        ObservableCollection<string> GenerateTypeList()
        {
            ObservableCollection<string> result = new ObservableCollection<string>();

            result.Add("Collected");
            result.Add("Generated");
            result.Add("Variable");

            return result;
        }

        ObservableCollection<object> GenerateLinkList(string type)
        {
            ObservableCollection<object> result = new ObservableCollection<object>();

            switch (type.ToLower())
            {
                case "collected":

                    CollectedItems.Sort((x, y) => string.Compare(x.id, y.id));

                    //foreach (CollectedItem ci in CollectedItems) result.Add(ci.id);
                    foreach (CollectedItem ci in CollectedItems) result.Add(ci.display);

                    break;

                case "generated":

                    foreach (Event e in GeneratedEvents)
                    {
                        result.Add(TH_Global.Formatting.UppercaseFirst(e.name));
                    }

                    break;

                case "variable":


                    break;
            }

            return result;
        }

        //string GetDataItemId(string s)
        //{
        //    if (s.Contains(':'))
        //    {
        //        int index = s.IndexOf(':');
        //        return s.Substring(0, index).Trim();
        //    }
        //    return s;
        //}


        #region "MTC Data Items"  
     
        List<CollectedItem> CollectedItems = new List<CollectedItem>();

        class CollectedItem
        {
            public string id { get; set; }
            public string name { get; set; }

            public string display { get; set; }
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

                    // Conditions
                    foreach (DataItem dataItem in dataItems.Conditions) this.Dispatcher.BeginInvoke(new Action<DataItem>(AddDataItem), priority, new object[] { dataItem });

                    // Events
                    foreach (DataItem dataItem in dataItems.Events) this.Dispatcher.BeginInvoke(new Action<DataItem>(AddDataItem), priority, new object[] { dataItem });

                    // Samples
                    foreach (DataItem dataItem in dataItems.Samples) this.Dispatcher.BeginInvoke(new Action<DataItem>(AddDataItem), priority, new object[] { dataItem });
                }
            }

            // Set 'Loading' to false
            this.Dispatcher.BeginInvoke(new Action(OmitProbeFinished), priority, null);
        }

        void AddDataItem(DataItem item)
        {
            CollectedItem ci = new CollectedItem();
            ci.id = item.id;
            ci.name = item.name;

            if (ci.name != null) ci.display = ci.id + " : " + ci.name;
            else ci.display = ci.id;

            CollectedItems.Add(ci);
        }

        void OmitProbeFinished()
        {
            LoadSnapshotItems(configurationTable);

            LoadGeneratedEvents(GeneratedEvents);

            Loading = false;
        }


        void probe_ProbeError(Probe.ErrorData errorData)
        {

            // Set 'Loading' to false
            this.Dispatcher.BeginInvoke(new Action(OmitProbeFinished), priority, null);

        }

        #endregion

        #region "Snapshot Data"

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

            // Loop through SnapshotItems and add each item back to table with sequential id's
            foreach (Controls.Snapshot_Item item in SnapshotItems)
            {
                if (item.NameText != null && item.SelectedLink != null)
                {
                    int id = 0;
                    string adr = "/GeneratedData/SnapShotData/" + TH_Global.Formatting.UppercaseFirst(item.SelectedType) + "||";
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
                    attr += "name||" + item.NameText + ";";

                    string link = item.SelectedLink;
                    if (item.SelectedType.ToLower() == "collected")
                    {
                        CollectedItem ci = CollectedItems.Find(x => x.display == link);
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

        void LoadSnapshotItems(DataTable dt)
        {
            string address = "/GeneratedData/SnapShotData/";

            string filter = "address LIKE '" + address + "*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();
            temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

            SnapshotItems.Clear();

            foreach (DataRow row in temp_dt.Rows)
            {
                string type = GetSnapShotType(row);

                string name = GetAttribute("name", row);
                string link = GetAttribute("link", row);

                Controls.Snapshot_Item item = new Controls.Snapshot_Item();
                item.NameText = name;

                item.TypeChanged += item_TypeChanged;

                item.TypeItems = GenerateTypeList();
                item.SelectedType = type;

                if (link != null)
                {
                    if (type.ToLower() == "collected")
                    {
                        CollectedItem ci = CollectedItems.Find(x => x.id == link);
                        if (ci != null) link = ci.display;
                    }
                    else if (type.ToLower() == "generated")
                    {
                        link = TH_Global.Formatting.UppercaseFirst(link.Replace('_', ' '));
                    }
                }
                
                item.SelectedLink = link;

                item.SettingChanged += item_SettingChanged;
                item.RemoveClicked += item_RemoveClicked;
                item.RefreshClicked += item_RefreshClicked;

                SnapshotItems.Add(item);
            }
        }

        void item_RefreshClicked(Controls.Snapshot_Item item)
        {
            if (configurationTable != null)
            {
                GeneratedEvents = GetGeneratedEvents(configurationTable);
            }
        }

        void item_SettingChanged()
        {
            if (!Loading) if (SettingChanged != null) SettingChanged(null, null, null);
        }

        void item_RemoveClicked(Controls.Snapshot_Item item)
        {
            this.Dispatcher.BeginInvoke(new Action<Controls.Snapshot_Item>(RemoveSnapshotItem), priority, new object[] { item });
        }

        List<Controls.Snapshot_Item> Snapshot_RemoveList = new List<Controls.Snapshot_Item>();

        void RemoveSnapshotItem(Controls.Snapshot_Item item)
        {
            SnapshotItems.Remove(item);
        }

        private void AddValue_Clicked(TH_WPF.Button_01 bt)
        {
            Controls.Snapshot_Item item = new Controls.Snapshot_Item();
            
            item.TypeChanged += item_TypeChanged;

            item.TypeItems = GenerateTypeList();
            item.SelectedType = item.TypeItems[0];

            item.SettingChanged += item_SettingChanged;
            item.RemoveClicked += item_RemoveClicked;
            item.RefreshClicked += item_RefreshClicked;

            SnapshotItems.Insert(0, item);
        }

        //void AddSnapShotItem(Controls.Snapshot_Item item)
        //{
        //    SnapshotItems.Add(item);
        //}

        string GetSnapShotType(DataRow row)
        {
            string adr = row["address"].ToString();

            int slashIndex = adr.LastIndexOf('/');
            int idIndex = adr.IndexOf("||");

            return adr.Substring(slashIndex + 1, idIndex - slashIndex - 1);
        }

        #endregion

        #region "Generated Events"

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


            foreach (Event e in GeneratedEvents)
            {
                SaveEvent(e, dt);
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
            attr += "name||" + e.name.Replace(' ','_').ToLower() + ";";
            attr += "description||" + e.description + ";";

            

            Table_Functions.UpdateTableValue(null, attr, adr, dt);

            foreach (Value v in e.values) SaveValue(v, e, dt);

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


            //string prefix = "/GeneratedData/GeneratedEvents/Event||" + e.id.ToString("00");
            //prefix += "/Value||" + v.id.ToString("00");

            // Save Root
            string attr = "";
            attr += "id||" + v.id.ToString("00") + ";";
            Table_Functions.UpdateTableValue(null, attr, adr, dt);

            

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

                //string prefix = "/GeneratedData/GeneratedEvents/Event||" + e.id.ToString("00");
                //prefix += "/Value||" + v.id.ToString("00");
                //prefix += "/Triggers";
                //prefix += "/Trigger||" + t.id.ToString("00");

                // Save Root
                string attr = "";
                attr += "id||" + t.id.ToString("00") + ";";
                attr += "link||" + t.link + ";";
                attr += "value||" + t.value + ";";



                Table_Functions.UpdateTableValue(null, attr, adr, dt);
            }        
        }


        void LoadGeneratedEvents(List<Event> genEvents)
        {
            EventButtons.Clear();

            events = new List<Controls.Event>();

            bool first = true;

            foreach (Event e in genEvents)
            {
                Controls.Event ev = CreateEvent(e);

                Controls.EventButton event_bt = new Controls.EventButton();
                event_bt.EventName = TH_Global.Formatting.UppercaseFirst(e.name.Replace('_', ' '));
                event_bt.ParentEvent = e;

                TH_WPF.CollapseButton bt = new TH_WPF.CollapseButton();
                bt.ButtonContent = event_bt;

                if (first) bt.IsExpanded = true;
                first = false;

                bt.PageContent = ev;

                events.Add(ev);

                this.Dispatcher.BeginInvoke(new Action<TH_WPF.CollapseButton>(AddEventButton), priority, new object[] { bt });
            }
        }


        void AddEventButton(TH_WPF.CollapseButton bt)
        {
            EventButtons.Add(bt);
        }

        Controls.Event CreateEvent(Event e)
        {
            Controls.Event result = new Controls.Event();
            result.ParentEvent = e;

            result.Description = e.description;

            foreach (Value v in e.values)
            {
                Controls.Value val = CreateValue(v, e);
                result.Values.Add(val);
            }

            // Default
            Controls.Default def = new Controls.Default();
            def.ParentResult = e.Default;
            def.ValueName = e.Default.value;
            result.DefaultValue = def;

            return result;
        }

        Controls.Value CreateValue(Value v, Event e)
        {
            Controls.Value result = new Controls.Value();

            result.ParentEvent = e;
            result.ParentValue = v;
            result.RemoveClicked += Value_RemoveClicked;
            result.AddTriggerClicked += Value_AddTriggerClicked;

            if (v.result != null)
            {
                result.ValueName = v.result.value.Replace('_',' ');
            }

            foreach (Trigger t in v.triggers)
            {
                Controls.Trigger tr = CreateTrigger(t, v, e);
                result.Triggers.Add(tr);
            }

            result.TriggerCount = v.triggers.Count.ToString();

            return result;
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
                                val.Triggers.Add(tr);
                            }
                        }
                        
                        //Controls.Value v = e.Values.ToList().Find(x => x.ParentValue == val.ParentValue);
                        //if (v != null)
                        //{
                        //    Trigger t = new Trigger();
                        //    val.ParentValue.triggers.Add(t);

                        //    Controls.Trigger tr = CreateTrigger(t, val.ParentValue, val.ParentEvent);
                        //    val.Triggers.Add(tr);
                        //}
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

        Controls.Trigger CreateTrigger(Trigger t, Value v, Event e)
        {
            Controls.Trigger result = new Controls.Trigger();

            result.ParentEvent = e;
            result.ParentValue = v;
            result.ParentTrigger = t;
            result.RemoveClicked += trigger_RemoveClicked;

            foreach (CollectedItem item in CollectedItems)
            {
                result.DataItems.Add(item.id);      
            }

            result.SelectedLink = t.link;
            result.SelectedModifier = t.modifier;

            result.Value = t.value;

            return result;
        }

        void trigger_RemoveClicked(Controls.Trigger t)
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


        List<Event> GetGeneratedEvents(DataTable dt)
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
                string eventfilter = filter = "address LIKE '" + address + "Event||" + e.id.ToString("00") + "/";
                dv = dt.AsDataView();
                dv.RowFilter = filter + "*'";
                temp_dt = dv.ToTable();
                temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

                foreach (DataRow row in temp_dt.Rows)
                {
                    Value v = GetValueFromRow(e, row);
                    if (v != null) e.values.Add(v);

                    GetDefaultFromRow(e, row);
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


        Event GetEventFromRow(List<Event> genEvents, DataRow row)
        {
            Event result = null;

            string adr = row["address"].ToString();

            string lastNode = GetLastNode(row);

            if (lastNode != null)
            {
                if (lastNode.ToLower() == "event")
                {
                    int eventIndex = adr.IndexOf("Event");
                    int slashIndex = adr.IndexOf('/', eventIndex) + 1;
                    int separatorIndex = adr.IndexOf("||", slashIndex);

                    if (separatorIndex > slashIndex)
                    {
                        string name = GetAttribute("name", row);
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
                                result.description = GetAttribute("description", row);
                            }
                        }
                    }
                }
            }

            return result;
        }

        Value GetValueFromRow(Event e, DataRow row)
        {
            Value result = null;

            string adr = row["address"].ToString();

            string lastNode = GetLastNode(row);

            if (lastNode != null)
            {
                if (lastNode.ToLower() == "value")
                {
                    string strId = GetAttribute("id", row);
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

        void GetDefaultFromRow(Event e, DataRow row)
        {
            string adr = row["address"].ToString();

            if (adr.Contains("Default"))
            {
                string n = GetAttribute("numval", row);
                if (n != null)
                {
                    int numval;
                    if (int.TryParse(n, out numval))
                    {
                        Result r = new Result();
                        r.value = row["value"].ToString().Replace('_',' ');;
                        r.numval = numval;
                        e.Default = r;
                    }
                }
            }
        }

        void GetResultFromRow(Value v, DataRow row)
        {
            string adr = row["address"].ToString();

            if (adr.Contains("Result"))
            {
                string n = GetAttribute("numval", row);
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

        Trigger GetTriggerFromRow(Value v, DataRow row)
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
                            result.link = GetAttribute("link", row);
                            result.value = GetAttribute("value", row);

                            string modifier = "Equal To";
                            string mod = GetAttribute("modifier", row);
                            if (mod != null)
                            {
                                switch(mod.ToLower())
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


        public class Event
        {
            public Event() { values = new List<Value>(); }

            public List<Value> values { get; set; }

            public int id { get; set; }
            public string name { get; set; }

            public Result Default { get; set; }

            public string description { get; set; }
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

        #endregion

    }
}
