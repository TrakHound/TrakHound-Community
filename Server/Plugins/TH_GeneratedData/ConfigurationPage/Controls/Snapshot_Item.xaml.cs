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

using System.Collections.ObjectModel;

namespace TH_GeneratedData.ConfigurationPage.Controls
{
    /// <summary>
    /// Interaction logic for Snapshot_Item.xaml
    /// </summary>
    public partial class Snapshot_Item : UserControl
    {
        public Snapshot_Item()
        {
            InitializeComponent();
            DataContext = this;
        }

        public Page ParentPage
        {
            get { return (Page)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.Register("ParentPage", typeof(Page), typeof(Snapshot_Item), new PropertyMetadata(null));

        public Page.Snapshot ParentSnapshot;

        public bool Loading { get; set; }

        
        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;

        public delegate void Clicked_Handler(Snapshot_Item item);

        //string prefix = "/GeneratedData/SnapShotData/";

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
            DependencyProperty.Register("NameText", typeof(string), typeof(Snapshot_Item), new PropertyMetadata(null));

        private void name_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentSnapshot != null) ParentSnapshot.name = txt.Text;

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
            DependencyProperty.Register("SelectedType", typeof(string), typeof(Snapshot_Item), new PropertyMetadata(null));

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
            ComboBox combo = (ComboBox)sender;

            if (combo.SelectedItem != null)
            {
                SelectedType = combo.SelectedItem.ToString().ToLower();

                if (ParentSnapshot != null) ParentSnapshot.type = SelectedType;
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
            DependencyProperty.Register("SelectedVariableLink", typeof(string), typeof(Snapshot_Item), new PropertyMetadata(null));

        
        public string SelectedLink
        {
            get { return (string)GetValue(SelectedLinkProperty); }
            set
            { 
                SetValue(SelectedLinkProperty, value);             
            }
        }

        public static readonly DependencyProperty SelectedLinkProperty =
            DependencyProperty.Register("SelectedLink", typeof(string), typeof(Snapshot_Item), new PropertyMetadata(null));


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
                if (ParentSnapshot != null) ParentSnapshot.link = combo.SelectedItem.ToString();
            }

            if (combo.IsKeyboardFocusWithin)
            {
                if (SettingChanged != null) SettingChanged();
            }
        }

        private void variable_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentSnapshot != null) ParentSnapshot.link = txt.Text;
            
            if (txt.IsKeyboardFocused || txt.IsMouseCaptured)
            {
                if (SettingChanged != null) SettingChanged();
            }
        }

        private void ComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (ParentSnapshot != null) ParentSnapshot.link = combo.Text;

            if (combo.IsKeyboardFocusWithin || combo.IsMouseCaptured)
            {
                if (SettingChanged != null) SettingChanged();
            }
        }

        #endregion

        #region "Remove"
 
        public event Clicked_Handler RemoveClicked;

        private void Removed_Clicked(TH_WPF.Button_04 bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);
            if (SettingChanged != null) SettingChanged();
        }

        #endregion

        public event Clicked_Handler RefreshClicked;

        private void Refresh_Clicked(TH_WPF.Button_04 bt)
        {
            if (RefreshClicked != null) RefreshClicked(this);
        }


    }
}
