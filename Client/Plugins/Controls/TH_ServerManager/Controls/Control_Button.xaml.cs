// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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

namespace TH_ServerManager.Controls
{
    /// <summary>
    /// Interaction logic for Control_Button.xaml
    /// </summary>
    public partial class Control_Button : UserControl
    {
        public Control_Button()
        {
            MouseEnterBrush = new SolidColorBrush(Color.FromArgb(150, 255, 255, 255));
            MouseLeaveBrush = new SolidColorBrush(Colors.Transparent);
            MouseDownBrush = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));

            InitializeComponent();

            ClickTimer = new System.Timers.Timer();
            ClickTimer.Interval = 200;
            ClickTimer.Elapsed += ClickTimer_Elapsed;
        }

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(Control_Button), new PropertyMetadata(null));


        #region "Effects"

        SolidColorBrush MouseEnterBrush;
        SolidColorBrush MouseLeaveBrush;
        SolidColorBrush MouseDownBrush;

        public SolidColorBrush EllipseFill
        {
            get { return (SolidColorBrush)GetValue(EllipseFillProperty); }
            set { SetValue(EllipseFillProperty, value); }
        }

        public static readonly DependencyProperty EllipseFillProperty =
            DependencyProperty.Register("EllipseFill", typeof(SolidColorBrush), typeof(Control_Button), new PropertyMetadata(null));

        bool MouseOver = false;

        private void Ellipse_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseOver = true;
            EllipseFill = MouseEnterBrush;
        }

        private void Ellipse_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseOver = false;
            EllipseFill = MouseLeaveBrush;
            ClickTimer.Enabled = false;
        }

        System.Timers.Timer ClickTimer;

        void Clicked()
        {
            ClickTimer.Enabled = false;
            ClickTimer.Enabled = true;
            EllipseFill = MouseDownBrush;
        }

        void ClickTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(ClickTimer_Elapsed_GUI));
        }

        void ClickTimer_Elapsed_GUI()
        {
            ClickTimer.Enabled = false;
            EllipseFill = MouseEnterBrush;
        }

        #endregion

        #region "Click"

        public delegate void Click_Handler();
        public event Click_Handler Click;

        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Clicked();

            if (Click != null) Click();

        }

        #endregion


    }
}
