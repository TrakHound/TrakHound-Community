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
using TH_Plugins.Server;
using TH_WPF;
using TH_UserManagement;
using TH_UserManagement.Management;

namespace TH_DeviceManager.Pages.Databases
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, IConfigurationPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;

            CreateAddDatabaseButtons();
        }

        #region "Page Interface"

        public string PageName { get { return "Databases"; } }

        //public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/DatabaseConfig_01.png")); } }

        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (_image == null)
                {
                    _image = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/DatabaseConfig_01.png"));
                    _image.Freeze();
                }

                return _image;
            }
        }

        public UserConfiguration currentUser { get; set; }

        public event TH_Plugins.Server.SettingChanged_Handler SettingChanged;

        public void LoadConfiguration(DataTable dt)
        {
            DatabaseList.Clear();

            configurationTable = dt;

            GetPageGroups(dt);

            UpdatePageType(PageType);

            configurationTable = dt;

            DatabaseId = DataTable_Functions.GetTableValue(dt, "address", "/DatabaseId", "value");
        }

        public void SaveConfiguration(DataTable dt)
        {
            // Clear all old database rows first
            ClearAddresses("/Databases/", dt); // OBSOLETE SO Make sure it clears it (2-11-16)
            ClearAddresses("/Databases_Client/", dt);
            ClearAddresses("/Databases_Server/", dt);

            SaveConfigurationPages(clientPageGroups, dt, Page_Type.Client);
            SaveConfigurationPages(serverPageGroups, dt, Page_Type.Server);

            DataTable_Functions.UpdateTableValue(dt, "address", "/DatabaseId", "value", DatabaseId);

            LoadConfiguration(dt);
        }

        private void SaveConfigurationPages(List<PageGroup> groups, DataTable dt, Page_Type pageType)
        {
            if (groups != null)
            {
                foreach (var group in groups)
                {
                    string address = SaveDatabaseRoot(group.Page, dt, pageType);

                    group.Page.SaveConfiguration(dt);
                }
            }
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

        string SaveDatabaseRoot(TH_Database.DatabaseConfigurationPage page, DataTable dt, Page_Type pageType)
        {
            string type = page.Plugin.Type.Replace(' ', '_');

            string test = null;
            if (pageType == Page_Type.Client) test = "/Databases_Client/" + type + "||";
            else if (pageType == Page_Type.Server) test = "/Databases_Server/" + type + "||";

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
        private Page_Type PageType
        {
            get { return _pageType; }
            set
            {
                _pageType = value;

                UpdatePageType(_pageType);
            }
        }

        enum Page_Type
        {
            Client,
            Server
        }

        #endregion

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

                var group = GetPageGroup(plugin, PageType);

                if (PageType == Page_Type.Client) clientPageGroups.Add(group);
                else if (PageType == Page_Type.Server) serverPageGroups.Add(group);

                var pageBt = CreatePageButton(group);

                foreach (CollapseButton ocbt in DatabaseList.OfType<CollapseButton>().ToList()) ocbt.IsExpanded = false;
                pageBt.IsExpanded = true;

                DatabaseList.Add(pageBt);
            }
        }

        #endregion

        #region "Database Configuration"

        class PageGroup
        {
            public DatabaseConfigurationPage Page { get; set; }
            public object ButtonContent { get; set; }
        }

        List<PageGroup> clientPageGroups;
        List<PageGroup> serverPageGroups;

        
        private void GetPageGroups(DataTable dt)
        {
            clientPageGroups = GetPageGroups(dt, Page_Type.Client);
            serverPageGroups = GetPageGroups(dt, Page_Type.Server);
        }

        private List<PageGroup> GetPageGroups(DataTable dt, Page_Type pageType)
        {
            var result = new List<PageGroup>();

            if (dt != null)
            {
                if (Global.Plugins != null)
                {
                    foreach (var plugin in Global.Plugins)
                    {
                        string type = plugin.Type.Replace(' ', '_');

                        string prefix = null;
                        if (pageType == Page_Type.Client) prefix = "/Databases_Client/" + type + "||";
                        else if (pageType == Page_Type.Server) prefix = "/Databases_Server/" + type + "||";

                        List<string> addresses = GetAddressesForDatabase(prefix, dt);

                        foreach (string address in addresses)
                        {
                            string filter = "address LIKE '" + address + "*'";
                            DataView dv = dt.AsDataView();
                            dv.RowFilter = filter;
                            DataTable temp_dt = dv.ToTable();
                            temp_dt.PrimaryKey = new DataColumn[] { temp_dt.Columns["address"] };

                            var group = GetPageGroup(plugin, pageType, temp_dt, address);
                            result.Add(group);
                        }
                    }
                }
            }

            return result;
        }

        private PageGroup GetPageGroup(IDatabasePlugin plugin, Page_Type pageType, DataTable dt = null, string prefix = null)
        {
            Type config_type = plugin.Config_Page;
            if (config_type != null)
            {
                object configButton = plugin.CreateConfigurationButton(dt);
                if (configButton != null)
                {
                    object o = Activator.CreateInstance(config_type);
                    var page = (DatabaseConfigurationPage)o;

                    if (pageType == Page_Type.Client) page.ApplicationType = Application_Type.Client;
                    else page.ApplicationType = Application_Type.Server;

                    page.prefix = prefix;
                    if (dt != null) page.LoadConfiguration(dt);
                    page.SettingChanged += Configuration_Page_SettingChanged;

                    var group = new PageGroup();
                    group.Page = page;
                    group.ButtonContent = configButton;

                    return group;
                }
            }
            return null;
        }

        private void CreatePageButtons(DataTable dt, Page_Type type)
        {
            DatabaseList.Clear();

            List<PageGroup> groups;

            if (type == Page_Type.Client) groups = clientPageGroups;
            else groups = serverPageGroups;

            bool first = true;

            foreach (var group in groups)
            {
                var bt = CreatePageButton(group);
                DatabaseList.Add(bt);

                if (first) bt.IsExpanded = true;
                first = false;
            }
        }

        private CollapseButton CreatePageButton(PageGroup group)
        {
            var item = new Controls.DatabaseItemContainer();
            item.prefix = group.Page.prefix;
            item.ItemContent = group.ButtonContent;
            item.Clicked += item_Clicked;
            item.RemoveClicked += item_RemoveClicked;

            var bt = new CollapseButton();
            item.collapseButton = bt;
            bt.ButtonContent = item;
            bt.PageContent = group.Page;
            return bt;
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
            CreatePageButtons(configurationTable, pageType);

            if (pageType == Page_Type.Client)
            {
                Client_TOGGLE.IsChecked = true;
                Server_TOGGLE.IsChecked = false;
            }
            else
            {
                Client_TOGGLE.IsChecked = false;
                Server_TOGGLE.IsChecked = true;
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
                    List<PageGroup> groups;

                    if (PageType == Page_Type.Client) groups = clientPageGroups;
                    else groups = serverPageGroups;

                    var group = groups.Find(x => x.Page == page);
                    if (group != null) groups.Remove(group);
                }

                // Remove Collapse Button From List
                DatabaseList.Remove(item.collapseButton);
            }
        }

        #endregion

        private void Client_Checked(object sender, RoutedEventArgs e)
        {
            PageType = Page_Type.Client;
        }

        private void Server_Checked(object sender, RoutedEventArgs e)
        {
            PageType = Page_Type.Server;
        }


        public string DatabaseId
        {
            get { return (string)GetValue(DatabaseIdProperty); }
            set { SetValue(DatabaseIdProperty, value); }
        }

        public static readonly DependencyProperty DatabaseIdProperty =
            DependencyProperty.Register("DatabaseId", typeof(string), typeof(Page), new PropertyMetadata(null));


        private void GenerateId_Clicked(TH_WPF.Button bt)
        {
            DatabaseId = Configuration.GenerateDatabaseId();

            if (SettingChanged != null) SettingChanged(null, null, null);
        }

        private void TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (System.Windows.Controls.TextBox)sender;

            if (txt.IsMouseCaptured || txt.IsKeyboardFocused)
            {
                if (SettingChanged != null) SettingChanged(null, null, null);
            }
        }
    }
}
