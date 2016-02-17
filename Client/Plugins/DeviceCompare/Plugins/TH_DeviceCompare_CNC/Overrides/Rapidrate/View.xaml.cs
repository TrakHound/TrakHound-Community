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

namespace TH_DeviceCompare_CNC.Overrides.Rapidrate
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

        const string overrideLink = "Rapidrate Override";


        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(Plugin), new PropertyMetadata(0d));


        const System.Windows.Threading.DispatcherPriority Priority_Background = System.Windows.Threading.DispatcherPriority.Background;

        const System.Windows.Threading.DispatcherPriority Priority_Context = System.Windows.Threading.DispatcherPriority.ContextIdle;


        void Update(DataEvent_Data de_d)
        {
            if (de_d != null && de_d.data01 != null && de_d.data01.GetType() == typeof(Configuration))
            {
                // Snapshot Table Data
                if (de_d.id.ToLower() == "statusdata_snapshots")
                {
                    this.Dispatcher.BeginInvoke(new Action<object>(UpdateOverride), Priority_Context, new object[] { de_d.data02 });
                }
            }
        }


        void UpdateOverride(object snapshotData)
        {
            DataTable dt = snapshotData as DataTable;
            if (dt != null)
            {
                string value = DataTable_Functions.GetTableValue(dt, "name", overrideLink, "value");

                if (value != null)
                {
                    double ovr = 0;
                    if (double.TryParse(value, out ovr))
                    {
                        Value = ovr * 0.01;
                    }
                }
            }
        }

    }
}
