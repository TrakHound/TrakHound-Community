using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;
using System.Reflection;
using System.Diagnostics;

using TH_Global;

namespace AppStart
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            CheckForUpdates();
        }

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        static void CheckForUpdates()
        {
            string[] keyNames = GetRegistryKeyNames();
            if (keyNames != null)
            {
                if (keyNames.Length > 0)
                {
                    OpenUpdater();
                }
                else OpenClient();
            }
            else OpenClient();
        }

        static void OpenClient()
        {
            try
            {
                string clientPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + "trakhound-client.exe";

                //string clientPath = @"F:\feenux\TrakHound\TrakHound\Client\TrakHound-Client\bin\Debug\trakhound-client.exe";

                Process.Start(clientPath);
            }
            catch (Exception ex) { Console.WriteLine("TrakHound-Client-Updater.OpenClient() :: " + ex.Message); }
        }

        static void OpenUpdater()
        {
            try
            {

                string clientPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + "trakhound-client-updater.exe";

                //string clientPath = @"F:\feenux\TrakHound\TrakHound\Client\TrakHound-Client-Updater\bin\Debug\trakhound-client-updater.exe";

                Process.Start(clientPath, "runas");
            }
            catch (Exception ex) { Console.WriteLine("TrakHound-Client-Updater.OpenUpdater() :: " + ex.Message); }
        }


        public static string GetRegistryKey(string keyName)
        {
            string Result = null;

            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound");

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey("Updates");

                // Open CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                RegistryKey updateKey = updatesKey.OpenSubKey(keyName);

                // Read value for [keyName] to [keyValue]
                Result = updateKey.GetValue(keyName).ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoUpdater_GetRegistryKey() : " + ex.Message);
            }

            return Result;
        }

        public static string[] GetRegistryKeyNames()
        {
            string[] Result = null;

            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound");

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey("Updates");

                Result = updatesKey.GetSubKeyNames();



                //// Open CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                //RegistryKey updateKey = updatesKey.OpenSubKey(keyName);

                //// Read value for [keyName] to [keyValue]
                //Result = updateKey.GetValue(keyName).ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoUpdater_GetRegistryKeys() : " + ex.Message);
            }

            return Result;
        }

        public static void DeleteRegistryKey(string keyName)
        {
            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound", true);

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey("Updates", true);

                // Delete CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                updatesKey.DeleteSubKey(keyName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoUpdater_DeleteRegistryKey() : " + ex.Message);
            }
        }


    }
}
