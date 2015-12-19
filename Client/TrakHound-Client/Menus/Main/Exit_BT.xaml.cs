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

namespace TrakHound_Client.Menus.Main
{
    /// <summary>
    /// Interaction logic for Exit_BT.xaml
    /// </summary>
    public partial class Exit_BT : UserControl
    {
        public Exit_BT()
        {
            InitializeComponent();
        }

        public delegate void Clicked_Handler(Exit_BT bt);
        public event Clicked_Handler Clicked;

        private void Border_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Clicked != null) Clicked(this);
        }
    }
}
