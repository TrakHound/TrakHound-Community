// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Win32;
using System;

using TH_Global;

namespace TrakHound_Updater
{
    /// <summary>
    /// Functions to Set/Get/Delete Registry Keys for TrakHound Updater
    /// </summary>
    static class Registry
    {

        public const string WOW64_KEY = "WOW6432Node";
        public const string ROOT_KEY = "Software";
        public const string APP_KEY = "TrakHound";


        public static void SetKey(string keyName, object keyValue, string groupName = null)
        {
            try
            {
                // Open LOCAL_MACHINE/Software Key
                RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY, true);

                // Create/Open LOCAL_MACHINE/Software/TrakHound Key
                key = key.OpenSubKey(APP_KEY);

                // Try looking for 64 bit version in WOW6432Node key
                if (key == null)
                {
                    key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY, true);
                    key = key.OpenSubKey(WOW64_KEY);
                    if (key != null) key = key.OpenSubKey(APP_KEY, true);
                }

                // Create/Open LOCAL_MACHINE/Software/TrakHound/[groupName] Key
                if (groupName != null) key = key.CreateSubKey(groupName);

                // Update value for [keyName] to [keyValue]
                key.SetValue(keyName, keyValue);
            }
            catch(Exception ex) { Logger.Log("SetKey() :: Exception :: " + ex.Message); }
        }

        public static string GetValue(string keyName, string groupName = null)
        {
            string result = null;

            try
            {
                // Open LOCAL_MACHINE/Software Key
                RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY, true);

                // Create/Open LOCAL_MACHINE/Software/TrakHound Key
                key = key.OpenSubKey(APP_KEY);

                // Try looking for 64 bit version in WOW6432Node key
                if (key == null)
                {
                    key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY, true);
                    key = key.OpenSubKey(WOW64_KEY, true);
                    if (key != null) key = key.OpenSubKey(APP_KEY);
                }

                // Open LOCAL_MACHINE/Software/TrakHound/[groupName] Key
                if (groupName != null) key = key.OpenSubKey(groupName);

                // Read value for [keyName] to [keyValue]
                result = key.GetValue(keyName).ToString();
            }
            catch (Exception ex) { Logger.Log("GetValue() :: Exception :: keyName = " + keyName + " :: groupName = " + groupName + " :: " + ex.Message); }

            return result;
        }

        public static string[] GetValueNames(string groupName = null)
        {
            string[] result = null;

            try
            {
                // Open LOCAL_MACHINE/Software Key
                RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY, true);

                // Create/Open LOCAL_MACHINE/Software/TrakHound Key
                key = key.OpenSubKey(APP_KEY);

                // Try looking for 64 bit version in WOW6432Node key
                if (key == null)
                {
                    key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY, true);
                    key = key.OpenSubKey(WOW64_KEY, true);
                    if (key != null) key = key.OpenSubKey(APP_KEY);
                }

                // Open LOCAL_MACHINE/Software/TrakHound/[groupName] Key
                if (groupName != null) key = key.OpenSubKey(groupName);

                result = key.GetValueNames();
            }
            catch (Exception ex) { Logger.Log("GetValueNames() :: Exception :: " + ex.Message); }

            return result;
        }

        public static void DeleteValue(string keyName, string groupName = null)
        {
            try
            {
                // Open LOCAL_MACHINE/Software Key
                RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY, true);

                // Create/Open LOCAL_MACHINE/Software/TrakHound Key
                key = key.OpenSubKey(APP_KEY);

                // Try looking for 64 bit version in WOW6432Node key
                if (key == null)
                {
                    key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY, true);
                    key = key.OpenSubKey(WOW64_KEY, true);
                    if (key != null) key = key.OpenSubKey(APP_KEY, true);
                }

                // Open LOCAL_MACHINE/Software/TrakHound/[groupName] Key
                if (groupName != null) key = key.OpenSubKey(groupName, true);

                // Delete LOCAL_MACHINE/Software/TrakHound/[groupName]/[keyName] Key
                key.DeleteValue(keyName, true);
            }
            catch (Exception ex) { Logger.Log("DeleteValue() :: Exception :: " + ex.Message); }
        }

    }
}
