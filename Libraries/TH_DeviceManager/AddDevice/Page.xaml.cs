using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

using System.Threading;
using System.Net;
using TH_Global;
using TH_Global.Functions;
using TH_MTConnect.Components;
using TH_UserManagement.Management;

namespace TH_DeviceManager.AddDevice
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl, IPage
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Title { get { return "Add Device"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Add_01.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }



        public DeviceManagerList ParentManager { get; set; }

        Pages.AutoDetect autoDetectPage;
        Pages.Manual manualPage;
        Pages.LoadFromFile loadFromFilePage;


        public object PageContent
        {
            get { return (object)GetValue(PageContentProperty); }
            set { SetValue(PageContentProperty, value); }
        }

        public static readonly DependencyProperty PageContentProperty =
            DependencyProperty.Register("PageContent", typeof(object), typeof(Page), new PropertyMetadata(null));

        public void ShowAutoDetect()
        {
            if (autoDetectPage == null)
            {
                autoDetectPage = new Pages.AutoDetect();
                autoDetectPage.ParentPage = this;
                autoDetectPage.LoadCatalog();
            }

            PageContent = autoDetectPage;
        }

        public void ShowManual()
        {
            if (manualPage == null)
            {
                manualPage = new Pages.Manual();
                manualPage.ParentPage = this;
                manualPage.LoadCatalog();
            }

            PageContent = manualPage;
        }

        public void ShowLoadFromFile()
        {
            if (loadFromFilePage == null)
            {
                loadFromFilePage = new Pages.LoadFromFile();
                loadFromFilePage.ParentPage = this;
            }

            PageContent = loadFromFilePage;

            //LoadDeviceFromFile();
        }

        public void ShowCreateNew()
        {


        }

        private void AutoDetect_Clicked(TH_DeviceManager.Controls.PageItem item) { ShowAutoDetect(); }


        private void Manual_Clicked(TH_DeviceManager.Controls.PageItem item) { ShowManual(); }

        private void LoadFromFile_Clicked(TH_DeviceManager.Controls.PageItem item) { ShowLoadFromFile(); }

        private void CreateNew_Clicked(TH_DeviceManager.Controls.PageItem item) { ShowCreateNew(); }
    }
}
