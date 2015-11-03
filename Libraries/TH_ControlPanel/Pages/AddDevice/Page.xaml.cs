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

using TH_Configuration;
using TH_Global;

namespace TH_DeviceManager.Pages.AddDevice
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public DeviceManager deviceManager;

        public static string OpenConfigurationBrowse()
        {
            string result = null;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = FileLocations.TrakHound;
            dlg.Multiselect = false;
            dlg.Title = "Browse for Device Configuration File";
            dlg.Filter = "Device Configuration files (*.xml) | *.xml";

            Nullable<bool> dialogResult = dlg.ShowDialog();

            if (dialogResult == true)
            {
                if (dlg.FileName != null) result = dlg.FileName;
            }

            return result;
        }

        private void DeviceFromFile_Clicked(object o)
        {
            // Browse for Device Configuration path
            string configPath = OpenConfigurationBrowse();
            if (configPath != null)
            {
                // Get Configuration from path
                Configuration config = Configuration.ReadConfigFile(configPath);
                if (config != null)
                {
                    //UserConfiguration currentuser = CurrentUser;

                    if (deviceManager.CurrentUser != null)
                    {
                        if (deviceManager.userDatabaseSettings == null)
                        {
                            TH_Configuration.User.Management.AddConfigurationToUser(deviceManager.CurrentUser, config);

                            //Configurations = TH_Configuration.User.Management.GetConfigurationsForUser(currentuser);
                        }
                        else
                        {
                            //Configurations = TH_Database.Tables.Users.GetConfigurationsForUser(currentuser, mw.userDatabaseSettings);
                        }
                    }
                    // If not logged in Read from File in 'C:\TrakHound\'
                    else
                    {
                        //Configurations = ReadConfigurationFile();
                    }

                    deviceManager.LoadDevices();
                }
            }
        }

        private void NewDevice_Clicked(object o)
        {

        }

    }
}
