using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using TH_Global.Functions;
using TH_Plugins;
using TH_Plugins.ConfigurationPage;

namespace TH_Parts_Config
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

            CalculationTypes.Add(CalculationType.Incremental.ToString());
            CalculationTypes.Add(CalculationType.Total.ToString());
        }

        public string Title { get { return "Parts"; } }
   
        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TH_Parts;component/Resources/Block_01.png"));
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


        public void LoadConfiguration(DataTable dt)
        {
            configurationTable = dt;

            LoadGeneratedEventItems(dt);

            ClearData();
            LoadData(dt);
        }

        void ClearData()
        {
            SelectedPartsEventName = null;
            SelectedProducedEventValue = null;
            SelectedPartCountLink = null;
        }

        void LoadData(DataTable dt)
        {
            LoadPartsEventName(dt);
            LoadProducedEventValueName(dt);
            LoadPartCountLink(dt);
            LoadCalculationType(dt);      
        }

        public void SaveConfiguration(DataTable dt)
        { 
            SavePartsEventName(dt);
            SaveProducedEventValueName(dt);
            SavePartCountLink(dt);
            SaveCalculationType(dt);
        }

        DataTable configurationTable;

        const string prefix = "/Parts/";

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

        ObservableCollection<string> _calculationTypes;
        public ObservableCollection<string> CalculationTypes
        {
            get
            {
                if (_calculationTypes == null)
                    _calculationTypes = new ObservableCollection<string>();
                return _calculationTypes;
            }

            set
            {
                _calculationTypes = value;
            }
        }

        public class GeneratedEventItem
        {
            public GeneratedEventItem(TH_GeneratedData_Config.GeneratedEvents.Page.Event e)
            {
                Id = e.name;
                Name = String_Functions.UppercaseFirst(e.name.Replace('_', ' ').ToLower());
                Event = e;
            }

            public string Id { get; set; }
            public string Name { get; set; }

            public TH_GeneratedData_Config.GeneratedEvents.Page.Event Event { get; set; }
        }

        private void LoadGeneratedEventItems(DataTable dt)
        {
            GeneratedEventItems.Clear();

            var events = TH_GeneratedData_Config.GeneratedEvents.Page.GetGeneratedEvents(dt);
            foreach (var e in events)
            {
                GeneratedEventItems.Add(new GeneratedEventItem(e));
            }
        }


        #region "Generated Events"

        ObservableCollection<object> _generatedEventValues;
        public ObservableCollection<object> GeneratedEventValues
        {
            get
            {
                if (_generatedEventValues == null)
                    _generatedEventValues = new ObservableCollection<object>();
                return _generatedEventValues;
            }

            set
            {
                _generatedEventValues = value;
            }
        }

        void GetGeneratedEventValues(string Id)
        {
            GeneratedEventValues.Clear();

            if (GeneratedEventItems != null)
            {
                var e = GeneratedEventItems.ToList().Find(x => x.Id == Id);
                if (e != null)
                {
                    // Add each Value
                    foreach (var value in e.Event.values)
                    {
                        if (value.result != null) GeneratedEventValues.Add(value.result.value);
                    }

                    // Add Default Value
                    if (e.Event.Default != null)
                    {
                        if (e.Event.Default != null) GeneratedEventValues.Add(e.Event.Default.value);
                    }
                }
            }
        }

        public class CaptureItem
        {
            public CaptureItem(TH_GeneratedData_Config.GeneratedEvents.Page.CaptureItem item)
            {
                Id = item.name;
                Name = String_Functions.UppercaseFirst(item.name.Replace('_', ' '));
            }

            public string Id { get; set; }
            public string Name { get; set; }
        }

        ObservableCollection<CaptureItem> _generatedEventCaptureItems;
        public ObservableCollection<CaptureItem> GeneratedEventCaptureItems
        {
            get
            {
                if (_generatedEventCaptureItems == null)
                    _generatedEventCaptureItems = new ObservableCollection<CaptureItem>();
                return _generatedEventCaptureItems;
            }

            set
            {
                _generatedEventCaptureItems = value;
            }
        }

        void GetGeneratedEventCaptureItems(string Id)
        {
            GeneratedEventCaptureItems.Clear();

            if (GeneratedEventItems != null)
            {
                var e = GeneratedEventItems.ToList().Find(x => x.Id == Id);
                if (e != null)
                {
                    // Add each Value
                    foreach (var item in e.Event.captureItems)
                    {
                        var i = new CaptureItem(item);
                        GeneratedEventCaptureItems.Add(i);
                    }
                }
            }
        }

        #endregion

        #region "Cycle Event Name"

        public object SelectedPartsEventName
        {
            get { return (object)GetValue(SelectedPartsEventNameProperty); }
            set { SetValue(SelectedPartsEventNameProperty, value); }
        }

        public static readonly DependencyProperty SelectedPartsEventNameProperty =
            DependencyProperty.Register("SelectedPartsEventName", typeof(object), typeof(Page), new PropertyMetadata(null));

        private void PartsEventName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbox = (ComboBox)sender;
            if (cmbox.SelectedItem != null)
            {
                var item = (GeneratedEventItem)cmbox.SelectedItem;
                GetGeneratedEventValues(item.Id);
                GetGeneratedEventCaptureItems(item.Id);
            }

            if (cmbox.IsKeyboardFocused || cmbox.IsMouseCaptured)
            {
                if (SettingChanged != null) SettingChanged("Parts Event Name", null, null);
            }
        }

        void LoadPartsEventName(DataTable dt)
        {
            string query = "Address = '" + prefix + "PartsEventName'";

            var rows = dt.Select(query);
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    string val = DataTable_Functions.GetRowValue("Value", row);
                    if (val != null) SelectedPartsEventName = val;
                }
            }
        }

        void SavePartsEventName(DataTable dt)
        {
            string val = "";
            if (SelectedPartsEventName != null) val = SelectedPartsEventName.ToString();

            if (val != null) DataTable_Functions.UpdateTableValue(dt, "address", prefix + "PartsEventName", "value", val.Replace(' ', '_').ToLower());
        }

        #endregion

        #region "Parts Produced Event Value"

        public object SelectedProducedEventValue
        {
            get { return (object)GetValue(SelectedProducedEventValueProperty); }
            set { SetValue(SelectedProducedEventValueProperty, value); }
        }

        public static readonly DependencyProperty SelectedProducedEventValueProperty =
            DependencyProperty.Register("SelectedProducedEventValue", typeof(object), typeof(Page), new PropertyMetadata(null));


        private void producedValue_COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbox = (ComboBox)sender;
            if (cmbox.IsKeyboardFocused || cmbox.IsMouseCaptured) if (SettingChanged != null) SettingChanged("Parts Produced Event Value", null, null);
        }

        void LoadProducedEventValueName(DataTable dt)
        {
            string query = "Address = '" + prefix + "PartsEventValue'";

            var rows = dt.Select(query);
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    string val = DataTable_Functions.GetRowValue("Value", row);
                    if (val != null) SelectedProducedEventValue = String_Functions.UppercaseFirst(val.Replace('_', ' '));
                }
            }
        }

        void SaveProducedEventValueName(DataTable dt)
        {
            string val = "";
            if (SelectedProducedEventValue != null) val = SelectedProducedEventValue.ToString();

            if (val != null) DataTable_Functions.UpdateTableValue(dt, "address", prefix + "PartsEventValue", "value", val.Replace(' ', '_').ToLower());
        }

        #endregion

        #region "Part Count Link"

        public object SelectedPartCountLink
        {
            get { return (object)GetValue(SelectedPartCountLinkProperty); }
            set { SetValue(SelectedPartCountLinkProperty, value); }
        }

        public static readonly DependencyProperty SelectedPartCountLinkProperty =
            DependencyProperty.Register("SelectedPartCountLink", typeof(object), typeof(Page), new PropertyMetadata(null));

        private void partCountLink_COMBO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbox = (ComboBox)sender;
            if (cmbox.IsKeyboardFocused || cmbox.IsMouseCaptured) if (SettingChanged != null) SettingChanged("Part Count Link", null, null);
        }

        void LoadPartCountLink(DataTable dt)
        {
            string query = "Address = '" + prefix + "PartsCaptureItemLink'";

            var rows = dt.Select(query);
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    string val = DataTable_Functions.GetRowValue("Value", row);
                    if (val != null) SelectedPartCountLink = val;
                }
            }
        }

        void SavePartCountLink(DataTable dt)
        {
            string val = "";
            if (SelectedPartCountLink != null) val = SelectedPartCountLink.ToString();

            if (val != null) DataTable_Functions.UpdateTableValue(dt, "address", prefix + "PartsCaptureItemLink", "value", val.Replace(' ', '_').ToLower());
        }

        #endregion

        #region "Calculation Type"

        public string SelectedCalculationType
        {
            get { return (string)GetValue(SelectedCalculationTypeProperty); }
            set { SetValue(SelectedCalculationTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedCalculationTypeProperty =
            DependencyProperty.Register("SelectedCalculationType", typeof(string), typeof(Page), new PropertyMetadata("Interval"));



        private void CalculationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmbox = (ComboBox)sender;
            if (cmbox.IsKeyboardFocused || cmbox.IsMouseCaptured) if (SettingChanged != null) SettingChanged("Calculation Type Changed", null, null);
        }

        void LoadCalculationType(DataTable dt)
        {
            string query = "Address = '" + prefix + "CalculationType'";

            var rows = dt.Select(query);
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    string val = DataTable_Functions.GetRowValue("Value", row);
                    if (val != null) SelectedCalculationType = val;
                }
            }
        }

        void SaveCalculationType(DataTable dt)
        {
            string val = "";
            if (SelectedPartCountLink != null) val = SelectedCalculationType.ToString();

            if (val != null) DataTable_Functions.UpdateTableValue(dt, "address", prefix + "CalculationType", "value", val);
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public enum CalculationType
    {
        /// <summary>
        /// Incrementally increased as each event is added to the total
        /// </summary>
        Incremental,

        /// <summary>
        /// Event carries total value
        /// </summary>
        Total,
    }
}
