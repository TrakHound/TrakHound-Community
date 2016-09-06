// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using TrakHound;

namespace TrakHound_Dashboard.Pages.Options.General
{
    /// <summary>
    /// Interaction logic for General.xaml
    /// </summary>
    public partial class Page : UserControl, IPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;

            Themes.Add("Light");
            Themes.Add("Dark");

            mw = Application.Current.MainWindow as MainWindow;
        }

        MainWindow mw;

        public string Title { get { return "General"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Dashboard;component/Resources/Home_02.png")); } }

        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }


        public ResourceDictionary ThemeDictionary
        {
            // You could probably get it via its name with some query logic as well.
            get
            {
                return Application.Current.Resources.MergedDictionaries[0];
            }
        }

        public void ChangeTheme(Uri uri)
        {
            //ThemeDictionary.MergedDictionaries.Clear();
            ThemeDictionary.MergedDictionaries.RemoveAt(1);
            ThemeDictionary.MergedDictionaries.Insert(1, new ResourceDictionary() { Source = uri });
        }

        private ObservableCollection<string> _themes;
        public ObservableCollection<string> Themes
        {
            get
            {
                if (_themes == null)
                    _themes = new ObservableCollection<string>();
                return _themes;
            }

            set
            {
                _themes = value;
            }
        }

        public string SelectedTheme
        {
            get { return (string)GetValue(SelectedThemeProperty); }
            set
            {
                SetValue(SelectedThemeProperty, value);

                UpdateTheme(value);
            }
        }

        public static readonly DependencyProperty SelectedThemeProperty =
            DependencyProperty.Register("SelectedTheme", typeof(string), typeof(Page), new PropertyMetadata("Light", new PropertyChangedCallback(SelectedTheme_PropertyChanged)));


        private static void SelectedTheme_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var o = obj as Page;
            if (o != null) o.UpdateTheme(e.NewValue.ToString());
        }


        private void UpdateTheme(string key)
        {
            switch (key)
            {
                case "Light":

                    ChangeTheme(new Uri("/TrakHound-UI;component/Styles/Theme_Light.xaml", UriKind.RelativeOrAbsolute));
                    //TrakHound_UI.MkThemeSelector.SetCurrentThemeDictionary(this, new Uri("/TrakHound-Dashboard;component/Styles/Dashboard_Light.xaml"));

                    break;

                case "Dark":

                    ChangeTheme(new Uri("/TrakHound-UI;component/Styles/Theme_Dark.xaml", UriKind.RelativeOrAbsolute));
                    //TrakHound_UI.MkThemeSelector.SetCurrentThemeDictionary(this, new Uri("/TrakHound-Dashboard;component/Styles/Dashboard_Dark.xaml"));

                    break;
            }


        }

    }
}
