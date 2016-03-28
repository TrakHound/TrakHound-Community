using System;
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

namespace TH_DevicePage.Controls
{
    /// <summary>
    /// Interaction logic for AvailabilityStatusPanel.xaml
    /// </summary>
    public partial class AvailabilityStatusPanel : UserControl
    {
        public AvailabilityStatusPanel()
        {
            InitializeComponent();
            root.DataContext = this;
        }


        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(AvailabilityStatusPanel), new PropertyMetadata(null));


        public string Availability
        {
            get { return (string)GetValue(AvailabilityProperty); }
            set { SetValue(AvailabilityProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityProperty =
            DependencyProperty.Register("Availability", typeof(string), typeof(AvailabilityStatusPanel), new PropertyMetadata("UNAVAILABLE"));

    }
}
