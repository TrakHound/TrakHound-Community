// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

using TH_Configuration;
using TH_Global;
using TH_Global.Functions;
using TH_UserManagement.Management;

namespace TH_DeviceManager.AddDevice.Pages
{
    /// <summary>
    /// Page containing options for manually adding Devices from the Online TrakHound Device Catalog
    /// </summary>
    public partial class Manual : UserControl, IPage
    {
        public Manual()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "IPage"

        public string Title { get { return "Manual"; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_DeviceManager;component/Resources/Edit_02.png")); } }


        public void Opened() { }
        public bool Opening() { return true; }

        public void Closed() { }
        public bool Closing() { return true; }

        #endregion

        /// <summary>
        /// Parent AddDevice.Page object
        /// </summary>
        public Page ParentPage { get; set; }

        #region "Dependency Properties"

        /// <summary>
        /// Used to tell whether the Device Catalog is currently being loaded
        /// </summary>
        public bool CatalogLoading
        {
            get { return (bool)GetValue(CatalogLoadingProperty); }
            set { SetValue(CatalogLoadingProperty, value); }
        }

        public static readonly DependencyProperty CatalogLoadingProperty =
            DependencyProperty.Register("CatalogLoading", typeof(bool), typeof(Manual), new PropertyMetadata(false));

        /// <summary>
        /// Contains the selected CatalogItem to display in the Details panel
        /// </summary>
        public Controls.CatalogItem SelectedCatalogItem
        {
            get { return (Controls.CatalogItem)GetValue(SelectedCatalogItemProperty); }
            set { SetValue(SelectedCatalogItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedCatalogItemProperty =
            DependencyProperty.Register("SelectedCatalogItem", typeof(Controls.CatalogItem), typeof(Manual), new PropertyMetadata(null));

        #endregion

        const System.Windows.Threading.DispatcherPriority PRIORITY_BACKGROUND = System.Windows.Threading.DispatcherPriority.Background;

        #region "Device Catalog"

        private class CatalogInfo
        {
            public Shared.SharedListItem Item { get; set; }
            public System.Drawing.Image Image { get; set; }
        }

        Thread catalog_THREAD;

        /// <summary>
        /// Load the items found in the Online TrakHound Device Catalog
        /// </summary>
        public void LoadCatalog()
        {
            if (ParentPage != null && ParentPage.DeviceManager != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    CatalogLoading = true;
                }
                ), PRIORITY_BACKGROUND, new object[] { });

                if (catalog_THREAD != null) catalog_THREAD.Abort();

                catalog_THREAD = new Thread(new ThreadStart(LoadCatalog_Worker));
                catalog_THREAD.Start();
            }
        }

        private void LoadCatalog_Worker()
        {
            List<Shared.SharedListItem> items = Shared.GetSharedList();

            var infos = new List<CatalogInfo>();

            // Create CatalogInfo objects for each SharedListItem and get the corresponding image
            foreach (var item in items)
            {
                var info = new CatalogInfo();
                info.Item = item;
                info.Image = GetCatalogImage(item);

                infos.Add(info);
            }

            this.Dispatcher.BeginInvoke(new Action<List<CatalogInfo>>(LoadCatalog_GUI), PRIORITY_BACKGROUND, new object[] { infos });
        }

        private static System.Drawing.Image GetCatalogImage(Shared.SharedListItem item)
        {
            System.Drawing.Image img = null;

            if (item.image_url != null)
            {
                // Just use Remote.Images (don't look for local)
                img = TH_UserManagement.Management.Remote.Images.GetImage(item.image_url);
            }

            return img;
        }

        private void LoadCatalog_GUI(List<CatalogInfo> infos)
        {
            CatalogLoading = false;

            catalogInfos = infos.ToList();

            AddCatalogItems(infos);
        }

        #endregion

        #region "Catalog Items"

        /// <summary>
        /// List of All of the Catalog Infos
        /// </summary>
        List<CatalogInfo> catalogInfos;

        /// <summary>
        /// List of filtered (by search bar) Catalog Items
        /// </summary>
        //List<CatalogInfo> filteredCatalogInfos;

        private ObservableCollection<Controls.CatalogItem> _catalogItems;
        /// <summary>
        /// List of Controls representing the Catalog Device Configurations
        /// </summary>
        public ObservableCollection<Controls.CatalogItem> CatalogItems
        {
            get
            {
                if (_catalogItems == null)
                    _catalogItems = new ObservableCollection<Controls.CatalogItem>();
                return _catalogItems;
            }

            set
            {
                _catalogItems = value;
            }
        }

        private void AddCatalogItems(List<CatalogInfo> infos)
        {
            CatalogItems.Clear();

            if (infos != null)
            {
                infos = infos.OrderByDescending(x => x.Item.downloads).ToList();

                foreach (var info in infos)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        AddCatalogItem(info);
                    }
                    ), PRIORITY_BACKGROUND, new object[] { });
                }
            }
        }

        private void AddCatalogItem(CatalogInfo info)
        {
            var item = new Controls.CatalogItem();

            // Set Images
            item.Image = GetSourceFromImage(info.Image, 100, 40);
            item.FullSizeImage = GetSourceFromImage(info.Image, 200, 75);

            // Set Owner
            item.IsOwner = IsOwner(info);
            item.Author = info.Item.author;

            // Set Description properties
            item.Description = info.Item.description;
            item.Manufacturer = info.Item.manufacturer;
            item.DeviceType = info.Item.device_type;
            item.Model = info.Item.model;
            item.Controller = info.Item.controller;
            item.LastUpdated = info.Item.upload_date.ToString();

            // Set parent ListItem
            item.SharedListItem = info.Item;

            // Assign event handlers
            item.Clicked += CatalogItem_Clicked;
            item.AddClicked += CatalogItem_AddClicked;

            // Add item to list
            CatalogItems.Add(item);
        }

        private static ImageSource GetSourceFromImage(System.Drawing.Image img, int width, int height)
        {
            BitmapImage bitmap = null;

            if (img != null)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);

                IntPtr bmpPt = bmp.GetHbitmap();
                BitmapSource bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmpPt, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                bmpSource.Freeze();

                if (bmpSource.PixelWidth > bmpSource.PixelHeight)
                {
                    bitmap = Image_Functions.SetImageSize(bmpSource, width);
                }
                else
                {
                    bitmap = Image_Functions.SetImageSize(bmpSource, 0, height);
                }
            }

            return bitmap;
        }

        private bool IsOwner(CatalogInfo info)
        {
            if (ParentPage.DeviceManager.CurrentUser != null)
            {
                if (ParentPage.DeviceManager.CurrentUser.Username.ToLower() == info.Item.author.ToLower()) return true;
            }
            return false;
        }


        private void CatalogItem_Clicked(Controls.CatalogItem item) { SelectedCatalogItem = item; }

        private void CatalogItem_AddClicked(Controls.CatalogItem item)
        {
            if (item.SharedListItem != null)
            {
                AddDevice(item);
            }
        }

        #endregion

        #region "Catalog Search"

        private void Refresh_Clicked(TH_WPF.Button bt) { LoadCatalog(); }

        System.Timers.Timer search_TIMER;

        private void search_TXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (search_TIMER != null) search_TIMER.Enabled = false;

            search_TIMER = new System.Timers.Timer();
            search_TIMER.Interval = 500;
            search_TIMER.Elapsed += search_TIMER_Elapsed;
            search_TIMER.Enabled = true;
        }

        private void search_TIMER_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            search_TIMER.Enabled = false;

            this.Dispatcher.BeginInvoke(new Action(FilterCatalog));
        }

        private void FilterCatalog()
        {
            // string search = search_TXT.Text.ToLower();
            string search = search_TXT.Text;

            //if (search != String.Empty)
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                string[] searchList = search.Split(' ');

                var filterList = new List<CatalogInfo>();

                foreach (string s in searchList)
                {
                    List<CatalogInfo> items = catalogInfos.FindAll(
                    x =>
                    (TestFilter(x.Item.manufacturer, s) ||
                    TestFilter(x.Item.model, s) ||
                    TestFilter(x.Item.author, s) ||
                    TestFilter(x.Item.description, s) ||
                    TestFilter(x.Item.controller, s) ||
                    TestFilter(x.Item.device_type, s))
                    );

                    foreach (var item in items)
                    {
                        //if (filterList.Find(x => x.Item.list_id == item.Item.list_id) == null) filterList.Add(item);
                        if (!filterList.Exists(x => x.Item.list_id == item.Item.list_id)) filterList.Add(item);
                    }
                }

                AddCatalogItems(filterList);
            }
            else
            {
                AddCatalogItems(catalogInfos);
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

        #region "Add Device"

        private class AddDeviceInfo
        {
            public Controls.CatalogItem CatalogItem { get; set; }
            public Configuration Configuration { get; set; }
            public Shared.SharedListItem SharedListItem { get; set; }
            public bool Success { get; set; }
        }

        private void AddDevice(Controls.CatalogItem item)
        {
            item.Loading = true;

            var info = new AddDeviceInfo();
            info.CatalogItem = item;
            info.SharedListItem = item.SharedListItem;

            ThreadPool.QueueUserWorkItem(new WaitCallback(AddDevice_Worker), info);
        }

        private void AddDevice_Worker(object o)
        {
            if (o != null)
            {
                var item = (AddDeviceInfo)o;

                if (item.SharedListItem != null)
                {
                    string tablename = item.SharedListItem.tablename;

                    if (tablename != null)
                    {
                        // Update (increment up) the number of times the selected Catalog Item has been downloaded
                        Shared.UpdateDownloads(item.SharedListItem);

                        // Retrieve the Configuration Table for the selected Catalog Item
                        DataTable dt = TH_UserManagement.Management.Remote.Configurations.GetTable(tablename);
                        if (dt != null)
                        {
                            // Convert Table an XML file
                            XmlDocument xml = Converter.TableToXML(dt);
                            if (xml != null)
                            {
                                // Process the XML file and get a TH_Configuration.Configuration object
                                Configuration config = Configuration.Read(xml);
                                if (config != null)
                                {
                                    item.Configuration = config;

                                    // If using a Local account (User or 'non user') save the images to PC
                                    if (UserManagementSettings.Database != null)
                                    {
                                        SaveLocalImage(config.FileLocations.Manufacturer_Logo_Path);
                                        SaveLocalImage(config.FileLocations.Image_Path);
                                    }

                                    // Add the configuration to either the DeviceManager's current user or (if not logged in)
                                    // Save to the Devices directory (ex. C:\TrakHound\Devices\)
                                    if (ParentPage.DeviceManager.CurrentUser != null)
                                    {
                                        var userConfig = UserConfiguration.FromNewUserConfiguration(ParentPage.DeviceManager.CurrentUser);

                                        item.Success = Configurations.AddConfigurationToUser(userConfig, config);
                                        item.Configuration.TableName = config.TableName;
                                    }
                                    else
                                    {
                                        item.Success = SaveLocalConfigurationToUser(config);
                                    }
                                }
                            }
                        }
                    }
                }

                Dispatcher.BeginInvoke(new Action<AddDeviceInfo>(AddDevice_GUI), PRIORITY_BACKGROUND, new object[] { item });
            }
        }


        private bool SaveLocalConfigurationToUser(Configuration config)
        {
            bool result = false;

            // Set new Unique Id
            string uniqueId = String_Functions.RandomString(20);
            config.UniqueId = uniqueId;
            XML_Functions.SetInnerText(config.ConfigurationXML, "UniqueId", uniqueId);

            // Set new FilePath
            config.FilePath = uniqueId;
            XML_Functions.SetInnerText(config.ConfigurationXML, "FilePath", config.FilePath);

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

        private void SaveLocalImage(string filename)
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


        private void AddDevice_GUI(AddDeviceInfo info)
        {
            if (info.CatalogItem != null) info.CatalogItem.Loading = false;

            if (info.Success && info.Configuration != null)
            {
                // Update DeviceManager list with new Device
                ParentPage.DeviceManager.AddDevice(info.Configuration);
            }
            else
            {
                TH_WPF.MessageBox.Show("Add device failed. Try Again.", "Add device failed", TH_WPF.MessageBoxButtons.Ok);
            }
        }

        #endregion

    }
}
