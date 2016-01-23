using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;

using TH_Global.Functions;

namespace TH_WPF
{
    /// <summary>
    /// Interaction logic for ImageBox.xaml
    /// </summary>
    public partial class ImageBox : UserControl
    {
        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.ContextIdle;

        public ImageBox()
        {
            InitializeComponent();
            root_GRID.DataContext = this;
        }

        #region "Properties"

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ImageBox), new PropertyMetadata(null));


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
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(ImageBox), new PropertyMetadata(null, new PropertyChangedCallback(ImagePropertyChanged)));


        private static void ImagePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ImageBox imgBox = (ImageBox)dependencyObject;
            imgBox.SetBackgroundColor(eventArgs.NewValue as ImageSource);
        }


        public string HelpText
        {
            get { return (string)GetValue(HelpTextProperty); }
            set { SetValue(HelpTextProperty, value); }
        }

        public static readonly DependencyProperty HelpTextProperty =
            DependencyProperty.Register("HelpText", typeof(string), typeof(ImageBox), new PropertyMetadata(null));



        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(ImageBox), new PropertyMetadata(false));



        public bool ImageSet
        {
            get { return (bool)GetValue(ImageSetProperty); }
            set { SetValue(ImageSetProperty, value); }
        }

        public static readonly DependencyProperty ImageSetProperty =
            DependencyProperty.Register("ImageSet", typeof(bool), typeof(ImageBox), new PropertyMetadata(false));



        public bool ShowBorder
        {
            get { return (bool)GetValue(ShowBorderProperty); }
            set { SetValue(ShowBorderProperty, value); }
        }

        public static readonly DependencyProperty ShowBorderProperty =
            DependencyProperty.Register("ShowBorder", typeof(bool), typeof(ImageBox), new PropertyMetadata(false));


        public bool ShowClear
        {
            get { return (bool)GetValue(ShowClearProperty); }
            set { SetValue(ShowClearProperty, value); }
        }

        public static readonly DependencyProperty ShowClearProperty =
            DependencyProperty.Register("ShowClear", typeof(bool), typeof(ImageBox), new PropertyMetadata(true));

        #endregion

        #region "Events"

        public delegate void Clicked_Handler(ImageBox sender);
        public event Clicked_Handler UploadClicked;
        public event Clicked_Handler ClearClicked;

        #endregion

        #region "Background Color (Shade)"

        public bool DarkBackground
        {
            get { return (bool)GetValue(DarkBackgroundProperty); }
            set { SetValue(DarkBackgroundProperty, value); }
        }

        public static readonly DependencyProperty DarkBackgroundProperty =
            DependencyProperty.Register("DarkBackground", typeof(bool), typeof(ImageBox), new PropertyMetadata(false));

        
        Thread backgroundcolor_THREAD;

        //public void SetBackgroundColor(ImageSource img)
        //{
        //    double brightness = 0;

        //    if (img != null)
        //    {
        //        BitmapImage bmpImg = img as BitmapImage;
        //        if (bmpImg != null)
        //        {
        //            System.Drawing.Bitmap bmp = TH_Global.Functions.Image_Functions.BitmapImage2Bitmap(bmpImg);
        //            if (bmp != null)
        //            {
        //                System.Drawing.Color color = TH_Global.Functions.Image_Functions.CalculateAverageColor(bmp);
        //                brightness = color.GetBrightness();
        //            }
        //        }
        //    }

        //    if (brightness > 0.8) bd.Background = new SolidColorBrush(Color_Functions.GetColorFromString("#ddd"));
        //    else bd.Background = null;
        //}

        void SetBackgroundColor(ImageSource img)
        {
            if (img != null)
            {
                img.Freeze();

                if (backgroundcolor_THREAD != null) backgroundcolor_THREAD.Abort();

                backgroundcolor_THREAD = new Thread(new ParameterizedThreadStart(SetBackgroundColor_Worker));
                backgroundcolor_THREAD.Start(img);
            }
        }

        void SetBackgroundColor_Worker(object o)
        {
            if (o != null)
            {
                ImageSource img = (ImageSource)o;

                double brightness = 0;

                if (img != null)
                {
                    BitmapImage bmpImg = img as BitmapImage;
                    if (bmpImg != null)
                    {
                        System.Drawing.Bitmap bmp = TH_Global.Functions.Image_Functions.BitmapImage2Bitmap(bmpImg);
                        if (bmp != null)
                        {
                            System.Drawing.Color color = TH_Global.Functions.Image_Functions.CalculateAverageColor(bmp);
                            brightness = color.GetBrightness();
                        }
                    }
                }

                this.Dispatcher.BeginInvoke(new Action<double>(LoadProfileImage_GUI), priority, new object[] { brightness });
            }
        }

        void LoadProfileImage_GUI(double brightness)
        {
            //if (brightness > 0.8) bd.Background = new SolidColorBrush(Color_Functions.GetColorFromString("#ddd"));
            //else bd.Background = null;

            if (brightness > 0.8) DarkBackground = true;
            else DarkBackground = false;
        }

        #endregion

        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = false;
                    }
                }
            }
        }

        private void Clear_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ClearClicked != null) ClearClicked(this);
        }

        private void Upload_Clicked(Button bt)
        {
            if (UploadClicked != null) UploadClicked(this);
        }

    }
}
