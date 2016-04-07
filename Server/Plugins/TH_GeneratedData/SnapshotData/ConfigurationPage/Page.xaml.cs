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

using System.Collections.ObjectModel;
using System.Data;

using TH_Global;
using TH_Plugins;
using TH_Plugins.Server;

namespace TH_GeneratedData.SnapshotData.ConfigurationPage
{
    /// <summary>
    /// Interaction logic for Snapshots.xaml
    /// </summary>
    public partial class Page : UserControl, IConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        public string Title { get { return "Snapshot Data"; } }


        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TH_GeneratedData;component/Resources/Camera_01.png"));
                    _image.Freeze();
                }

                return _image;
            }
        }


        public event SettingChanged_Handler SettingChanged;


        public event SendData_Handler SendData;

        public void GetSentData(EventData data)
        {

        }


        public void LoadConfiguration(DataTable dt)
        {

        }

        public void SaveConfiguration(DataTable dt)
        {

        }

        ObservableCollection<Snapshot> _snapshots;
        public ObservableCollection<Snapshot> Snapshots
        {
            get
            {
                if (_snapshots == null)
                    _snapshots = new ObservableCollection<Snapshot>();
                return _snapshots;
            }

            set
            {
                _snapshots = value;
            }
        }

        private void Remove_Clicked(TH_WPF.Button bt)
        {

        }
    }

    public class Snapshot
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Link { get; set; }
    }
}
