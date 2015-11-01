using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;

using System.IO;
using System.Net;
//using System.Windows.Media.Imaging;

using TH_Global;
using TH_Global.Functions;

namespace TH_Configuration.User
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

        //public static System.Drawing.Image ProcessImage(string path)
        //{
        //    System.Drawing.Image result = null;

        //    if (File.Exists(path))
        //    {
        //        result = System.Drawing.Image.FromFile(path);
        //    }

        //    return result;
        //}


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
                Image img = Image.FromFile(localpath);
                if (img != null)
                {
                    string contentFormat = null;

                    if (ImageFormat.Jpeg.Equals(img.RawFormat)) contentFormat = "image/jpeg";
                    else if (ImageFormat.Png.Equals(img.RawFormat)) contentFormat = "image/png";
                    else if (ImageFormat.Gif.Equals(img.RawFormat)) contentFormat = "image/gif";
                    else if (ImageFormat.Bmp.Equals(img.RawFormat)) contentFormat = "image/bmp";
                    else if (ImageFormat.Tiff.Equals(img.RawFormat)) contentFormat = "image/tiff";

                    NameValueCollection nvc = new NameValueCollection();
                    if (Web.HttpUploadFile("http://www.feenux.com/php/configurations/uploadimage.php", localpath, "file", contentFormat, nvc))
                    {
                        result = true;
                    }
                }
            }

            return result;
        }


        public static System.Drawing.Image GetImage(string filename)
        {
            System.Drawing.Image result = null;

            if (filename != String.Empty)
            {
                using (WebClient webClient = new WebClient())
                {
                    try
                    {
                        byte[] data = webClient.DownloadData("http://www.feenux.com/trakhound/users/files/" + filename);

                        using (MemoryStream mem = new MemoryStream(data))
                        {
                            result = System.Drawing.Image.FromStream(mem);
                        }
                    }
                    catch (Exception ex) { Logger.Log("GetImage() : Exception : " + ex.Message); }
                }
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
