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
    /// Interaction logic for Event.xaml
    /// </summary>
    public partial class Event : UserControl
    {
        public Event()
        {
            InitializeComponent();
            DataContext = this;
        }

        public TH_GeneratedData.ConfigurationPage.Page.Event ParentEvent;

        ObservableCollection<Value> values;
        public ObservableCollection<Value> Values
        {
            get
            {
                if (values == null)
                    values = new ObservableCollection<Value>();
                return values;
            }

            set
            {
                values = value;
            }
        }


        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(Event), new PropertyMetadata(null));

        public delegate void Clicked_Handler(Event e);
        public event Clicked_Handler AddValueClicked;

        private void AddValue_Clicked(TH_WPF.Button_01 bt)
        {
            if (AddValueClicked != null) AddValueClicked(this);
        }

        private void Description_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ParentEvent != null) ParentEvent.description = ((TextBox)sender).Text;
        }

    }
}
