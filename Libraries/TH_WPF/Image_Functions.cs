using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TH_WPF
{
    public static class Image_Functions
    {

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
            bImg.EndInit();

            memoryStream.Close();

            return bImg;
            }
        else return null;

        }

        public static Bitmap SetImageSize(Image image, int width, int height)
        {
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
                bImg.EndInit();

                memoryStream.Close();

                return bImg;
            }
            else return null;

        }


        public static BitmapImage GetImageFromFile(string Filepath)
            {

            BitmapImage Result = null;

            if (Filepath != null)
                if (File.Exists(Filepath))
                    {
                    BitmapImage BIMG = new BitmapImage();
                    BIMG.BeginInit();
                    BIMG.UriSource = new Uri(Filepath);
                    BIMG.EndInit();

                    Result = BIMG;
                    }

            return Result;

            }

    }
}
