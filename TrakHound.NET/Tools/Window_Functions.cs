using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace TrakHound.Tools
{

    public static class Window_Functions
    {

        public static void CenterWindow(System.Windows.Window window)
        {
            double screen_width = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screen_height = System.Windows.SystemParameters.PrimaryScreenHeight;

            double window_width = window.ActualWidth;
            double window_height = window.ActualHeight;

            double center_x = (screen_width / 2) - (window_width / 2);
            double center_y = (screen_height / 2) - (window_height / 2);

            window.Left = center_x;
            window.Top = center_y;
        }

        public static void GetScreenShot(Visual target, string fileName)
        {
            if (target == null || string.IsNullOrEmpty(fileName))
            {
                return;
            }

            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);

            RenderTargetBitmap renderTarget = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext context = visual.RenderOpen())
            {
                VisualBrush visualBrush = new VisualBrush(target);
                context.DrawRectangle(visualBrush, null, new Rect(new Point(), bounds.Size));
            }

            renderTarget.Render(visual);
            PngBitmapEncoder bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTarget));
            using (Stream stm = File.Create(fileName))
            {
                bitmapEncoder.Save(stm);
            }
        }

    }
}
