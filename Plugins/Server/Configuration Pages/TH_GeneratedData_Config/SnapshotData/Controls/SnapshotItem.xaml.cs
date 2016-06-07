using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

using TH_GeneratedData.SnapshotData;
using TH_Global;
using TH_Global.Functions;

namespace TH_GeneratedData_Config.SnapshotData.Controls
{
    /// <summary>
    /// Interaction logic for SnapshotItem.xaml
    /// </summary>
    public partial class SnapshotItem : UserControl
    {
        public SnapshotItem(Page parentPage)
        {
            Init();

            ParentPage = parentPage;
            ParentSnapshot = new Snapshot();
        }

        public SnapshotItem(Page parentPage, Snapshot snapshot)
        {
            Init();

            ParentPage = parentPage;
            ParentSnapshot = snapshot;

            NameText = snapshot.Name;
            SelectedType = snapshot.Type.ToString();
            SelectedLink = snapshot.Link;
        }

        private void Init()
        {
            InitializeComponent();
            root.DataContext = this;

            Id = String_Functions.RandomString(20);
        }

        public string Id { get; set; }

        public Page ParentPage
        {
            get { return (Page)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.Register("ParentPage", typeof(Page), typeof(SnapshotItem), new PropertyMetadata(null));

        public Snapshot ParentSnapshot;

        public bool Loading { get; set; }


        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;

        public delegate void Clicked_Handler(SnapshotItem item);


        #region "Name"

        public string NameText
        {
            get { return (string)GetValue(NameTextProperty); }
            set
            {
                SetValue(NameTextProperty, value);
            }
        }

        public static readonly DependencyProperty NameTextProperty =
            DependencyProperty.Register("NameText", typeof(string), typeof(SnapshotItem), new PropertyMetadata(null));

        private void name_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentSnapshot != null) ParentSnapshot.Name = txt.Text;

            if (txt.IsKeyboardFocusWithin) if (SettingChanged != null) SettingChanged();
        }

        #endregion

        #region "Type"

        public string SelectedType
        {
            get { return (string)GetValue(SelectedTypeProperty); }
            set
            {
                SetValue(SelectedTypeProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedTypeProperty =
            DependencyProperty.Register("SelectedType", typeof(string), typeof(SnapshotItem), new PropertyMetadata(null));


        ObservableCollection<string> typeitems;
        public ObservableCollection<string> TypeItems
        {
            get
            {
                if (typeitems == null)
                    typeitems = new ObservableCollection<string>();
                return typeitems;
            }

            set
            {
                typeitems = value;
            }
        }


        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox)sender;

            if (combo.SelectedItem != null)
            {
                var s = combo.SelectedItem.ToString().ToLower();

                try
                {
                    var type = (SnapshotType)Enum.Parse(typeof(SnapshotType), s, true);
                    if (ParentSnapshot != null) ParentSnapshot.Type = type;
                }
                catch (Exception ex) { Logger.Log("Exception :: " + ex.Message, Logger.LogLineType.Warning); }
            }

            if (combo.IsKeyboardFocusWithin) if (SettingChanged != null) SettingChanged();
        }

        #endregion

        #region "Link"

        public string SelectedVariableLink
        {
            get { return (string)GetValue(SelectedVariableLinkProperty); }
            set { SetValue(SelectedVariableLinkProperty, value); }
        }


        public static readonly DependencyProperty SelectedVariableLinkProperty =
            DependencyProperty.Register("SelectedVariableLink", typeof(string), typeof(SnapshotItem), new PropertyMetadata(null));


        public string SelectedLink
        {
            get { return (string)GetValue(SelectedLinkProperty); }
            set
            {
                SetValue(SelectedLinkProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedLinkProperty =
            DependencyProperty.Register("SelectedLink", typeof(string), typeof(SnapshotItem), new PropertyMetadata(null));


        ObservableCollection<object> linkitems;
        public ObservableCollection<object> LinkItems
        {
            get
            {
                if (linkitems == null)
                    linkitems = new ObservableCollection<object>();
                return linkitems;
            }

            set
            {
                linkitems = value;
            }
        }


        private void Link_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (combo.SelectedItem != null)
            {
                if (ParentSnapshot != null)
                {
                    if (SelectedType == "Collected") ParentSnapshot.Link = ((Page.CollectedItem)combo.SelectedItem).Id;
                    else if (SelectedType == "Generated") ParentSnapshot.Link = ((Page.GeneratedEventItem)combo.SelectedItem).Id;
                    else ParentSnapshot.Link = combo.Text;
                }
            }

            if (combo.IsKeyboardFocusWithin)
            {
                if (SettingChanged != null) SettingChanged();
            }
        }

        private void variable_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentSnapshot != null) ParentSnapshot.Link = txt.Text;

            if (txt.IsKeyboardFocused || txt.IsMouseCaptured)
            {
                if (SettingChanged != null) SettingChanged();
            }
        }

        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (ParentSnapshot != null) ParentSnapshot.Link = combo.Text;

            if (combo.IsKeyboardFocusWithin || combo.IsMouseCaptured)
            {
                if (SettingChanged != null) SettingChanged();
            }
        }

        private void Link_COMBO_TextChanged(object sender, RoutedEventArgs e)
        {
            //ComboBox combo = (ComboBox)sender;

            //if (ParentSnapshot != null) ParentSnapshot.Link = combo.Text;

            //if (combo.IsKeyboardFocusWithin || combo.IsMouseCaptured)
            //{
            //    if (SettingChanged != null) SettingChanged();
            //}
        }

        #endregion

        #region "Remove"

        public event Clicked_Handler RemoveClicked;

        private void Removed_Clicked(TH_WPF.Button bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);
            if (SettingChanged != null) SettingChanged();
        }

        #endregion

        public event Clicked_Handler RefreshClicked;

        private void Refresh_Clicked(TH_WPF.Button bt)
        {
            if (RefreshClicked != null) RefreshClicked(this);
        }

    }
}
