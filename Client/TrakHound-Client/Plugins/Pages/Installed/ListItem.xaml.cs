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

using TH_PlugIns_Client;

namespace TrakHound_Client.Plugins.Pages.Installed
{
    /// <summary>
    /// Interaction logic for ListItem.xaml
    /// </summary>
    public partial class ListItem : UserControl
    {
        public ListItem()
        {
            InitializeComponent();

            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;
        }

        TrakHound_Client.MainWindow mw;

        public PlugInConfiguration config;


        #region "Plugin Information"

        public string PlugIn_Title
        {
            get { return (string)GetValue(PlugIn_TitleProperty); }
            set { SetValue(PlugIn_TitleProperty, value); }
        }

        public static readonly DependencyProperty PlugIn_TitleProperty =
            DependencyProperty.Register("PlugIn_Title", typeof(string), typeof(ListItem), new PropertyMetadata(null));

        public string PlugIn_Description
        {
            get { return (string)GetValue(PlugIn_DescriptionProperty); }
            set { SetValue(PlugIn_DescriptionProperty, value); }
        }

        public static readonly DependencyProperty PlugIn_DescriptionProperty =
            DependencyProperty.Register("PlugIn_Description", typeof(string), typeof(ListItem), new PropertyMetadata(null));

        public ImageSource PlugIn_Image
        {
            get { return (ImageSource)GetValue(PlugIn_ImageProperty); }
            set { SetValue(PlugIn_ImageProperty, value); }
        }

        public static readonly DependencyProperty PlugIn_ImageProperty =
            DependencyProperty.Register("PlugIn_Image", typeof(ImageSource), typeof(ListItem), new PropertyMetadata(null));

        public string PlugIn_Version
        {
            get { return (string)GetValue(PlugIn_VersionProperty); }
            set { SetValue(PlugIn_VersionProperty, value); }
        }

        public static readonly DependencyProperty PlugIn_VersionProperty =
            DependencyProperty.Register("PlugIn_Version", typeof(string), typeof(ListItem), new PropertyMetadata(null));

        #endregion

        #region "Author Information"

        public string Author_Name
        {
            get { return (string)GetValue(Author_NameProperty); }
            set { SetValue(Author_NameProperty, value); }
        }

        public static readonly DependencyProperty Author_NameProperty =
            DependencyProperty.Register("Author_Name", typeof(string), typeof(ListItem), new PropertyMetadata(null));


        public string Author_Text
        {
            get { return (string)GetValue(Author_TextProperty); }
            set { SetValue(Author_TextProperty, value); }
        }

        public static readonly DependencyProperty Author_TextProperty =
            DependencyProperty.Register("Author_Text", typeof(string), typeof(ListItem), new PropertyMetadata(null));

        public ImageSource Author_Image
        {
            get { return (ImageSource)GetValue(Author_ImageProperty); }
            set { SetValue(Author_ImageProperty, value); }
        }

        public static readonly DependencyProperty Author_ImageProperty =
            DependencyProperty.Register("Author_Image", typeof(ImageSource), typeof(ListItem), new PropertyMetadata(null));

        #endregion


        public bool PlugIn_Enabled
        {
            get { return (bool)GetValue(PlugIn_EnabledProperty); }
            set { SetValue(PlugIn_EnabledProperty, value); }
        }

        public static readonly DependencyProperty PlugIn_EnabledProperty =
            DependencyProperty.Register("PlugIn_Enabled", typeof(bool), typeof(ListItem), new PropertyMetadata(false));

        
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (config != null)
            {
                config.enabled = true;
                PlugIn_Enabled = config.enabled;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (config != null)
            {
                config.enabled = false;
                PlugIn_Enabled = config.enabled;
            }
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            if (mw != null)
            {

                Grid dummy = new Grid();
                dummy.Height = 300;
                dummy.Width = 300;
                dummy.Background = new SolidColorBrush(Colors.Red);

                mw.AddPageAsTab(dummy, "About | " + PlugIn_Title, new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/About_01.png")));

            }
        }


    }
}
