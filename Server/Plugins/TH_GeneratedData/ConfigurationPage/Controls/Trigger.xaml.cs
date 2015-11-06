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

  
    }
}
