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

namespace TrakHound_Client.Main_Menu
{
    /// <summary>
    /// Interaction logic for DropDown.xaml
    /// </summary>
    public partial class DropDown : UserControl
    {
        public DropDown()
        {
            InitializeComponent();

            DataContext = this;

            mw = Application.Current.MainWindow as MainWindow;

            if (mw != null) mw.ZoomLevelChanged += mw_ZoomLevelChanged;

            Root_GRID.Width = 0;
            Root_GRID.Height = 0;

            AddOptions_MenuItem();
            AddPlugins_MenuItem();
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
            DependencyProperty.Register("Shown", typeof(bool), typeof(DropDown), new PropertyMetadata(false));

        public void Hide()
        {
            if (!IsMouseOver) Shown = false;
        }

        public delegate void ShownChanged_Handler(bool val);
        public event ShownChanged_Handler ShownChanged;

        public delegate void Clicked_Handler();


        #region "Menu Items"

        ObservableCollection<MenuItem> menuitems;
        public ObservableCollection<MenuItem> MenuItems
        {
            get
            {
                if (menuitems == null)
                    menuitems = new ObservableCollection<MenuItem>();
                return menuitems;
            }

            set
            {
                menuitems = value;
            }
        }


        #region "Options"

        void AddOptions_MenuItem()
        {
            MenuItem mi = new MenuItem();
            mi.Image = new BitmapImage(new Uri("pack://application:,,,/TrakHound-Client;component/Resources/options_gear_30px.png"));
            mi.Text = "Options";
            mi.Clicked += Options_Clicked;
            MenuItems.Add(mi);
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
            MenuItems.Add(mi);
        }

        void Plugins_Clicked()
        {
            Shown = false;
            if (mw != null) mw.Plugins_Open();
        }

        #endregion

        #endregion

        #region "Bottom Buttons"

        private void About_Clicked(Controls.TH_Button bt)
        {
            Shown = false;
            mw.About_Open(); 
        }

        private void Exit_BT_Clicked(Exit_BT bt)
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
            DependencyProperty.Register("ZoomLevelDisplay", typeof(string), typeof(DropDown), new PropertyMetadata("100%"));

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
