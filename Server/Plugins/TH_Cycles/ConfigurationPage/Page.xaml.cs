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

using TH_Plugins_Server;
using TH_Configuration;
using TH_Global.Functions;
using TH_MTConnect.Components;
using TH_UserManagement;
using TH_UserManagement.Management;

using TH_GeneratedData.ConfigurationPage;

namespace TH_Cycles.ConfigurationPage
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, TH_Plugins_Server.ConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string PageName
        {
            get { return (string)GetValue(PageNameProperty); }
        }

        public static readonly DependencyProperty PageNameProperty =
            DependencyProperty.Register("PageName", typeof(string), typeof(Page), new PropertyMetadata("Cycles"));

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(Page), new PropertyMetadata(new BitmapImage(new Uri("pack://application:,,,/TH_Cycles;component/Resources/Cycle_01.png"))));

        public event SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            configurationTable = dt;

            GetGeneratedEvents(dt);
            LoadAgentSettings(dt);

            GetProductionTypeValues();
            LoadProductionTypes(dt);
            LoadOverrideLinks(dt);

            ClearData();
            LoadData(dt);
        }

        void ClearData()
        {
            SelectedCycleEventName = null;
            SelectedStoppedEventValue = null;
            SelectedCycleNameLink = null;
            ProductionTypes.Clear();
        }

        void LoadData(DataTable dt)
        {
            LoadCycleEventName(dt);
            LoadStoppedEventValueName(dt);
            LoadCycleNameLink(dt);
            LoadProductionTypes(dt);
            LoadOverrideLinks(dt);
        }

        public void SaveConfiguration(DataTable dt)
        {
            SaveCycleEventName(dt);
            SaveStoppedEventValueName(dt);
            SaveCycleNameLink(dt);
            SaveProductionTypes(dt);
            SaveOverrideLinks(dt);
        }

        public Page_Type PageType { get; set; }

        DataTable configurationTable;

        const string prefix = "/Cycles/";

        ObservableCollection<object> cycleNameLinks;
        public ObservableCollection<object> CycleNameLinks
        {
            get
            {
                if (cycleNameLinks == null)
                    cycleNameLinks = new ObservableCollection<object>();
                return cycleNameLinks;
            }

            set
            {
                cycleNameLinks = value;
            }
        }

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

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

            public string category { get; set; }
            public string type { get; set; }

            public override string ToString()
            {
                return display;
            }
        }

        void LoadAgentSettings(DataTable dt)
        {
            string prefix = "/Agent/";

            string ip = Table_Functions.GetTableValue(prefix + "Address", dt);
            // Get deprecated value if new value is not found
            if (String.IsNullOrEmpty(ip)) ip = Table_Functions.GetTableValue(prefix + "IP_Address", dt);

            string p = Table_Functions.GetTableValue(prefix + "Port", dt);

            string devicename = Table_Functions.GetTableValue(prefix + "DeviceName", dt);
            // Get deprecated value if new value is not found
            if (String.IsNullOrEmpty(devicename)) devicename = Table_Functions.GetTableValue(prefix + "Device_Name", dt);

            string proxyAddress = Table_Functions.GetTableValue(prefix + "ProxyAddress", dt);
            string proxyPort = Table_Functions.GetTableValue(prefix + "ProxyPort", dt);

            int port;
            int.TryParse(p, out port);

            // Proxy Settings
            TH_MTConnect.HTTP.ProxySettings proxy = null;
            if (proxyPort != null)
            {
                int proxy_p = -1;
                if (int.TryParse(proxyPort, out proxy_p))
                {
                    proxy = new TH_MTConnect.HTTP.ProxySettings();
                    proxy.Address = proxyAddress;
                    proxy.Port = proxy_p;
                }
            }

            CollectedItems.Clear();

            RunProbe(ip, proxy, port, devicename);
        }

        Thread runProbe_THREAD;

        class Probe_Info
        {
            public string address;
            public int port;
            public string deviceName;
            public TH_MTConnect.HTTP.ProxySettings proxy;
        }

        void RunProbe(string address, TH_MTConnect.HTTP.ProxySettings proxy, int port, string deviceName)
        {
            if (runProbe_THREAD != null) runProbe_THREAD.Abort();

            var info = new Probe_Info();
            info.address = address;
            info.port = port;
            info.deviceName = deviceName;
            info.proxy = proxy;

            runProbe_THREAD = new Thread(new ParameterizedThreadStart(RunProbe_Worker));
            runProbe_THREAD.Start(info);
        }

        void RunProbe_Worker(object o)
        {
            if (o != null)
            {
                var info = o as Probe_Info;
                if (info != null)
                {
                    string url = TH_MTConnect.HTTP.GetUrl(info.address, info.port, info.deviceName);

                    ReturnData returnData = TH_MTConnect.Components.Requests.Get(url, info.proxy, 2000, 1);
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
                    else
                    {

                    }

                    // Set 'Loading' to false
                    this.Dispatcher.BeginInvoke(new Action(ProbeFinished), priority, null);
                }
            }
        }

        void AddDataItems(List<DataItem> items)
        {
            List<CollectedItem> list = new List<CollectedItem>();

            foreach (DataItem item in items)
            {
                CollectedItem ci = new CollectedItem();
                ci.id = item.id;
                ci.name = item.name;
                ci.category = item.category;
                ci.type = item.type;

                if (ci.name != null) ci.display = ci.id + " : " + ci.name;
                else ci.display = ci.id;

                if (list.Find(x => x.id == ci.id) == null) list.Add(ci);
            }

            list.Sort((x, y) => string.Compare(x.id, y.id));

            foreach (CollectedItem item in list) CollectedItems.Add(item);

        }

        void ProbeFinished()
        {
            this.Dispatcher.BeginInvoke(new Action(CycleNameLink_UpdateCollectedLink), priority, new object[] { });

            foreach (var item in OverrideLinks)
            {
                this.Dispatcher.BeginInvoke(new Action<Controls.OverrideLinkItem>(OverrideLink_UpdateCollectedLink), priority, new object[] { item });
            }

            //foreach (Controls.Snapshot_Item item in SnapshotItems)
            //{
            //    this.Dispatcher.BeginInvoke(new Action<Controls.Snapshot_Item>(SnapshotItem_UpdateCollectedLink), priority, new object[] { item });
            //}

            //foreach (Controls.Event ev in events)
            //{
            //    foreach (Controls.Value v in ev.Values)
            //    {
            //        foreach (Controls.Trigger t in v.Triggers)
            //        {
            //            this.Dispatcher.BeginInvoke(new Action<Controls.Trigger>(Trigger_UpdateCollectedLink), priority, new object[] { t });
            //        }
            //    }

            //    foreach (Controls.CaptureItem ci in ev.CaptureItems)
            //    {
            //        this.Dispatcher.BeginInvoke(new Action<Controls.CaptureItem>(CaptureItem_UpdateCollectedLink), priority, new object[] { ci });
            //    }
            //}
        }

        void CycleNameLink_UpdateCollectedLink()
        {
            if (SelectedCycleNameLink != null)
            {
                Page.CollectedItem ci = CollectedItems.ToList().Find(x => x.id == SelectedCycleNameLink.ToString());
                if (ci != null) SelectedCycleNameLink = ci.display;
            }
        }

        void OverrideLink_UpdateCollectedLink(Controls.OverrideLinkItem item)
        {
            Page.CollectedItem ci = CollectedItems.ToList().Find(x => x.id == item.collectedlink_COMBO.Text);
            if (ci != null) item.collectedlink_COMBO.Text = ci.display;
        }

        #endregion

        #region "Generated Events"

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

        void GetGeneratedEvents(DataTable dt)
        {
            GeneratedEvents.Clear();

            genEvents = TH_GeneratedData.ConfigurationPage.Page.GetGeneratedEvents(dt);

            if (genEvents != null)
            {
                foreach (TH_GeneratedData.ConfigurationPage.Page.Event ev in genEvents)
                {
                    GeneratedEvents.Add(String_Functions.UppercaseFirst(ev.name.Replace('_', ' ')));
                }
            }
        }


        ObservableCollection<object> generatedeventvalues;
        public ObservableCollection<object> GeneratedEventValues
        {
            get
            {
                if (generatedeventvalues == null)
                    generatedeventvalues = new ObservableCollection<object>();
                return generatedeventvalues;
            }

            set
            {
                generatedeventvalues = value;
            }
        }

        void GetGeneratedEventValues(string eventName)
        {
            GeneratedEventValues.Clear();

            if (genEvents != null)
            {
                TH_GeneratedData.ConfigurationPage.Page.Event ev = genEvents.Find(x => String_Functions.UppercaseFirst(x.name.Replace('_', ' ')).ToLower() == eventName.ToLower());
                if (ev != null)
                {
                    // Add each Value
                    foreach (var value in ev.values)
                    {
                        GeneratedEventValues.Add(value.result.value);
                    }

                    // Add Default Value
                    if (ev.Default != null)
                    {
                        GeneratedEventValues.Add(ev.Default.value);
                    }
                }
            }
        }

        #endregion

        #region "Cycle Event Name"

        public object SelectedCycleEventName
        {
            get { return (object)GetValue(SelectedCycleEventNameProperty); }
            set { SetValue(SelectedCycleEventNameProperty, value); }
        }

        public static readonly DependencyProperty SelectedCycleEventNameProperty =
            DependencyProperty.Register("SelectedCycleEventName", typeof(object), typeof(Page), new PropertyMetadata(null));

        private void CycleEventName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItem = null;

            ComboBox cmbox = (ComboBox)sender;
            if (cmbox.SelectedItem != null) selectedItem = cmbox.SelectedItem.ToString();

            if (selectedItem != null)
            {
                GetGeneratedEventValues(selectedItem);
            }

            if (cmbox.IsKeyboardFocused || cmbox.IsMouseCaptured)
            {
                LoadProductionTypes();

                if (SettingChanged != null) SettingChanged("Cycle Event Name", null, null);
            }
        }

        void LoadCycleEventName(DataTable dt)
        {
            string query = "Address = '" + prefix + "CycleEventName'";

            var rows = dt.Select(query);
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    string val = DataTable_Functions.GetRowValue("Value", row);
                    if (val != null) SelectedCycleEventName = String_Functions.UppercaseFirst(val.Replace('_', ' '));
                }
            }
        }

        void SaveCycleEventName(DataTable dt)
        {
            string val = "";
            if (SelectedCycleEventName != null) val = SelectedCycleEventName.ToString();

            if (val != null) Table_Functions.UpdateTableValue(val.Replace(' ', '_').ToLower(), prefix + "CycleEventName", dt);
        }

        #endregion

        #region "Stopped Event Value"

        public object SelectedStoppedEventValue
        {
            get { return (object)GetValue(SelectedStoppedEventValueProperty); }
            set { SetValue(SelectedStoppedEventValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedStoppedEventValueProperty =
            DependencyProperty.Register("SelectedStoppedEventValue", typeof(object), typeof(Page), new PropertyMetadata(null));


        private void stoppedValue_COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbox = (ComboBox)sender;
            if (cmbox.IsKeyboardFocused || cmbox.IsMouseCaptured) if (SettingChanged != null) SettingChanged("Stopped Event Value", null, null);
        }

        void LoadStoppedEventValueName(DataTable dt)
        {
            string query = "Address = '" + prefix + "StoppedEventValue'";

            var rows = dt.Select(query);
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    SelectedStoppedEventValue = DataTable_Functions.GetRowValue("Value", row);
                }
            }
        }

        void SaveStoppedEventValueName(DataTable dt)
        {
            string val = "";
            if (SelectedStoppedEventValue != null) val = SelectedStoppedEventValue.ToString();

            if (val != null) Table_Functions.UpdateTableValue(val, prefix + "StoppedEventValue", dt);
        }

        #endregion

        #region "Cycle Name Link"

        public object SelectedCycleNameLink
        {
            get { return (object)GetValue(SelectedCycleNameLinkProperty); }
            set { SetValue(SelectedCycleNameLinkProperty, value); }
        }

        public static readonly DependencyProperty SelectedCycleNameLinkProperty =
            DependencyProperty.Register("SelectedCycleNameLink", typeof(object), typeof(Page), new PropertyMetadata(null));

        private void cyclenamelink_COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbox = (ComboBox)sender;
            if (cmbox.IsKeyboardFocused || cmbox.IsMouseCaptured) if (SettingChanged != null) SettingChanged("Cycle Name Link", null, null);
        }

        void LoadCycleNameLink(DataTable dt)
        {
            string query = "Address = '" + prefix + "CycleNameLink'";

            var rows = dt.Select(query);
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    string val = DataTable_Functions.GetRowValue("Value", row);
                    //var dataItem = CollectedItems.ToList().Find(x => x.id == val);
                    //if (dataItem != null) val = dataItem.display;
                    SelectedCycleNameLink = val;
                }
            }
        }

        void SaveCycleNameLink(DataTable dt)
        {
            string val = null;
            string text = null;
            if (SelectedCycleNameLink != null) text = SelectedCycleNameLink.ToString();
            if (text != null)
            {
                var link = CollectedItems.ToList().Find(x => x.display == text);
                if (link != null) val = link.id;
            }

            if (val != null) Table_Functions.UpdateTableValue(val, prefix + "CycleNameLink", dt);
        }

        #endregion

        #region "Production Types"

        ObservableCollection<object> productionTypes;
        public ObservableCollection<object> ProductionTypes
        {
            get
            {
                if (productionTypes == null)
                    productionTypes = new ObservableCollection<object>();
                return productionTypes;
            }

            set
            {
                productionTypes = value;
            }
        }

        ObservableCollection<object> productionTypeValues;
        public ObservableCollection<object> ProductionTypeValues
        {
            get
            {
                if (productionTypeValues == null)
                    productionTypeValues = new ObservableCollection<object>();
                return productionTypeValues;
            }

            set
            {
                productionTypeValues = value;
            }
        }


        void GetProductionTypeValues()
        {
            ProductionTypeValues.Clear();

            var values = Enum.GetValues(typeof(TH_Cycles.CycleData.CycleProductionType));
            foreach (var value in values)
            {
                ProductionTypeValues.Add(value);
            }
        }

        void LoadProductionTypes()
        {
            ProductionTypes.Clear();

            foreach (var value in GeneratedEventValues)
            {
                var item = new Controls.ProductionTypeItem();
                item.ParentPage = this;
                item.ValueName = value.ToString();
                item.SettingChanged += ProductionTypeItem_SettingChanged;
                ProductionTypes.Add(item);
            }
        }

        void LoadProductionTypes(DataTable dt)
        {
            ProductionTypes.Clear();

            string query = "Address LIKE '" + prefix + "ProductionTypes/*'";

            var rows = dt.Select(query);
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var item = new Controls.ProductionTypeItem();
                    item.ParentPage = this;
                    item.ValueName = Table_Functions.GetAttribute("name", row);
                    item.productionType_COMBO.Text = Table_Functions.GetAttribute("type", row);
                    item.SettingChanged += ProductionTypeItem_SettingChanged;
                    ProductionTypes.Add(item);
                }
            }
        }

        void ProductionTypeItem_SettingChanged()
        {
            if (SettingChanged != null) SettingChanged("Production Type", null, null);
        }

        void SaveProductionTypes(DataTable dt)
        {
            // Clear all old rows first (so that Ids can be sequentially assigned)
            string filter = "address LIKE '" + prefix + "ProductionTypes/*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();
            foreach (DataRow row in temp_dt.Rows)
            {
                DataRow dbRow = dt.Rows.Find(row["address"]);
                if (dbRow != null) dt.Rows.Remove(dbRow);
            }

            foreach (var productionType in ProductionTypes)
            {
                var item = (Controls.ProductionTypeItem)productionType;

                int id = 0;
                string adr = prefix + "ProductionTypes/Value||";
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
                attr += "name||" + item.ValueName + ";";
                attr += "type||" + item.productionType_COMBO.Text + ";";

                Table_Functions.UpdateTableValue(null, attr, adr, dt);
            }
        }

        #endregion

        #region "Override Links"

        ObservableCollection<object> overrideLinks;
        public ObservableCollection<object> OverrideLinks
        {
            get
            {
                if (overrideLinks == null)
                    overrideLinks = new ObservableCollection<object>();
                return overrideLinks;
            }

            set
            {
                overrideLinks = value;
            }
        }

        private void AddOverrideLink_Clicked(TH_WPF.Button bt)
        {
            var item = new Controls.OverrideLinkItem();
            item.ParentPage = this;
            item.RemoveClicked += OverrideLinkItem_RemoveClicked;
            item.SettingChanged += OverrideLinkItem_SettingChanged;
            OverrideLinks.Add(item);
        }

        void LoadOverrideLinks(DataTable dt)
        {
            OverrideLinks.Clear();

            string query = "Address LIKE '" + prefix + "OverrideLinks/*'";

            var rows = dt.Select(query);
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var item = new Controls.OverrideLinkItem();
                    item.ParentPage = this;

                    //string val = DataTable_Functions.GetRowValue("Value", row);
                    //var dataItem = CollectedItems.ToList().Find(x => x.id == val);
                    //if (dataItem != null) val = dataItem.display;
                    //item.collectedlink_COMBO.Text = val;

                    item.collectedlink_COMBO.Text = DataTable_Functions.GetRowValue("Value", row);
                    item.RemoveClicked += OverrideLinkItem_RemoveClicked;
                    item.SettingChanged += OverrideLinkItem_SettingChanged;
                    OverrideLinks.Add(item);
                }
            }
        }

        void OverrideLinkItem_SettingChanged()
        {
            if (SettingChanged != null) SettingChanged("Override Link", null, null);
        }

        void OverrideLinkItem_RemoveClicked(Controls.OverrideLinkItem item)
        {
            if (OverrideLinks.Contains(item)) OverrideLinks.Remove(item);
        }

        void SaveOverrideLinks(DataTable dt)
        {
            // Clear all old rows first (so that Ids can be sequentially assigned)
            string filter = "address LIKE '" + prefix + "OverrideLinks/*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();
            foreach (DataRow row in temp_dt.Rows)
            {
                DataRow dbRow = dt.Rows.Find(row["address"]);
                if (dbRow != null) dt.Rows.Remove(dbRow);
            }

            foreach (var overrideLink in OverrideLinks)
            {
                var item = (Controls.OverrideLinkItem)overrideLink;

                int id = 0;
                string adr = prefix + "OverrideLinks/Value||";
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

                string val = item.collectedlink_COMBO.Text;
                string text = item.collectedlink_COMBO.Text;
                if (text != null)
                {
                    var link = CollectedItems.ToList().Find(x => x.display == text);
                    if (link != null) val = link.id;
                }

                string attr = "";
                attr += "id||" + id.ToString("00") + ";";

                Table_Functions.UpdateTableValue(val, attr, adr, dt);
            }
        }

        #endregion

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
 
    }
}
