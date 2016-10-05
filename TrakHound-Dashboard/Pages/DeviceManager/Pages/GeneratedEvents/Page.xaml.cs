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

using TrakHound;
using TrakHound.Tools;
using MTConnect.Application.Components;
using TrakHound.Plugins;
using TrakHound.Plugins.Server;

namespace TrakHound_Dashboard.Pages.DeviceManager.Pages.GeneratedEvents
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

            LoadEventValues();
        }

        public string Title { get { return "Generated Events"; } }

        public Uri Image { get { return new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Group_01.png"); } }

        public bool Loaded { get; set; }

        public event SettingChanged_Handler SettingChanged;


        public event SendData_Handler SendData;

        public bool ZoomEnabled { get { return false; } }

        public void SetZoom(double zoomPercentage) { }

        public void GetSentData(EventData data)
        {
            GetProbeData(data);
        }

        public void LoadConfiguration(DataTable dt)
        {
            Loading = true;
            loading = true;
            configurationTable = dt;

            List<Event> genEvents = GetGeneratedEvents(dt);

            if (genEvents.Count > 0) DisplayEvents = true;
            else DisplayEvents = false;

            CreateGeneratedEvents(genEvents);

            GeneratedEvents.Clear();
            foreach (Event e in genEvents)
            {
                Dispatcher.BeginInvoke(new Action(() => { GeneratedEvents.Add(e); }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
            }

            // Load MTConnect DataItems using Probe Data
            if (!Loaded) LoadCollectedItems(probeData);

            loading = false;
            Dispatcher.BeginInvoke(new Action(() => { Loading = false; }), System.Windows.Threading.DispatcherPriority.Background, new object[] { });
        }

        public void SaveConfiguration(DataTable dt)
        {
            string prefix = "/GeneratedData/GeneratedEvents/";

            // Clear all generated event rows first (so that Ids can be sequentially assigned)
            DataTable_Functions.TrakHound.DeleteRows(prefix + "*", "address", dt);
           
            if (GeneratedEvents != null)
            {
                foreach (Event e in GeneratedEvents)
                {
                    SaveEvent(e, dt);
                }
            }
        }

        bool loading = false;

        void ChangeSetting(string address, string name, string val)
        {
            string newVal = val;
            string oldVal = null;

            if (configurationTable != null)
            {
                oldVal = DataTable_Functions.GetTableValue(configurationTable, "address", address, "value");
            }

            if (!loading) SettingChanged?.Invoke(name, oldVal, newVal);
        }


        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        DataTable configurationTable;


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Page), new PropertyMetadata(true));


        #region "MTC Data Items"  

        List_Functions.ObservableCollectionEx<CollectedItem> _collectedItems;
        public List_Functions.ObservableCollectionEx<CollectedItem> CollectedItems
        {
            get
            {
                if (_collectedItems == null)
                    _collectedItems = new List_Functions.ObservableCollectionEx<CollectedItem>();
                return _collectedItems;
            }

            set
            {
                _collectedItems = value;
            }
        }

        ObservableCollection<string> _dataItemTypes;
        public ObservableCollection<string> DataItemTypes
        {
            get
            {
                if (_dataItemTypes == null)
                    _dataItemTypes = new ObservableCollection<string>();
                return _dataItemTypes;
            }

            set
            {
                _dataItemTypes = value;
            }
        }

        private List<DataItem> probeData = new List<DataItem>();

        public class CollectedItem : IComparable
        {
            public CollectedItem() { }

            public CollectedItem(DataItem dataItem)
            {
                Id = dataItem.Id;
                Name = dataItem.Name;
                Category = dataItem.Category.ToString();
                Type = dataItem.Type;

                if (Name != null) Display = Id + " : " + Name;
                else Display = Id;
            }

            public string Id { get; set; }
            public string Name { get; set; }

            public string Display { get; set; }

            public string Category { get; set; }
            public string Type { get; set; }

            public override string ToString()
            {
                return Display;
            }

            public CollectedItem Copy()
            {
                var copy = new CollectedItem();
                copy.Id = Id;
                copy.Name = Name;
                copy.Display = Display;
                copy.Category = Category;
                copy.Type = Type;

                return copy;
            }

            public int CompareTo(object obj)
            {
                if (obj == null) return 1;

                var i = obj as CollectedItem;
                if (i != null)
                {
                    return Display.CompareTo(i.Display);
                }
                else return 1;
            }
        }

        void GetProbeData(EventData data)
        {
            if (data != null && data.Id != null && data.Data02 != null)
            {
                if (data.Id.ToLower() == "mtconnect_probe_dataitems")
                {
                    var dataItems = (List<DataItem>)data.Data02;
                    probeData = dataItems;
                    if (Loaded) LoadCollectedItems(dataItems);
                }
            }
        }

        private void LoadCollectedItems(List<DataItem> dataItems)
        {
            var newItems = new List<CollectedItem>();

            foreach (var dataItem in dataItems)
            {
                var item = new CollectedItem(dataItem);
                newItems.Add(item.Copy());
            }

            foreach (var newItem in newItems)
            {
                if (!CollectedItems.ToList().Exists(x => x.Id == newItem.Id)) CollectedItems.Add(newItem);
            }

            foreach (var item in CollectedItems)
            {
                if (!newItems.Exists(x => x.Id == item.Id)) CollectedItems.Remove(item);
            }

            CollectedItems.SupressNotification = true;
            CollectedItems.Sort();
            CollectedItems.SupressNotification = false;


            foreach (Controls.Event ev in events)
            {
                foreach (Controls.CaptureItem ci in ev.CaptureItems)
                {
                    Dispatcher.BeginInvoke(new Action<Controls.CaptureItem>(CaptureItem_UpdateCollectedLink), priority, new object[] { ci });
                }
            }
        }
        
        public MTConnect.Types.EventType[] EventValues;

        void LoadEventValues()
        {
            EventValues = MTConnect.Types.Events.Get();
            var sampleValues = MTConnect.Types.Samples.Get();
            var conditionValues = MTConnect.Types.Conditions.Get();

            DataItemTypes.Clear();

            foreach (var eventValue in EventValues)
            {
                if (!DataItemTypes.ToList().Exists(x => x == eventValue.Type)) DataItemTypes.Add(eventValue.Type);
            } 
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


        ObservableCollection<TrakHound_UI.CollapseButton> eventbuttons;
        public ObservableCollection<TrakHound_UI.CollapseButton> EventButtons
        {
            get
            {
                if (eventbuttons == null)
                    eventbuttons = new ObservableCollection<TrakHound_UI.CollapseButton>();
                return eventbuttons;
            }

            set
            {
                eventbuttons = value;
            }
        }

        public List<Controls.Event> events;

        #region "Save"

        void SaveEvent(Event e, DataTable dt)
        {
            int id = DataTable_Functions.TrakHound.GetUnusedAddressId("/GeneratedData/GeneratedEvents/Event", dt);
            string adr = "/GeneratedData/GeneratedEvents/Event||" + id.ToString("00");

            e.id = id;

            string attr = "";
            attr += "id||" + e.id.ToString("00") + ";";
            attr += "name||" + e.name.Replace(' ', '_').ToLower() + ";";
            attr += "description||" + e.description + ";";

            DataTable_Functions.UpdateTableValue(dt, "address", adr, "attributes", attr);

            int numval = e.values.Count;
            foreach (Value v in e.values)
            {
                if (v.result != null)
                {
                    v.result.numval = numval;
                    numval -= 1;
                }
                SaveValue(v, e, dt);
            }

            foreach (CaptureItem ci in e.captureItems) SaveCaptureItems(ci, e, dt);

            if (e.Default != null)
            {
                string addr = adr + "/Default";
                attr = "";
                attr += "numval||" + e.Default.numval.ToString() + ";";
                string val = e.Default.value;

                DataTable_Functions.UpdateTableValue(dt, "address", addr, "attributes", attr);
                DataTable_Functions.UpdateTableValue(dt, "address", addr, "value", val);
            }
        }

        void SaveValue(Value v, Event e, DataTable dt)
        {
            int id = DataTable_Functions.TrakHound.GetUnusedAddressId("/GeneratedData/GeneratedEvents/Event||" + e.id.ToString("00") + "/Value", dt);
            string adr = "/GeneratedData/GeneratedEvents/Event||" + e.id.ToString("00") + "/Value||" + id.ToString("00");

            v.id = id;

            // Save Root
            string attr = "";
            attr += "id||" + v.id.ToString("00") + ";";
            DataTable_Functions.UpdateTableValue(dt, "address", adr, "attributes", attr);

            // Save Triggers
            foreach (Trigger t in v.triggers.OfType<Trigger>()) SaveTrigger(t, v, e, dt);

            // Save MultiTriggers
            foreach (MultiTrigger mt in v.triggers.OfType<MultiTrigger>()) SaveMultiTrigger(mt, v, e, dt);

            // Save Result
            if (v.result != null)
            {
                string addr = adr + "/Result";
                attr = "";
                attr += "numval||" + v.result.numval.ToString() + ";";
                string val = v.result.value;
                DataTable_Functions.UpdateTableValue(dt, "address", addr, "attributes", attr);
                DataTable_Functions.UpdateTableValue(dt, "address", addr, "value", val);
            }
        }

        void SaveTrigger(Trigger t, Value v, Event e, DataTable dt)
        {
            string adr = "/GeneratedData/GeneratedEvents/Event||" + e.id.ToString("00");
            adr += "/Value||" + v.id.ToString("00") + "/Triggers";

            SaveTrigger(t, v, e, dt, adr);
        }

        void SaveTrigger(Trigger t, Value v, Event e, DataTable dt, string addressPrefix)
        {
            if (t.link != null && t.modifier != null)
            {
                string adr = addressPrefix + "/Trigger";

                int id = DataTable_Functions.TrakHound.GetUnusedAddressId(adr, dt);
                adr = adr + "||" + id.ToString("00");

                t.id = id;

                // Save Root
                string attr = "";
                attr += "id||" + t.id.ToString("00") + ";";

                string link = t.link;
                List<CollectedItem> linkitems = CollectedItems.ToList();
                CollectedItem dataitem = linkitems.Find(x => x.Display == link);
                if (dataitem != null) link = dataitem.Id;

                attr += "link||" + link + ";";
                attr += "link_type||" + t.linkType + ";";

                if (t.modifier != null)
                {
                    switch (t.modifier)
                    {
                        case "Not Equal To": attr += "modifier||" + "not" + ";"; break;
                        case "Greater Than": attr += "modifier||" + "greater_than" + ";"; break;
                        case "Less Than": attr += "modifier||" + "less_than" + ";"; break;
                        case "Contains": attr += "modifier||" + "contains" + ";"; break;
                        case "Contains Match Case": attr += "modifier||" + "contains_match_case" + ";"; break;
                        case "Contains Whole Word": attr += "modifier||" + "contains_whole_word" + ";"; break;
                        case "Contains Whole Word Match Case": attr += "modifier||" + "contains_whole_word_match_case" + ";"; break;
                    }
                }

                attr += "value||" + t.value + ";";

                DataTable_Functions.UpdateTableValue(dt, "address", adr, "attributes", attr);
            }
        }

        void SaveMultiTrigger(MultiTrigger mt, Value v, Event e, DataTable dt)
        {
            string adr = "/GeneratedData/GeneratedEvents/Event||" + e.id.ToString("00");
            adr += "/Value||" + v.id.ToString("00") + "/Triggers";
            adr += "/MultiTrigger";

            int id = DataTable_Functions.TrakHound.GetUnusedAddressId(adr, dt);
            adr = adr + "||" + id.ToString("00");

            // Set Attributes
            string attr = "";
            attr += "id||" + id.ToString("00") + ";";

            DataTable_Functions.UpdateTableValue(dt, "address", adr, "attributes", attr);

            foreach (var trigger in mt.triggers)
            {
                SaveTrigger(trigger, v, e, dt, adr);
            }
        }

        void SaveCaptureItems(CaptureItem ci, Event e, DataTable dt)
        {
            string adr = "/GeneratedData/GeneratedEvents/Event||" + e.id.ToString("00") + "/Capture/Item";
            int id = DataTable_Functions.TrakHound.GetUnusedAddressId(adr, dt);
            adr = adr + "||" + id.ToString("00");

            ci.id = id;

            // Save Root
            string attr = "";
            attr += "id||" + ci.id.ToString("00") + ";";
            attr += "name||" + ci.name.Replace(' ', '_').ToLower() + ";";

            string link = ci.link;
            List<CollectedItem> linkitems = CollectedItems.ToList();
            CollectedItem dataitem = linkitems.Find(x => x.Display == link);
            if (dataitem != null) link = dataitem.Id;

            attr += "link||" + link + ";";
            DataTable_Functions.UpdateTableValue(dt, "address", adr, "attributes", attr);
        }

        #endregion

        #region "Load"

        public static List<Event> GetGeneratedEvents(DataTable dt)
        {
            var result = new List<Event>();

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
                        if (t != null)
                        {
                            var match = GetMultiTriggerFromRow(v, row);
                            if (match != null)
                            {
                                MultiTrigger multiTrigger = v.triggers.OfType<MultiTrigger>().ToList().Find(x => x.id == match.id);
                                if (multiTrigger != null)
                                {
                                    multiTrigger.triggers.Add(t);
                                }  
                            }
                            else
                            {
                                v.triggers.Add(t);
                            }
                        }

                        MultiTrigger mt = GetMultiTriggerFromRow(v, row);
                        if (mt != null)
                        {
                            MultiTrigger multiTrigger = v.triggers.OfType<MultiTrigger>().ToList().Find(x => x.id == mt.id);
                            if (multiTrigger == null) v.triggers.Add(mt);
                        }

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

            string lastNode = DataTable_Functions.TrakHound.GetLastNode(row);

            if (lastNode != null)
            {
                if (lastNode.ToLower() == "event")
                {
                    int eventIndex = adr.IndexOf("Event");
                    int slashIndex = adr.IndexOf('/', eventIndex) + 1;
                    int separatorIndex = adr.IndexOf("||", slashIndex);

                    if (separatorIndex > slashIndex)
                    {
                        string name = DataTable_Functions.TrakHound.GetRowAttribute("name", row);
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
                                result.description = DataTable_Functions.TrakHound.GetRowAttribute("description", row);
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

            string lastNode = DataTable_Functions.TrakHound.GetLastNode(row);
            if (lastNode != null)
            {
                if (lastNode.ToLower() == "value")
                {
                    string strId = DataTable_Functions.TrakHound.GetRowAttribute("id", row);
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
                string n = DataTable_Functions.TrakHound.GetRowAttribute("numval", row);
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
                string n = DataTable_Functions.TrakHound.GetRowAttribute("numval", row);
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

        static MultiTrigger GetMultiTriggerFromRow(Value v, DataRow row)
        {
            MultiTrigger result = null;

            string adr = row["address"].ToString();

            if (adr.Contains("MultiTrigger"))
            {
                int eventIndex = adr.IndexOf("MultiTrigger");
                int separatorIndex = adr.IndexOf("||", eventIndex);
                int slashIndex = adr.IndexOf("/", separatorIndex + 1);

                if (slashIndex > separatorIndex || slashIndex == -1)
                {
                    string val = row["value"].ToString();
                    string strId = null;
                    if (slashIndex >= 0) strId = adr.Substring(separatorIndex + 2, 2);
                    else strId = adr.Substring(separatorIndex + 2);

                    int id;
                    if (int.TryParse(strId, out id))
                    {
                        result = new MultiTrigger();
                        result.id = id;
                    }
                }
            }

            return result;
        }

        static Trigger GetTriggerFromRow(Value v, DataRow row)
        {
            Trigger result = null;

            string adr = row["address"].ToString();

            if (DataTable_Functions.TrakHound.GetLastNode(row) == "Trigger")
            {
                int eventIndex = adr.LastIndexOf("Trigger");
                int slashIndex = adr.IndexOf('/', eventIndex) + 1;
                int separatorIndex = adr.IndexOf("||", slashIndex);

                if (separatorIndex > slashIndex)
                {
                    string val = row["value"].ToString();
                    string strId = adr.Substring(separatorIndex + 2, 2);

                    int id;
                    if (int.TryParse(strId, out id))
                    {
                        result = new Trigger();
                        result.id = id;

                        result.linkType = DataTable_Functions.TrakHound.GetRowAttribute("link_type", row);
                        if (result.linkType == null) result.linkType = "ID";

                        result.link = DataTable_Functions.TrakHound.GetRowAttribute("link", row);
                        result.value = DataTable_Functions.TrakHound.GetRowAttribute("value", row);

                        string modifier = "Equal To";
                        string mod = DataTable_Functions.TrakHound.GetRowAttribute("modifier", row);
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

            return result;
        }

        static CaptureItem GetCaptureItemFromRow(Event e, DataRow row)
        {
            CaptureItem result = null;

            string adr = row["address"].ToString();

            if (adr.Contains("Capture"))
            {
                string node = DataTable_Functions.TrakHound.GetLastNode(row);
                if (node.ToLower() == "item")
                {
                    string strId = DataTable_Functions.TrakHound.GetRowAttribute("id", row);
                    int id;
                    if (int.TryParse(strId, out id))
                    {
                        result = new CaptureItem();
                        result.id = id;
                        result.name = DataTable_Functions.TrakHound.GetRowAttribute("name", row);
                        result.link = DataTable_Functions.TrakHound.GetRowAttribute("link", row);
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

            foreach (Event e in genEvents)
            {
                Dispatcher.BeginInvoke(new Action<Event, bool>(AddEvent), priority, new object[] { e, false});
            }
        }

        void AddEvent(Event e, bool select = false)
        {
            Controls.Event ev = CreateEvent(e);

            var event_bt = new Controls.EventButton();
            event_bt.EventName = String_Functions.UppercaseFirst(e.name.Replace('_', ' '));
            event_bt.SettingChanged += event_bt_SettingChanged;
            event_bt.RemoveClicked += Event_RemoveClicked;
            event_bt.ParentEvent = e;

            var bt = new TrakHound_UI.CollapseButton();
            bt.ButtonContent = event_bt;

            if (select)
            {
                event_bt.eventname_TXT.Focus();

                foreach (var obt in EventButtons) obt.IsExpanded = false;
                bt.IsExpanded = true;
            }

            bt.PageContent = ev;

            events.Add(ev);

            EventButtons.Add(bt);
        }

        void event_bt_SettingChanged()
        {
            if (!loading) SettingChanged?.Invoke(null, null, null);
        }

        #region "Event"

        Controls.Event CreateEvent(Event e)
        {
            var result = new Controls.Event();
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
            var def = new Controls.Default();
            def.ParentResult = e.Default;
            if (e.Default != null) def.ValueName = e.Default.value;
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

            SettingChanged?.Invoke(null, null, null);
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
                var value = new Value();

                e.ParentEvent.values.Add(value);

                var uiValue = CreateValue(value, e.ParentEvent);

                var t = new Trigger();
                uiValue.ParentValue.triggers.Add(t);

                Controls.Trigger tr = CreateTrigger(t, uiValue.ParentValue, uiValue.ParentEvent);
                tr.modifier_COMBO.SelectedItem = "Equal To";
                uiValue.Triggers.Add(tr);

                e.Values.Add(uiValue);
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
            result.AddMultiTriggerClicked += Value_AddMultiTriggerClicked;

            if (v.result != null)
            {
                result.ValueName = v.result.value.Replace('_', ' ');
            }

            foreach (Trigger t in v.triggers.OfType<Trigger>())
            {
                Controls.Trigger tr = CreateTrigger(t, v, e);
                result.Triggers.Add(tr);
            }

            foreach (MultiTrigger t in v.triggers.OfType<MultiTrigger>())
            {
                Controls.MultiTrigger tr = CreateMultiTrigger(t, v, e);
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
                                var t = new Trigger();
                                val.ParentValue.triggers.Add(t);

                                Controls.Trigger tr = CreateTrigger(t, val.ParentValue, val.ParentEvent);
                                tr.modifier_COMBO.SelectedItem = "Equal To";
                                val.Triggers.Add(tr);
                            }
                        }
                    }
                }
            }
        }

        private void Value_AddMultiTriggerClicked(Controls.Value val)
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
                                var multitrigger = new MultiTrigger();
                                val.ParentValue.triggers.Add(multitrigger);

                                Controls.MultiTrigger uiMultiTrigger = CreateMultiTrigger(multitrigger, val.ParentValue, val.ParentEvent);
                                
                                var trigger = new Trigger();
                                multitrigger.triggers.Add(trigger);

                                Controls.Trigger uiTrigger = CreateTrigger(trigger, uiMultiTrigger.ParentValue, uiMultiTrigger.ParentEvent);
                                uiTrigger.modifier_COMBO.SelectedItem = "Equal To";
                                uiMultiTrigger.Triggers.Add(uiTrigger);

                                val.Triggers.Add(uiMultiTrigger);
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
            var result = new Controls.Trigger();

            result.ParentPage = this;
            result.ParentEvent = e;
            result.ParentValue = v;
            result.ParentTrigger = t;

            result.SettingChanged += Trigger_SettingChanged;
            result.RemoveClicked += Trigger_RemoveClicked;

            foreach (CollectedItem item in CollectedItems)
            {
                result.DataItems.Add(item.Id);
            }

            result.LinkType = t.linkType;

            //result.link_COMBO.Text = t.link;
            result.SelectedLink = t.link;

            //result.SelectedLink = t.link;
            result.modifier_COMBO.SelectedItem = t.modifier;

            result.value_COMBO.SelectedItem = t.value;
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

        #endregion

        #region "MultiTrigger"

        Controls.MultiTrigger CreateMultiTrigger(MultiTrigger t, Value v, Event e)
        {
            var result = new Controls.MultiTrigger();

            result.ParentPage = this;
            result.ParentEvent = e;
            result.ParentValue = v;
            result.ParentMultiTrigger = t;

            foreach (var trigger in t.triggers)
            {
                result.Triggers.Add(CreateTrigger(trigger, v, e));
            }

            result.AddTriggerClicked += MultiTrigger_AddTriggerClicked;
            result.SettingChanged += MultiTrigger_SettingChanged;
            result.RemoveClicked += MultiTrigger_RemoveClicked;

            return result;
        }

        private void MultiTrigger_AddTriggerClicked(Controls.MultiTrigger sender)
        {
            if (sender.ParentMultiTrigger != null && sender.ParentValue != null && sender.ParentEvent != null)
            {
                if (sender.ParentEvent.values.Contains(sender.ParentValue))
                {
                    Controls.Event e = events.Find(x => x.ParentEvent == sender.ParentEvent);
                    if (e != null)
                    {
                        int index = e.Values.ToList().FindIndex(x => x.ParentValue == sender.ParentValue);
                        if (index >= 0)
                        {
                            Controls.Value v = e.Values[index];
                            if (v != null)
                            {
                                var t = new Trigger();
                                Controls.Trigger tr = CreateTrigger(t, sender.ParentValue, sender.ParentEvent);

                                sender.ParentMultiTrigger.triggers.Add(t);

                                tr.modifier_COMBO.SelectedItem = "Equal To";
                                sender.Triggers.Add(tr);
                            }
                        }
                    }
                }
            }
        }

        void MultiTrigger_SettingChanged()
        {
            ChangeSetting(null, null, null);
        }

        void MultiTrigger_RemoveClicked(Controls.MultiTrigger t)
        {
            if (t.ParentMultiTrigger != null && t.ParentValue != null && t.ParentEvent != null)
            {
                t.ParentValue.triggers.Remove(t.ParentMultiTrigger);

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

            if (ci.name != null)
            {
                result.CaptureName = String_Functions.UppercaseFirst(ci.name.Replace('_', ' '));
            }

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

            SettingChanged?.Invoke(null, null, null);
        }

        void CaptureItem_UpdateCollectedLink(Controls.CaptureItem item)
        {
            Page.CollectedItem ci = CollectedItems.ToList().Find(x => x.Id == item.link_COMBO.Text);
            if (ci != null) item.link_COMBO.Text = ci.Display;
        }

        #endregion

        #endregion

        private void AddEvent_Clicked(TrakHound_UI.Button bt)
        {
            Event e = new Event();
            e.name = "EVENT";

            e.Default = new Result();
            e.Default.value = "DEFAULT";

            GeneratedEvents.Add(e);

            AddEvent(e, true);

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
                return String_Functions.UppercaseFirst(name.Replace('_', ' '));
            }
        }

        public class Value
        {
            public Value() { triggers = new List<object>(); }

            public List<object> triggers { get; set; }

            public int id { get; set; }

            public Result result { get; set; }
        }

        public class MultiTrigger
        {
            public MultiTrigger() { triggers = new List<Trigger>(); }

            public int id { get; set; }

            public List<Trigger> triggers { get; set; }
        }

        public class Trigger
        {
            public int id { get; set; }
            public int numval { get; set; }
            public string value { get; set; }
            public string link { get; set; }
            public string linkType { get; set; }

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

    }
}
