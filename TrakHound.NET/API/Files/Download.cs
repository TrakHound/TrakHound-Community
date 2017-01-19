// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

using TrakHound.API.Users;
using TrakHound.Logging;
using TrakHound.Tools.Web;

namespace TrakHound.API
{
    public static partial class Files
    {
        public static bool Download(UserConfiguration userConfig, string fileId, string destinationPath)
        {
            bool result = false;

            if (userConfig != null && !string.IsNullOrEmpty(destinationPath))
            {
                if (!File.Exists(destinationPath))
                {
                    Uri apiHost = ApiConfiguration.AuthenticationApiHost;

                    string url = new Uri(apiHost, "files/download/index.php").ToString();

                    string format = "{0}?token={1}&sender_id={2}&file_id={3}";

                    string token = userConfig.SessionToken;
                    string senderId = UserManagement.SenderId.Get();

                    url = string.Format(format, url, token, senderId, fileId);

                    var response = HTTP.GET(url, true);
                    if (response != null && response.Body != null && response.Headers != null)
                    {
                        string filename = Path.GetFileName(destinationPath);
                        string dir = destinationPath;

                        // Make sure destination directory exists
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                        if (string.IsNullOrEmpty(filename))
                        {
                            // Get filename from HTTP Header
                            var header = response.Headers.ToList().Find(x => x.Key == "Content-Disposition");
                            if (header != null && header.Value != null)
                            {
                                try
                                {
                                    if (header.Value.Contains("filename"))
                                    {
                                        string s = header.Value;
                                        int i = s.IndexOf("filename");
                                        if (i >= 0) i = s.IndexOf("=", i);
                                        if (i >= 0) i = s.IndexOf("\"", i);
                                        if (i >= 0)
                                        {
                                            i++;
                                            int x = s.IndexOf("\"", i);
                                            if (x >= 0)
                                            {
                                                filename = s.Substring(i, x - i);
                                            }
                                        }
                                    }
                                } catch (Exception ex) { Logger.Log("Error Reading Filename from Download Response Header"); }
                            }
                        }

                        if (filename != null && dir != null)
                        {
                            string dest = Path.Combine(dir, filename);

                            try
                            {
                                using (var fileStream = new FileStream(dest, FileMode.CreateNew, FileAccess.ReadWrite))
                                using (var memStream = new MemoryStream())
                                {
                                    memStream.Write(response.Body, 0, response.Body.Length);
                                    memStream.WriteTo(fileStream);
                                }

                                Logger.Log("Download File Successful", LogLineType.Notification);
                                result = true;
                            }
                            catch (Exception ex) { Logger.Log("Download File Error : Exception : " + ex.Message); }
                        }
                    }
                }
                else Logger.Log("Download File Failed : File Already Exists @ " + destinationPath, LogLineType.Error);
            }

            return result;
        }


        private class CachedImage
        {
            public CachedImage(string id, Image image)
            {
                Id = id;

                FileLocations.CreateStorageDirectory();

                try
                {
                    string filename = System.IO.Path.ChangeExtension(Id, ".image");

                    Path = System.IO.Path.Combine(FileLocations.Storage, filename);

                    image.Save(Path);
                }
                catch (Exception ex) { Logger.Log("Error Adding Image to Cache :: " + ex.Message, LogLineType.Error); }

            }

            public string Id { get; set; }
            public string Path { get; set; }
            public Image Image
            {
                get
                {
                    if (!string.IsNullOrEmpty(Path))
                    {
                        try
                        {
                            if (File.Exists(Path)) return System.Drawing.Image.FromFile(Path);
                        }
                        catch (Exception ex) { Logger.Log("Error Loading Image from Cache :: " + ex.Message, LogLineType.Error); }
                    }

                    return null;
                }
            }
        }

        private static List<CachedImage> cachedImages;

        private static void AddImageToCache(CachedImage cachedImage)
        {
            if (!cachedImages.Exists(o => o.Id == cachedImage.Id))
            {
                cachedImages.Add(cachedImage);
            }
        }

        private static void LoadCachedImages()
        {
            cachedImages = new List<CachedImage>();

            if (Directory.Exists(FileLocations.Storage))
            {
                var files = Directory.GetFiles(FileLocations.Storage, "*.image");
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            string id = Path.GetFileNameWithoutExtension(file);

                            var img = Image.FromFile(file);
                            if (img != null)
                            {
                                cachedImages.Add(new CachedImage(id, img));
                            }
                        }
                        catch (Exception ex) { Logger.Log("Image Cache Load Error :: " + ex.Message); }
                    }
                }
            }
        }

        public static Image DownloadImage(UserConfiguration userConfig, string fileId)
        {
            return DownloadImage(userConfig, fileId, true);
        }

        public static Image DownloadImage(UserConfiguration userConfig, string fileId, bool useCache)
        {
            Uri uri;
            if (Uri.TryCreate(fileId, UriKind.Absolute, out uri))
            {
                return DownloadImagev2(uri, useCache);
            }
            else
            {
                return DownloadImagev1(userConfig, fileId, useCache);
            }
        }

        public static Image DownloadImagev1(UserConfiguration userConfig, string fileId, bool useCache)
        {
            Image result = null;

            if (useCache)
            {
                if (cachedImages == null) LoadCachedImages();

                var cachedImage = cachedImages.Find(o => o.Id == fileId);
                if (cachedImage != null) result = cachedImage.Image;
            }

            if (result == null && userConfig != null)
            {
                Uri apiHost = ApiConfiguration.AuthenticationApiHost;

                string url = new Uri(apiHost, "files/download/index.php").ToString();

                string format = "{0}?token={1}&sender_id={2}&file_id={3}";

                string token = userConfig.SessionToken;
                string senderId = UserManagement.SenderId.Get();

                url = string.Format(format, url, token, senderId, fileId);

                var response = HTTP.GET(url, true);
                if (response != null && response.Body != null && response.Headers != null)
                {
                    bool success = false;

                    string dummy = System.Text.Encoding.ASCII.GetString(response.Body);

                    // Takes forever to process an image
                    if (response.Body.Length < 500)
                    {
                        success = ApiError.ProcessResponse(response, "Download File");
                    }
                    else success = true;

                    if (success)
                    {
                        try
                        {
                            using (var memStream = new MemoryStream())
                            {
                                memStream.Write(response.Body, 0, response.Body.Length);
                                result = Image.FromStream(memStream);
                                Logger.Log("Download File Successful", LogLineType.Notification);
                            }
                        }
                        catch (Exception ex) { Logger.Log("Response Not an Image : Exception : " + ex.Message); }
                    }

                    // Add Image to Cache
                    if (useCache && result != null)
                    {
                        AddImageToCache(new CachedImage(fileId, result));
                    }
                }
            }

            return result;
        }

        public static Image DownloadImagev2(Uri uri, bool useCache)
        {
            string manufacturer = HttpUtility.ParseQueryString(uri.Query).Get("manufacturer");
            string model = HttpUtility.ParseQueryString(uri.Query).Get("model");

            Image result = null;

            if (useCache)
            {
                if (cachedImages == null) LoadCachedImages();

                var cachedImage = cachedImages.Find(o => o.Id == CreateCachePath(manufacturer, model));
                if (cachedImage != null) result = cachedImage.Image;
            }

            if (result == null)
            {
                var image = v2.Images.DeviceImage.Download(manufacturer, model);
                if (image != null)
                {
                    using (var ms = new MemoryStream(image.Bytes))
                    {
                        var img = Image.FromStream(ms);

                        // Add Image to Cache
                        if (img != null)
                        {
                            AddImageToCache(new CachedImage(CreateCachePath(manufacturer, model), img));
                        }

                        result = img;
                    }
                }
            }

            return result;
        }

        private static string CreateCachePath(string manufacturer, string model)
        {
            string s = manufacturer;
            if (!string.IsNullOrEmpty(model)) s += "_" + model;
            return s;
        }

    }
}
