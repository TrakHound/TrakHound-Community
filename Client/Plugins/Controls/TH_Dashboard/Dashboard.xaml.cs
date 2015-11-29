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

using System.IO;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using TH_Configuration;
using TH_PlugIns_Client_Control;
//using TH_Device_Client;
using TH_WPF;

namespace TH_Dashboard
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class Dashboard : UserControl, Control_PlugIn
    {

        #region "PlugIn"

        #region "Descriptive"

        public string Title { get { return "Dashboard"; } }

        public string Description { get { return "Contains and organizes pages for displaying Device data in various ways. Acts as the Home page for other Device Monitoring Plugins."; } }

        public ImageSource Image { get { return new BitmapImage(new Uri("pack://application:,,,/TH_Dashboard;component/Images/Dashboard.png")); } }


        public string Author { get { return "TrakHound"; } }

        public string AuthorText { get { return "©2015 Feenux LLC. All Rights Reserved"; } }

        public ImageSource AuthorImage { get { return new BitmapImage(new Uri("pack://application:,,,/TH_Dashboard;component/Resources/TrakHound_Logo_10_200px.png")); } }


        public string LicenseName { get { return "GPLv3"; } }

        public string LicenseText { get { return File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\License\" + "License.txt"); } }

        #endregion

        #region "Plugin Properties/Options"

        public string DefaultParent { get { return null; } }
        public string DefaultParentCategory { get { return null; } }

        public bool AcceptsPlugIns { get { return true; } }

        public bool OpenOnStartUp { get { return true; } }

        public bool ShowInAppMenu { get { return true; } }

        public List<PlugInConfigurationCategory> SubCategories { get; set; }

        public List<Control_PlugIn> PlugIns { get; set; }

        #endregion

        #region "Update Information"

        public string UpdateFileURL { get { return "http://www.feenux.com/trakhound/appinfo/th/dashboard-appinfo.txt"; } }

        #endregion

        #region "Methods"

        const System.Windows.Threading.DispatcherPriority Priority = System.Windows.Threading.DispatcherPriority.Background;

        public void Initialize()
        {
            EnabledPlugIns = new List<PlugInConfiguration>();

            foreach (PlugInConfigurationCategory category in SubCategories)
            {
                foreach (PlugInConfiguration config in category.PlugInConfigurations)
                {
                    config.EnabledChanged += config_EnabledChanged;

                    if (config.enabled) PlugIns_Load(config);
                }
            }
        }

        //public void Update(ReturnData rd)
        //{
        //    if (PlugIns != null)
        //    {
        //        foreach (Control_PlugIn CP in PlugIns)
        //        {
        //            this.Dispatcher.BeginInvoke(new Action<ReturnData>(CP.Update), Priority, new object[] { rd });
        //        }
        //    }
        //}

        public void Closing() { }

        public void Show() 
        {
            if (ShowRequested != null)
            {
                PluginShowInfo info = new PluginShowInfo();
                info.Page = this;
                ShowRequested(info);
            }
        }

        #endregion

        #region "Events"

        public void Update_DataEvent(DataEvent_Data de_d)
        {
            if (PlugIns != null)
            {
                foreach (Control_PlugIn CP in PlugIns)
                {
                    this.Dispatcher.BeginInvoke(new Action<DataEvent_Data>(CP.Update_DataEvent), Priority, new object[] { de_d });
                }
            }        
        }

        public event DataEvent_Handler DataEvent;

        public event PlugInTools.ShowRequested_Handler ShowRequested;

        #endregion

        #region "Device Properties"

        //public List<Device_Client> Devices { get; set; }

        public List<Configuration> Devices { get; set; }

        #endregion

        #region "Options"

        public OptionsPage Options { get; set; }

        #endregion

        public object RootParent { get; set; }

        #endregion

        #region "Dashboard"

        public Dashboard()
        {
            InitializeComponent();

            SubCategories = new List<PlugInConfigurationCategory>();
            PlugInConfigurationCategory pages = new PlugInConfigurationCategory();
            pages.name = "Pages";
            SubCategories.Add(pages);
        }

        #region "Child PlugIns"

        PlugInConfiguration currentPage;

        List<PlugInConfiguration> EnabledPlugIns;

        public void PlugIns_Load(PlugInConfiguration config)
        {
            if (PlugIns != null)
            {
                if (!EnabledPlugIns.Contains(config))
                {
                    Control_PlugIn CP = PlugIns.Find(x => x.Title.ToUpper() == config.name.ToUpper());
                    if (CP != null)
                    {
                        try
                        {
                            //CP.Devices = Devices;
                            CP.SubCategories = config.SubCategories;
                            CP.DataEvent += CP_DataEvent;

                            CP.PlugIns = new List<Control_PlugIn>();

                            if (CP.SubCategories != null)
                            {
                                foreach (PlugInConfigurationCategory subcategory in CP.SubCategories)
                                {
                                    foreach (PlugInConfiguration subConfig in subcategory.PlugInConfigurations)
                                    {
                                        Control_PlugIn sCP = PlugIns.Find(x => x.Title.ToUpper() == subConfig.name.ToUpper());
                                        if (sCP != null)
                                        {
                                            CP.PlugIns.Add(sCP);
                                        }
                                    }
                                }
                            }

                            CP.Initialize();
                        }

                        catch { }

                        ListButton lb = new ListButton();
                        lb.Text = config.name;
                        lb.Image = CP.Image;
                        lb.Selected += lb_Selected;
                        lb.DataObject = CP;
                        Pages_STACK.Children.Add(lb);

                        EnabledPlugIns.Add(config);
                    }
                }
            }
        }

        void CP_DataEvent(DataEvent_Data de_d)
        {
            if (DataEvent != null) DataEvent(de_d);
        }

        public void PlugIns_Unload(PlugInConfiguration config)
        {

            if (config != null)
            {
                if (!config.enabled)
                {
                    ListButton lb = Pages_STACK.Children.OfType<ListButton>().ToList().Find(x => x.Text.ToUpper() == config.name.ToUpper());
                    if (lb != null)
                    {
                        Pages_STACK.Children.Remove(lb);
                    }

                    if (config == currentPage) Content_GRID.Children.Clear();

                    if (EnabledPlugIns.Contains(config)) EnabledPlugIns.Remove(config);

                }
            }

        }

        private void lb_Selected(ListButton LB)
        {
            foreach (ListButton oLB in Pages_STACK.Children.OfType<ListButton>())
            {
                if (oLB == LB) oLB.IsSelected = true;
                else oLB.IsSelected = false;
            }

            foreach (PlugInConfigurationCategory category in SubCategories)
            {
                PlugInConfiguration config = category.PlugInConfigurations.Find(x => x.name.ToUpper() == LB.Text.ToUpper());
                if (config != null)
                {
                    currentPage = config;
                    break;
                }

            }

            UserControl childPlugIn = LB.DataObject as UserControl;

            Content_GRID.Children.Clear();
            Content_GRID.Children.Add(childPlugIn);
        }

        void config_EnabledChanged(PlugInConfiguration config)
        {
            if (config.enabled) PlugIns_Load(config);
            else PlugIns_Unload(config);
        }

        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Pages_STACK.Children.Count > 0)
            {
                if (Pages_STACK.Children[0].GetType() == typeof(ListButton))
                {
                    ListButton lb = (ListButton)Pages_STACK.Children[0];

                    foreach (ListButton oLB in Pages_STACK.Children.OfType<ListButton>())
                    {
                        if (oLB == lb) oLB.IsSelected = true;
                        else oLB.IsSelected = false;
                    }

                    foreach (PlugInConfigurationCategory category in SubCategories)
                    {
                        PlugInConfiguration config = category.PlugInConfigurations.Find(x => x.name.ToUpper() == lb.Text.ToUpper());
                        if (config != null)
                        {
                            currentPage = config;
                            break;
                        }
                    }

                    UserControl childPlugIn = lb.DataObject as UserControl;

                    Content_GRID.Children.Clear();
                    Content_GRID.Children.Add(childPlugIn);
                }
            }
        }

        #endregion
      
    }
}
