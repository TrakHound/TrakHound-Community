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

namespace TH_ServerManager.Controls
{
    /// <summary>
    /// Interaction logic for DeviceItem.xaml
    /// </summary>
    public partial class DeviceItem : UserControl
    {
        public DeviceItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region "Properties"

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(DeviceItem), new PropertyMetadata(false));


        public TH_Device_Server.Device_Server.ConnectionStatus Device_ConnectionStatus
        {
            get { return (TH_Device_Server.Device_Server.ConnectionStatus)GetValue(Device_ConnectionStatusProperty); }
            set { SetValue(Device_ConnectionStatusProperty, value); }
        }

        public static readonly DependencyProperty Device_ConnectionStatusProperty =
            DependencyProperty.Register("Device_ConnectionStatus", typeof(TH_Device_Server.Device_Server.ConnectionStatus), typeof(DeviceItem), new PropertyMetadata(TH_Device_Server.Device_Server.ConnectionStatus.Stopped));


        public string Device_CompanyName
        {
            get { return (string)GetValue(Device_CompanyNameProperty); }
            set { SetValue(Device_CompanyNameProperty, value); }
        }

        public static readonly DependencyProperty Device_CompanyNameProperty =
            DependencyProperty.Register("Device_CompanyName", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));

        
        public string Device_Description
        {
            get { return (string)GetValue(Device_DescriptionProperty); }
            set { SetValue(Device_DescriptionProperty, value); }
        }

        public static readonly DependencyProperty Device_DescriptionProperty =
            DependencyProperty.Register("Device_Description", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));

        public string Device_Manufacturer
        {
            get { return (string)GetValue(Device_ManufacturerProperty); }
            set { SetValue(Device_ManufacturerProperty, value); }
        }

        public static readonly DependencyProperty Device_ManufacturerProperty =
            DependencyProperty.Register("Device_Manufacturer", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));

        public string Device_Model
        {
            get { return (string)GetValue(Device_ModelProperty); }
            set { SetValue(Device_ModelProperty, value); }
        }

        public static readonly DependencyProperty Device_ModelProperty =
            DependencyProperty.Register("Device_Model", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));

        public string Device_ID
        {
            get { return (string)GetValue(Device_IDProperty); }
            set { SetValue(Device_IDProperty, value); }
        }

        public static readonly DependencyProperty Device_IDProperty =
            DependencyProperty.Register("Device_ID", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));

        public ImageSource Device_ManufacturerLogo
        {
            get { return (ImageSource)GetValue(Device_ManufacturerLogoProperty); }
            set { SetValue(Device_ManufacturerLogoProperty, value); }
        }

        public static readonly DependencyProperty Device_ManufacturerLogoProperty =
            DependencyProperty.Register("Device_ManufacturerLogo", typeof(ImageSource), typeof(DeviceItem), new PropertyMetadata(null));

        public ImageSource Device_Image
        {
            get { return (ImageSource)GetValue(Device_ImageProperty); }
            set { SetValue(Device_ImageProperty, value); }
        }

        public static readonly DependencyProperty Device_ImageProperty =
            DependencyProperty.Register("Device_Image", typeof(ImageSource), typeof(DeviceItem), new PropertyMetadata(null));


        public string Device_ProcessingStatus
        {
            get { return (string)GetValue(Device_ProcessingStatusProperty); }
            set { SetValue(Device_ProcessingStatusProperty, value); }
        }

        public static readonly DependencyProperty Device_ProcessingStatusProperty =
            DependencyProperty.Register("Device_ProcessingStatus", typeof(string), typeof(DeviceItem), new PropertyMetadata(null));


        #endregion

        public int Index;

        public delegate void Click_Handler(int Index);

        #region "Click"

        public event Click_Handler Selected;

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Selected != null) Selected(Index);
        }



        public event Click_Handler StartClicked;
        public event Click_Handler StopClicked;

        private void Start_Click()
        {
            if (StartClicked != null) StartClicked(Index);
        }

        private void Stop_Click()
        {
            if (StopClicked != null) StopClicked(Index);
        }

        #endregion


        #region "MouseOver"

        public bool MouseOver
        {
            get { return (bool)GetValue(MouseOverProperty); }
            set { SetValue(MouseOverProperty, value); }
        }

        public static readonly DependencyProperty MouseOverProperty =
            DependencyProperty.Register("MouseOver", typeof(bool), typeof(DeviceItem), new PropertyMetadata(false));

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseOver = true;
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseOver = false;
        }

        #endregion

        private void Control_Button_Click()
        {

        }

        #region "Right Click Context Menu"

        public event Click_Handler DropTablesClicked;

        private void MenuItem_DropTables_Click(object sender, RoutedEventArgs e)
        {
            if (DropTablesClicked != null) DropTablesClicked(Index);
        }

        public event Click_Handler StartFromLastClicked;

        private void MenuItem_StartFromLast_Click(object sender, RoutedEventArgs e)
        {
            if (StartFromLastClicked != null) StartFromLastClicked(Index);
        }

        #endregion




    }
}
