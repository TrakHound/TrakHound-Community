using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.IO;

namespace TH_Global.Functions
{
    public static class Image_Functions
    {
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

        /// <summary>
        /// Returns a resized image based on the Width and Height given
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap SetImageSize(Image image, int width, int height)
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

            return destImage;
        }


        public static BitmapImage SourceFromImage(System.Drawing.Image img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, img.RawFormat);

                return ImageFromBuffer(stream.ToArray());
            }
        }

        public static BitmapImage ImageFromBuffer(Byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
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


    }
}
