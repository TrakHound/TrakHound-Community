using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TH_Configuration;
using TH_Global;
using TH_UserManagement.Management;

namespace TH_DeviceManager.AddDevice
{
    public partial class Page
    {

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

        private void LoadDeviceFromFile()
        {
            // Browse for Device Configuration path
            string configPath = OpenConfigurationBrowse();
            if (configPath != null)
            {
                // Get Configuration from path
                Configuration config = Configuration.Read(configPath);
                if (config != null)
                {
                    if (ParentManager.CurrentUser != null)
                    {
                        Configurations.AddConfigurationToUser(ParentManager.CurrentUser, config);
                    }
                    // If not logged in Read from File in 'C:\TrakHound\'
                    else
                    {
                        DeviceManagerList.SaveFileConfiguration(config);
                    }

                    ParentManager.AddDevice(config);
                }
            }
        }

    }
}
