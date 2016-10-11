// Copyright (c) 2016 Feenux LLC, All Rights Reserved.

// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Windows;

namespace TrakHound_Dashboard
{
    /// <summary>
    /// Interaction logic for UpdateNotification.xaml
    /// </summary>
    public partial class UpdateNotification : Window
    {
        public UpdateNotification()
        {
            InitializeComponent();
        }

        public MainWindow mw;

        private void DeviceManager_Clicked(TrakHound_UI.Button bt)
        {
            mw.DeviceManager_DeviceList_Open();
            Close();
        }

        private void Close_Clicked(TrakHound_UI.Button bt)
        {
            Close();
        }
    }
}
