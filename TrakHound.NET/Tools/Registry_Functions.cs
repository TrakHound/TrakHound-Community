// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using Microsoft.Win32;
using NLog;
using System;

namespace TrakHound.Tools
{
    public static class Registry_Functions
    {
        public const string WOW64_KEY = "WOW6432Node";
        public const string ROOT_KEY = "Software";
        public const string APP_KEY = "TrakHound";

        private static Logger logger = LogManager.GetCurrentClassLogger();

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
                    if (key != null) key = key.OpenSubKey(WOW64_KEY);
                    if (key != null) key = key.OpenSubKey(APP_KEY, true);
                }

                // Create/Open LOCAL_MACHINE/Software/TrakHound/[groupName] Key
                if (groupName != null && key != null) key = key.CreateSubKey(groupName);

                // Update value for [keyName] to [keyValue]
                if (key != null) key.SetValue(keyName, keyValue);
            }
            catch (Exception ex) { logger.Error(ex); }
        }

        public static string GetValue(string keyName, string groupName = null)
        {
            string result = null;

            try
            {
                // Open LOCAL_MACHINE/Software Key
                RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY);

                // Create/Open LOCAL_MACHINE/Software/TrakHound Key
                if (key != null) key = key.OpenSubKey(APP_KEY);

                // Try looking for 64 bit version in WOW6432Node key
                if (key == null)
                {
                    key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY);
                    if (key != null) key = key.OpenSubKey(WOW64_KEY);
                    if (key != null) key = key.OpenSubKey(APP_KEY);
                }

                // Open LOCAL_MACHINE/Software/TrakHound/[groupName] Key
                if (groupName != null && key != null) key = key.OpenSubKey(groupName);

                // Read value for [keyName] to [keyValue]
                if (key != null)
                {
                    var val = key.GetValue(keyName);
                    if (val != null) result = val.ToString();
                }
            }
            catch (Exception ex) { logger.Error(ex); }

            return result;
        }
        
        public static string[] GetValueNames(string groupName = null)
        {
            string[] result = null;

            try
            {
                // Open LOCAL_MACHINE/Software Key
                RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY);

                // Create/Open LOCAL_MACHINE/Software/TrakHound Key
                if (key != null) key = key.OpenSubKey(APP_KEY);

                // Try looking for 64 bit version in WOW6432Node key
                if (key == null)
                {
                    key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY);
                    if (key != null) key = key.OpenSubKey(WOW64_KEY);
                    if (key != null) key = key.OpenSubKey(APP_KEY);
                }

                // Open LOCAL_MACHINE/Software/TrakHound/[groupName] Key
                if (groupName != null && key != null) key = key.OpenSubKey(groupName);

                if (key != null) result = key.GetValueNames();
            }
            catch (Exception ex) { logger.Error(ex); }

            return result;
        }
        
        public static string[] GetKeyNames(string groupName = null)
        {
            string[] result = null;

            try
            {
                // Open LOCAL_MACHINE/Software Key
                RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY);

                // Create/Open LOCAL_MACHINE/Software/TrakHound Key
                if (key != null) key = key.OpenSubKey(APP_KEY);

                // Try looking for 64 bit version in WOW6432Node key
                if (key == null)
                {
                    key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY);
                    if (key != null) key = key.OpenSubKey(WOW64_KEY);
                    if (key != null) key = key.OpenSubKey(APP_KEY);
                }

                // Open LOCAL_MACHINE/Software/TrakHound/[groupName] Key
                if (groupName != null && key != null) key = key.OpenSubKey(groupName);

                if (key != null) result = key.GetSubKeyNames();
            }
            catch (Exception ex) { logger.Error(ex); }

            return result;
        }

        public static void DeleteValue(string keyName, string groupName = null)
        {
            try
            {
                // Open LOCAL_MACHINE/Software Key
                RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY, true);

                // Create/Open LOCAL_MACHINE/Software/TrakHound Key
                if (key != null) key = key.OpenSubKey(APP_KEY);

                // Try looking for 64 bit version in WOW6432Node key
                if (key == null)
                {
                    key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY, true);
                    if (key != null) key = key.OpenSubKey(WOW64_KEY, true);
                    if (key != null) key = key.OpenSubKey(APP_KEY, true);
                }

                // Open LOCAL_MACHINE/Software/TrakHound/[groupName] Key
                if (groupName != null && key != null) key = key.OpenSubKey(groupName, true);

                // Delete LOCAL_MACHINE/Software/TrakHound/[groupName]/[keyName] Key
                if (key != null) key.DeleteValue(keyName, false);
            }
            catch (Exception ex) { logger.Error(ex); }
        }

        public static void DeleteKey(string keyName, string groupName = null)
        {
            try
            {
                // Open LOCAL_MACHINE/Software Key
                RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY, true);

                // Create/Open LOCAL_MACHINE/Software/TrakHound Key
                if (key != null) key = key.OpenSubKey(APP_KEY);

                // Try looking for 64 bit version in WOW6432Node key
                if (key == null)
                {
                    key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(ROOT_KEY, true);
                    if (key != null) key = key.OpenSubKey(WOW64_KEY, true);
                    if (key != null) key = key.OpenSubKey(APP_KEY, true);
                }

                // Open LOCAL_MACHINE/Software/TrakHound/[groupName] Key
                if (groupName != null && key != null) key = key.OpenSubKey(groupName, true);

                // Delete LOCAL_MACHINE/Software/TrakHound/[groupName]/[keyName] Key
                if (key != null) key.DeleteSubKey(keyName);
            }
            catch (Exception ex) { logger.Error(ex); }
        }
        
    }
}
