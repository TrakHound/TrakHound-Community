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

using System.Collections.ObjectModel;

using TH_Global;

namespace TrakHound_Client.Controls
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

            //ZoomLevels.Add("50%");
            //ZoomLevels.Add("75%");
            //ZoomLevels.Add("100%");
            //ZoomLevels.Add("150%");
            //ZoomLevels.Add("200%");
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


        //public string ZoomLevelText
        //{
        //    get { return (string)GetValue(ZoomLevelTextProperty); }
        //    set { SetValue(ZoomLevelTextProperty, value); }
        //}

        //public static readonly DependencyProperty ZoomLevelTextProperty =
        //    DependencyProperty.Register("ZoomLevelText", typeof(string), typeof(TabPage), new PropertyMetadata(null));

        //ObservableCollection<string> zoomlevels;
        //public ObservableCollection<string> ZoomLevels
        //{
        //    get
        //    {
        //        if (zoomlevels == null)
        //            zoomlevels = new ObservableCollection<string>();
        //        return zoomlevels;
        //    }

        //    set
        //    {
        //        zoomlevels = value;
        //    }
        //}

        public void SetZoom(double zoom)
        {
            ZoomLevel = zoom;
        }

        public void ZoomOut()
        {
            ZoomLevel = Math.Max(ZoomLevel - 0.1, 0.5);
        }

        public void ZoomIn()
        {
            ZoomLevel = Math.Min(ZoomLevel + 0.1, 2.0);
        }

        //private void ZoomOut_Clicked(TH_WPF.Button bt) { ZoomOut(); }

        //private void ZoomIn_Clicked(TH_WPF.Button bt) { ZoomIn(); }

        //private void Fullscreen_Clicked(TH_WPF.Button bt)
        //{
        //    FullScreen();
        //}

        public void FullScreen()
        {
            Fullscreen fs = new Fullscreen();
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

        //private void zoom_COMBO_TextChanged(object sender, TextChangedEventArgs e)
        //{

        //    ComboBox combo = (ComboBox)sender;

        //    if (combo.Text != null)
        //    {
        //        string val = combo.Text;

        //        if (val.Contains('%')) val = val.Substring(0, val.IndexOf('%'));

        //        double zoom;
        //        if (double.TryParse(val, out zoom))
        //        {
        //            this.Dispatcher.BeginInvoke(new Action<double>(SetZoom), new object[] { zoom / 100 });
        //        }
        //    }

        //}

        #endregion

    }
}
