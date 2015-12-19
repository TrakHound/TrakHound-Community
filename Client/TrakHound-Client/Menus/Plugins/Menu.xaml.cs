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

namespace TrakHound_Client.Menus.Plugins
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : UserControl
    {
        public Menu()
        {
            InitializeComponent();
            DataContext = this;
        }

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

        #region "Plugin List"

        ObservableCollection<PluginItem> plugins;
        public ObservableCollection<PluginItem> Plugins
        {
            get
            {
                if (plugins == null) plugins = new ObservableCollection<PluginItem>();
                return plugins;
            }
            set
            {
                plugins = value;
            }
        }

        #endregion

    }
}
