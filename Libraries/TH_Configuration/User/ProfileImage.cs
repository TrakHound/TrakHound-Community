using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Windows.Media.Imaging;

using TH_Global;
using TH_Global.Functions;

namespace TH_Configuration.User
{
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


        /// <summary>
        /// Uploads a Profile Image to the TrakHound Server
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="localpath"></param>
        /// <returns></returns>
        public static bool UploadProfileImage(string filename, string localpath)
        {
            bool result = false;

            NameValueCollection nvc = new NameValueCollection();
            if (Web.HttpUploadFile("http://www.feenux.com/php/users/uploadprofileimage.php", localpath, "file", "image/jpeg", nvc))
            {
                result = true;
            }

            return result;
        }


        public static System.Drawing.Image GetProfileImage(UserConfiguration userConfig)
        {
            System.Drawing.Image result = null;

            if (userConfig.image_url != String.Empty)
            {
                using (WebClient webClient = new WebClient())
                {
                    try
                    {
                        byte[] data = webClient.DownloadData("http://www.feenux.com/trakhound/users/files/" + userConfig.image_url);

                        using (MemoryStream mem = new MemoryStream(data))
                        {
                            result = System.Drawing.Image.FromStream(mem);
                        }
                    }
                    catch (Exception ex) { Logger.Log("GetProfileImage() : Exception : " + ex.Message); }
                }
            }

            return result;
        }

    }
}
