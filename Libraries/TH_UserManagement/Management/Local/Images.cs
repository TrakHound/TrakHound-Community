using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using TH_Global;
using TH_Global.Functions;

namespace TH_UserManagement.Management.Local
{
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

            if (File.Exists(localpath))
            {
                string fileName = Path.GetFileName(localpath);

                if (!Directory.Exists(FileLocations.TrakHoundTemp)) Directory.CreateDirectory(FileLocations.TrakHoundTemp);

                string newPath = FileLocations.TrakHoundTemp + "\\" + fileName;

                File.Copy(localpath, newPath, true);

                result = true;
            }

            return result;
        }

        public static bool UploadImage(string filename, string localpath)
        {
            bool result = false;

            if (File.Exists(localpath))
            {
                if (!Directory.Exists(FileLocations.TrakHoundTemp)) Directory.CreateDirectory(FileLocations.TrakHoundTemp);

                string newPath = FileLocations.TrakHoundTemp + "\\" + filename;

                File.Copy(localpath, newPath, true);

                result = true;
            }

            return result;
        }

        public static System.Drawing.Image GetImage(string filename)
        {
            System.Drawing.Image result = null;

            if (filename != String.Empty)
            {
                try
                {
                    string path = FileLocations.TrakHoundTemp + "\\" + filename;
                    if (File.Exists(path))
                    {
                        using (var fs = File.OpenRead(path))
                        {
                            result = System.Drawing.Image.FromStream(fs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("GetImage() : Exception : " + ex.Message);
                }
            }

            return result;
        }
    }

    public static class ProfileImages
    {
        public static System.Drawing.Image GetProfileImage(UserConfiguration userConfig)
        {
            System.Drawing.Image result = null;

            if (userConfig.image_url != String.Empty)
            {
                result = Images.GetImage(userConfig.image_url);
            }

            return result;
        }
    }

}
