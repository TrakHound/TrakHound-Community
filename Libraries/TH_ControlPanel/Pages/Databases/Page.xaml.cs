// Copyright (c) 2015 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
using TH_UserManagement;
using TH_UserManagement.Management;

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

            // Read Database plugins and create 'Add' buttons for each
            TH_Database.DatabasePluginReader dpr = new TH_Database.DatabasePluginReader();
            plugins = new List<TH_Database.Database_Plugin>();
            CreateAddDatabaseButtons(dpr);
        }

        #region "Page Interface"

        public string PageName { get { return "Databases"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/DatabaseConfig_01.png")); } }

        public UserConfiguration currentUser { get; set; }

        public event SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            DatabaseList.Clear();

            if (plugins != null)
            {
                configurationTable = dt;

                CreateDatabaseButtons(dt);

                configurationTable = dt;
            }

            if (DatabaseList.Count > 0) DisplayDatabases = true;
            else DisplayDatabases = false;
        }

        public void SaveConfiguration(DataTable dt)
        {
            if (databaseConfigurationPages != null)
            {
                // Add each of the root addresses to add
                foreach (Tuple<string, string> root in AddRootList)
                {
                    Table_Functions.UpdateTableValue("", root.Item2, root.Item1, dt);
                }

                // Remove each of the addresses set to be removed
                foreach (string address in RemoveList)
                {
                    Table_Functions.RemoveTableRow(address, dt);
                }

                // Loop through each page and save to the DataTable
                foreach (TH_Database.DatabaseConfigurationPage page in databaseConfigurationPages)
                {
                    page.SaveConfiguration(dt);
                }

                AddRootList.Clear();
                RemoveList.Clear();
            }

            LoadConfiguration(dt);
        }

        public Page_Type PageType { get; set; }

        #endregion


        List<TH_Database.Database_Plugin> plugins;

        DataTable configurationTable;

        void Configuration_Page_SettingChanged(string name, string oldVal, string newVal)
        {
            if (SettingChanged != null) SettingChanged(name, oldVal, newVal);
        }

        public object DatabaseConfigurationPage
        {
            get { return (object)GetValue(DatabaseConfigurationPageProperty); }
            set { SetValue(DatabaseConfigurationPageProperty, value); }
        }

        public static readonly DependencyProperty DatabaseConfigurationPageProperty =
            DependencyProperty.Register("DatabaseConfigurationPage", typeof(object), typeof(Page), new PropertyMetadata(null));

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



        public bool DisplayDatabases
        {
            get { return (bool)GetValue(DisplayDatabasesProperty); }
            set { SetValue(DisplayDatabasesProperty, value); }
        }

        public static readonly DependencyProperty DisplayDatabasesProperty =
            DependencyProperty.Register("DisplayDatabases", typeof(bool), typeof(Page), new PropertyMetadata(false));

        

        #region "Add Database"

        void CreateAddDatabaseButtons(TH_Database.DatabasePluginReader dpr)
        {
            foreach (Lazy<TH_Database.Database_Plugin> lplugin in dpr.plugins)
            {
                TH_Database.Database_Plugin plugin = lplugin.Value;
                plugins.Add(plugin);

                TH_WPF.Button bt = new TH_WPF.Button();
                bt.Text = plugin.Type;
                bt.DataObject = plugin;
                bt.Clicked += AddDatabase_Clicked;
                DatabaseTypeList.Add(bt);
            }
        }

        List<Tuple<string, string>> AddRootList = new List<Tuple<string, string>>();

        void AddDatabase_Clicked(TH_WPF.Button bt)
        {
            if (configurationTable != null)
            {
                TH_Database.Database_Plugin plugin = (TH_Database.Database_Plugin)bt.DataObject;

                // Find Id that is not used
                //string test = "/Databases/" + plugin.Type + "||";

                string test = null;
                if (PageType == Page_Type.Client) test = "/Databases_Client/" + plugin.Type + "||";
                else if (PageType == Page_Type.Server) test = "/Databases_Server/" + plugin.Type + "||";

                int i = 0;
                string address = test + i.ToString("00");
                while (Table_Functions.GetTableValue(address, configurationTable) != null || AddRootList.Find(x => x.Item1 == address) != null)
                {
                    i += 1;
                    address = test + i.ToString("00");
                }


                // Add to list of root database address to add when saved
                AddRootList.Add(new Tuple<string, string>(address, "id||" + i.ToString("00")));

                address += "/";

                object configButton = plugin.CreateConfigurationButton(null);
                if (configButton != null)
                {
                    Type config_type = plugin.Config_Page;

                    object o = Activator.CreateInstance(config_type);

                    TH_Database.DatabaseConfigurationPage page = (TH_Database.DatabaseConfigurationPage)o;

                    if (PageType == Page_Type.Client) page.PageType = TH_Database.Page_Type.Client;
                    else if (PageType == Page_Type.Server) page.PageType = TH_Database.Page_Type.Server;

                    page.prefix = address;
                    page.SettingChanged += Configuration_Page_SettingChanged;
                    databaseConfigurationPages.Add(page);

                    Controls.DatabaseItemContainer item = new Controls.DatabaseItemContainer();
                    item.prefix = address;
                    item.ItemContent = configButton;
                    item.Clicked += item_Clicked;
                    item.RemoveClicked += item_RemoveClicked;

                    CollapseButton cbt = new CollapseButton();
                    item.collapseButton = cbt;
                    cbt.ButtonContent = item;
                    cbt.PageContent = page;

                    foreach (CollapseButton ocbt in DatabaseList.OfType<CollapseButton>().ToList()) ocbt.IsExpanded = false;
                    cbt.IsExpanded = true;

                    DatabaseList.Add(cbt);
                }
            }

            DisplayDatabases = true;
        }

        #endregion

        #region "Database Configuration"

        List<TH_Database.DatabaseConfigurationPage> databaseConfigurationPages;

        void CreateDatabaseButtons(DataTable dt)
        {
            databaseConfigurationPages = new List<TH_Database.DatabaseConfigurationPage>();

            bool openfirst = true;

            foreach (TH_Database.Database_Plugin plugin in plugins)
            {
                string prefix = null;
                if (PageType == Page_Type.Client) prefix = "/Databases_Client/" + plugin.Type + "||";
                else if (PageType == Page_Type.Server) prefix = "/Databases_Server/" + plugin.Type + "||";

                List<string> addresses = GetAddressesForDatabase(prefix, dt);

                foreach (string address in addresses)
                {
                    string filter = "address LIKE '" + address + "*'";
                    DataView dv = dt.AsDataView();
                    dv.RowFilter = filter;
                    DataTable temp_dt = dv.ToTable();
                    temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

                    object configButton = plugin.CreateConfigurationButton(temp_dt);
                    if (configButton != null)
                    {
                        Type config_type = plugin.Config_Page;

                        object o = Activator.CreateInstance(config_type);

                        TH_Database.DatabaseConfigurationPage page = (TH_Database.DatabaseConfigurationPage)o;

                        if (PageType == Page_Type.Client) page.PageType = TH_Database.Page_Type.Client;
                        else if (PageType == Page_Type.Server) page.PageType = TH_Database.Page_Type.Server;

                        page.prefix = address;
                        page.LoadConfiguration(temp_dt);
                        page.SettingChanged += Configuration_Page_SettingChanged;
                        databaseConfigurationPages.Add(page);

                        Controls.DatabaseItemContainer item = new Controls.DatabaseItemContainer();
                        item.prefix = address;
                        item.ItemContent = configButton;
                        item.Clicked += item_Clicked;
                        item.RemoveClicked += item_RemoveClicked;

                        CollapseButton bt = new CollapseButton();
                        item.collapseButton = bt;
                        bt.ButtonContent = item;
                        bt.PageContent = page;

                        if (openfirst) bt.IsExpanded = true;
                        openfirst = false;

                        DatabaseList.Add(bt);
                    }
                }
            }
        }

        void item_Clicked(Controls.DatabaseItemContainer item)
        {
            if (item.collapseButton != null) item.collapseButton.IsExpanded = !item.collapseButton.IsExpanded;
        }

        List<string> GetAddressesForDatabase(string prefix, DataTable dt)
        {
            List<string> result = new List<string>();

            string filter = "Address LIKE '" + prefix + "*' AND Value <> ''";
            //string filter = "address LIKE '" + prefix + "*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();

            List<string> rootAddresses = new List<string>();

            // Loop through each row and parse out root database address
            foreach (DataRow row in temp_dt.Rows)
            {
                string s = row["address"].ToString();

                int last = s.LastIndexOf("/");
                s = s.Substring(0, last + 1);

                rootAddresses.Add(s);
            }

            // Get Distinct list of root addresses (represents each database)
            IEnumerable<string> distinct = rootAddresses.Select(x => x).Distinct();

            result = distinct.ToList();

            return result;
        }

        #endregion

        #region "Remove Database"

        List<string> RemoveList = new List<string>();

        void item_RemoveClicked(Controls.DatabaseItemContainer item)
        {
            if (configurationTable != null)
            {

                string filter = "address LIKE '" + item.prefix + "*'";
                DataView dv = configurationTable.AsDataView();
                dv.RowFilter = filter;
                DataTable temp_dt = dv.ToTable();
                temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

                // Get rid of last forward slash to get root database item
                string root = item.prefix;
                if (root[root.Length - 1] == '/') root = root.Substring(0, root.Length - 1);

                // Add root to list of addresses to remove from datatable when saved
                RemoveList.Add(root);

                foreach (DataRow row in temp_dt.Rows)
                {
                    string address = row["address"].ToString();
                    string oldVal = row["value"].ToString();

                    // Add to list of addresses to remove from datatable when saved
                    RemoveList.Add(address);

                    if (SettingChanged != null) SettingChanged(address, oldVal, null);
                }

                // Remove Configuration page from list
                TH_Database.DatabaseConfigurationPage page = (TH_Database.DatabaseConfigurationPage)item.collapseButton.PageContent;
                if (page != null)
                {
                    databaseConfigurationPages.Remove(page);
                }

                // Remove Collapse Button From List
                DatabaseList.Remove(item.collapseButton);
            }

            if (DatabaseList.Count > 0) DisplayDatabases = true;
            else DisplayDatabases = false;
        }

        #endregion

    }
}
