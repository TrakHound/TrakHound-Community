// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;

using TrakHound;

namespace TrakHound_Dashboard.Controls
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class TabPage : UserControl
    {
        public TabPage()
        {
            Init();
        }

        public TabPage(IPage content)
        {
            Init();

            PageContent = content;
        }

        private void Init()
        {
            InitializeComponent();
            DataContext = this;
        }


        public IPage PageContent
        {
            get { return (IPage)GetValue(PageContentProperty); }
            set { SetValue(PageContentProperty, value); }
        }

        public static readonly DependencyProperty PageContentProperty =
            DependencyProperty.Register("PageContent", typeof(IPage), typeof(TabPage), new PropertyMetadata(null));


        public bool Closing
        {
            get { return (bool)GetValue(ClosingProperty); }
            set { SetValue(ClosingProperty, value); }
        }

        public static readonly DependencyProperty ClosingProperty =
            DependencyProperty.Register("Closing", typeof(bool), typeof(TabPage), new PropertyMetadata(false));


        #region "Page Control"

        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(double), typeof(TabPage), new PropertyMetadata(1D));

        public void SetZoom(double zoom)
        {
            ZoomLevel = zoom;
        }

        public void ZoomOut()
        {
            ZoomLevel = Math.Max(ZoomLevel - 0.05, 0.25);
        }

        public void ZoomIn()
        {
            ZoomLevel = Math.Min(ZoomLevel + 0.05, 3.0);
        }

        public void FullScreen()
        {
            var fs = new Windows.Fullscreen();
            fs.FullScreenClosing += fs_FullScreenClosing;

            object o = PageContent;

            PageContent = null;

            fs.WindowContent = o;

            fs.Show();
        }

        void fs_FullScreenClosing(object windowcontent)
        {
            if (windowcontent != null)
            {
                object o = windowcontent;

                PageContent = (IPage)o;
            }
        }

        #endregion

    }
}
