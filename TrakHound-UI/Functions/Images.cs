// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TrakHound_UI.Functions
{
    public class Images
    {

        public static System.Drawing.Color CalculateAverageColor(Bitmap bm)
        {
            int width = bm.Width;
            int height = bm.Height;
            int red = 0;
            int green = 0;
            int blue = 0;
            int minDiversion = 15; // drop pixels that do not differ by at least minDiversion between color values (white, gray or black)
            int dropped = 0; // keep track of dropped pixels
            long[] totals = new long[] { 0, 0, 0 };
            int bppModifier = bm.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb ? 3 : 4; // cutting corners, will fail on anything else but 32 and 24 bit images

            BitmapData srcData = bm.LockBits(new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height), ImageLockMode.ReadOnly, bm.PixelFormat);
            int stride = srcData.Stride;
            IntPtr Scan0 = srcData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (y * stride) + x * bppModifier;
                        red = p[idx + 2];
                        green = p[idx + 1];
                        blue = p[idx];
                        if (Math.Abs(red - green) > minDiversion || Math.Abs(red - blue) > minDiversion || Math.Abs(green - blue) > minDiversion)
                        {
                            totals[2] += red;
                            totals[1] += green;
                            totals[0] += blue;
                        }
                        else
                        {
                            dropped++;
                        }
                    }
                }
            }

            int count = width * height - dropped;
            if (count > 0)
            {
                int avgR = (int)(totals[2] / count);
                int avgG = (int)(totals[1] / count);
                int avgB = (int)(totals[0] / count);

                return System.Drawing.Color.FromArgb(avgR, avgG, avgB);
            }

            return System.Drawing.Color.Transparent;

        }

        public static Bitmap BitmapImage2Bitmap(BitmapImage img)
        {
            if (img != null)
            {
                using (var stream = new MemoryStream())
                {
                    var enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(img));
                    enc.Save(stream);
                    var bitmap = new Bitmap(stream);

                    return new Bitmap(bitmap);
                }
            }
            return null;
        }

        public static string OpenImageBrowse(string title)
        {
            string result = null;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = Environment.GetEnvironmentVariable("SYSTEMDRIVE");
            dlg.Multiselect = false;
            dlg.Title = title;
            dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            var dialogResult = dlg.ShowDialog();

            if (dialogResult == true)
            {
                if (dlg.FileName != null) result = dlg.FileName;
            }

            return result;
        }

        public static BitmapImage SourceFromImage(Image img)
        {
            if (img != null)
            {
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        var format = ImageFormat.Png;
                        img.Save(stream, format);

                        return ImageFromBuffer(stream.ToArray());
                    }
                }
                catch (Exception ex) { }
            }

            return null;
        }

        public static BitmapImage ImageFromBuffer(byte[] bytes)
        {
            if (bytes != null)
            {
                try
                {
                    var stream = new MemoryStream(bytes);
                    var img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = stream;
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();
                    return img;
                }
                catch (Exception ex) { }
            }

            return null;
        }

        public static byte[] BufferFromImage(BitmapImage img)
        {
            if (img != null)
            {
                var stream = img.StreamSource;
                byte[] buffer = null;
                if (stream != null && stream.Length > 0)
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        buffer = reader.ReadBytes((int)stream.Length);
                    }
                }

                return buffer;
            }

            return null;
        }

        public static BitmapImage SetImageSize(ImageSource src, int width)
        {
            if (src != null)
            {
                var encoder = new PngBitmapEncoder();
                var stream = new MemoryStream();
                var img = new BitmapImage();
                var bmp = src as BitmapSource;
                if (bmp != null)
                {
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    encoder.Save(stream);

                    img.BeginInit();
                    img.DecodePixelWidth = width;
                    img.StreamSource = new MemoryStream(stream.ToArray());
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();

                    stream.Close();

                    return img;
                } 
            }

            return null;
        }

        public static BitmapImage SetImageSize(ImageSource src, int width, int height)
        {
            if (src != null)
            {
                var encoder = new PngBitmapEncoder();
                var stream = new MemoryStream();
                var img = new BitmapImage();
                var bmp = src as BitmapSource;
                if (bmp != null)
                {
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    encoder.Save(stream);

                    img.BeginInit();
                    if (width > 0) img.DecodePixelWidth = width;
                    if (height > 0) img.DecodePixelHeight = height;
                    img.StreamSource = new MemoryStream(stream.ToArray());
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();

                    stream.Close();

                    return img;
                }
            }

            return null;
        }

    }
}
