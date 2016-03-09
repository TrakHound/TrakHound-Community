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

using TH_Configuration;
using TH_Global;
using TH_UserManagement.Management;

namespace TH_DeviceManager.AddDevice.Pages
{
    /// <summary>
    /// Interaction logic for LoadFromFile.xaml
    /// </summary>
    public partial class LoadFromFile : UserControl, IPage
    {
        public LoadFromFile()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string PageName { get { return "Load Device Configuration From File"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/List_01.png")); } }

        public Page ParentPage { get; set; }


        public int AddedSuccessfully
        {
            get { return (int)GetValue(AddedSuccessfullyProperty); }
            set { SetValue(AddedSuccessfullyProperty, value); }
        }

        public static readonly DependencyProperty AddedSuccessfullyProperty =
            DependencyProperty.Register("AddedSuccessfully", typeof(int), typeof(LoadFromFile), new PropertyMetadata(0));

        private void Browse_Clicked(TH_WPF.Button bt)
        {
            LoadDeviceFromFile();
        }

        public static string[] OpenConfigurationBrowse()
        {
            string[] result = null;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = FileLocations.TrakHound;
            dlg.Multiselect = true;
            dlg.Title = "Browse for Device Configuration File(s)";
            dlg.Filter = "Device Configuration files (*.xml) | *.xml";

            Nullable<bool> dialogResult = dlg.ShowDialog();

            if (dialogResult == true)
            {
                if (dlg.FileName != null) result = dlg.FileNames;
            }

            return result;
        }

        private void LoadDeviceFromFile()
        {
            // Browse for Device Configuration path
            string[] paths = OpenConfigurationBrowse();
            if (paths != null)
            {
                AddedSuccessfully = 0;

                foreach (var path in paths)
                {
                    LoadDevice(path);

                    AddedSuccessfully++;
                }
            }
        }

        private bool LoadDevice(string path)
        {
            // Get Configuration from path
            Configuration config = Configuration.Read(path);
            if (config != null)
            {
                if (ParentPage.ParentManager.CurrentUser != null)
                {
                    Configurations.AddConfigurationToUser(ParentPage.ParentManager.CurrentUser, config);
                }
                // If not logged in Read from File in 'C:\TrakHound\'
                else
                {
                    DeviceManagerList.SaveFileConfiguration(config);
                }

                ParentPage.ParentManager.AddDevice(config);

                return true;
            }

            return false;
        }

        private void Rectangle_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;
        }

        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                int added = 0;
                AddedSuccessfully = 0;

                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var path in paths)
                {
                    if (LoadDevice(path))
                    {
                        added++;
                        AddedSuccessfully++;
                    }
                }

                if (added < paths.Length) TH_WPF.MessageBox.Show("Some devices did not get added correctly. Review the Developer Console for further information.");
            }
        }

        private void DeviceManager_Clicked(TH_WPF.Button bt)
        {
            if (ParentPage != null && ParentPage.ParentManager != null)
            {
                ParentPage.ParentManager.Open();
            }
        }
    }
}
