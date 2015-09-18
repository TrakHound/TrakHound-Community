using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Windows;
using System.Windows.Markup;

namespace TH_Functions
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

        public static BitmapImage SetImageSize(ImageSource Source, int Width, int Height)
        {

            if (Source != null)
            {

                BitmapImage Result;
                
                PngBitmapEncoder Height_encoder = new PngBitmapEncoder();
                MemoryStream Height_memoryStream = new MemoryStream();
                BitmapImage Height_bImg = new BitmapImage();

                Height_encoder.Frames.Add(BitmapFrame.Create(Source as BitmapSource));
                Height_encoder.Save(Height_memoryStream);

                Height_bImg.BeginInit();
                if (Height > 0) Height_bImg.DecodePixelHeight = Height;
                Height_bImg.StreamSource = new MemoryStream(Height_memoryStream.ToArray());
                Height_bImg.EndInit();

                Result = Height_bImg;

                Height_memoryStream.Close();

                if (Width > 0)
                {

                    PngBitmapEncoder Width_encoder = new PngBitmapEncoder();
                    MemoryStream Width_memoryStream = new MemoryStream();
                    BitmapImage Width_bImg = new BitmapImage();

                    Width_encoder.Frames.Add(BitmapFrame.Create(Height_bImg as BitmapSource));
                    Width_encoder.Save(Width_memoryStream);

                    Width_bImg.BeginInit();
                    if (Width > 0) Width_bImg.DecodePixelHeight = Width;
                    Width_bImg.StreamSource = new MemoryStream(Width_memoryStream.ToArray());
                    Width_bImg.EndInit();

                    Result = Width_bImg;

                    Width_memoryStream.Close();

                }

                return Result;
            }
            else return null;

            //if (Source != null)
            //{
            //    PngBitmapEncoder encoder = new PngBitmapEncoder();
            //    MemoryStream memoryStream = new MemoryStream();
            //    BitmapImage bImg = new BitmapImage();

            //    encoder.Frames.Add(BitmapFrame.Create(Source as BitmapSource));
            //    encoder.Save(memoryStream);

            //    bImg.BeginInit();
            //    if (Width > 0) bImg.DecodePixelWidth = Width;
            //    if (Height > 0) bImg.DecodePixelHeight = Height;
            //    bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
            //    bImg.EndInit();

            //    memoryStream.Close();

            //    return bImg;
            //}
            //else return null;

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
