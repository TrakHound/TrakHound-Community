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

namespace TH_DevicePage.Controls.Controller
{
    /// <summary>
    /// Interaction logic for Panel.xaml
    /// </summary>
    public partial class Panel : UserControl
    {
        public Panel()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public string Id
        {
            get { return (string)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(string), typeof(Panel), new PropertyMetadata(null));



        public string Availability
        {
            get { return (string)GetValue(AvailabilityProperty); }
            set { SetValue(AvailabilityProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityProperty =
            DependencyProperty.Register("Availability", typeof(string), typeof(Panel), new PropertyMetadata(null));


        public string ControllerMode
        {
            get { return (string)GetValue(ControllerModeProperty); }
            set { SetValue(ControllerModeProperty, value); }
        }

        public static readonly DependencyProperty ControllerModeProperty =
            DependencyProperty.Register("ControllerMode", typeof(string), typeof(Panel), new PropertyMetadata(null));


        public string EmergencyStop
        {
            get { return (string)GetValue(EmergencyStopProperty); }
            set { SetValue(EmergencyStopProperty, value); }
        }

        public static readonly DependencyProperty EmergencyStopProperty =
            DependencyProperty.Register("EmergencyStop", typeof(string), typeof(Panel), new PropertyMetadata(null));


        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(Panel), new PropertyMetadata(null));



        public double FeedrateOverride
        {
            get { return (double)GetValue(FeedrateOverrideProperty); }
            set { SetValue(FeedrateOverrideProperty, value); }
        }

        public static readonly DependencyProperty FeedrateOverrideProperty =
            DependencyProperty.Register("FeedrateOverride", typeof(double), typeof(Panel), new PropertyMetadata(0d));



        public double RapidOverride
        {
            get { return (double)GetValue(RapidOverrideProperty); }
            set { SetValue(RapidOverrideProperty, value); }
        }

        public static readonly DependencyProperty RapidOverrideProperty =
            DependencyProperty.Register("RapidOverride", typeof(double), typeof(Panel), new PropertyMetadata(0d));




    }
}
