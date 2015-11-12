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
using System.Xml;
using System.Collections.ObjectModel;

using System.Threading;

using TH_Configuration;
using TH_Configuration.User;
using TH_Global;

namespace TH_DeviceManager.Pages.AddDevice
{
    /// <summary>
    /// Interaction logic for Page.xaml
    /// </summary>
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public UserConfiguration currentuser;

        public delegate void DeviceAdded_Handler();
        public event DeviceAdded_Handler DeviceAdded;

        public DeviceManager deviceManager;

        #region "Shared Configurations"

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

        public void LoadCatalog()
        {
            List<Management.SharedListItem> listitems = Management.GetSharedList();

            shareditems = listitems;

            LoadSharedItems(listitems);
        }

        #endregion

        #region "Buttons"

        public static string OpenConfigurationBrowse()
        {
            string result = null;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = FileLocations.TrakHound;
            dlg.Multiselect = false;
            dlg.Title = "Browse for Device Configuration File";
            dlg.Filter = "Device Configuration files (*.xml) | *.xml";

            Nullable<bool> dialogResult = dlg.ShowDialog();

            if (dialogResult == true)
            {
                if (dlg.FileName != null) result = dlg.FileName;
            }

            return result;
        }

        private void DeviceFromFile_Clicked(TH_DeviceManager.Controls.PageItem item)
        {
            // Browse for Device Configuration path
            string configPath = OpenConfigurationBrowse();
            if (configPath != null)
            {
                // Get Configuration from path
                Configuration config = Configuration.ReadConfigFile(configPath);
                if (config != null)
                {
                    //UserConfiguration currentuser = CurrentUser;

                    if (deviceManager.CurrentUser != null)
                    {
                        if (deviceManager.userDatabaseSettings == null)
                        {
                            TH_Configuration.User.Management.AddConfigurationToUser(deviceManager.CurrentUser, config);

                            //Configurations = TH_Configuration.User.Management.GetConfigurationsForUser(currentuser);
                        }
                        else
                        {
                            //Configurations = TH_Database.Tables.Users.GetConfigurationsForUser(currentuser, mw.userDatabaseSettings);
                        }
                    }
                    // If not logged in Read from File in 'C:\TrakHound\'
                    else
                    {
                        //Configurations = ReadConfigurationFile();
                    }

                    deviceManager.LoadDevices();
                }
            }
        }

        private void NewDevice_Clicked(TH_DeviceManager.Controls.PageItem item)
        {

        }

        #endregion

        const System.Windows.Threading.DispatcherPriority priority = System.Windows.Threading.DispatcherPriority.Background;

        List<Management.SharedListItem> shareditems;
        List<Management.SharedListItem> shareditems_search;

        #region "Shared Items"

        public bool CatalogLoading
        {
            get { return (bool)GetValue(CatalogLoadingProperty); }
            set { SetValue(CatalogLoadingProperty, value); }
        }

        public static readonly DependencyProperty CatalogLoadingProperty =
            DependencyProperty.Register("CatalogLoading", typeof(bool), typeof(Page), new PropertyMetadata(false));


        Thread LoadImage_THREAD;

        void LoadSharedItems(List<Management.SharedListItem> items)
        {
            CatalogLoading = true;

            shareditems_search = new List<Management.SharedListItem>();
            SharedList.Clear();

            if (LoadImage_THREAD != null) LoadImage_THREAD.Abort();

            LoadImage_THREAD = new Thread(new ParameterizedThreadStart(LoadSharedItems_Worker));
            LoadImage_THREAD.Start(items);
        }

        void LoadSharedItems_Worker(object o)
        {
            if (o != null)
            {
                List<Management.SharedListItem> items = (List<Management.SharedListItem>)o;

                foreach (Management.SharedListItem item in items)
                {
                    System.Drawing.Image img = null;

                    // Set Image --------------------------------------------
                    if (item.image_url != null)
                    {
                        img = Images.GetImage(item.image_url);                   
                    }
                    // ------------------------------------------------------

                    this.Dispatcher.BeginInvoke(new Action<Management.SharedListItem, System.Drawing.Image>(LoadSharedItems_GUI), priority, new object[] { item, img });
                } 
            }

            this.Dispatcher.BeginInvoke(new Action(LoadSharedItems_Finish), priority, new object[] { });
        }

        void LoadSharedItems_GUI(Management.SharedListItem listitem, System.Drawing.Image img)
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

            item.Description = listitem.description;
            item.Manufacturer = listitem.manufacturer;
            item.DeviceType = listitem.device_type;
            item.Model = listitem.model;
            item.Controller = listitem.controller;

            item.listitem = listitem;

            item.AddClicked += item_AddClicked;

            SharedList.Add(item);
        }

        void item_AddClicked(Controls.SharedItem item)
        {
            if (item.listitem != null)
            {
                string tablename = item.listitem.tablename;

                if (tablename != null)
                {
                    DataTable dt = Management.GetConfigurationTable(tablename);
                    if (dt != null)
                    {
                        XmlDocument xml = Converter.TableToXML(dt);
                        if (xml != null)
                        {
                            Configuration config = Configuration.ReadConfigFile(xml);
                            if (config != null)
                            {
                                if (currentuser != null)
                                {
                                    Management.AddConfigurationToUser(currentuser, config);
                                }
                                else
                                {

                                }

                                if (DeviceAdded != null) DeviceAdded();
                            }
                        }   
                    }
                }
            }
        }

        void LoadSharedItems_Finish()
        {
            CatalogLoading = false;
        }

        #endregion

        private void Refresh_Clicked(TH_WPF.Button_04 bt)
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

                List<Management.SharedListItem> results = new List<Management.SharedListItem>();

                foreach (string s in searchList)
                {
                    List<Management.SharedListItem> list = shareditems_search.FindAll(
                    x => 
                    (x.manufacturer.ToLower().Contains(s) || x.manufacturer.ToLower() == s) ||
                    (x.model.ToLower().Contains(s) || x.model.ToLower() == s) ||
                    (x.author.ToLower().Contains(s) || x.author.ToLower() == s) ||
                    (x.description.ToLower().Contains(s) || x.description.ToLower() == s) ||
                    (x.controller.ToLower().Contains(s) || x.controller.ToLower() == s) ||
                    (x.device_type.ToLower().Contains(s) || x.device_type.ToLower() == s)
                    );

                    foreach (Management.SharedListItem item in list)
                    {
                        if (results.Find(x => x.uniqueId == item.uniqueId) == null) results.Add(item);
                    }
                }

                LoadSharedItems(results);

            }
            else
            {
                LoadSharedItems(shareditems);
            }

        }

    }
}
