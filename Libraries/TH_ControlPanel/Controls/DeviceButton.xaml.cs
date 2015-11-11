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

namespace TH_DeviceManager.Controls
{
    /// <summary>
    /// Interaction logic for DeviceButton.xaml
    /// </summary>
    public partial class DeviceButton : UserControl
    {
        public DeviceButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        public object Parent { get; set; }


        public bool DeviceEnabled
        {
            get { return (bool)GetValue(DeviceEnabledProperty); }
            set { SetValue(DeviceEnabledProperty, value); }
        }

        public static readonly DependencyProperty DeviceEnabledProperty =
            DependencyProperty.Register("DeviceEnabled", typeof(bool), typeof(DeviceButton), new PropertyMetadata(false));


        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(DeviceButton), new PropertyMetadata(null));


        public string Manufacturer
        {
            get { return (string)GetValue(ManufacturerProperty); }
            set { SetValue(ManufacturerProperty, value); }
        }

        public static readonly DependencyProperty ManufacturerProperty =
            DependencyProperty.Register("Manufacturer", typeof(string), typeof(DeviceButton), new PropertyMetadata(null));


        public string Model
        {
            get { return (string)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(string), typeof(DeviceButton), new PropertyMetadata(null));


        public string Serial
        {
            get { return (string)GetValue(SerialProperty); }
            set { SetValue(SerialProperty, value); }
        }

        public static readonly DependencyProperty SerialProperty =
            DependencyProperty.Register("Serial", typeof(string), typeof(DeviceButton), new PropertyMetadata(null));


        public string Id
        {
            get { return (string)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }

        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(string), typeof(DeviceButton), new PropertyMetadata(null));

        public delegate void Clicked_Handler(DeviceButton bt);
        public event Clicked_Handler ShareClicked;

        private void Share_Clicked(TH_WPF.Button_02 bt)
        {
            if (ShareClicked != null) ShareClicked(this);
        }

        public event Clicked_Handler Clicked;

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this);
        }

        public event Clicked_Handler RemoveClicked;

        private void Remove_Clicked(TH_WPF.Button_05 bt)
        {
            if (RemoveClicked != null) RemoveClicked(this);
        }

        public event Clicked_Handler Enabled;
        public event Clicked_Handler Disabled;

        private void enabled_CHK_Checked(object sender, RoutedEventArgs e)
        {
            if (Enabled != null) Enabled(this);
        }

        private void enabled_CHK_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Disabled != null) Disabled(this);
        }




    }
}
