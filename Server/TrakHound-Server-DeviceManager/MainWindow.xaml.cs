using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

using TH_DeviceManager;
using TH_Global;
using TH_Global.TrakHound.Users;
using TH_WPF;

namespace TrakHound_Server_DeviceManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            deviceManager = new DeviceManager();
            deviceManager.LoadDevices();

            StartLoginMonitor();

            DeviceList_Initialize();
        }


        private DeviceManager deviceManager;
        private DeviceList deviceList;


        private void DeviceList_Initialize()
        {
            deviceList = new DeviceList();
            deviceList.DeviceManager = deviceManager;
            deviceList.AddDeviceSelected += DeviceList_AddDeviceSelected;
            deviceList.EditSelected += DeviceList_EditSelected;

            AddPage(deviceList);
        }

        private void DeviceList_AddDeviceSelected()
        {
            var addDevicePage = new TH_DeviceManager.AddDevice.Page();

            addDevicePage.DeviceManager = deviceManager;
            addDevicePage.ShowAutoDetect();

            addDevicePage.DeviceListSelected += AddDevicePage_DeviceListSelected;

            AddPage(addDevicePage);
        }

        private void AddDevicePage_DeviceListSelected()
        {
            ListButton_Selected(Pages[0]);
        }

        private void DeviceList_EditSelected(TH_Configuration.Configuration config)
        {
            var editPage = new EditPage(config);
            AddPage(editPage, "Edit Device - " + config.Description.Description);
        }


        ObservableCollection<ListButton> pages;
        public ObservableCollection<ListButton> Pages
        {
            get
            {
                if (pages == null)
                    pages = new ObservableCollection<ListButton>();
                return pages;
            }

            set
            {
                pages = value;
            }
        }

        public IPage PageContent
        {
            get { return (IPage)GetValue(PageContentProperty); }
            set { SetValue(PageContentProperty, value); }
        }

        public static readonly DependencyProperty PageContentProperty =
            DependencyProperty.Register("PageContent", typeof(IPage), typeof(MainWindow), new PropertyMetadata(null));

        public void AddPage(IPage page, string title = null, string image = null)
        {
            var lb = new ListButton();

            if (title == null) title = page.Title;

            var bt = new Controls.PageButton();
            bt.Title = title;
            bt.Image = page.Image;
            bt.ParentButton = lb;
            bt.Clicked += Bt_Clicked;
            bt.CloseClicked += Bt_CloseClicked;
            lb.ButtonContent = bt;

            lb.Selected += ListButton_Selected;
            lb.DataObject = page;

            Pages.Add(lb);
            ListButton_Selected(lb);
        }

        private void Bt_Clicked(ListButton bt)
        {
            ListButton_Selected(bt);
        }

        private void Bt_CloseClicked(ListButton bt)
        {
            if (bt.IsSelected) PageContent = null;
            Pages.Remove(bt);
        }


        public void RemovePage(IPage page)
        {
            int index = Pages.ToList().FindIndex(x => ((Controls.PageButton)x.ButtonContent).Title.ToUpper() == page.Title.ToUpper());
            if (index >= 0) Pages.RemoveAt(index);

            PageContent = null;
        }

        private void ListButton_Selected(ListButton lb)
        {
            foreach (ListButton oLB in Pages)
            {
                if (oLB == lb) oLB.IsSelected = true;
                else oLB.IsSelected = false;
            }

            var page = lb.DataObject as IPage;
            PageContent = page;
        }

        private void pagemanager_Loaded(object sender, RoutedEventArgs e)
        {
            if (Pages.Count > 0 && PageContent == null)
            {
                ListButton lb = Pages[0];

                foreach (ListButton oLB in Pages)
                {
                    if (oLB == lb) oLB.IsSelected = true;
                    else oLB.IsSelected = false;
                }

                var page = lb.DataObject as IPage;
                PageContent = page;
            }
        }


        private void StartLoginMonitor()
        {
            string path = FileLocations.AppData + @"\nigolresu";
            if (File.Exists(path))
            {
                string dir = Path.GetDirectoryName(path);

                var watcher = new FileSystemWatcher(dir);
                watcher.Changed += FileSystemWatcher_UserLogin_Changed;
                watcher.Created += FileSystemWatcher_UserLogin_Changed;
                watcher.Deleted += FileSystemWatcher_UserLogin_Changed;
                watcher.EnableRaisingEvents = true;
            }
        }

        private void FileSystemWatcher_UserLogin_Changed(object sender, FileSystemEventArgs e)
        {
            Login();
        }

        private void Login()
        {
            UserLoginFile.LoginData loginData = UserLoginFile.Read();
            if (loginData != null)
            {
                deviceManager.CurrentUser = UserManagement.TokenLogin(loginData.Token);
            }
        }
    }
}
