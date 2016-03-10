using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_Plugins_Server;
using TH_WPF;

using TH_DeviceManager.Controls;

namespace TH_DeviceManager
{
    public partial class DeviceManagerPage
    {

        #region "Properties"

        public object CurrentPage
        {
            get { return (object)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(object), typeof(DeviceManagerPage), new PropertyMetadata(null));

        public bool SaveNeeded
        {
            get { return (bool)GetValue(SaveNeededProperty); }
            set { SetValue(SaveNeededProperty, value); }
        }

        public static readonly DependencyProperty SaveNeededProperty =
            DependencyProperty.Register("SaveNeeded", typeof(bool), typeof(DeviceManagerPage), new PropertyMetadata(false));

        #endregion

        ObservableCollection<object> pagelist;
        public ObservableCollection<object> PageList
        {
            get
            {
                if (pagelist == null)
                    pagelist = new ObservableCollection<object>();
                return pagelist;
            }

            set
            {
                pagelist = value;
            }
        }

        List<ConfigurationPage> ConfigurationPages = new List<ConfigurationPage>();

        int selectedPageIndex = 0;

        //Pages.Description.Page descriptionPage;

        public void InitializePages()
        {
            var pages = CreatePages();

            AddPages(pages);

            ConfigurationPages = pages;
        }

        private List<ConfigurationPage> CreatePages()
        {
            var result = new List<ConfigurationPage>();

            // Description
            result.Add(new Pages.Description.Page());

            // Agent
            result.Add(new Pages.Agent.Page());

            // Databases
            result.Add(new Pages.Databases.Page());

            var types = GetPluginPageTypes();

            var pluginPages = GetPluginPages(types);

            // Load configuration pages from plugins
            if (pluginPages != null)
            {
                result.AddRange(pluginPages);
            }

            return result;
        }

        private List<ConfigurationPage> GetPluginPages(List<Type> pageTypes)
        {
            var result = new List<ConfigurationPage>();

            foreach (var type in pageTypes)
            {
                object o = Activator.CreateInstance(type);

                var page = (ConfigurationPage)o;
                result.Add(page);
            }

            return result;
        }

        //private void CreatePages()
        //{
        //    ConfigurationPages.Clear();

        //    // Description
        //    ConfigurationPages.Add(new Pages.Description.Page());

        //    // Agent
        //    ConfigurationPages.Add(new Pages.Agent.Page());

        //    // Databases
        //    ConfigurationPages.Add(new Pages.Databases.Page());

        //    // Load configuration pages from plugins
        //    if (Plugins != null)
        //    {
        //        List<ConfigurationPage> pages = GetConfigurationPages(Plugins);
        //        ConfigurationPages.AddRange(pages);
        //    }
        //}

        private void AddPages(List<ConfigurationPage> pages)
        {
            //Create PageItem and add to PageList
            foreach (ConfigurationPage page in pages)
            {
                this.Dispatcher.BeginInvoke(new Action(() => {

                    AddPageButton(page);

                    page.SettingChanged += page_SettingChanged;

                }));
            }

            // Select the first page
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (PageList.Count > 0)
                {
                    if (PageList.Count > selectedPageIndex) Page_Selected((ListButton)PageList[selectedPageIndex]);
                    else Page_Selected((ListButton)PageList[0]);
                }
            }));
        }

        //void InitializePages(DeviceManagerType type)
        //{
        //    CurrentPage = null;

        //    SelectedManagerType = type;

        //    PageList.Clear();
        //    ConfigurationPages.Clear();

        //    // Description
        //    var descriptionPage = new Pages.Description.Page();
        //    ConfigurationPages.Add(descriptionPage);

        //    // Agent
        //    if (type == DeviceManagerType.Server) ConfigurationPages.Add(new Pages.Agent.Page());

        //    // Databases
        //    ConfigurationPages.Add(new Pages.Databases.Page());

        //    // Load configuration pages from plugins
        //    if (Plugins != null)
        //    {
        //        if (type == DeviceManagerType.Server) ConfigurationPages.AddRange(AddConfigurationPageButtons(Plugins));
        //    }

        //    // Create PageItem and add to PageList
        //    foreach (ConfigurationPage page in ConfigurationPages)
        //    {
        //        if (type == DeviceManagerType.Client) page.PageType = TH_Plugins_Server.Page_Type.Client;
        //        else page.PageType = TH_Plugins_Server.Page_Type.Server;

        //        //AddPageButton(page);

        //        this.Dispatcher.BeginInvoke(new Action<ConfigurationPage>(AddPageButton), PRIORITY_BACKGROUND, new object[] { page });
        //    }

        //    // Select the first page
        //    if (PageList.Count > 0)
        //    {
        //        if (PageList.Count > selectedPageIndex) Page_Selected((ListButton)PageList[selectedPageIndex]);
        //        else Page_Selected((ListButton)PageList[0]);
        //    }
        //}

        List<ConfigurationPage> GetConfigurationPages(List<IServerPlugin> plugins)
        {
            List<ConfigurationPage> result = new List<ConfigurationPage>();

            foreach (IServerPlugin plugin in plugins)
            {
                try
                {
                    Type config_type = plugin.Config_Page;
                    if (config_type != null)
                    {
                        object o = Activator.CreateInstance(config_type);
                        var page = (ConfigurationPage)o;

                        page.SettingChanged += page_SettingChanged;

                        result.Add(page);
                    }
                }
                catch (Exception ex) { Logger.Log("AddConfigurationPageButtons() :: Exception :: " + ex.Message); }
            }

            return result;
        }

        ConfigurationPage GetConfigurationPage(IServerPlugin tp)
        {
            ConfigurationPage result = null;

            if (tp != null)
            {
                Type config_type = tp.Config_Page;
                object o = Activator.CreateInstance(config_type);
                var page = (ConfigurationPage)o;

                page.SettingChanged += page_SettingChanged;

                return page;

                //PageItem item = new PageItem();
                //item.Text = page.PageName;

                //if (page.Image != null) item.Image = page.Image;
                //else item.Image = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Plug_01.png"));

                //PageList.Add(item);
            }

            return result;
        }




        void ShowClientPages()
        {
            //InitializePages(DeviceManagerType.Client);
            //LoadConfiguration();
        }

        void ShowServerPages()
        {
            //InitializePages(DeviceManagerType.Server);
            //LoadConfiguration();
        }

        private void ShowClient_Checked(object sender, RoutedEventArgs e)
        {
            ShowClientPages();
        }

        private void ShowServer_Checked(object sender, RoutedEventArgs e)
        {
            ShowServerPages();
        }

        void AddPageButton(ConfigurationPage page)
        {
            var bt = new ListButton();
            bt.Text = page.PageName;

            if (page.Image != null) bt.Image = page.Image;
            else bt.Image = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Plug_01.png"));

            bt.Selected += Page_Selected;
            bt.DataObject = page;

            //page.SettingChanged += page_SettingChanged;

            //PageItem item = new PageItem();
            //item.Text = page.PageName;
            //item.Clicked += item_Clicked;

            //if (page.Image != null) item.Image = page.Image;
            //else item.Image = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Plug_01.png"));

            //ListButton bt = new ListButton();
            //bt.ButtonContent = item;
            //bt.Selected += Page_Selected;
            //bt.DataObject = page;

            //item.Parent = bt;

            PageList.Add(bt);
        }


        void item_Clicked(PageItem item)
        {
            if (item.Parent != null)
            {
                if (item.Parent.GetType() == typeof(ListButton))
                {
                    ListButton lb = (ListButton)item.Parent;
                    Page_Selected(lb);
                }
            }
        }

        void Page_Selected(ListButton lb)
        {
            foreach (ListButton olb in PageList) if (olb != lb) olb.IsSelected = false;
            lb.IsSelected = true;

            selectedPageIndex = PageList.IndexOf(lb);

            if (lb.DataObject != null)
            {
                if (CurrentPage != null)
                {
                    //if (CurrentPage.GetType() != lb.DataObject.GetType())
                    //{
                    CurrentPage = lb.DataObject;
                    //}
                }
                else CurrentPage = lb.DataObject;
            }
        }


        


        void page_SettingChanged(string name, string oldVal, string newVal)
        {
            SaveNeeded = true;
        }

        private void Restore_Clicked(TH_WPF.Button bt)
        {
            //SelectDevice(SelectedDevice);
            LoadConfiguration();
        }


        void PageItem_Clicked(object data)
        {
            if (data != null)
            {
                if (CurrentPage != null)
                {
                    if (CurrentPage.GetType() != data.GetType())
                    {
                        CurrentPage = data;
                    }
                }
                else CurrentPage = data;
            }
        }

        
    }
}
