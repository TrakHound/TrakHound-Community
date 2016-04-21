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



namespace SimpleClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Read Database Plugins (stores to static list in TH_Database.Global.Plugins)
            TH_Database.DatabasePluginReader.ReadPlugins();

            //Application.Current.MainWindow = this;

            DeviceManager_Initialize();

            LoadPlugins();
        }



        public object SelectedPlugin
        {
            get { return (object)GetValue(SelectedPluginProperty); }
            set { SetValue(SelectedPluginProperty, value); }
        }

        public static readonly DependencyProperty SelectedPluginProperty =
            DependencyProperty.Register("SelectedPlugin", typeof(object), typeof(MainWindow), new PropertyMetadata(null));

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Plugins_Closed();
        }
    }
}
