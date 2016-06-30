// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UI_Tools
{
    /// <summary>
    /// Interaction logic for ImageDisplay.xaml
    /// </summary>
    public partial class ImageDisplay : UserControl
    {
        public ImageDisplay()
        {
            InitializeComponent();
            root.DataContext = this;
        }

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.ContextIdle;


        #region "Properties"

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set
            {
                SetValue(ImageProperty, value);
                SetBackgroundColor(value);
            }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageDisplay), new PropertyMetadata(null, new PropertyChangedCallback(ImagePropertyChanged)));


        private static void ImagePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ImageDisplay imgBox = (ImageDisplay)dependencyObject;
            imgBox.SetBackgroundColor(eventArgs.NewValue as ImageSource);
        }


        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(ImageDisplay), new PropertyMetadata(false));


        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(ImageDisplay), new PropertyMetadata(new Thickness(0)));

        #endregion

        #region "Background Color (Shade)"

        public bool DarkBackground
        {
            get { return (bool)GetValue(DarkBackgroundProperty); }
            set { SetValue(DarkBackgroundProperty, value); }
        }

        public static readonly DependencyProperty DarkBackgroundProperty =
            DependencyProperty.Register("DarkBackground", typeof(bool), typeof(ImageDisplay), new PropertyMetadata(false));


        void SetBackgroundColor(ImageSource img)
        {
            if (img != null)
            {
                img.Freeze();

                ThreadPool.QueueUserWorkItem(new WaitCallback(SetBackgroundColor_Worker), img);
            }
        }

        void SetBackgroundColor_Worker(object o)
        {
            if (o != null)
            {
                var img = (ImageSource)o;

                double brightness = 0;

                if (img != null)
                {
                    var bmpImg = img as BitmapImage;
                    if (bmpImg != null)
                    {
                        System.Drawing.Bitmap bmp = Functions.Images.BitmapImage2Bitmap(bmpImg);
                        if (bmp != null)
                        {
                            System.Drawing.Color color = Functions.Images.CalculateAverageColor(bmp);
                            brightness = color.GetBrightness();
                        }
                    }
                }

                this.Dispatcher.BeginInvoke(new Action<double>(LoadProfileImage_GUI), priority, new object[] { brightness });
            }
        }

        void LoadProfileImage_GUI(double brightness)
        {
            if (brightness > 0.8) DarkBackground = true;
            else DarkBackground = false;
        }

        #endregion

    }
}
