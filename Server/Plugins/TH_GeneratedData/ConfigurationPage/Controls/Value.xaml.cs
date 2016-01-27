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
    /// Interaction logic for Value.xaml
    /// </summary>
    public partial class Value : UserControl
    {
        public Value()
        {
            InitializeComponent();
            DataContext = this;
        }

        public TH_GeneratedData.ConfigurationPage.Page.Event ParentEvent;
        public TH_GeneratedData.ConfigurationPage.Page.Value ParentValue;

        #region "Help"

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


        #endregion

        public delegate void Clicked_Handler(Value val);

        public delegate void SettingChanged_Handler();
        public event SettingChanged_Handler SettingChanged;
        
        #region "Name"

        public string ValueName
        {
            get { return (string)GetValue(ValueNameProperty); }
            set { SetValue(ValueNameProperty, value); }
        }

        public static readonly DependencyProperty ValueNameProperty =
            DependencyProperty.Register("ValueName", typeof(string), typeof(Value), new PropertyMetadata(null));

        private void ValueName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            if (ParentValue != null)
            {
                Page.Result result = ParentValue.result;

                if (result == null)
                {
                    result = new Page.Result();
                    ParentValue.result = result;
                }

                result.value = txt.Text;
            }

            if (txt.IsKeyboardFocused) if (SettingChanged != null) SettingChanged();
        }

        #endregion

        #region "Triggers"

        ObservableCollection<Trigger> triggers;
        public ObservableCollection<Trigger> Triggers
        {
            get
            {
                if (triggers == null)
                    triggers = new ObservableCollection<Trigger>();
                return triggers;
            }

            set
            {
                triggers = value;
            }
        }

        public event Clicked_Handler AddTriggerClicked;

        private void AddTrigger_Clicked(TH_WPF.Button bt)
        {
            if (AddTriggerClicked != null) AddTriggerClicked(this);

            if (SettingChanged != null) SettingChanged();
        }

        #endregion

        #region "Remove"

        public event Clicked_Handler RemoveClicked;

        private void Remove_Clicked(TH_WPF.Button bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);

            if (SettingChanged != null) SettingChanged();
        }

        #endregion

        object oldFocus = null;

        private void TXT_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_GotFocus(object sender, RoutedEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_GotMouseCapture(object sender, MouseEventArgs e) { if (oldFocus != sender) ((TextBox)sender).SelectAll(); oldFocus = sender; }

        private void TXT_LostFocus(object sender, RoutedEventArgs e) { oldFocus = null; }


    }
}
