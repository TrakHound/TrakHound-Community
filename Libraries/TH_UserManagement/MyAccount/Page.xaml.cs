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

using TH_Configuration;
//using TH_Database.Tables;
using TH_Global;
//using TH_PlugIns_Client_Control;

namespace TH_UserManagement.MyAccount
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string PageName { get { return "Profile"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/About_01.png")); } }


        public ImageSource ProfileImage
        {
            get { return (ImageSource)GetValue(ProfileImageProperty); }
            set { SetValue(ProfileImageProperty, value); }
        }

        public static readonly DependencyProperty ProfileImageProperty =
            DependencyProperty.Register("ProfileImage", typeof(ImageSource), typeof(Page), new PropertyMetadata(null));


        #region "Properties"

        public string Fullname
        {
            get { return (string)GetValue(FullnameProperty); }
            set { SetValue(FullnameProperty, value); }
        }

        public static readonly DependencyProperty FullnameProperty =
            DependencyProperty.Register("Fullname", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string Email
        {
            get { return (string)GetValue(EmailProperty); }
            set { SetValue(EmailProperty, value); }
        }

        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register("Email", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string Company
        {
            get { return (string)GetValue(CompanyProperty); }
            set { SetValue(CompanyProperty, value); }
        }

        public static readonly DependencyProperty CompanyProperty =
            DependencyProperty.Register("Company", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string Phone
        {
            get { return (string)GetValue(PhoneProperty); }
            set { SetValue(PhoneProperty, value); }
        }

        public static readonly DependencyProperty PhoneProperty =
            DependencyProperty.Register("Phone", typeof(string), typeof(Page), new PropertyMetadata(null));
     
        public string Address1
        {
            get { return (string)GetValue(Address1Property); }
            set { SetValue(Address1Property, value); }
        }

        public static readonly DependencyProperty Address1Property =
            DependencyProperty.Register("Address1", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string Address2
        {
            get { return (string)GetValue(Address2Property); }
            set { SetValue(Address2Property, value); }
        }

        public static readonly DependencyProperty Address2Property =
            DependencyProperty.Register("Address2", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string City
        {
            get { return (string)GetValue(CityProperty); }
            set { SetValue(CityProperty, value); }
        }

        public static readonly DependencyProperty CityProperty =
            DependencyProperty.Register("City", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string Country
        {
            get { return (string)GetValue(CountryProperty); }
            set { SetValue(CountryProperty, value); }
        }

        public static readonly DependencyProperty CountryProperty =
            DependencyProperty.Register("Country", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string State
        {
            get { return (string)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(string), typeof(Page), new PropertyMetadata(null));

        public string Zipcode
        {
            get { return (string)GetValue(ZipcodeProperty); }
            set { SetValue(ZipcodeProperty, value); }
        }

        public static readonly DependencyProperty ZipcodeProperty =
            DependencyProperty.Register("Zipcode", typeof(string), typeof(Page), new PropertyMetadata(null));

        
        #endregion

        public void LoadProfile(UserConfiguration userConfig)
        {
            Fullname = Formatting.UppercaseFirst(userConfig.first_name) + " " + Formatting.UppercaseFirst(userConfig.last_name);
            Username = Formatting.UppercaseFirst(userConfig.username);
            Email = userConfig.email;
            Company = Formatting.UppercaseFirst(userConfig.company);
            Phone = userConfig.phone;
            Address1 = userConfig.address1;
            Address2 = userConfig.address2;
            City = Formatting.UppercaseFirst(userConfig.city);
            Country = Formatting.UppercaseFirst(userConfig.country);
            State = Formatting.UppercaseFirst(userConfig.state);
            Zipcode = userConfig.zipcode;
        }


        private void ProfileImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {



        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            //if (mw != null)
            //{
            //    ProfileImage = mw.ProfileImage;

            //    LoadProfile(mw.CurrentUser);
            //}
                
        }

    }
}
