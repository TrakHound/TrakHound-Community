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

        public TH_GeneratedData.ConfigurationPage.Page.Event ParentEvent;
        public TH_GeneratedData.ConfigurationPage.Page.Value ParentValue;
        public TH_GeneratedData.ConfigurationPage.Page.Trigger ParentTrigger;

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


        public object SelectedLink
        {
            get { return (object)GetValue(SelectedLinkProperty); }
            set { SetValue(SelectedLinkProperty, value); }
        }

        public static readonly DependencyProperty SelectedLinkProperty =
            DependencyProperty.Register("SelectedLink", typeof(object), typeof(Trigger), new PropertyMetadata(null));


        public object SelectedModifier
        {
            get { return (object)GetValue(SelectedModifierProperty); }
            set { SetValue(SelectedModifierProperty, value); }
        }

        public static readonly DependencyProperty SelectedModifierProperty =
            DependencyProperty.Register("SelectedModifier", typeof(object), typeof(Trigger), new PropertyMetadata(null));


        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(Trigger), new PropertyMetadata(null));

        public delegate void RemoveClicked_Handler(Trigger t);
        public event RemoveClicked_Handler RemoveClicked;

        private void Remove_Clicked(TH_WPF.Button_03 bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);
        }

        private void Link_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParentTrigger != null) ParentTrigger.link = ((ComboBox)sender).SelectedItem.ToString();
        }

        private void Modifier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ParentTrigger != null) ParentTrigger.modifier = ((ComboBox)sender).SelectedItem.ToString();
        }

        private void value_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ParentTrigger != null) ParentTrigger.value = ((TextBox)sender).Text;
        }

  
    }
}
