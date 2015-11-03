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

using System.Data;

using TH_Database;
using TH_Global.Functions;

namespace TH_MySQL.ConfigurationPage
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, DatabaseConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
        }

        public string PageName { get { return "MySQL"; } }

        public ImageSource Image { get { return null; } }

        public event SettingChanged_Handler SettingChanged;

        public string prefix { get; set; }

        public void LoadConfiguration(DataTable dt)
        {
            Loading = true;

            configurationTable = dt;

            // Load Database Name
            databasename_TXT.Text = DataTable_Functions.GetTableValue(prefix + "Database", dt);

            // Load Server
            server_TXT.Text = DataTable_Functions.GetTableValue(prefix + "Server", dt);

            // Load Port
            port_TXT.Text = DataTable_Functions.GetTableValue(prefix + "Port", dt);

            // Load Username
            username_TXT.Text = DataTable_Functions.GetTableValue(prefix + "Username", dt);

            Loading = false;
        }

        public void SaveConfiguration(DataTable dt)
        {
            // Save Database Name
            DataTable_Functions.UpdateTableValue(databasename_TXT.Text, prefix + "Database", dt);

            // Save Server
            DataTable_Functions.UpdateTableValue(server_TXT.Text, prefix + "Server", dt);

            // Save Port
            DataTable_Functions.UpdateTableValue(port_TXT.Text, prefix + "Port", dt);

            // Save Username
            DataTable_Functions.UpdateTableValue(username_TXT.Text, prefix + "Username", dt);
        }

        private void databasename_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(prefix + "Database", ((TextBox)sender).Text);
        }

        private void server_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(prefix + "Server", ((TextBox)sender).Text);
        }

        private void port_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(prefix + "Port", ((TextBox)sender).Text);
        }

        private void username_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangeSetting(prefix + "Username", ((TextBox)sender).Text);
        }

        

        void ChangeSetting(string name, string val)
        {
            if (!Loading)
            {
                string newVal = val;
                string oldVal = null;

                if (configurationTable != null)
                {
                    oldVal = DataTable_Functions.GetTableValue(name, configurationTable);
                }

                if (SettingChanged != null) SettingChanged(name, oldVal, newVal);
            }
        }

        public bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(Page), new PropertyMetadata(false));

        DataTable configurationTable;


        private void Help_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = true;
                    }
                }
            }
        }

        private void Help_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender.GetType() == typeof(Rectangle))
            {
                Rectangle rect = (Rectangle)sender;

                if (rect.ToolTip != null)
                {
                    if (rect.ToolTip.GetType() == typeof(ToolTip))
                    {
                        ToolTip tt = (ToolTip)rect.ToolTip;
                        tt.IsOpen = false;
                    }
                }
            }
        }

        private void password_TXT_PasswordChanged(object sender, RoutedEventArgs e)
        {

        }

        private void confirmpassword_TXT_PasswordChanged(object sender, RoutedEventArgs e)
        {

        }


    }
}
