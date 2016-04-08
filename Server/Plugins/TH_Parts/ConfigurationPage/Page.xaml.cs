using System;
using System.Collections.ObjectModel;
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

using System.Data;

using TH_Global.Functions;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_Parts.ConfigurationPage
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

            GetGeneratedEvents(dt);

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
        }

        public void SaveConfiguration(DataTable dt)
        { 
                SavePartsEventName(dt);
                SaveProducedEventValueName(dt);
                SavePartCountLink(dt);
        }

        DataTable configurationTable;

        const string prefix = "/Parts/";



        #region "Generated Events"

        ObservableCollection<object> _generatedEvents;
        public ObservableCollection<object> GeneratedEvents
        {
            get
            {
                if (_generatedEvents == null)
                    _generatedEvents = new ObservableCollection<object>();
                return _generatedEvents;
            }

            set
            {
                _generatedEvents = value;
            }
        }

        List<TH_GeneratedData.GeneratedEvents.ConfigurationPage.Page.Event> genEvents;

        void GetGeneratedEvents(DataTable dt)
        {
            GeneratedEvents.Clear();

            genEvents = TH_GeneratedData.GeneratedEvents.ConfigurationPage.Page.GetGeneratedEvents(dt);

            if (genEvents != null)
            {
                foreach (TH_GeneratedData.GeneratedEvents.ConfigurationPage.Page.Event ev in genEvents)
                {
                    GeneratedEvents.Add(String_Functions.UppercaseFirst(ev.name.Replace('_', ' ')));
                }
            }
        }


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

        void GetGeneratedEventValues(string eventName)
        {
            GeneratedEventValues.Clear();

            if (genEvents != null)
            {
                TH_GeneratedData.GeneratedEvents.ConfigurationPage.Page.Event ev = genEvents.Find(x => String_Functions.UppercaseFirst(x.name.Replace('_', ' ')).ToLower() == eventName.ToLower());
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

        ObservableCollection<object> _generatedEventCaptureItems;
        public ObservableCollection<object> GeneratedEventCaptureItems
        {
            get
            {
                if (_generatedEventCaptureItems == null)
                    _generatedEventCaptureItems = new ObservableCollection<object>();
                return _generatedEventCaptureItems;
            }

            set
            {
                _generatedEventCaptureItems = value;
            }
        }

        void GetGeneratedEventCaptureItems(string eventName)
        {
            GeneratedEventCaptureItems.Clear();

            if (genEvents != null)
            {
                TH_GeneratedData.GeneratedEvents.ConfigurationPage.Page.Event ev = genEvents.Find(x => String_Functions.UppercaseFirst(x.name.Replace('_', ' ')).ToLower() == eventName.ToLower());
                if (ev != null)
                {
                    // Add each Value
                    foreach (var item in ev.captureItems)
                    {
                        string name = item.name;
                        if (name != null) GeneratedEventCaptureItems.Add(String_Functions.UppercaseFirst(name.Replace('_', ' ')));
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
            string selectedItem = null;

            ComboBox cmbox = (ComboBox)sender;
            if (cmbox.SelectedItem != null) selectedItem = cmbox.SelectedItem.ToString();

            if (selectedItem != null)
            {
                GetGeneratedEventValues(selectedItem);
                GetGeneratedEventCaptureItems(selectedItem);
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
                    if (val != null) SelectedPartsEventName = String_Functions.UppercaseFirst(val.Replace('_', ' '));
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
                    if (val != null) SelectedPartCountLink = String_Functions.UppercaseFirst(val.Replace('_', ' '));
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
