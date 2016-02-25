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
using TH_Database;
using TH_Global.Functions;
using TH_Plugins_Server;
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

            CreateAddDatabaseButtons();
        }

        #region "Page Interface"

        public string PageName { get { return "Databases"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/DatabaseConfig_01.png")); } }

        public UserConfiguration currentUser { get; set; }

        public event TH_Plugins_Server.SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            DatabaseList.Clear();

            configurationTable = dt;

            CreateDatabaseButtons(dt);

            UpdatePageType(PageType);

            configurationTable = dt;

            if (DatabaseList.Count > 0) DisplayDatabases = true;
            else DisplayDatabases = false;
        }

        public void SaveConfiguration(DataTable dt)
        {

            // Clear all old database rows first
            ClearAddresses("/Databases/", dt); // OBSOLETE SO Make sure it clears it (2-11-16)

            if (PageType == Page_Type.Client) ClearAddresses("/Databases_Client/", dt);
            else if (PageType == Page_Type.Server) ClearAddresses("/Databases_Server/", dt);

            if (databaseConfigurationPages != null)
            {
                // Loop through each page and save to the DataTable
                foreach (TH_Database.DatabaseConfigurationPage page in databaseConfigurationPages)
                {
                    string address = SaveDatabaseRoot(page, dt);
                    page.SaveConfiguration(dt);
                }
            }

            LoadConfiguration(dt);
        }

        static void ClearAddresses(string prefix, DataTable dt)
        {
            string filter = "address LIKE '" + prefix + "*'";
            DataView dv = dt.AsDataView();
            dv.RowFilter = filter;
            DataTable temp_dt = dv.ToTable();
            foreach (DataRow row in temp_dt.Rows)
            {
                DataRow dbRow = dt.Rows.Find(row["address"]);
                if (dbRow != null) dt.Rows.Remove(dbRow);
            }
        }

        string SaveDatabaseRoot(TH_Database.DatabaseConfigurationPage page, DataTable dt)
        {
            string type = page.Plugin.Type.Replace(' ', '_');

            string test = null;
            if (PageType == Page_Type.Client) test = "/Databases_Client/" + type + "||";
            else if (PageType == Page_Type.Server) test = "/Databases_Server/" + type + "||";

            int i = 0;
            string address = test + i.ToString("00");
            while (Table_Functions.GetTableValue(address, dt) != null)
            {
                i += 1;
                address = test + i.ToString("00");
            }

            Table_Functions.UpdateTableValue("", "id||" + i.ToString("00"), address, dt);

            address += "/";

            page.prefix = address;

            return address;
        }

        private Page_Type _pageType;
        public Page_Type PageType
        {
            get { return _pageType; }
            set
            {
                _pageType = value;

                UpdatePageType(_pageType);
            }
        }

        #endregion


        //List<TH_Database.IDatabasePlugin> plugins;

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

        void CreateAddDatabaseButtons()
        {
            var plugins = TH_Database.Global.Plugins;
            if (plugins != null)
            {
                var list = new List<TH_Database.IDatabasePlugin>();
                foreach (var plugin in plugins)
                {
                    list.Add(plugin);
                }

                list.Sort((a, b) => a.Type.CompareTo(b.Type));

                foreach (var plugin in list)
                {
                    TH_WPF.Button bt = new TH_WPF.Button();
                    bt.Text = plugin.Type.Replace('_', ' ');
                    bt.DataObject = plugin;
                    bt.Clicked += AddDatabase_Clicked;
                    DatabaseTypeList.Add(bt);
                }
            }  
        }

        void AddDatabase_Clicked(TH_WPF.Button bt)
        {
            if (configurationTable != null)
            {
                TH_Database.IDatabasePlugin plugin = (TH_Database.IDatabasePlugin)bt.DataObject;

                object configButton = plugin.CreateConfigurationButton(null);
                if (configButton != null)
                {
                    Type config_type = plugin.Config_Page;

                    object o = Activator.CreateInstance(config_type);

                    TH_Database.DatabaseConfigurationPage page = o as TH_Database.DatabaseConfigurationPage;
                    if (page != null)
                    {
                        if (PageType == Page_Type.Client) page.ApplicationType = TH_Database.Application_Type.Client;
                        else if (PageType == Page_Type.Server) page.ApplicationType = TH_Database.Application_Type.Server;

                        page.SettingChanged += Configuration_Page_SettingChanged;
                        databaseConfigurationPages.Add(page);

                        Controls.DatabaseItemContainer item = new Controls.DatabaseItemContainer();
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

            if (Global.Plugins != null)
            {
                foreach (var plugin in Global.Plugins)
                {
                    string type = plugin.Type.Replace(' ', '_');

                    string prefix = null;
                    if (PageType == Page_Type.Client) prefix = "/Databases_Client/" + type + "||";
                    else if (PageType == Page_Type.Server) prefix = "/Databases_Server/" + type + "||";

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

                            //if (PageType == Page_Type.Client) page.ApplicationType = TH_Database.Application_Type.Client;
                            //else if (PageType == Page_Type.Server) page.ApplicationType = TH_Database.Application_Type.Server;

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
        }

        void item_Clicked(Controls.DatabaseItemContainer item)
        {
            if (item.collapseButton != null) item.collapseButton.IsExpanded = !item.collapseButton.IsExpanded;
        }

        List<string> GetAddressesForDatabase(string prefix, DataTable dt)
        {
            List<string> result = new List<string>();

            string filter = "Address LIKE '" + prefix + "*' AND Value <> ''";
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

        void UpdatePageType(Page_Type pageType)
        {
            if (databaseConfigurationPages != null)
            {
                foreach (var page in databaseConfigurationPages)
                {
                    if (PageType == Page_Type.Client) page.ApplicationType = TH_Database.Application_Type.Client;
                    else if (PageType == Page_Type.Server) page.ApplicationType = TH_Database.Application_Type.Server;
                }
            }
        }

        #endregion

        #region "Remove Database"

        void item_RemoveClicked(Controls.DatabaseItemContainer item)
        {
            if (configurationTable != null)
            {
                if (SettingChanged != null) SettingChanged(null, null, null);

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
