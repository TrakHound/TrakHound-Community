// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TH_ServerManager.Controls
{
    public class TH_TabItem : TabItem
    {

        public string Title { get; set; }

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(TH_TabItem), new PropertyMetadata(null));

        protected override void OnSelected(RoutedEventArgs e)
        {
            if (Header.GetType() == typeof(TH_TabHeader))
            {
                TH_TabHeader header = (TH_TabHeader)Header;
                header.IsSelected = true;
            }
            //base.OnSelected(e);
        }

        protected override void OnUnselected(RoutedEventArgs e)
        {
            if (Header.GetType() == typeof(TH_TabHeader))
            {
                TH_TabHeader header = (TH_TabHeader)Header;
                header.IsSelected = false;
            }
            base.OnUnselected(e);
        }        

        System.Timers.Timer Close_TIMER;

        public void Close()
        {
            Close_TIMER = new System.Timers.Timer();
            Close_TIMER.Interval = 500;
            Close_TIMER.Elapsed += Close_TIMER_Elapsed;
            Close_TIMER.Enabled = true;

            if (Header.GetType() == typeof(TH_TabHeader))
            {
                TH_TabHeader header = (TH_TabHeader)Header;
                header.Closing = true;
            }
 
        }

        void Close_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Close_TIMER.Enabled = false;
            this.Dispatcher.Invoke(new Action(Close_TIMER_Elapsed_GUI));
        }

        void Close_TIMER_Elapsed_GUI()
        {
            TabControl tabParent = (TabControl)this.Parent;

            tabParent.Items.Remove(this);
        }



    }
}
