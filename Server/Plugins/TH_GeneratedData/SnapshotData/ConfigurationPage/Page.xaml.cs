using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.ObjectModel;
using System.Data;

using TH_Global;
using TH_Global.Functions;

using TH_MTConnect.Components;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_GeneratedData.SnapshotData.ConfigurationPage
{
    /// <summary>
    /// Interaction logic for Snapshots.xaml
    /// </summary>
    public partial class Page : UserControl, IConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public string Title { get { return "Snapshot Data"; } }

        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TH_GeneratedData;component/Resources/Camera_01.png"));
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
            GetProbeData(data);
        }


        public void LoadConfiguration(DataTable dt)
        {
            LoadGeneratedEventItems(dt);

            string address = "/GeneratedData/SnapShotData/";

            string filter = "address LIKE '" + address + "*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();
            temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

            Snapshots.Clear();

            foreach (DataRow row in temp_dt.Rows)
            {
                var snapshot = new Snapshot();
                snapshot.Name = DataTable_Functions.TrakHound.GetRowAttribute("name", row);

                string type = DataTable_Functions.TrakHound.GetLastNode(row);
                if (type != null)
                {
                    switch (type.ToLower())
                    {
                        case "collected": snapshot.Type = SnapshotType.Collected; break;
                        case "generated": snapshot.Type = SnapshotType.Generated; break;
                        case "variable": snapshot.Type = SnapshotType.Variable; break;
                    }
                }

                snapshot.Link = DataTable_Functions.TrakHound.GetRowAttribute("link", row);
                Snapshots.Add(snapshot);
            }

            Snapshots_DG.Items.Refresh();

            if (!Loaded) LoadCollectedItems(probeData);
        }

        public void SaveConfiguration(DataTable dt)
        {
            string prefix = "/GeneratedData/SnapShotData/";

            // Clear all snapshot rows first (so that Ids can be sequentially assigned)
            DataTable_Functions.TrakHound.DeleteRows(prefix + "*", "address", dt);
            
            // Loop through SnapshotItems and add each item back to table with sequential id's
            foreach (var snapshot in Snapshots)
            {
                if (snapshot.Name != null && snapshot.Link != null)
                {
                    string adr = "/GeneratedData/SnapShotData/" + String_Functions.UppercaseFirst(snapshot.Type.ToString().ToLower());
                    int id = DataTable_Functions.TrakHound.GetUnusedAddressId(adr, dt);
                    adr = adr + "||" + id.ToString("00");

                    string attr = "";
                    attr += "id||" + id.ToString("00") + ";";
                    attr += "name||" + snapshot.Name + ";";

                    string link = snapshot.Link;

                    attr += "link||" + link + ";";

                    DataTable_Functions.UpdateTableValue(dt, "address", adr, "attributes", attr);
                }
            }
        }

        ObservableCollection<Snapshot> _snapshots;
        public ObservableCollection<Snapshot> Snapshots
        {
            get
            {
                if (_snapshots == null)
                    _snapshots = new ObservableCollection<Snapshot>();
                return _snapshots;
            }

            set
            {
                _snapshots = value;
            }
        }

        //ObservableCollection<CollectedItem> _collectedItems;
        //public ObservableCollection<CollectedItem> CollectedItems
        //{
        //    get
        //    {
        //        if (_collectedItems == null)
        //            _collectedItems = new ObservableCollection<CollectedItem>();
        //        return _collectedItems;
        //    }

        //    set
        //    {
        //        _collectedItems = value;
        //    }
        //}

        //private List<DataItem> probeData = new List<DataItem>();


        ObservableCollection<GeneratedEventItem> _generatedEventItems;
        public ObservableCollection<GeneratedEventItem> GeneratedEventItems
        {
            get
            {
                if (_generatedEventItems == null)
                    _generatedEventItems = new ObservableCollection<GeneratedEventItem>();
                return _generatedEventItems;
            }

            set
            {
                _generatedEventItems = value;
            }
        }


        //public class CollectedItem
        //{
        //    public CollectedItem(DataItem dataItem)
        //    {
        //        Id = dataItem.Id;
        //        Name = dataItem.Name;

        //        if (Name != null) Display = Id + " : " + Name;
        //        else Display = Id;
        //    }          

        //    public string Id { get; set; }
        //    public string Name { get; set; }

        //    public string Display { get; set; }

        //    public override string ToString()
        //    {
        //        return Display;
        //    }
        //}

        public class GeneratedEventItem
        {
            public GeneratedEventItem(GeneratedEvents.ConfigurationPage.Page.Event e)
            {
                Id = e.name;
                Name = String_Functions.UppercaseFirst(e.name.Replace('_', ' ').ToLower());
            }

            public string Id { get; set; }
            public string Name { get; set; }
        }

        //void GetProbeData(EventData data)
        //{
        //    if (data != null && data.Id != null && data.Data02 != null)
        //    {
        //        if (data.Id.ToLower() == "mtconnect_probe_dataitems")
        //        {
        //            var dataItems = (List<DataItem>)data.Data02;
        //            LoadCollectedItems(dataItems);
        //        }
        //    }
        //}

        //private void LoadCollectedItems(List<DataItem> dataItems)
        //{
        //    CollectedItems.Clear();

        //    foreach (var dataItem in dataItems)
        //    {
        //        var item = new CollectedItem(dataItem);
        //        CollectedItems.Add(item);
        //        probeData.Add(dataItem);
        //    }
        //}

        private void LoadGeneratedEventItems(DataTable dt)
        {
            GeneratedEventItems.Clear();

            var events = GeneratedEvents.ConfigurationPage.Page.GetGeneratedEvents(dt);
            foreach (var e in events)
            {
                GeneratedEventItems.Add(new GeneratedEventItem(e));
            }
        }


        #region "MTC Probe Data"

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

        private List<DataItem> probeData = new List<DataItem>();

        public class CollectedItem : IComparable
        {
            public CollectedItem() { }

            public CollectedItem(DataItem dataItem)
            {
                Id = dataItem.Id;
                Name = dataItem.Name;

                if (Name != null) Display = Id + " : " + Name;
                else Display = Id;
            }

            public string Id { get; set; }
            public string Name { get; set; }

            public string Display { get; set; }

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
        }

        #endregion


        private void Add_Clicked(TH_WPF.Button bt)
        {
            Snapshots.Add(new Snapshot());
            if (SettingChanged != null) SettingChanged(null, null, null);
        }

        private void Remove_Clicked(TH_WPF.Button bt)
        {
            if (bt.DataObject != null)
            {
                var snapshot = (Snapshot)bt.DataObject;

                int index = Snapshots.ToList().FindIndex(x => x.Name == snapshot.Name);
                if (index >= 0) Snapshots.RemoveAt(index);

                if (SettingChanged != null) SettingChanged(null, null, null);
            }
        }

        private void TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox)sender;
            if (txt.IsMouseCaptured || txt.IsKeyboardFocused) if (SettingChanged != null) SettingChanged(null, null, null);
        }

        private void CMBX_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbx = (ComboBox)sender;
            if (cmbx.IsMouseCaptured || cmbx.IsKeyboardFocused) if (SettingChanged != null) SettingChanged(null, null, null);
        }
    }

}
