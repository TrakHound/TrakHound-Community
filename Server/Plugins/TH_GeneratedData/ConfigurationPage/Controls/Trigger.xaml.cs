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
    /// Interaction logic for Trigger.xaml
    /// </summary>
    public partial class Trigger : UserControl
    {
        public Trigger()
        {
            InitializeComponent();
            DataContext = this;

            Modifiers.Add("Equal To");
            Modifiers.Add("Not Equal To");
            Modifiers.Add("Greater Than");
            Modifiers.Add("Less Than");
            Modifiers.Add("Contains");
            Modifiers.Add("Contains Match Case");
            Modifiers.Add("Contains Whole Word");
            Modifiers.Add("Contains Whole Word Match Case");          
        }

       
        public Page.Event ParentEvent;
        public Page.Value ParentValue;
        public Page.Trigger ParentTrigger;


        public TH_GeneratedData.ConfigurationPage.Page ParentPage
        {
            get { return (TH_GeneratedData.ConfigurationPage.Page)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public static readonly DependencyProperty ParentPageProperty =
            DependencyProperty.Register("ParentPage", typeof(TH_GeneratedData.ConfigurationPage.Page), typeof(Trigger), new PropertyMetadata(null));


        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;


        ObservableCollection<object> dataitems;
        public ObservableCollection<object> DataItems
        {
            get
            {
                if (dataitems == null)
                    dataitems = new ObservableCollection<object>();
                return dataitems;
            }

            set
            {
                dataitems = value;
            }
        }

        ObservableCollection<object> modifiers;
        public ObservableCollection<object> Modifiers
        {
            get
            {
                if (modifiers == null)
                    modifiers = new ObservableCollection<object>();
                return modifiers;
            }

            set
            {
                modifiers = value;
            }
        }


        public object SelectedModifier
        {
            get { return (object)GetValue(SelectedModifierProperty); }
            set { SetValue(SelectedModifierProperty, value); }
        }

        public static readonly DependencyProperty SelectedModifierProperty =
            DependencyProperty.Register("SelectedModifier", typeof(object), typeof(Trigger), new PropertyMetadata(null));


        #region "Value"

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(Trigger), new PropertyMetadata(null));

        private void value_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentTrigger != null) ParentTrigger.value = txt.Text;

            if (txt.IsKeyboardFocused) if (SettingChanged != null) SettingChanged();
        }

        #endregion

        #region "Modifier"

        private void Modifier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (ParentTrigger != null)
            {
                if (combo.SelectedItem != null)
                {
                    ParentTrigger.modifier = combo.SelectedItem.ToString();
                }
            }

            if (combo.IsKeyboardFocusWithin) if (SettingChanged != null) SettingChanged();
        }

        #endregion

        #region "Link"

        private void Link_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (ParentTrigger != null)
            {
                if (combo.SelectedItem != null)
                {
                    ParentTrigger.link = combo.SelectedItem.ToString();
                }
            }

            if (combo.IsKeyboardFocusWithin) if (SettingChanged != null) SettingChanged();
        }

        private void Link_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (ParentTrigger != null) ParentTrigger.link = combo.Text;

            if (combo.IsKeyboardFocusWithin) if (SettingChanged != null) SettingChanged();
        }

        #endregion

        #region "Remove"

        public delegate void RemoveClicked_Handler(Trigger t);
        public event RemoveClicked_Handler RemoveClicked;

        private void Remove_Clicked(TH_WPF.Button_03 bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);

            if (SettingChanged != null) SettingChanged();
        }

        #endregion

        object oldFocus = null;

        private void TXT_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_GotFocus(object sender, RoutedEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_GotMouseCapture(object sender, MouseEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { oldFocus = null; }

    }
}
