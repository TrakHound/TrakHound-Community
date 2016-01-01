using System;
using System.Text;
using System.Net;
using System.IO;
using Microsoft.Win32;

namespace TH_Updater
{
    public static class Tools
    {

        public static void DownloadFile(string url, string localFile)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(url, localFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during Update DownloadFile() : " + ex.Message);
            }
        }

        static Random random = new Random();
        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }


        public static string CreateLocalFileName()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string directory = appData + "\\" + "TrakHound";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            directory = appData + "\\" + "temp";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            //string directory = AppDomain.CurrentDomain.BaseDirectory + "temp";
            //if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string file = RandomString(20);

            string path = directory + "\\" + file;

            if (File.Exists(path)) CreateLocalFileName();

            return path;
        }

        public static string CreateLocalFileName(string directory)
        {
            string file = RandomString(20);

            string path = directory + "\\" + file;

            if (File.Exists(path)) CreateLocalFileName(directory);

            return path;
        }


        #region "Registry Functions"

        public static void SetRegistryKey(string groupName, string keyName, object keyValue)
        {
            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Create/Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.CreateSubKey("TrakHound");

                // Create/Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.CreateSubKey(groupName);

                // Create/Open CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                RegistryKey updateKey = updatesKey.CreateSubKey(keyName);

                // Update value for [keyName] to [keyValue]
                updateKey.SetValue(keyName, keyValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoUpdater_SetRegistryKey() : " + ex.Message);
            }
        }

        public static string GetRegistryKey(string groupName, string keyName)
        {
            string Result = null;

            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound");

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey(groupName);

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

        public static string[] GetRegistryKeyNames(string groupName)
        {
            string[] Result = null;

            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound");

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey(groupName);

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

        public static void DeleteRegistryKey(string groupName, string keyName)
        {
            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey("TrakHound", true);

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey(groupName, true);

                // Delete CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                updatesKey.DeleteSubKey(keyName, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("AutoUpdater_DeleteRegistryKey() : " + ex.Message);
            }
        }

        #endregion

        
    }
}
