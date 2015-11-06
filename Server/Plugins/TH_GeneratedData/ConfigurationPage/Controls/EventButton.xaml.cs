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

namespace TH_GeneratedData.ConfigurationPage.Controls
{
    /// <summary>
    /// Interaction logic for EventButton.xaml
    /// </summary>
    public partial class EventButton : UserControl
    {
        public EventButton()
        {
            InitializeComponent();
            DataContext = this;
        }


        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register("EventName", typeof(string), typeof(EventButton), new PropertyMetadata(null));


        public string EventValues
        {
            get { return (string)GetValue(EventValuesProperty); }
            set { SetValue(EventValuesProperty, value); }
        }

        public static readonly DependencyProperty EventValuesProperty =
            DependencyProperty.Register("EventValues", typeof(string), typeof(EventButton), new PropertyMetadata(null));

        private void Remove_Clicked(TH_WPF.Button_02 bt)
        {

        }

        
    }
}
