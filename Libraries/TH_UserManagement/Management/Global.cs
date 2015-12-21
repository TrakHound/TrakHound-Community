using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using System.IO;
using System.Data;
using System.Net;
using System.Collections.Specialized;
using System.Xml;
using System.Drawing;
using System.Drawing.Imaging;

using Newtonsoft.Json;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;

namespace TH_UserManagement.Management
{

    public static class Users
    {
        public static UserConfiguration Login(string username, string password, Database_Settings userDatabaseSettings)
        {
            UserConfiguration result = null;

            if (userDatabaseSettings == null)
            {
                result = Remote.Users.Login(username, password);
            }
            else
            {
                result = Local.Users.Login(username, password, userDatabaseSettings);
            }

            return result;
        }

        public static bool CreateUser(UserConfiguration userConfig, string password, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.Users.CreateUser(userConfig, password);
            }
            else
            {
                //result = Local.Users.CreateUser(userConfig, password, userDatabaseSettings);
            }
            
            return result;
        }

        public static VerifyUsernameReturn VerifyUsername(string username, Database_Settings userDatabaseSettings)
        {
            VerifyUsernameReturn result = null;

            if (userDatabaseSettings == null)
            {
                result = Remote.Users.VerifyUsername(username);
            }
            else
            {
                //result = Local.Users.VerifyUsername(username, userDatabaseSettings);
            }

            return result;
        }

        public static bool UpdateLoginTime(UserConfiguration userConfig, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.Users.UpdateLoginTime(userConfig);
            }
            else
            {
                //result = Local.Users.UpdateLoginTime(userConfig, userDatabaseSettings);
            }

            return result;
        }

        public static bool UpdateImageURL(string imageURL, UserConfiguration userConfig, Database_Settings userDatabaseSettings)
        {
            userConfig.image_url = imageURL;

            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.Users.UpdateImageURL(imageURL, userConfig);
            }
            else
            {
                //result = Local.Users.UpdateImageURL(imageURL, userConfig, userDatabaseSettings);
            }

            return result;

        }
    }

    public static class Configurations
    {
        public static bool AddConfigurationToUser(UserConfiguration userConfig, Configuration configuration, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.Configurations.AddConfigurationToUser(userConfig, configuration);
            }
            else
            {
                //result = Local.Configurations.AddConfigurationToUser(userConfig, configuration, userDatabaseSettings);
            }

            return result;
        }

        public static string[] GetConfigurationsForUser(UserConfiguration userConfig, Database_Settings userDatabaseSettings)
        {
            string[] result = null;

            if (userDatabaseSettings == null)
            {
                result = Remote.Configurations.GetConfigurationsForUser(userConfig);
            }
            else
            {
                //result = Local.Configurations.GetConfigurationsForUser(userConfig, userDatabaseSettings);
            }

            return result;
        }

        //public static List<Configuration> GetConfigurationsForUser(UserConfiguration userConfig, Database_Settings userDatabaseSettings)
        //{
        //    List<Configuration> result = null;

        //    if (userDatabaseSettings == null)
        //    {
        //        result = Remote.Configurations.GetConfigurationsForUser(userConfig);
        //    }
        //    else
        //    {
        //        //result = Local.Configurations.GetConfigurationsForUser(userConfig, userDatabaseSettings);
        //    }

        //    return result;
        //}

        public static DataTable GetConfigurationTable(string table, Database_Settings userDatabaseSettings)
        {
            DataTable result = null;

            if (userDatabaseSettings == null)
            {
                result = Remote.Configurations.GetConfigurationTable(table);
            }
            else
            {
                //result = Local.Configurations.GetConfigurationTable(table, userDatabaseSettings);
            }

            return result;
        }


        public class UpdateInfo
        {
            public string UniqueId { get; set; }
            public string UpdateId { get; set; }
            public string Enabled { get; set; }
        }

        public static UpdateInfo GetServerUpdateInfo(string table, Database_Settings userDatabaseSettings)
        {
            UpdateInfo result = null;

            if (userDatabaseSettings == null)
            {
                result = Remote.Configurations.GetServerUpdateInfo(table);
            }
            else
            {
                //result = Local.Configurations.GetConfigurationTable(table, userDatabaseSettings);
            }

            return result;
        }

        public static bool UpdateConfigurationTable(string tableName, DataTable dt, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.Configurations.UpdateConfigurationTable(tableName, dt);
            }
            else
            {
                //result = Local.Configurations.UpdateConfigurationTable(tableName, dt, userDatabaseSettings);
            }

            return result;
        }

        public static bool UpdateConfigurationTable(string address, string value, string tableName, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.Configurations.UpdateConfigurationTable(address, value, tableName);
            }
            else
            {
                //result = Local.Configurations.UpdateConfigurationTable(address, value, tableName, userDatabaseSettings);
            }

            return result;
        }

        public static bool UpdateConfigurationTable(string address, string value, string attributes, string tableName, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.Configurations.UpdateConfigurationTable(address, value, attributes, tableName);
            }
            else
            {
                //result = Local.Configurations.UpdateConfigurationTable(address, value, attributes, tableName, userDatabaseSettings);
            }

            return result;
        }

        public static bool ClearConfigurationTable(string tableName, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.Configurations.ClearConfigurationTable(tableName);
            }
            else
            {
                //result = Local.Configurations.ClearConfigurationTable(tableName, userDatabaseSettings);
            }

            return result;
        }

        public static bool CreateConfigurationTable(string tableName, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.Configurations.CreateConfigurationTable(tableName);
            }
            else
            {
                //result = Local.Configurations.CreateConfigurationTable(tableName, userDatabaseSettings);
            }

            return result;
        }

        public static bool CreateConfigurationTable(UserConfiguration userConfig, Configuration configuration, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.Configurations.CreateConfigurationTable(userConfig, configuration);
            }
            else
            {
                //result = Local.Configurations.CreateConfigurationTable(userConfig, configuration, userDatabaseSettings);
            }

            return result;
        }

        //public static string GetConfigurationTableName(UserConfiguration userConfig, Configuration configuration)
        //{
        //    string table = userConfig.username + "_" + configuration.Description.Manufacturer + "_" + configuration.Description.Machine_Type + "_" + configuration.Description.Machine_ID + "_Configuration";
        //    table = table.Replace(' ', '_');

        //    return table;
        //}

        public static string CreateConfigurationTableName(UserConfiguration userConfig)
        {
            return userConfig.username + "_" + String_Functions.RandomString(20);
        }

        public static bool RemoveConfigurationTable(string tableName, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.Configurations.RemoveConfigurationTable(tableName);
            }
            else
            {
                //result = Local.Configurations.RemoveConfigurationTable(tableName, userDatabaseSettings);
            }

            return result;
        }
    }


    public static class Images
    {

        public static string OpenImageBrowse(string title)
        {
            string result = null;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = FileLocations.TrakHound;
            dlg.Multiselect = false;
            dlg.Title = title;
            dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            Nullable<bool> dialogResult = dlg.ShowDialog();

            if (dialogResult == true)
            {
                if (dlg.FileName != null) result = dlg.FileName;
            }

            return result;
        }

        /// <summary>
        /// Uploads an Image to the TrakHound Server
        /// </summary>
        /// <param name="localpath"></param>
        /// <returns></returns>
        public static bool UploadImage(string localpath, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.Images.UploadImage(localpath);
            }
            else
            {
                //result = Local.Configurations.RemoveConfigurationTable(tableName, userDatabaseSettings);
            }

            return result;
        }

        public static System.Drawing.Image GetImage(string filename, Database_Settings userDatabaseSettings)
        {
            System.Drawing.Image result = null;

            if (userDatabaseSettings == null)
            {
                result = Remote.Images.GetImage(filename);
            }
            else
            {
                //result = Local.Configurations.RemoveConfigurationTable(tableName, userDatabaseSettings);
            }

            return result;
        }

    }

    public static class ProfileImages
    {

        public static string OpenImageBrowse()
        {
            string result = null;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = FileLocations.TrakHound;
            dlg.Multiselect = false;
            dlg.Title = "Browse for Profile Image";
            dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            Nullable<bool> dialogResult = dlg.ShowDialog();

            if (dialogResult == true)
            {
                if (dlg.FileName != null) result = dlg.FileName;
            }

            return result;
        }

        public static System.Drawing.Image ProcessImage(string path)
        {
            System.Drawing.Image result = null;

            if (File.Exists(path))
            {
                System.Drawing.Image img = Image_Functions.CropImageToCenter(System.Drawing.Image.FromFile(path));

                result = Image_Functions.SetImageSize(img, 200, 200);
            }

            return result;
        }

        public static System.Drawing.Image ProcessImage(System.Drawing.Image image)
        {
            System.Drawing.Image result = null;

            System.Drawing.Image img = Image_Functions.CropImageToCenter(image);

            result = Image_Functions.SetImageSize(img, 200, 200);

            return result;
        }


        /// <summary>
        /// Uploads a Profile Image
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="localpath"></param>
        /// <returns></returns>
        public static bool UploadProfileImage(string filename, string localpath, Database_Settings userDatabaseSettings)
        {
            bool result = false;

            if (userDatabaseSettings == null)
            {
                result = Remote.ProfileImages.UploadProfileImage(filename, localpath);
            }
            else
            {
                //result = Local.Configurations.RemoveConfigurationTable(tableName, userDatabaseSettings);
            }

            return result;
        }


        public static System.Drawing.Image GetProfileImage(UserConfiguration userConfig, Database_Settings userDatabaseSettings)
        {
            System.Drawing.Image result = null;

            if (userDatabaseSettings == null)
            {
                result = Remote.ProfileImages.GetProfileImage(userConfig);
            }
            else
            {
                //result = Local.Configurations.RemoveConfigurationTable(tableName, userDatabaseSettings);
            }

            return result;
        }

    }

}
