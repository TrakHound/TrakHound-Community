// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TrakHound_Dashboard.Windows
{
    /// <summary>
    /// Interaction logic for Fullscreen.xaml
    /// </summary>
    public partial class Fullscreen : Window
    {
        public Fullscreen()
        {
            InitializeComponent();
            DataContext = this;

            ZoomLevels.Add("25%");
            ZoomLevels.Add("30%");
            ZoomLevels.Add("40%");
            ZoomLevels.Add("50%");
            ZoomLevels.Add("75%");
            ZoomLevels.Add("100%");
            ZoomLevels.Add("125%");
            ZoomLevels.Add("150%");
            ZoomLevels.Add("200%");
            ZoomLevels.Add("250%");
            ZoomLevels.Add("300%");

            fPreviousExecutionState = NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED);
        }

        private uint fPreviousExecutionState;

        internal static class NativeMethods
        {
            // Import SetThreadExecutionState Win32 API and necessary flags
            [DllImport("kernel32.dll")]
            public static extern uint SetThreadExecutionState(uint esFlags);
            public const uint ES_CONTINUOUS = 0x80000000;
            public const uint ES_SYSTEM_REQUIRED = 0x00000001;
        }

        public object WindowContent
        {
            get { return (object)GetValue(WindowContentProperty); }
            set { SetValue(WindowContentProperty, value); }
        }

        public static readonly DependencyProperty WindowContentProperty =
            DependencyProperty.Register("WindowContent", typeof(object), typeof(Fullscreen), new PropertyMetadata(null));

        public delegate void FullScreenClosing_Handler(object windowcontent);
        public event FullScreenClosing_Handler FullScreenClosing;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            object content = WindowContent;

            WindowContent = null;

            NativeMethods.SetThreadExecutionState(fPreviousExecutionState);

            if (FullScreenClosing != null) FullScreenClosing(content);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }


        #region "Page Control"

        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(double), typeof(Fullscreen), new PropertyMetadata(1D));


        public string ZoomLevelText
        {
            get { return (string)GetValue(ZoomLevelTextProperty); }
            set { SetValue(ZoomLevelTextProperty, value); }
        }

        public static readonly DependencyProperty ZoomLevelTextProperty =
            DependencyProperty.Register("ZoomLevelText", typeof(string), typeof(Fullscreen), new PropertyMetadata(null));

        ObservableCollection<string> zoomlevels;
        public ObservableCollection<string> ZoomLevels
        {
            get
            {
                if (zoomlevels == null)
                    zoomlevels = new ObservableCollection<string>();
                return zoomlevels;
            }

            set
            {
                zoomlevels = value;
            }
        }
        
        private void ZoomOut_Clicked(TrakHound_UI.Button bt)
        {
            double zoom = Math.Max(ZoomLevel - 0.05, 0.25);
            zoom_COMBO.Text = zoom.ToString("P0");
            SetZoom(zoom);
        }

        private void ZoomIn_Clicked(TrakHound_UI.Button bt)
        {
            double zoom = Math.Min(ZoomLevel + 0.05, 3.0);
            SetZoom(zoom);
            zoom_COMBO.Text = zoom.ToString("P0");
        }

        private void Fullscreen_Clicked(TrakHound_UI.Button bt)
        {
            this.Close();
        }

        void SetZoom(double zoom)
        {
            ZoomLevel = zoom;
        }

        private void zoom_COMBO_TextChanged(object sender, TextChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;

            if (combo.Text != null)
            {
                string val = combo.Text;

                if (val.Contains('%')) val = val.Substring(0, val.IndexOf('%'));

                double zoom;
                if (double.TryParse(val, out zoom))
                {
                    this.Dispatcher.BeginInvoke(new Action<double>(SetZoom), new object[] { zoom / 100 });
                }
            }
        }

        #endregion
    }
}
