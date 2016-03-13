using System;
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

using System.Collections.ObjectModel;
using System.Threading;
using System.Data;
using System.Xml;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement.Management;

namespace TH_DeviceManager.AddDevice.Pages
{
    /// <summary>
    /// Interaction logic for Manual.xaml
    /// </summary>
    public partial class Manual : UserControl, IPage
    {
        public Manual()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "Properties"

        public string Title { get { return "Manual"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Edit_02.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }


        public Page ParentPage { get; set; }

        #endregion


        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;


        #region "Catalog"

        public bool CatalogLoading
        {
            get { return (bool)GetValue(CatalogLoadingProperty); }
            set { SetValue(CatalogLoadingProperty, value); }
        }

        public static readonly DependencyProperty CatalogLoadingProperty =
            DependencyProperty.Register("CatalogLoading", typeof(bool), typeof(Manual), new PropertyMetadata(false));

        Thread LoadCatalog_THREAD;

        public void LoadCatalog()
        {
            if (ParentPage != null && ParentPage.ParentManager != null)
            {
                CatalogLoading = true;
                SharedList.Clear();

                if (LoadCatalog_THREAD != null) LoadCatalog_THREAD.Abort();

                LoadCatalog_THREAD = new Thread(new ThreadStart(LoadCatalog_Worker));
                LoadCatalog_THREAD.Start();
            }
        }

        void LoadCatalog_Worker()
        {
            List<Shared.SharedListItem> items = Shared.GetSharedList();

            this.Dispatcher.BeginInvoke(new Action<List<Shared.SharedListItem>>(LoadCatalog_GUI), PRIORITY_BACKGROUND, new object[] { items });
        }

        void LoadCatalog_GUI(List<Shared.SharedListItem> items)
        {
            CatalogLoading = false;

            shareditems = items;

            LoadSharedItems(items);
        }


        #endregion

        List<Shared.SharedListItem> shareditems;
        List<Shared.SharedListItem> shareditems_search;

        #region "Shared Items"

        ObservableCollection<Controls.SharedItem> sharedlist;
        public ObservableCollection<Controls.SharedItem> SharedList
        {
            get
            {
                if (sharedlist == null)
                    sharedlist = new ObservableCollection<Controls.SharedItem>();
                return sharedlist;
            }

            set
            {
                sharedlist = value;
            }
        }

        Thread LoadShared_THREAD;

        void LoadSharedItems(List<Shared.SharedListItem> items)
        {
            CatalogLoading = true;

            shareditems_search = new List<Shared.SharedListItem>();
            SharedList.Clear();

            //items = items.OrderByDescending(x => x.downloads).ToList();

            //foreach (Shared.SharedListItem item in items)
            //{
            //    LoadSharedItems_GUI(item, null);

            //    //ThreadPool.QueueUserWorkItem(new WaitCallback(LoadSharedItems_Worker2), item);
            //}

            //LoadSharedItems_Finish();

            if (LoadShared_THREAD != null) LoadShared_THREAD.Abort();

            LoadShared_THREAD = new Thread(new ParameterizedThreadStart(LoadSharedItems_Worker));
            LoadShared_THREAD.Start(items);
        }

        void LoadSharedItems_Worker2(object o)
        {
            if (o != null)
            {
                Shared.SharedListItem item = (Shared.SharedListItem)o;

                System.Drawing.Image img = null;

                // Set Image --------------------------------------------
                if (item.image_url != null)
                {
                    // Just use Remote.Images (don't look for local)
                    img = TH_UserManagement.Management.Remote.Images.GetImage(item.image_url);
                }
                // ------------------------------------------------------

                this.Dispatcher.BeginInvoke(new Action<Shared.SharedListItem, System.Drawing.Image>(LoadSharedItems_GUI), PRIORITY_BACKGROUND, new object[] { item, img });
            }
        }

        void LoadSharedItems_Worker(object o)
        {
            if (o != null)
            {
                List<Shared.SharedListItem> items = (List<Shared.SharedListItem>)o;

                items = items.OrderByDescending(x => x.downloads).ToList();

                foreach (Shared.SharedListItem item in items)
                {
                    System.Drawing.Image img = null;

                    // Set Image --------------------------------------------
                    if (item.image_url != null)
                    {
                        // Just use Remote.Images (don't look for local)
                        img = TH_UserManagement.Management.Remote.Images.GetImage(item.image_url);
                    }
                    // ------------------------------------------------------

                    this.Dispatcher.BeginInvoke(new Action<Shared.SharedListItem, System.Drawing.Image>(LoadSharedItems_GUI), PRIORITY_BACKGROUND, new object[] { item, img });
                }
            }

            this.Dispatcher.BeginInvoke(new Action(LoadSharedItems_Finish), PRIORITY_BACKGROUND, new object[] { });
        }

        void LoadSharedItems_GUI(Shared.SharedListItem listitem, System.Drawing.Image img)
        {
            shareditems_search.Add(listitem);

            Controls.SharedItem item = new Controls.SharedItem();

            BitmapImage bitmap = null;

            if (img != null)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                IntPtr bmpPt = bmp.GetHbitmap();
                BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                bmpSource.Freeze();

                item.FullSizeImage = bmpSource;

                if (bmpSource.PixelWidth > bmpSource.PixelHeight)
                {
                    bitmap = TH_WPF.Image_Functions.SetImageSize(bmpSource, 60);
                }
                else
                {
                    bitmap = TH_WPF.Image_Functions.SetImageSize(bmpSource, 0, 30);
                }
            }

            item.Image = bitmap;

            if (ParentPage.ParentManager.CurrentUser != null)
            {
                if (ParentPage.ParentManager.CurrentUser.username.ToLower() == listitem.author.ToLower()) item.Owner = true;
            }

            item.Description = listitem.description;
            item.Manufacturer = listitem.manufacturer;
            item.DeviceType = listitem.device_type;
            item.Model = listitem.model;
            item.Controller = listitem.controller;

            item.listitem = listitem;

            item.AddClicked += item_AddClicked;
            item.Clicked += item_Clicked;

            SharedList.Add(item);
        }

        void item_Clicked(Controls.SharedItem item)
        {
            LoadDetails(item);
        }

        void item_AddClicked(Controls.SharedItem item)
        {
            if (item.listitem != null)
            {
                AddSharedItem(item);
            }
        }

        void LoadSharedItems_Finish()
        {
            CatalogLoading = false;
        }

        #endregion

        #region "Catalog Search"

        private void Refresh_Clicked(TH_WPF.Button bt)
        {
            LoadCatalog();
        }


        System.Timers.Timer search_TIMER;

        private void search_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (search_TIMER != null) search_TIMER.Enabled = false;

            search_TIMER = new System.Timers.Timer();
            search_TIMER.Interval = 500;
            search_TIMER.Elapsed += search_TIMER_Elapsed;
            search_TIMER.Enabled = true;
        }

        void search_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            search_TIMER.Enabled = false;

            this.Dispatcher.BeginInvoke(new Action(FilterCatalog));
        }

        void FilterCatalog()
        {
            string search = search_TXT.Text.ToLower();

            if (search != String.Empty)
            {
                string[] searchList = search.Split(' ');

                List<Shared.SharedListItem> results = new List<Shared.SharedListItem>();

                foreach (string s in searchList)
                {
                    List<Shared.SharedListItem> list = shareditems_search.FindAll(
                    x =>
                    (TestFilter(x.manufacturer, s) ||
                    TestFilter(x.model, s) ||
                    TestFilter(x.author, s) ||
                    TestFilter(x.description, s) ||
                    TestFilter(x.controller, s) ||
                    TestFilter(x.device_type, s))
                    );

                    foreach (Shared.SharedListItem item in list)
                    {
                        if (results.Find(x => x.list_id == item.list_id) == null) results.Add(item);
                    }
                }

                LoadSharedItems(results);

            }
            else
            {
                LoadSharedItems(shareditems);
            }

        }

        static bool TestFilter(string s, string test)
        {
            if (s != null && test != null)
            {
                if (s.ToLower().Contains(test)) return true;
                if (s.Contains(test)) return true;
            }
            return false;
        }

        #endregion

        #region "Details"

        Shared.SharedListItem selectedItem = null;

        #region "Properties"

        public bool DetailsShown
        {
            get { return (bool)GetValue(DetailsShownProperty); }
            set { SetValue(DetailsShownProperty, value); }
        }

        public static readonly DependencyProperty DetailsShownProperty =
            DependencyProperty.Register("DetailsShown", typeof(bool), typeof(Manual), new PropertyMetadata(false));



        public ImageSource Details_Image
        {
            get { return (ImageSource)GetValue(Details_ImageProperty); }
            set { SetValue(Details_ImageProperty, value); }
        }

        public static readonly DependencyProperty Details_ImageProperty =
            DependencyProperty.Register("Details_Image", typeof(ImageSource), typeof(Manual), new PropertyMetadata(null));


        public bool Details_Owner
        {
            get { return (bool)GetValue(Details_OwnerProperty); }
            set { SetValue(Details_OwnerProperty, value); }
        }

        public static readonly DependencyProperty Details_OwnerProperty =
            DependencyProperty.Register("Details_Owner", typeof(bool), typeof(Manual), new PropertyMetadata(false));


        public string Details_Author
        {
            get { return (string)GetValue(Details_AuthorProperty); }
            set { SetValue(Details_AuthorProperty, value); }
        }

        public static readonly DependencyProperty Details_AuthorProperty =
            DependencyProperty.Register("Details_Author", typeof(string), typeof(Manual), new PropertyMetadata(null));


        public string Details_LastUpdated
        {
            get { return (string)GetValue(Details_LastUpdatedProperty); }
            set { SetValue(Details_LastUpdatedProperty, value); }
        }

        public static readonly DependencyProperty Details_LastUpdatedProperty =
            DependencyProperty.Register("Details_LastUpdated", typeof(string), typeof(Manual), new PropertyMetadata(null));




        public string Details_Description
        {
            get { return (string)GetValue(Details_DescriptionProperty); }
            set { SetValue(Details_DescriptionProperty, value); }
        }

        public static readonly DependencyProperty Details_DescriptionProperty =
            DependencyProperty.Register("Details_Description", typeof(string), typeof(Manual), new PropertyMetadata(null));



        public string Details_Manufacturer
        {
            get { return (string)GetValue(Details_ManufacturerProperty); }
            set { SetValue(Details_ManufacturerProperty, value); }
        }

        public static readonly DependencyProperty Details_ManufacturerProperty =
            DependencyProperty.Register("Details_Manufacturer", typeof(string), typeof(Manual), new PropertyMetadata(null));


        public string Details_DeviceType
        {
            get { return (string)GetValue(Details_DeviceTypeProperty); }
            set { SetValue(Details_DeviceTypeProperty, value); }
        }

        public static readonly DependencyProperty Details_DeviceTypeProperty =
            DependencyProperty.Register("Details_DeviceType", typeof(string), typeof(Manual), new PropertyMetadata(null));


        public string Details_Model
        {
            get { return (string)GetValue(Details_ModelProperty); }
            set { SetValue(Details_ModelProperty, value); }
        }

        public static readonly DependencyProperty Details_ModelProperty =
            DependencyProperty.Register("Details_Model", typeof(string), typeof(Manual), new PropertyMetadata(null));


        public string Details_Controller
        {
            get { return (string)GetValue(Details_ControllerProperty); }
            set { SetValue(Details_ControllerProperty, value); }
        }

        public static readonly DependencyProperty Details_ControllerProperty =
            DependencyProperty.Register("Details_Controller", typeof(string), typeof(Manual), new PropertyMetadata(null));



        public string Details_Dependencies
        {
            get { return (string)GetValue(Details_DependenciesProperty); }
            set { SetValue(Details_DependenciesProperty, value); }
        }

        public static readonly DependencyProperty Details_DependenciesProperty =
            DependencyProperty.Register("Details_Dependencies", typeof(string), typeof(Manual), new PropertyMetadata(null));



        public string Details_Tags
        {
            get { return (string)GetValue(Details_TagsProperty); }
            set { SetValue(Details_TagsProperty, value); }
        }

        public static readonly DependencyProperty Details_TagsProperty =
            DependencyProperty.Register("Details_Tags", typeof(string), typeof(Manual), new PropertyMetadata(null));


        #endregion


        void LoadDetails(Controls.SharedItem item)
        {
            if (item.listitem != null)
            {
                Shared.SharedListItem i = item.listitem;

                selectedItem = i;

                // Image
                ImageSource img = item.FullSizeImage;

                if (img != null)
                {
                    if (img.Width > img.Height)
                    {
                        int width = Convert.ToInt32(img.Width);
                        Details_Image = TH_WPF.Image_Functions.SetImageSize(img, Math.Min(width, 200));
                    }
                    else
                    {
                        int height = Convert.ToInt32(img.Height);
                        Details_Image = TH_WPF.Image_Functions.SetImageSize(img, 0, Math.Min(height, 75));
                    }
                }
                else Details_Image = null;


                Details_Owner = item.Owner;

                // Author
                Details_Author = i.author;
                Details_LastUpdated = i.upload_date.ToString();

                Details_Description = i.description;

                Details_Manufacturer = i.manufacturer;
                Details_DeviceType = i.device_type;
                Details_Model = i.model;
                Details_Controller = i.controller;

                Details_Dependencies = i.dependencies;
                Details_Tags = i.tags;

                DetailsShown = true;
            }
        }


        private void Remove_Clicked(TH_WPF.Button bt)
        {
            if (selectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("Remove this Configuration from the Shared Catalog?", "Remove Configuration", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.Yes)
                {
                    this.Cursor = Cursors.Wait;

                    if (Shared.RemoveSharedConfiguration_FromList(selectedItem))
                    {
                        Configurations.RemoveConfigurationTable(selectedItem.tablename);
                    }

                    DetailsShown = false;

                    LoadCatalog();

                    this.Cursor = Cursors.Arrow;
                }
            }
        }


        #endregion

        #region "Add Shared Configuration to User"

        class AddShared_Return
        {
            public Controls.SharedItem item { get; set; }
            public Configuration config { get; set; }
            public bool success { get; set; }
        }

        void AddSharedItem(Controls.SharedItem item)
        {
            item.Loading = true;

            ThreadPool.QueueUserWorkItem(new WaitCallback(AddSharedItem_Worker), item);
        }

        void AddSharedItem_Worker(object o)
        {
            AddShared_Return result = new AddShared_Return();
            result.success = false;

            if (o != null)
            {
                Controls.SharedItem item = (Controls.SharedItem)o;

                result.item = item;

                if (item.listitem != null)
                {
                    string tablename = item.listitem.tablename;

                    if (tablename != null)
                    {
                        Shared.UpdateDownloads(item.listitem);

                        DataTable dt = TH_UserManagement.Management.Remote.Configurations.GetTable(tablename);
                        if (dt != null)
                        {
                            XmlDocument xml = Converter.TableToXML(dt);
                            if (xml != null)
                            {
                                Configuration config = Configuration.Read(xml);
                                if (config != null)
                                {
                                    result.config = config;

                                    if (TH_UserManagement.Management.UserManagementSettings.Database != null)
                                    {
                                        SaveLocalImage(config.FileLocations.Manufacturer_Logo_Path);
                                        SaveLocalImage(config.FileLocations.Image_Path);
                                    }

                                    if (ParentPage.ParentManager.CurrentUser != null)
                                    {
                                        result.success = Configurations.AddConfigurationToUser(ParentPage.ParentManager.CurrentUser, config);

                                        result.config.TableName = config.TableName;
                                    }
                                    else
                                    {
                                        result.success = SaveLocalConfigurationToUser(config);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action<AddShared_Return>(AddSharedItem_GUI), PRIORITY_BACKGROUND, new object[] { result });
        }

        private bool SaveLocalConfigurationToUser(Configuration config)
        {
            bool result = false;

            // Set new Unique Id
            string uniqueId = String_Functions.RandomString(20);
            config.UniqueId = uniqueId;
            XML_Functions.SetInnerText(config.ConfigurationXML, "UniqueId", uniqueId);

            config.ClientEnabled = false;
            XML_Functions.SetInnerText(config.ConfigurationXML, "/ClientEnabled", "False");

            config.ServerEnabled = false;
            XML_Functions.SetInnerText(config.ConfigurationXML, "/ServerEnabled", "False");

            try
            {
                string localPath = FileLocations.Devices + "\\" + uniqueId + ".xml";

                config.ConfigurationXML.Save(localPath);

                result = true;
            }
            catch (Exception ex)
            {
                Logger.Log("SaveLocalConfiguartionToUser() :: Exception :: " + ex.Message);
            }

            return result;
        }


        void SaveLocalImage(string filename)
        {
            if (filename != null)
            {
                var image = TH_UserManagement.Management.Remote.Images.GetImage(filename);
                if (image != null)
                {
                    string savePath = FileLocations.TrakHoundTemp + "\\" + filename;

                    image.Save(savePath);
                }
            }
        }

        void AddSharedItem_GUI(AddShared_Return result)
        {
            if (result.item != null) result.item.Loading = false;

            if (result.success && result.config != null)
            {
                ParentPage.ParentManager.AddDevice(result.config);
                //if (DeviceAdded != null) DeviceAdded(result.config);
            }
            else
            {
                TH_WPF.MessageBox.Show("Add device failed. Try Again.", "Add device failed", TH_WPF.MessageBoxButtons.Ok);
            }
        }

        #endregion

    }
}
