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

using System.Collections.ObjectModel;

using TH_UserManagement.Management;

namespace TrakHound_Client.Menus.Main
{
    /// <summary>
    /// Interaction logic for DropDown.xaml
    /// </summary>
    public partial class Menu : UserControl
    {
        public Menu()
        {
            InitializeComponent();

            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;

            if (mw != null)
            {
                mw.ZoomLevelChanged += mw_ZoomLevelChanged;
                mw.CurrentUserChanged += mw_CurrentUserChanged;
            }

            Root_GRID.Width = 0;
            Root_GRID.Height = 0;
           
            //AddOptions_MenuItem();
            AddPlugins_MenuItem();
            AddDeviceManager_MenuItem();
            AddDeveloperConsole_MenuItem();
        }

        public TrakHound_Client.MainWindow mw;

        public bool Shown
        {
            get { return (bool)GetValue(ShownProperty); }
            set
            {
                SetValue(ShownProperty, value);
                if (ShownChanged != null) ShownChanged(value);
            }
        }

        public static readonly DependencyProperty ShownProperty =
            DependencyProperty.Register("Shown", typeof(bool), typeof(Menu), new PropertyMetadata(false));

        public void Hide()
        {
            if (!IsMouseOver) Shown = false;
        }

        public delegate void ShownChanged_Handler(bool val);
        public event ShownChanged_Handler ShownChanged;

        public delegate void Clicked_Handler();


        #region "Menu Items"

        ObservableCollection<object> menuitems;
        public ObservableCollection<object> MenuItems
        {
            get
            {
                if (menuitems == null)
                    menuitems = new ObservableCollection<object>();
                return menuitems;
            }

            set
            {
                menuitems = value;
            }
        }

        #region "Device Manager"

        void AddDeviceManager_MenuItem()
        {
            MenuItem mi = new MenuItem();
            mi.Image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Root.png"));
            mi.Text = "Device Manager";
            mi.Clicked += DeviceManager_Clicked;

            var bt = new TH_WPF.Button();
            bt.ButtonContent = mi;

            MenuItems.Add(bt);
        }

        void DeviceManager_Clicked()
        {
            Shown = false;
            if (mw != null) mw.DeviceManager_DeviceList_Open();
        }

        #endregion

        #region "Options"

        void AddOptions_MenuItem()
        {
            var mi = new MenuItem();
            mi.Image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/options_gear_30px.png"));
            mi.Text = "Options";
            mi.Clicked += Options_Clicked;

            var bt = new TH_WPF.Button();
            bt.ButtonContent = mi;

            MenuItems.Add(bt);
        }

        void Options_Clicked()
        {
            Shown = false;
            if (mw != null) mw.Options_Open();
        }

        #endregion

        #region "Plugins"

        void AddPlugins_MenuItem()
        {
            MenuItem mi = new MenuItem();
            mi.Image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Rocket_02.png"));
            mi.Text = "Plugins";
            mi.Clicked += Plugins_Clicked;

            var bt = new TH_WPF.Button();
            bt.ButtonContent = mi;

            MenuItems.Add(bt);
        }

        void Plugins_Clicked()
        {
            Shown = false;
            if (mw != null) mw.Plugins_Open();
        }

        #endregion

        #region "Developer Console"

        void AddDeveloperConsole_MenuItem()
        {
            MenuItem mi = new MenuItem();
            mi.Image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/Developer_01.png"));
            mi.Text = "Developer Console";
            mi.Clicked += DeveloperConsole_Clicked;

            var bt = new TH_WPF.Button();
            bt.ButtonContent = mi;

            MenuItems.Add(bt);
        }

        void DeveloperConsole_Clicked()
        {
            if (mw != null) mw.developerConsole.Shown = !mw.developerConsole.Shown;

        }

        #endregion

        #region "My Account"

        void AddMyAccount_MenuItem()
        {
            MenuItem mi = new MenuItem();
            mi.Image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/blank_profile_01_sm.png"));
            mi.Text = "My Account";
            mi.Clicked += MyAccount_Clicked;

            var bt = new TH_WPF.Button();
            bt.ButtonContent = mi;

            if (MenuItems.OfType<TH_WPF.Button>().ToList().Find(x => CheckButtonText(x, mi)) == null) MenuItems.Add(bt);
        }

        bool CheckButtonText(TH_WPF.Button bt, MenuItem mi)
        {
            object obj = bt.ButtonContent;
            if (obj != null)
            {
                var content = (MenuItem)obj;
                return content.Text == mi.Text;
            }
            return false;
        }

        void RemoveMyAccount_MenuItem()
        {
            int index = MenuItems.OfType<TH_WPF.Button>().ToList().FindIndex(x => x.Text == "My Account");
            if (index >= 0)
            {
                MenuItems.RemoveAt(index);
            }
        }

        void MyAccount_Clicked()
        {
            Shown = false;
            if (mw != null) mw.AccountManager_Open();
        }

        void mw_CurrentUserChanged(UserConfiguration userConfig)
        {
            if (userConfig != null)
            {
                AddMyAccount_MenuItem();
            }
            else
            {
                RemoveMyAccount_MenuItem();
            }
        }

        #endregion

        #endregion

        #region "Bottom Buttons"

        private void About_Clicked(TH_WPF.Button bt)
        {
            Shown = false;
            mw.About_Open(); 
        }

        private void Exit_BT_Clicked(TH_WPF.Button bt)
        {
            mw.Close();
        }

        #endregion

        #region "Zoom"

        public string ZoomLevelDisplay
        {
            get { return (string)GetValue(ZoomLevelDisplayProperty); }
            set { SetValue(ZoomLevelDisplayProperty, value); }
        }

        public static readonly DependencyProperty ZoomLevelDisplayProperty =
            DependencyProperty.Register("ZoomLevelDisplay", typeof(string), typeof(Menu), new PropertyMetadata("100%"));

        void mw_ZoomLevelChanged(double zoomlevel)
        {
            ZoomLevelDisplay = zoomlevel.ToString("P0");
        }

        private void Zoom_Out_Click(object sender, MouseButtonEventArgs e)
        {
            if (mw != null)
            {
                mw.ZoomLevel = Math.Max(0.75, mw.ZoomLevel - 0.05);
            }
        }

        private void Zoom_In_Click(object sender, MouseButtonEventArgs e)
        {
            if (mw != null)
            {
                mw.ZoomLevel = Math.Min(1.25, mw.ZoomLevel + 0.05);
            }
        }

        #endregion

    }
}
