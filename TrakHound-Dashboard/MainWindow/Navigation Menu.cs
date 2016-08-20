// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

using TrakHound.Tools;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {

        public bool NavigationMenuShown
        {
            get { return (bool)GetValue(NavigationMenuShownProperty); }
            set { SetValue(NavigationMenuShownProperty, value); }
        }

        public static readonly DependencyProperty NavigationMenuShownProperty =
            DependencyProperty.Register("NavigationMenuShown", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public string LoginUsername
        {
            get { return (string)GetValue(LoginUsernameProperty); }
            set { SetValue(LoginUsernameProperty, value); }
        }

        public static readonly DependencyProperty LoginUsernameProperty =
            DependencyProperty.Register("LoginUsername", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public string LoginPassword
        {
            get { return (string)GetValue(LoginPasswordProperty); }
            set { SetValue(LoginPasswordProperty, value); }
        }

        public static readonly DependencyProperty LoginPasswordProperty =
            DependencyProperty.Register("LoginPassword", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public bool IsUsernameFocused
        {
            get { return (bool)GetValue(IsUsernameFocusedProperty); }
            set { SetValue(IsUsernameFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsUsernameFocusedProperty =
            DependencyProperty.Register("IsUsernameFocused", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public bool IsPasswordFocused
        {
            get { return (bool)GetValue(IsPasswordFocusedProperty); }
            set { SetValue(IsPasswordFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsPasswordFocusedProperty =
            DependencyProperty.Register("IsPasswordFocused", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));



        private void NavigationMenuButton_Clicked(TrakHound_UI.Button bt)
        {
            NavigationMenu_Show();
        }

        private void NavigationMenuCollapse_Clicked(TrakHound_UI.Button bt)
        {
            NavigationMenu_Hide();
        }

        private void ShadedPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationMenu_Hide();
        }


        private void NavigationMenu_Show()
        {
            NavigationMenuShown = true;
        }

        private void NavigationMenu_Hide()
        {
            NavigationMenuShown = false;
        }


        private void Login_Clicked(TrakHound_UI.Button bt)
        {
            Login(LoginUsername, LoginPassword);
            LoginPassword = null;
        }

        private void Logout_Clicked(TrakHound_UI.Button bt)
        {
            Logout();
        }

        private void Username_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) IsUsernameFocused = true;
        }

        private void Password_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter) Login(LoginUsername, LoginPassword);
        }

        private void LoginUsername_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) { UserLoginError = false; }

        private void LoginPassword_PasswordChanged(object sender, RoutedEventArgs e) { UserLoginError = false; }


        private void CreateAccount_Clicked(TrakHound_UI.Button bt)
        {
            AccountManager_Open();
            NavigationMenu_Hide();
        }


        private void Dashboard_Clicked(TrakHound_UI.Button bt)
        {
            var plugin = Plugins.Find(x => x.Title == "Dashboard");
            if (plugin != null) AddTab(plugin);

            NavigationMenu_Hide();
        }

        private void Cycles_Clicked(TrakHound_UI.Button bt)
        {
            var plugin = Plugins.Find(x => x.Title == "Cycles");
            if (plugin != null) AddTab(plugin);

            NavigationMenu_Hide();
        }

        private void DeviceManager_Clicked(TrakHound_UI.Button bt)
        {
            DeviceManager_DeviceList_Open();
            NavigationMenu_Hide();
        }

        private void Options_Clicked(TrakHound_UI.Button bt)
        {
            Options_Open();
            NavigationMenu_Hide();
        }

        private void MyAccount_Clicked(TrakHound_UI.Button bt)
        {
            AccountManager_Open();
            NavigationMenu_Hide();
        }

        private void About_Clicked(TrakHound_UI.Button bt)
        {
            About_Open();
            NavigationMenu_Hide();
        }

        private void Fullscreen_Clicked(TrakHound_UI.Button bt)
        {
            if (CurrentPage != null) CurrentPage.FullScreen();
            NavigationMenu_Hide();
        }

        private void TableManager_Clicked(TrakHound_UI.Button bt)
        {
            var plugin = Plugins.Find(x => x.Title == "Table Manager");
            if (plugin != null) AddTab(plugin);

            NavigationMenu_Hide();
        }

        //private void DeveloperConsole_Clicked(TrakHound_UI.Button bt)
        //{
        //    developerConsole.Shown = !developerConsole.Shown;
        //}

        private void ReportBug_Clicked(TrakHound_UI.Button bt)
        {
            OpenBugReport();
            NavigationMenu_Hide();
        }


        private void ZoomOut_Clicked(TrakHound_UI.Button bt)
        {
            if (CurrentPage != null) CurrentPage.ZoomOut();
        }

        private void ZoomIn_Clicked(TrakHound_UI.Button bt)
        {
            if (CurrentPage != null) CurrentPage.ZoomIn();
        }

        private void RestoreZoom_Clicked(TrakHound_UI.Button bt)
        {
            if (CurrentPage != null) CurrentPage.SetZoom(1);
        }

    }

    public abstract class BaseConverter : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    [ValueConversion(typeof(object), typeof(string))]
    public class UppercaseConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                string val = value.ToString();

                return String_Functions.UppercaseFirst(val);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}

