using System;
using System.Threading;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

using TH_Global;
//using TH_Updater;

namespace TH_Startup
{
    class Program
    {
        static void Main(string[] args)
        {
            Update();

            OpenClient();
        }

        static void Update()
        {
            string[] keyNames = GetRegistryKeyNames();
            if (keyNames != null)
            {
                foreach (string keyName in keyNames)
                {
                    Console.WriteLine("Registry Update Key Found :: " + keyName);

                    string keyValue = GetRegistryKey(keyName);
                    if (keyValue != null)
                    {
                        if (keyValue.Contains(";"))
                        {
                            string unzipDirectory = keyValue.Substring(0, keyValue.IndexOf(';'));
                            string copyDirectory = keyValue.Substring(keyValue.IndexOf(';') + 1);

                            Console.WriteLine(keyName + " = " + keyValue);
                            Console.WriteLine("unzipDirectory = " + unzipDirectory);
                            Console.WriteLine("copyDirectory = " + copyDirectory);

                            foreach (string filePath in Directory.GetFiles(unzipDirectory))
                            {
                                string fileName = Path.GetFileName(filePath);

                                string copyPath = copyDirectory + "\\" + fileName;

                                try
                                {
                                    File.Copy(filePath, copyPath, true);
                                }
                                catch { }
                            }
                            DeleteRegistryKey(keyName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Open/Start the TrakHound-Client application
        /// </summary>
        static void OpenClient()
        {
            try
            {
                Process.Start("trakhound-client.exe");
            }
            catch (Exception ex ) { Console.WriteLine("TH_StartUp.OpenClient() :: " + ex.Message); }
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
