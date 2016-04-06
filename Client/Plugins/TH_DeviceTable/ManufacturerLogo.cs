// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TH_UserManagement.Management;

namespace TH_DeviceTable
{
    public partial class DeviceTable
    {

        private class LoadManufacturerLogoInfo
        {
            public string Filename { get; set; }
            public ImageSource Image { get; set; }
            public DeviceInfo DeviceInfo { get; set; }
        }

        public void LoadManufacturerLogo(string filename, DeviceInfo info)
        {
            info.ManufacturerLogoLoading = true;

            var deviceInfo = new LoadManufacturerLogoInfo();
            deviceInfo.Filename = filename;
            deviceInfo.DeviceInfo = info;

            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadManufacturerLogo_Worker), deviceInfo);
        }

        private void LoadManufacturerLogo_Worker(object o)
        {
            if (o != null)
            {
                var info = (LoadManufacturerLogoInfo)o;

                System.Drawing.Image img = Images.GetImage(info.Filename);
                if (img != null)
                {
                    var bmp = new System.Drawing.Bitmap(img);
                    if (bmp != null)
                    {
                        BitmapSource bmpSource;
                        IntPtr bmpPt = bmp.GetHbitmap();
                        bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        if (bmpSource != null)
                        {
                            if (bmpSource.PixelWidth > bmpSource.PixelHeight)
                            {
                                bmpSource = TH_WPF.Image_Functions.SetImageSize(bmpSource, 180);
                            }
                            else
                            {
                                bmpSource = TH_WPF.Image_Functions.SetImageSize(bmpSource, 0, 80);
                            }

                            bmpSource.Freeze();

                            info.Image = bmpSource;
                        }
                    }
                }

                this.Dispatcher.BeginInvoke(new Action<LoadManufacturerLogoInfo>(LoadManufacturerLogo_GUI), PRIORITY_BACKGROUND, new object[] { info });
            }
        }

        void LoadManufacturerLogo_GUI(LoadManufacturerLogoInfo info)
        {
            info.DeviceInfo.ManufacturerLogo = info.Image;

            info.DeviceInfo.ManufacturerLogoLoading = false;
        }


    }
}
