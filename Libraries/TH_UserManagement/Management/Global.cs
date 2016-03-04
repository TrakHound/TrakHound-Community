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

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;

namespace TH_UserManagement.Management
{

    public static class Users
    {
        public static UserConfiguration Login(string username, string password)
        {
            UserConfiguration result = null;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Users.Login(username, password);
            }
            else
            {
                result = Local.Users.Login(username, password, UserManagementSettings.Database);
            }

            return result;
        }

        public static bool CreateUser(UserConfiguration userConfig, string password)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Users.CreateUser(userConfig, password);
            }
            else
            {
                result = Local.Users.CreateUser(userConfig, password, UserManagementSettings.Database);
            }
            
            return result;
        }

        public static VerifyUsernameReturn VerifyUsername(string username)
        {
            VerifyUsernameReturn result = null;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Users.VerifyUsername(username);
            }
            else
            {
                result = new VerifyUsernameReturn();
                result.available = true;
                result.message = "Available";
                //result = Local.Users.VerifyUsername(username, userDatabaseSettings);
            }

            return result;
        }

        public static bool UpdateLoginTime(UserConfiguration userConfig)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Users.UpdateLoginTime(userConfig);
            }
            else
            {
                result = Local.Users.UpdateLoginTime(userConfig, UserManagementSettings.Database);
            }

            return result;
        }

        public static bool UpdateImageURL(string imageURL, UserConfiguration userConfig)
        {
            userConfig.image_url = imageURL;

            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Users.UpdateImageURL(imageURL, userConfig);
            }
            else
            {
                result = Local.Users.UpdateImageURL(imageURL, userConfig, UserManagementSettings.Database);
            }

            return result;

        }
    }

    public static class Configurations
    {
        public static bool AddConfigurationToUser(UserConfiguration userConfig, Configuration configuration)
        {
            bool result = false;


            //string tableName = CreateTableName(userConfig);

            //// Set new Unique Id
            //string uniqueId = String_Functions.RandomString(20);
            //configuration.UniqueId = uniqueId;
            //XML_Functions.SetInnerText(configuration.ConfigurationXML, "UniqueId", uniqueId);

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Configurations.Add(userConfig, configuration);
            }
            else
            {
                result = Local.Configurations.Add(userConfig, configuration, UserManagementSettings.Database);
            }

            return result;
        }

        //static string CreateTableName(UserConfiguration userConfig)
        //{
        //    return userConfig.username + "_" + String_Functions.RandomString(20);
        //}


        public static string[] GetConfigurationsForUser(UserConfiguration userConfig)
        {
            string[] result = null;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Configurations.GetList(userConfig);
            }
            else
            {
                result = Local.Configurations.GetList(userConfig, UserManagementSettings.Database);
            }

            return result;
        }

        public static List<Configuration> GetConfigurationsListForUser(UserConfiguration userConfig)
        {
            List<Configuration> result = null;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Configurations.Get(userConfig);
            }
            else
            {
                result = Local.Configurations.Get(userConfig, UserManagementSettings.Database);
            }

            return result;
        }

        public static List<DataTable> GetDeviceInfoList(UserConfiguration userConfig)
        {
            List<DataTable> result = null;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Configurations.GetDeviceInfos(userConfig);
            }
            else
            {
                result = Local.Configurations.GetDeviceInfos(userConfig, UserManagementSettings.Database);
            }

            return result;
        }

        public static DataTable GetConfigurationTable(string table)
        {
            DataTable result = null;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Configurations.GetTable(table);
            }
            else
            {
                result = Local.Configurations.GetTable(table, UserManagementSettings.Database);
            }

            return result;
        }

        public static bool UpdateIndexes(List<Tuple<string, int>> items)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Configurations.UpdateIndexes(items);
            }
            else
            {
                result = Local.Configurations.UpdateIndexes(items, UserManagementSettings.Database);
            }

            return result;
        }

        public static bool UpdateConfigurationTable(string tableName, DataTable dt)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Configurations.Update(tableName, dt);
            }
            else
            {
                result = Local.Configurations.Update(tableName, dt, UserManagementSettings.Database);
            }

            return result;
        }

        public static bool UpdateConfigurationTable(string address, string value, string tableName)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Configurations.Update(address, value, tableName);
            }
            else
            {
                result = Local.Configurations.Update(address, value, tableName, UserManagementSettings.Database);
            }

            return result;
        }

        public static bool UpdateConfigurationTable(string address, string value, string attributes, string tableName)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Configurations.Update(address, value, attributes, tableName);
            }
            else
            {
                result = Local.Configurations.Update(address, value, attributes, tableName, UserManagementSettings.Database);
            }

            return result;
        }

        public static bool ClearConfigurationTable(string tableName)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Configurations.Clear(tableName);
            }
            else
            {
                result = Local.Configurations.Clear(tableName, UserManagementSettings.Database);
            }

            return result;
        }

        public static bool CreateConfigurationTable(string tableName)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Configurations.Create(tableName);
            }
            else
            {
                result = Local.Configurations.Create(tableName, UserManagementSettings.Database);
            }

            return result;
        }

        public static string CreateConfigurationTableName(UserConfiguration userConfig)
        {
            return userConfig.username + "_" + String_Functions.RandomString(20);
        }

        public static bool RemoveConfigurationTable(string tableName)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Configurations.Remove(tableName);
            }
            else
            {
                result = Local.Configurations.Remove(tableName, UserManagementSettings.Database);
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
        public static bool UploadImage(string localpath)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Images.UploadImage(localpath);
            }
            else
            {
                result = Local.Images.UploadImage(localpath);
            }

            return result;
        }

        public static System.Drawing.Image GetImage(string filename)
        {
            System.Drawing.Image result = null;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.Images.GetImage(filename);
            }
            else
            {
                result = Local.Images.GetImage(filename);
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
        public static bool UploadProfileImage(string filename, string localpath)
        {
            bool result = false;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.ProfileImages.UploadProfileImage(filename, localpath);
            }
            else
            {
                result = Local.Images.UploadImage(filename, localpath);
            }

            return result;
        }


        public static System.Drawing.Image GetProfileImage(UserConfiguration userConfig)
        {
            System.Drawing.Image result = null;

            if (UserManagementSettings.Database == null)
            {
                result = Remote.ProfileImages.GetProfileImage(userConfig);
            }
            else
            {
                result = Local.ProfileImages.GetProfileImage(userConfig);
            }

            return result;
        }

    }

}
