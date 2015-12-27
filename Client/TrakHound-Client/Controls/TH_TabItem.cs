// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TrakHound_Client.Controls
{

    public class TH_TabItem : TabItem
    {

        public string Title { get; set; }

        public object TH_Header { get; set; }

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(TH_TabItem), new PropertyMetadata(null));

        protected override void OnSelected(RoutedEventArgs e)
        {
            //base.OnSelected(e);
        }

        protected override void OnUnselected(RoutedEventArgs e)
        {
            //base.OnUnselected(e);
        }        

        System.Timers.Timer Close_TIMER;

        public void Close()
        {
            Close_TIMER = new System.Timers.Timer();
            Close_TIMER.Interval = 600;
            Close_TIMER.Elapsed += Close_TIMER_Elapsed;
            Close_TIMER.Enabled = true;

            if (TH_Header.GetType() == typeof(TH_TabHeader_Top))
            {
                TH_TabHeader_Top header = (TH_TabHeader_Top)TH_Header;
                header.Closing = true;
            }

            if (Content.GetType() == typeof(Controls.Page))
            {
                Controls.Page page = (Controls.Page)Content;
                page.Closing = true;
            }
 
        }

        void Close_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Close_TIMER.Enabled = false;
            this.Dispatcher.BeginInvoke(new Action(Close_TIMER_Elapsed_GUI));
        }

        void Close_TIMER_Elapsed_GUI()
        {
            TabControl tabParent = (TabControl)this.Parent;
            if (tabParent != null)
            {
                if (tabParent.Items.Contains(this)) tabParent.Items.Remove(this); 
            }

            if (Closed != null) Closed(this);
        }

        public delegate void Closed_Handler(TH_TabItem tab);
        public event Closed_Handler Closed;

    }

}
