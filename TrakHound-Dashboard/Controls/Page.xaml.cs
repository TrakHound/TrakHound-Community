// Copyright (c) 2017 TrakHound Inc., All Rights Reserved.

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
            PageContent.Opening();
            PageContent.Opened();
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

    }
}
