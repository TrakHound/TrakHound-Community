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
using TH_Global.Functions;
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
                bt.DataObject = plugin;
                bt.Clicked += AddDatabase_Clicked;
                DatabaseTypeList.Add(bt);
            }
        }

        #region "Page Interface"

        public string PageName { get { return "Databases"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/DatabaseConfig_01.png")); } }

        public UserConfiguration currentUser { get; set; }

        public event SaveRequest_Handler SaveRequest;

        public event SettingChanged_Handler SettingChanged;

        DataTable configurationTable;

        public void LoadConfiguration(DataTable dt)
        {
            DatabaseList.Clear();

            if (plugins != null)
            {
                configurationTable = dt;

                databaseConfigurationPages = new List<TH_Database.DatabaseConfigurationPage>();

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
                            databaseConfigurationPages.Add(page);

                            TH_DeviceManager.Controls.DatabaseItemContainer item = new TH_DeviceManager.Controls.DatabaseItemContainer();
                            item.prefix = address;
                            item.ItemContent = configButton;
                            item.RemoveClicked += item_RemoveClicked;

                            CollapseButton bt = new CollapseButton();
                            item.collapseButton = bt;
                            bt.ButtonContent = item;
                            bt.PageContent = page;

                            DatabaseList.Add(bt);
                        }
                    }
                }
            }
        }


        List<string> RemoveList = new List<string>();

        void item_RemoveClicked(TH_DeviceManager.Controls.DatabaseItemContainer item)
        {
            if (configurationTable != null)
            {

                string filter = "Address LIKE '" + item.prefix + "*'";
                DataView dv = configurationTable.AsDataView();
                dv.RowFilter = filter;
                DataTable temp_dt = dv.ToTable();
                temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["Address"] };

                // Get rid of last forward slash to get root database item
                string root = item.prefix;
                if (root[root.Length - 1] == '/') root = root.Substring(0, root.Length - 1);

                RemoveList.Add(root);

                //DataTable_Functions.RemoveTableRow(root, configurationTable);

                foreach (DataRow row in temp_dt.Rows)
                {
                    string address = row["Address"].ToString();
                    string oldVal = row["Value"].ToString();

                    RemoveList.Add(address);
                    //DataTable_Functions.RemoveTableRow(address, configurationTable);
                    if (SettingChanged != null) SettingChanged(address, oldVal, null);
                }

                TH_Database.DatabaseConfigurationPage page = (TH_Database.DatabaseConfigurationPage)item.collapseButton.PageContent;
                if (page != null)
                {
                    databaseConfigurationPages.Remove(page);
                }

                DatabaseList.Remove(item.collapseButton);
            }
        }

        List<TH_Database.DatabaseConfigurationPage> databaseConfigurationPages;

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
            if (databaseConfigurationPages != null)
            {
                foreach (Tuple<string, string> root in AddRootList)
                {
                    DataTable_Functions.UpdateTableValue("", root.Item2, root.Item1, dt);
                }

                foreach (string address in RemoveList)
                {
                    DataTable_Functions.RemoveTableRow(address, dt);
                }

                foreach (TH_Database.DatabaseConfigurationPage page in databaseConfigurationPages)
                {
                    page.SaveConfiguration(dt);
                }
            }
        }

        List<Tuple<string, string>> AddRootList = new List<Tuple<string, string>>();

        void AddDatabase_Clicked(Button_01 bt)
        {
            if (configurationTable != null)
            {
                TH_Database.Database_Plugin plugin = (TH_Database.Database_Plugin)bt.DataObject;

                // Find Id that is not used
                string test = "/Databases/" + plugin.Type + "||";
                int i = 0;
                string address = test + i.ToString("00");
                while (DataTable_Functions.GetTableValue(address, configurationTable) != null)
                {
                    i += 1;
                    address = test + i.ToString("00");
                }

                AddRootList.Add(new Tuple<string, string>(address, "id||" + i.ToString("00")));

                //DataTable_Functions.UpdateTableValue("", "id||" + i.ToString("00"), address, configurationTable);

                address += "/";

                object configButton = plugin.CreateConfigurationButton(null);
                if (configButton != null)
                {
                    Type config_type = plugin.Config_Page;

                    object o = Activator.CreateInstance(config_type);

                    TH_Database.DatabaseConfigurationPage page = (TH_Database.DatabaseConfigurationPage)o;
                    page.prefix = address;
                    page.SettingChanged += Configuration_Page_SettingChanged;
                    databaseConfigurationPages.Add(page);

                    TH_DeviceManager.Controls.DatabaseItemContainer item = new TH_DeviceManager.Controls.DatabaseItemContainer();
                    item.prefix = address;
                    item.ItemContent = configButton;
                    item.RemoveClicked += item_RemoveClicked;

                    CollapseButton cbt = new CollapseButton();
                    item.collapseButton = cbt;
                    cbt.ButtonContent = item;
                    cbt.PageContent = page;

                    DatabaseList.Add(cbt);






                    //CollapseButton item = new CollapseButton();
                    //item.ButtonContent = configButton;
                    //item.PageContent = page;
                    //DatabaseList.Add(item);
                } 
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
