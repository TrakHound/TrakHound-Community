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

using System.Data;

using TH_Configuration;
using TH_Global.Functions;
using TH_Plugins_Client;

namespace TH_DeviceCompare_CNC.Text.Execution_Mode
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Plugin : UserControl
    {
        public Plugin()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        const string link = "Execution Mode";


        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(Plugin), new PropertyMetadata(null));


        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;


        void Update(DataEvent_Data de_d)
        {
            if (de_d != null && de_d.data01 != null && de_d.data01.GetType() == typeof(Configuration))
            {
                // Snapshot Table Data
                if (de_d.id.ToLower() == "statusdata_snapshots")
                {
                    this.Dispatcher.BeginInvoke(new Action<object>(UpdateText), Priority_Context, new object[] { de_d.data02 });
                }
            }
        }


        void UpdateText(object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "name", link, "value");

                Value = value;

                if (value != null)
                {
                    switch (value)
                    {
                        case "ACTIVE":
                            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                            Background = new SolidColorBrush(Color.FromRgb(25, 180, 25));
                            break;
                        default:
                            Foreground = (Brush)TryFindResource("Foreground_Normal");
                            Background = new SolidColorBrush(Colors.Transparent);
                            break;
                    }
                }
            }
        }

    }
}
