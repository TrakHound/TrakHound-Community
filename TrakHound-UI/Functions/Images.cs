// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            if (bitmapImage != null)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    BitmapEncoder enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                    enc.Save(outStream);
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

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

            Nullable<bool> dialogResult = dlg.ShowDialog();

            if (dialogResult == true)
            {
                if (dlg.FileName != null) result = dlg.FileName;
            }

            return result;
        }

        public static BitmapImage SourceFromImage(System.Drawing.Image img)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    ImageFormat format = ImageFormat.Png;

                    img.Save(stream, format);

                    return ImageFromBuffer(stream.ToArray());
                }
            }
            catch (Exception ex) { }

            return null;

        }

        public static BitmapImage ImageFromBuffer(Byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }

        public static Byte[] BufferFromImage(BitmapImage imageSource)
        {
            Stream stream = imageSource.StreamSource;
            Byte[] buffer = null;
            if (stream != null && stream.Length > 0)
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    buffer = br.ReadBytes((Int32)stream.Length);
                }
            }

            return buffer;
        }

        public static BitmapImage SetImageSize(ImageSource Source, int Width)
        {

            if (Source != null)
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                MemoryStream memoryStream = new MemoryStream();
                BitmapImage bImg = new BitmapImage();

                encoder.Frames.Add(BitmapFrame.Create(Source as BitmapSource));
                encoder.Save(memoryStream);

                bImg.BeginInit();
                bImg.DecodePixelWidth = Width;
                bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
                bImg.CacheOption = BitmapCacheOption.OnLoad;
                bImg.EndInit();

                memoryStream.Close();

                return bImg;
            }
            else return null;

        }

        public static BitmapImage SetImageSize(ImageSource Source, int Width, int Height)
        {

            if (Source != null)
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                MemoryStream memoryStream = new MemoryStream();
                BitmapImage bImg = new BitmapImage();

                encoder.Frames.Add(BitmapFrame.Create(Source as BitmapSource));
                encoder.Save(memoryStream);

                bImg.BeginInit();
                if (Width > 0) bImg.DecodePixelWidth = Width;
                if (Height > 0) bImg.DecodePixelHeight = Height;
                bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
                bImg.CacheOption = BitmapCacheOption.OnLoad;
                bImg.EndInit();

                memoryStream.Close();

                return bImg;
            }
            else return null;

        }

    }
}
