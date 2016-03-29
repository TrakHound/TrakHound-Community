// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Win32;
using System;

namespace TH_Updater
{
    /// <summary>
    /// Functions to Set/Get/Delete Registry Keys for TrakHound Updater
    /// </summary>
    static class Registry
    {

        public const string ROOT_KEY = "Software";
        public const string APP_KEY = "TrakHound";


        public static void SetKey(string groupName, string keyName, object keyValue)
        {
            try
            {
                // Open CURRENT_USER/Software Key
                RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(ROOT_KEY, true);

                // Create/Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.CreateSubKey(APP_KEY);

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
                RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(ROOT_KEY, true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey(APP_KEY);

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
                RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(ROOT_KEY, true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey(APP_KEY);

                // Open CURRENT_USER/Software/TrakHound/Updates Key
                RegistryKey updatesKey = rootKey.OpenSubKey(groupName);

                Result = updatesKey.GetSubKeyNames();
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
                RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(ROOT_KEY, true);

                // Open CURRENT_USER/Software/TrakHound Key
                RegistryKey rootKey = key.OpenSubKey(APP_KEY, true);

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

    }
}
