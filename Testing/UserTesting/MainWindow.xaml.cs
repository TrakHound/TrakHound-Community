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

using System.IO;
using System.Data;
using System.Collections.ObjectModel;

using TH_Configuration;
using TH_Database;
using TH_Database.Tables.Configurations;
using TH_Database.Tables;
using TH_Global;

namespace UserTesting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Database_Settings dbSettings;

        public string AddConfigurationPath
        {
            get { return (string)GetValue(AddConfigurationPathProperty); }
            set { SetValue(AddConfigurationPathProperty, value); }
        }

        public static readonly DependencyProperty AddConfigurationPathProperty =
            DependencyProperty.Register("AddConfigurationPath", typeof(string), typeof(MainWindow), new PropertyMetadata(null));


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            DatabasePluginReader dpr = new DatabasePluginReader();

            string localPath = AppDomain.CurrentDomain.BaseDirectory + "UserConfiguration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "UserConfiguration.Xml";

            string configPath;

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            Logger.Log(configPath);

            UserManagementSettings settings = UserManagementSettings.ReadConfiguration(configPath);

            dbSettings = settings.Databases;
            Global.Initialize(dbSettings);
        }

        Users.UserConfiguration currentUser;

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Users.UserConfiguration userConfig = Users.Login(username_TXT.Text, password_TXT.Password, dbSettings);
            if (userConfig != null)
            {
                LoadConfigurations(userConfig, dbSettings);
            }

            currentUser = userConfig;

        }

        private void Create_Button_Click(object sender, RoutedEventArgs e)
        {

            Users.CreateUserTable(dbSettings);

            Users.UserConfiguration userConfig = new Users.UserConfiguration();
            userConfig.username = create_username_TXT.Text;
            userConfig.first_name = create_firstname_TXT.Text;
            userConfig.last_name = create_lastname_TXT.Text;
            userConfig.company = create_company_TXT.Text;
            userConfig.email = create_email_TXT.Text;
            userConfig.phone = create_phone_TXT.Text;
            userConfig.address = create_address1_TXT.Text;
            userConfig.city = create_city_TXT.Text;
            userConfig.state = create_state_TXT.Text;
            userConfig.country = create_country_TXT.Text;
            userConfig.zipcode = create_zipcode_TXT.Text;

            Users.CreateUser(userConfig, create_password_TXT.Text, dbSettings);

        }

        ObservableCollection<TextBlock> configurations;
        public ObservableCollection<TextBlock> Configurations
        {
            get
            {
                if (configurations == null)
                    configurations = new ObservableCollection<TextBlock>();
                return configurations;
            }

            set
            {
                configurations = value;
            }
        }

        void LoadConfigurations(Users.UserConfiguration userConfig, Database_Settings db)
        {

            List<Configuration> configs = TH_Database.Tables.Users.GetConfigurationsForUser(userConfig, db);

            foreach (Configuration config in configs)
            {
                TextBlock txt = new TextBlock();
                txt.Text = config.Description.Description + config.Description.Machine_ID;
                Configurations.Add(txt);
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Configuration config = TH_Configuration.Configuration.ReadConfigFile(AddConfigurationPath);
            if (config != null)
            {
                TH_Database.Tables.Users.AddConfigurationToUser(currentUser, dbSettings, config);
            }
        }
        

        private void Browse_Button_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = FileLocations.TrakHound;
            dlg.Multiselect = false;
            dlg.Title = "Browse for Configurations";
            dlg.Filter = "Device Configuration File (*.xml)|*.xml";

            dlg.ShowDialog();

            try
            {
                string configPath = dlg.FileName;

                AddConfigurationPath = configPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Browse_Button_Click() : " + ex.Message);
            }


        }

        private void Share_Button_Click(object sender, RoutedEventArgs e)
        {

            Configuration config = TH_Configuration.Configuration.ReadConfigFile(AddConfigurationPath);

            Shared.SharedConfiguration sharedConfig = Shared.SharedConfiguration.Create("Example Configuration File", config, currentUser.username, "", "example");

            Shared.CreateSharedConfiguration(dbSettings, sharedConfig);

        }
    }
}
