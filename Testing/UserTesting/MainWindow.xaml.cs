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

using TH_Configuration;
using TH_Database;
using TH_Database.Tables;
using TH_Global;

namespace UserTesting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DatabasePluginReader dpr = new DatabasePluginReader();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            string localPath = AppDomain.CurrentDomain.BaseDirectory + "UserConfiguration.Xml";
            string systemPath = TH_Global.FileLocations.TrakHound + @"\" + "UserConfiguration.Xml";

            string configPath;

            // systemPath takes priority (easier for user to navigate to)
            if (File.Exists(systemPath)) configPath = systemPath;
            else configPath = localPath;

            Logger.Log(configPath);

            TH_Configuration.Users.User_Configuration config = TH_Configuration.Users.ReadConfiguration(configPath);

            if (config != null)
            {
                // Initialize Database Configurations
                Global.Initialize(config.Databases);

                TH_Database.Database.Create(config.Databases);

                TH_Database.Tables.Users.CreateTable(config.Databases);

                TH_Database.Tables.Users.Update(config.Databases, username_TXT.Text, password_TXT.Text, device_TXT.Text);
            }

        }
    }
}
