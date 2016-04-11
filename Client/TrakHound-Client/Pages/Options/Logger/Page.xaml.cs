// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;

using TH_Global;
using TH_Global.Functions;

namespace TrakHound_Client.Pages.Options.Logger
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, IPage
    {

        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Title { get { return "Logs"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Pages/About/Information/Information_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }



        #region "Heartbeat"

        public int QueueWriteInterval
        {
            get { return (int)GetValue(QueueWriteIntervalProperty); }
            set { SetValue(QueueWriteIntervalProperty, value); }
        }

        public static readonly DependencyProperty QueueWriteIntervalProperty =
            DependencyProperty.Register("QueueWriteInterval", typeof(int), typeof(Page), new PropertyMetadata(5000));


        public TimeSpan QueueWriteInterval_TimeSpan
        {
            get { return (TimeSpan)GetValue(QueueWriteInterval_TimeSpanProperty); }
            set { SetValue(QueueWriteInterval_TimeSpanProperty, value); }
        }

        public static readonly DependencyProperty QueueWriteInterval_TimeSpanProperty =
            DependencyProperty.Register("QueueWriteInterval_TimeSpan", typeof(TimeSpan), typeof(Page), new PropertyMetadata(TimeSpan.FromMilliseconds(5000)));


        private void queueWriteInterval_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (queueWriteInterval_TXT.Text != String.Empty)
            {
                TimeSpan ts = GetTimeSpanFromString(queueWriteInterval_TXT.Text);
                QueueWriteInterval_TimeSpan = ts;
                if (ts.TotalMilliseconds < int.MaxValue)
                {
                    QueueWriteInterval = Convert.ToInt32(ts.TotalMilliseconds);
                }
            }
        }

        private void QueueWriteInterval_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            QueueWriteInterval_TimeSpan = TimeSpan.FromMilliseconds(QueueWriteInterval);
        }

        private void queueWriteInterval_TXT_LostFocus(object sender, RoutedEventArgs e)
        {
            queueWriteInterval_TXT.Clear();
        }

        private static TimeSpan GetTimeSpanFromString(string s)
        {
            TimeSpan result = TimeSpan.Zero;
            if (TimeSpan.TryParse(s, out result)) return result;
            else
            {
                s = s.Trim();
                //Milliseconds
                if (s.Length > 2)
                {
                    string unit = s.Substring(s.Length - 2, 2);
                    if (unit == "ms")
                    {
                        double ms;
                        if (double.TryParse(s.Substring(0, s.Length - 2), out ms))
                        {
                            result = TimeSpan.FromMilliseconds(ms);
                        }
                    }
                }
                //Seconds
                if (result == TimeSpan.Zero && s.Length > 1)
                {
                    string unit = s.Substring(s.Length - 1, 1);
                    if (unit == "s")
                    {
                        double seconds;
                        if (double.TryParse(s.Substring(0, s.Length - 1), out seconds))
                        {
                            result = TimeSpan.FromSeconds(seconds);
                        }
                    }
                }
            }
            return result;
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


    }
}
