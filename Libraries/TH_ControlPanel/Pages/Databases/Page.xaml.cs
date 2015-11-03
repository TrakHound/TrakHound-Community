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
using System.Collections.ObjectModel;

using TH_Configuration;
using TH_PlugIns_Server;
using TH_WPF;

namespace TH_DeviceManager.Pages.Databases
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, ConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;

            TH_Database.DatabasePluginReader dpr = new TH_Database.DatabasePluginReader();

            plugins = new List<TH_Database.Database_Plugin>();

            foreach (Lazy<TH_Database.Database_Plugin> lplugin in dpr.plugins)
            {
                TH_Database.Database_Plugin plugin = lplugin.Value;

                plugins.Add(plugin);

                Button_01 bt = new Button_01();
                bt.Text = plugin.Type;
                DatabaseTypeList.Add(bt);
            }

        }

        #region "Page Interface"

        public string PageName { get { return "Databases"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/DatabaseConfig_01.png")); } }

        //public ImageSource Image { get { return null; } }

        public UserConfiguration currentUser { get; set; }

        public event SaveRequest_Handler SaveRequest;

        public event SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            if (plugins != null)
            {
                foreach (TH_Database.Database_Plugin plugin in plugins)
                {
                    string prefix = "/Databases/" + plugin.Type + "||";

                    string filter = "Address LIKE '" + prefix + "*' AND Value <> ''";                    
                    DataView dv = dt.AsDataView();
                    dv.RowFilter = filter;
                    DataTable temp_dt = dv.ToTable(true, "Address");

                    List<string> addresses = new List<string>();

                    foreach (DataRow row in temp_dt.Rows)
                    {
                        string s = row["Address"].ToString();

                        int last = s.LastIndexOf("/");
                        s = s.Substring(0, last + 1);

                        addresses.Add(s);

                        Console.WriteLine(s);
                    }

                    IEnumerable<string> distinct = addresses.Select(x => x).Distinct();   

                    foreach (string address in distinct)
                    {
                        filter = "Address LIKE '" + address + "*'";
                        dv = dt.AsDataView();
                        dv.RowFilter = filter;
                        temp_dt = dv.ToTable();
                        temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["Address"] };

                        object configButton = plugin.CreateConfigurationButton(temp_dt);
                        if (configButton != null)
                        {
                            Type config_type = plugin.Config_Page;

                            object o = Activator.CreateInstance(config_type);

                            TH_Database.DatabaseConfigurationPage page = (TH_Database.DatabaseConfigurationPage)o;

                            page.prefix = address;
                            page.LoadConfiguration(temp_dt);
                            page.SettingChanged += Configuration_Page_SettingChanged;

                            ListButton bt = new ListButton();
                            bt.Margin = new Thickness(0, 0, 0, 10);
                            bt.AlternateStyle = true;
                            bt.ShowImage = false;
                            bt.Selected += bt_Selected;
                            bt.DataObject = page;
                            bt.ButtonContent = configButton;

                            DatabaseList.Add(bt);
                        }
                    }



                    //object configButton = plugin.CreateConfigurationButton(dt);
                    //if (configButton != null)
                    //{
                    //    plugin.Configuration_Page.LoadConfiguration(dt);
                    //    plugin.Configuration_Page.SettingChanged += Configuration_Page_SettingChanged;

                    //    ListButton bt = new ListButton();
                    //    bt.Margin = new Thickness(0, 0, 0, 10);
                    //    bt.AlternateStyle = true;
                    //    bt.ShowImage = false;
                    //    bt.Selected += bt_Selected;
                    //    bt.DataObject = plugin.Configuration_Page;
                    //    bt.ButtonContent = configButton;

                    //    DatabaseList.Add(bt);
                    //}
                }
            }
        }

        void Configuration_Page_SettingChanged(string name, string oldVal, string newVal)
        {
            if (SettingChanged != null) SettingChanged(name, oldVal, newVal);
        }

        void bt_Selected(ListButton LB)
        {
            DatabaseConfigurationPage = LB.DataObject;
        }

        public void SaveConfiguration(DataTable dt)
        {
            if (DatabaseConfigurationPage != null)
            {
                TH_Database.DatabaseConfigurationPage page = (TH_Database.DatabaseConfigurationPage)DatabaseConfigurationPage;

                page.SaveConfiguration(dt);
            }
        }


        public object DatabaseConfigurationPage
        {
            get { return (object)GetValue(DatabaseConfigurationPageProperty); }
            set { SetValue(DatabaseConfigurationPageProperty, value); }
        }

        public static readonly DependencyProperty DatabaseConfigurationPageProperty =
            DependencyProperty.Register("DatabaseConfigurationPage", typeof(object), typeof(Page), new PropertyMetadata(null));

        
        #endregion

        List<TH_Database.Database_Plugin> plugins;

        ObservableCollection<object> databasetypelist;
        public ObservableCollection<object> DatabaseTypeList
        {
            get
            {
                if (databasetypelist == null)
                    databasetypelist = new ObservableCollection<object>();
                return databasetypelist;
            }

            set
            {
                databasetypelist = value;
            }
        }

        ObservableCollection<object> databaselist;
        public ObservableCollection<object> DatabaseList
        {
            get
            {
                if (databaselist == null)
                    databaselist = new ObservableCollection<object>();
                return databaselist;
            }

            set
            {
                databaselist = value;
            }
        }



    }
}
