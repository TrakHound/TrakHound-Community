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

using TH_Plugins_Client;

namespace TrakHound_Client.Pages.Plugins.Installed
{
    /// <summary>
    /// Interaction logic for ListItem.xaml
    /// </summary>
    public partial class ListItem : UserControl
    {
        public ListItem()
        {
            InitializeComponent();
            root.DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;
        }

        TrakHound_Client.MainWindow mw;

        public PluginConfiguration config;


        #region "Plugin Information"

        public string Plugin_Title
        {
            get { return (string)GetValue(Plugin_TitleProperty); }
            set { SetValue(Plugin_TitleProperty, value); }
        }

        public static readonly DependencyProperty Plugin_TitleProperty =
            DependencyProperty.Register("Plugin_Title", typeof(string), typeof(ListItem), new PropertyMetadata(null));

        public string Plugin_Description
        {
            get { return (string)GetValue(Plugin_DescriptionProperty); }
            set { SetValue(Plugin_DescriptionProperty, value); }
        }

        public static readonly DependencyProperty Plugin_DescriptionProperty =
            DependencyProperty.Register("Plugin_Description", typeof(string), typeof(ListItem), new PropertyMetadata(null));

        public ImageSource Plugin_Image
        {
            get { return (ImageSource)GetValue(Plugin_ImageProperty); }
            set { SetValue(Plugin_ImageProperty, value); }
        }

        public static readonly DependencyProperty Plugin_ImageProperty =
            DependencyProperty.Register("Plugin_Image", typeof(ImageSource), typeof(ListItem), new PropertyMetadata(null));

        public string Plugin_Version
        {
            get { return (string)GetValue(Plugin_VersionProperty); }
            set { SetValue(Plugin_VersionProperty, value); }
        }

        public static readonly DependencyProperty Plugin_VersionProperty =
            DependencyProperty.Register("Plugin_Version", typeof(string), typeof(ListItem), new PropertyMetadata(null));

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


        public bool Plugin_Enabled
        {
            get { return (bool)GetValue(Plugin_EnabledProperty); }
            set { SetValue(Plugin_EnabledProperty, value); }
        }

        public static readonly DependencyProperty Plugin_EnabledProperty =
            DependencyProperty.Register("Plugin_Enabled", typeof(bool), typeof(ListItem), new PropertyMetadata(false));

        public delegate void EnabledChanged_Handler(PluginConfiguration sender, bool enabled);
        public event EnabledChanged_Handler EnabledChanged;
        
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (config != null)
            {
                if (EnabledChanged != null) EnabledChanged(config, true);

                //config.Enabled = true;
                //Plugin_Enabled = config.Enabled;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (config != null)
            {
                if (EnabledChanged != null) EnabledChanged(config, false);

                //config.Enabled = false;
                //Plugin_Enabled = config.Enabled;
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

                mw.AddPageAsTab(dummy, "About | " + Plugin_Title, new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/About_01.png")));
            }
        }

        ObservableCollection<SubCategory> subCategories;
        public ObservableCollection<SubCategory> SubCategories
        {
            get
            {
                if (subCategories == null) subCategories = new ObservableCollection<SubCategory>();
                return subCategories;
            }
            set
            {
                subCategories = value;
            }
        }

    }
}
