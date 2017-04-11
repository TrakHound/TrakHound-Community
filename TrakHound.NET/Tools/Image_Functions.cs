// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using NLog;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace TrakHound.Tools
{
    public static class Image_Functions
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Returns a cropped image based on the 'cropArea' set
        /// </summary>
        /// <param name="img"></param>
        /// <param name="cropArea"></param>
        /// <returns></returns>
        public static Image CropImage(Image img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        /// <summary>
        /// Returns a Image that is automatically cropped to the center of the image
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static Image CropImageToCenter(Image img)
        {
            int width = img.Width;
            int height = img.Height;

            if (width > height)
            {
                int sqWidth = height;
                int widthOffset = (width - sqWidth) / 2;

                Rectangle widthCropRect = new Rectangle(new Point(widthOffset, 0), new Size(height, height));

                return CropImage(img, widthCropRect);
            }
            else if (height > width)
            {
                int sqHeight = width;
                int heightOffset = (height - sqHeight) / 2;

                Rectangle heightCropRect = new Rectangle(new Point(0, heightOffset), new Size(width, width));

                return CropImage(img, heightCropRect);
            }
            else return img;
        }

        public static Image ImageFromFile(Uri uri)
        {
            try
            {
                var wc = new System.Net.WebClient();
                return Image.FromStream(wc.OpenRead(uri));
            }
            catch (Exception ex)
            {
                logger.Error("Error Loading Image from URI");
                logger.Error(ex);
            }

            return null;
        }

        /// <summary>
        /// Returns a resized image based on the Width and Height given
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Image SetImageSize(Image image, int width, int height)
        {
            Image result = null;

            try
            {
                // Make sure the image stays in proportion
                if (image.Width != image.Height)
                {
                    if (image.Width > image.Height)
                    {
                        height = (width * image.Height) / image.Width;
                    }
                    else if (image.Height > image.Width)
                    {
                        width = (height * image.Width) / image.Height;
                    }
                }

                var destRect = new Rectangle(0, 0, width, height);
                var destImage = new Bitmap(width, height);

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                result = image;
            }
            catch (Exception ex)
            {
                logger.Error("SetImageSize Failed");
                logger.Error(ex);
            }

            return result;
        }

        public static Image GetImageFromFile(string path)
        {
            Image result = null;

            if (path != null && File.Exists(path))
            {
                result = Image.FromFile(path);
            }

            return result;
        }

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
        
    }
}
