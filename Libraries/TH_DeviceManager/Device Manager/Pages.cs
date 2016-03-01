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
    public partial class DeviceManager
    {

        public object CurrentPage
        {
            get { return (object)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(object), typeof(DeviceManager), new PropertyMetadata(null));


        public bool PageListOptionsShown
        {
            get { return (bool)GetValue(PageListOptionsShownProperty); }
            set { SetValue(PageListOptionsShownProperty, value); }
        }

        public static readonly DependencyProperty PageListOptionsShownProperty =
            DependencyProperty.Register("PageListOptionsShown", typeof(bool), typeof(DeviceManager), new PropertyMetadata(true));



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

        Pages.Description.Page descriptionPage;

        void InitializePages(DeviceManagerType type)
        {
            CurrentPage = null;

            SelectedManagerType = type;

            PageList.Clear();
            ConfigurationPages.Clear();

            // Description
            descriptionPage = new Pages.Description.Page();
            ConfigurationPages.Add(descriptionPage);

            // Agent
            if (type == DeviceManagerType.Server) ConfigurationPages.Add(new Pages.Agent.Page());

            // Databases
            ConfigurationPages.Add(new Pages.Databases.Page());

            // Load configuration pages from plugins
            if (Plugins != null)
            {
                if (type == DeviceManagerType.Server) ConfigurationPages.AddRange(AddConfigurationPageButtons(Plugins));
            }

            // Create PageItem and add to PageList
            foreach (ConfigurationPage page in ConfigurationPages)
            {
                if (type == DeviceManagerType.Client) page.PageType = TH_Plugins_Server.Page_Type.Client;
                else page.PageType = TH_Plugins_Server.Page_Type.Server;

                this.Dispatcher.BeginInvoke(new Action<ConfigurationPage>(AddPageButton), priority, new object[] { page });
            }

            // Select the first page
            if (PageList.Count > 0)
            {
                if (PageList.Count > selectedPageIndex) Page_Selected((ListButton)PageList[selectedPageIndex]);
                else Page_Selected((ListButton)PageList[0]);
            }
        }

        void ShowClientPages()
        {
            InitializePages(DeviceManagerType.Client);
            LoadConfiguration();
        }

        void ShowServerPages()
        {
            InitializePages(DeviceManagerType.Server);
            LoadConfiguration();
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
            page.SettingChanged += page_SettingChanged;

            PageItem item = new PageItem();
            item.Text = page.PageName;
            item.Clicked += item_Clicked;

            if (page.Image != null) item.Image = page.Image;
            else item.Image = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Plug_01.png"));

            ListButton bt = new ListButton();
            bt.ButtonContent = item;
            bt.ShowImage = false;
            bt.Selected += Page_Selected;
            bt.DataObject = page;
            bt.Height = 100;
            bt.Width = 100;
            bt.MinWidth = 100;

            item.Parent = bt;

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


        public bool SaveNeeded
        {
            get { return (bool)GetValue(SaveNeededProperty); }
            set { SetValue(SaveNeededProperty, value); }
        }

        public static readonly DependencyProperty SaveNeededProperty =
            DependencyProperty.Register("SaveNeeded", typeof(bool), typeof(DeviceManager), new PropertyMetadata(false));


        void page_SettingChanged(string name, string oldVal, string newVal)
        {
            SaveNeeded = true;
        }

        private void Restore_Clicked(TH_WPF.Button bt)
        {
            SelectDevice(SelectedDevice);
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

                ToolbarShown = true;
            }
        }

        List<ConfigurationPage> AddConfigurationPageButtons(List<IServerPlugin> plugins)
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

                        ConfigurationPage page = (ConfigurationPage)o;

                        result.Add(page);
                    }
                }
                catch (Exception ex) { Logger.Log("AddConfigurationPageButtons() :: Exception :: " + ex.Message); }
            }

            return result;
        }

        void AddConfigurationPageButton(IServerPlugin tp)
        {
            if (tp != null)
            {
                Type config_type = tp.Config_Page;
                object o = Activator.CreateInstance(config_type);
                ConfigurationPage page = (ConfigurationPage)o;

                PageItem item = new PageItem();
                item.Text = page.PageName;

                if (page.Image != null) item.Image = page.Image;
                else item.Image = new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Plug_01.png"));

                PageList.Add(item);
            }
        }

    }
}
