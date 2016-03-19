using System.Windows;

using TH_UserManagement.Management;

namespace TH_DeviceManager
{
    public partial class DeviceManagerTable
    {
        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;
            }
        }

        public bool LoggedIn
        {
            get { return (bool)GetValue(LoggedInProperty); }
            set { SetValue(LoggedInProperty, value); }
        }

        public static readonly DependencyProperty LoggedInProperty =
            DependencyProperty.Register("LoggedIn", typeof(bool), typeof(DeviceManagerTable), new PropertyMetadata(false));

    }
}
