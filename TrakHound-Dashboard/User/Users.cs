using System.Windows;
using TrakHound.API.Users;

namespace TrakHound_Dashboard
{
    public partial class MainWindow
    {
        private void Users_Initialize()
        {
            string token = Properties.Settings.Default.LoginRememberToken;

            if (!string.IsNullOrEmpty(token)) TokenLogin(token);
            else CurrentUser = null;
        }


        public delegate void CurrentUserChanged_Handler(UserConfiguration userConfig);
        public event CurrentUserChanged_Handler CurrentUserChanged;


        private UserConfiguration _currentuser;
        public UserConfiguration CurrentUser
        {
            get { return (UserConfiguration)GetValue(CurrentUserProperty); }
            set
            {
                SetValue(CurrentUserProperty, value);

                _currentuser = value;

                // Update other pages
                if (DeviceManager != null) DeviceManager.CurrentUser = _currentuser;
                if (accountpage != null) accountpage.LoadUserConfiguration(_currentuser);
                Plugins_UpdateUser(_currentuser);

                // Login Server user (set login file)
                Login(_currentuser);

                // Restart Message monitor
                StartMessageMonitor();
                messageCenter.ClearMessages();

                // Load the Profile Image (Nav Menu)
                LoadProfileImage(_currentuser);

                // Raise CurrentUserChanged Event
                if (CurrentUserChanged != null) CurrentUserChanged(_currentuser);
            }
        }

        public static readonly DependencyProperty CurrentUserProperty =
            DependencyProperty.Register("CurrentUser", typeof(UserConfiguration), typeof(MainWindow), new PropertyMetadata(null));


        public bool UserLoading
        {
            get { return (bool)GetValue(UserLoadingProperty); }
            set { SetValue(UserLoadingProperty, value); }
        }

        public static readonly DependencyProperty UserLoadingProperty =
            DependencyProperty.Register("UserLoading", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));


        public string UserLoadingText
        {
            get { return (string)GetValue(UserLoadingTextProperty); }
            set { SetValue(UserLoadingTextProperty, value); }
        }

        public static readonly DependencyProperty UserLoadingTextProperty =
            DependencyProperty.Register("UserLoadingText", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public bool UserLoginError
        {
            get { return (bool)GetValue(UserLoginErrorProperty); }
            set { SetValue(UserLoginErrorProperty, value); }
        }

        public static readonly DependencyProperty UserLoginErrorProperty =
            DependencyProperty.Register("UserLoginError", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

    }
}
