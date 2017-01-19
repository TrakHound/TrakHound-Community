// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE', which is part of this source code package.

using NLog;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrakHound;
using TrakHound.API;
using TrakHound.API.Users;
using TrakHound.Configurations;
using TrakHound_UI.Functions;

namespace TrakHound_Dashboard.Pages.Dashboard.Footprint.Controls
{
    /// <summary>
    /// Interaction logic for DeviceItem.xaml
    /// </summary>
    public partial class DeviceItem : UserControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DeviceItem(Footprint parent, DeviceDescription device)
        {
            InitializeComponent();
            root.DataContext = this;
            Parent = parent;
            Device = device;

            StatusBrush = Brush_Functions.GetSolidBrushFromResource(this, "Disabled");
        }

        #region "Dependancy Properties"

        public DeviceDescription Device
        {
            get { return (DeviceDescription)GetValue(DeviceProperty); }
            set
            {
                SetValue(DeviceProperty, value);

                if (value != null)
                {
                    var device = value;

                    if (device.Description != null)
                    {
                        // Load Device Image
                        if (!string.IsNullOrEmpty(device.Description.ImageUrl)) LoadDeviceImage(device.Description.ImageUrl);
                    }
                }
            }
        }

        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceDescription), typeof(DeviceItem), new PropertyMetadata(null));



        public Brush StatusBrush
        {
            get { return (Brush)GetValue(StatusBrushProperty); }
            set { SetValue(StatusBrushProperty, value); }
        }

        public static readonly DependencyProperty StatusBrushProperty =
            DependencyProperty.Register("StatusBrush", typeof(Brush), typeof(DeviceItem), new PropertyMetadata(null));



        public Footprint Parent
        {
            get { return (Footprint)GetValue(ParentProperty); }
            set { SetValue(ParentProperty, value); }
        }

        public static readonly DependencyProperty ParentProperty =
            DependencyProperty.Register("Parent", typeof(Footprint), typeof(DeviceItem), new PropertyMetadata(null));



        public bool Connected
        {
            get { return (bool)GetValue(ConnectedProperty); }
            set { SetValue(ConnectedProperty, value); }
        }

        public static readonly DependencyProperty ConnectedProperty =
            DependencyProperty.Register("Connected", typeof(bool), typeof(DeviceItem), new PropertyMetadata(false));


        public string DeviceStatus
        {
            get { return (string)GetValue(DeviceStatusProperty); }
            set { SetValue(DeviceStatusProperty, value); }
        }

        public static readonly DependencyProperty DeviceStatusProperty =
            DependencyProperty.Register("DeviceStatus", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));


        public string Availability
        {
            get { return (string)GetValue(AvailabilityProperty); }
            set { SetValue(AvailabilityProperty, value); }
        }

        public static readonly DependencyProperty AvailabilityProperty =
            DependencyProperty.Register("Availability", typeof(string), typeof(DeviceItem), new PropertyMetadata("UNAVAILABLE"));


        public string ExecutionMode
        {
            get { return (string)GetValue(ExecutionModeProperty); }
            set { SetValue(ExecutionModeProperty, value); }
        }

        public static readonly DependencyProperty ExecutionModeProperty =
            DependencyProperty.Register("ExecutionMode", typeof(string), typeof(DeviceItem), new PropertyMetadata("UNAVAILABLE"));


        public string ControllerMode
        {
            get { return (string)GetValue(ControllerModeProperty); }
            set { SetValue(ControllerModeProperty, value); }
        }

        public static readonly DependencyProperty ControllerModeProperty =
            DependencyProperty.Register("ControllerMode", typeof(string), typeof(DeviceItem), new PropertyMetadata("UNAVAILABLE"));

        #endregion

        public UserConfiguration UserConfiguration { get; set; }

        public delegate void Handler(DeviceItem item);
        public event Handler Moved;
        public event Handler Resized;
        public event Handler ViewDetails;
        public event Handler EditDevice;
        public event Handler CloseClicked;

        Point controlPoint;
        bool isInDrag = false;

        public void UpdateData(Data.StatusInfo info)
        {
            if (info != null)
            {
                if (!string.IsNullOrEmpty(info.DeviceStatus))
                {
                    switch (info.DeviceStatus.ToLower())
                    {
                        case "active": StatusBrush = Brush_Functions.GetSolidBrushFromResource(this, "StatusGreen"); break;
                        case "idle": StatusBrush = Brush_Functions.GetSolidBrushFromResource(this, "StatusOrange"); break;
                        case "alert": StatusBrush = Brush_Functions.GetSolidBrushFromResource(this, "StatusRed"); break;
                        default: StatusBrush = Brush_Functions.GetSolidBrushFromResource(this, "Disabled"); break;
                    }
                }
                else StatusBrush = Brush_Functions.GetSolidBrushFromResource(this, "Disabled");

                Connected = info.Connected == 1;
                if (!string.IsNullOrEmpty(info.DeviceStatus)) DeviceStatus = info.DeviceStatus;
            }
        }

        public void UpdateData(Data.ControllerInfo info)
        {
            if (info != null)
            {
                Availability = info.Availability;
                ControllerMode = info.ControllerMode;
                ExecutionMode = info.ExecutionMode;
            }
        }

        public void StartDrag()
        {
            isInDrag = true;
        }

        private void root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Parent.EditEnabled)
            {
                // Get Point relative to this Control (relative position of the cursor on the control)
                controlPoint = e.GetPosition(this);
                var element = sender as FrameworkElement;
                element.CaptureMouse();
                StartDrag();
                e.Handled = true;
            }
            else
            {
                ViewDetails?.Invoke(this);
            }
        }

        private void root_MouseMove(object sender, MouseEventArgs e)
        {
            if (isInDrag)
            {
                double left = Canvas.GetLeft(this);
                double top = Canvas.GetTop(this);

                left = !double.IsNaN(left) ? left : 0;
                top = !double.IsNaN(top) ? top : 0;

                // Get Point relative to the Canvas (absolute position of the cursor on the canvas)
                Point canvasPoint = e.GetPosition(Parent.shopCanvas);

                // Calculate change in position
                double deltaX = (canvasPoint.X - controlPoint.X) - left;
                double deltaY = (canvasPoint.Y - controlPoint.Y) - top;

                // Set new Canvas positions for this control
                Canvas.SetLeft(this, left + deltaX);
                Canvas.SetTop(this, top + deltaY);
            }
        }

        private void root_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isInDrag)
            {
                var element = sender as FrameworkElement;
                element.ReleaseMouseCapture();
                isInDrag = false;
                e.Handled = true;

                Moved?.Invoke(this);
            }
        }

        #region "Device Image"

        public ImageSource DeviceImage
        {
            get { return (ImageSource)GetValue(DeviceImageProperty); }
            set { SetValue(DeviceImageProperty, value); }
        }

        public static readonly DependencyProperty DeviceImageProperty =
            DependencyProperty.Register("DeviceImage", typeof(ImageSource), typeof(DeviceItem), new PropertyMetadata(null));

        public void LoadDeviceImage(string fileId)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadDeviceImage_Worker), fileId);
        }

        void LoadDeviceImage_Worker(object o)
        {
            BitmapSource result = null;

            if (o != null)
            {
                string fileId = o.ToString();

                System.Drawing.Image img = null;

                string path = Path.Combine(FileLocations.Storage, fileId);
                if (File.Exists(path)) img = System.Drawing.Image.FromFile(path);
                else img = Files.DownloadImage(UserConfiguration, fileId);

                if (img != null)
                {
                    try
                    {
                        var bmp = new System.Drawing.Bitmap(img);
                        if (bmp != null)
                        {
                            IntPtr bmpPt = bmp.GetHbitmap();
                            result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                            if (result != null)
                            {
                                if (result.PixelWidth > result.PixelHeight)
                                {
                                    result = TrakHound_UI.Functions.Images.SetImageSize(result, 300);
                                }
                                else
                                {
                                    result = TrakHound_UI.Functions.Images.SetImageSize(result, 0, 300);
                                }

                                result.Freeze();
                            }
                        }
                    }
                    catch (Exception ex) { logger.Error(ex); }
                }
            }

            Dispatcher.BeginInvoke(new Action<BitmapSource>(LoadDeviceImage_GUI), System.Windows.Threading.DispatcherPriority.DataBind, new object[] { result });
        }

        void LoadDeviceImage_GUI(BitmapSource img)
        {
            DeviceImage = img;
        }

        #endregion

        private void CloseButton_Clicked(TrakHound_UI.Button bt)
        {
            CloseClicked?.Invoke(this);
        }

        private void ResizeThumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            Resized?.Invoke(this);
        }

        private void MenuItem_ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            ViewDetails?.Invoke(this);
        }

        private void MenuItem_Edit_Click(object sender, RoutedEventArgs e)
        {
            EditDevice?.Invoke(this);
        }

        private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
        {
            CloseClicked?.Invoke(this);
        }

        public void IncreaseSize()
        {
            Width += 20;
            Height += 20;
        }

        public void DecreaseSize()
        {
            Width -= 20;
            Height -= 20;
        }

        private void MenuItem_IncreaseSize_Clicked(TrakHound_UI.Button bt)
        {
            IncreaseSize();
        }

        private void MenuItem_DecreaseSize_Clicked(TrakHound_UI.Button bt)
        {
            DecreaseSize();
        }
    }
}
