using System.Windows;

using TH_UserManagement.Management;

namespace TH_DeviceManager
{
    public partial class DeviceManager
    {
        UserConfiguration currentuser;
        public UserConfiguration CurrentUser
        {
            get { return currentuser; }
            set
            {
                currentuser = value;

                if (currentuser != null) LoggedIn = true;
                else LoggedIn = false;

                LoadDevices();

                AddDevice_Initialize();
                CopyDevice_Initialize();
            }
        }

        public bool LoggedIn
        {
            get { return (bool)GetValue(LoggedInProperty); }
            set { SetValue(LoggedInProperty, value); }
        }

        public static readonly DependencyProperty LoggedInProperty =
            DependencyProperty.Register("LoggedIn", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));

    }
}
