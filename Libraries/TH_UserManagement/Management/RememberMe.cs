using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

using TH_Configuration;
using TH_Global;

namespace TH_UserManagement.Management
{
    public static class RememberMe
    {

        public static bool Set(UserConfiguration userConfig, RememberMeType type)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Users.RememberMe.Set(userConfig, type);
            }
            else
            {
                result = Local.Users.RememberMe.Set(userConfig, type);
            }

            return result;
        }

        public static UserConfiguration Get(RememberMeType type)
        {
            UserConfiguration result = null;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Users.RememberMe.Get(type);
            }
            else
            {
                result = Local.Users.RememberMe.Get(type, UserManagementSettings.Database);
            }

            return result;
        }

        public static bool Clear(RememberMeType type)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Users.RememberMe.Clear(type);
            }
            else
            {
                result = Local.Users.RememberMe.Clear(type);
            }

            return result;
        }

        public static class Registry_Functions
        {
            public static void SetRegistryKey(string keyName, object keyValue)
            {
                try
                {
                    // Open CURRENT_USER/Software Key
                    RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                    // Create/Open CURRENT_USER/Software/TrakHound Key
                    RegistryKey rootKey = key.CreateSubKey("TrakHound");

                    // Create/Open CURRENT_USER/Software/TrakHound/Updates Key
                    RegistryKey updatesKey = rootKey.CreateSubKey("RememberMe");

                    // Create/Open CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                    RegistryKey updateKey = updatesKey.CreateSubKey(keyName);

                    // Update value for [keyName] to [keyValue]
                    updateKey.SetValue(keyName, keyValue);
                }
                catch (Exception ex)
                {
                    //Logger.Log("UserManagement_RememberMe_SetRegistryKey() : " + ex.Message);
                }
            }

            public static string GetRegistryKey(string keyName)
            {
                string Result = null;

                try
                {
                    // Open CURRENT_USER/Software Key
                    RegistryKey key = Registry.CurrentUser.OpenSubKey("Software", true);

                    // Open CURRENT_USER/Software/TrakHound Key
                    if (key != null) key = key.OpenSubKey("TrakHound");

                    // Open CURRENT_USER/Software/TrakHound/Updates Key
                    if (key != null) key = key.OpenSubKey("RememberMe");

                    // Open CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                    if (key != null) key = key.OpenSubKey(keyName);

                    // Read value for [keyName] to [keyValue]
                    if (key != null)
                    {
                        var val = key.GetValue(keyName);
                        if (val != null) Result = val.ToString();
                    }
                }
                catch (Exception ex)
                {
                    //Logger.Log("UserManagement_RememberMe_GetRegistryKey() : " + ex.Message);
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
                    if (key != null) key = key.OpenSubKey("TrakHound");

                    // Open CURRENT_USER/Software/TrakHound/Updates Key
                    if (key != null) key = key.OpenSubKey("RememberMe");

                    if (key != null) Result = key.GetSubKeyNames();
                }
                catch (Exception ex)
                {
                    //Logger.Log("UserManagement_RememberMe_GetRegistryKeys() : " + ex.Message);
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
                    if (key != null) key = key.OpenSubKey("TrakHound", true);

                    // Open CURRENT_USER/Software/TrakHound/Updates Key
                    if (key != null) key = key.OpenSubKey("RememberMe", true);

                    // Delete CURRENT_USER/Software/TrakHound/Updates/[keyName] Key
                    if (key != null) key.DeleteSubKey(keyName, false);
                }
                catch (Exception ex)
                {
                   //Logger.Log("UserManagement_RememberMe_DeleteRegistryKey() : " + ex.Message);
                }
            }
        }

    }
}
