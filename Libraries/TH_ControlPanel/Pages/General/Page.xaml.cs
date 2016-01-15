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
using System.Data;
using System.IO;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Server;
using TH_UserManagement;
using TH_UserManagement.Management;

namespace TH_DeviceManager.Pages.General
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, ConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "Page Interface"

        public string PageName { get { return "Device Overview"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/About_01.png")); } }

        public UserConfiguration currentUser { get; set; }

        public Database_Settings userDatabaseSettings;

        public event SaveRequest_Handler SaveRequest;

        public event SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            // Load Cloud Settings
            bool cloud = false;
            string cloud_str = Table_Functions.GetTableValue("/UseTrakHoundCloud", dt);
            if (cloud_str != null)
            {
                bool.TryParse(cloud_str, out cloud);
            }

            if (cloud) { cloud_RADIO.IsChecked = true; local_RADIO.IsChecked  = false; }
            else { cloud_RADIO.IsChecked = false; local_RADIO.IsChecked = true; }

            UseTrakHoundCloud = cloud;
        }

        public void SaveConfiguration(DataTable dt)
        {

            // Save Cloud Settings
            Table_Functions.UpdateTableValue(UseTrakHoundCloud.ToString(), "/UseTrakHoundCloud", dt);

        }

        public Page_Type PageType { get; set; }

        #endregion

        bool UseTrakHoundCloud = false;

        private void Cloud_Checked(object sender, RoutedEventArgs e)
        {
            UseTrakHoundCloud = true;

            RadioButton radio = (RadioButton)sender;
            if (radio.IsKeyboardFocused || radio.IsMouseCaptured) SettingChanged(null, null, null);
        }

        private void Local_Checked(object sender, RoutedEventArgs e)
        {
            UseTrakHoundCloud = false;

            RadioButton radio = (RadioButton)sender;
            if (radio.IsKeyboardFocused || radio.IsMouseCaptured) SettingChanged(null, null, null);
        }
    }
}
